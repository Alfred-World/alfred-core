using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Alfred.Core.Domain.Abstractions.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace Alfred.Core.Infrastructure.Services;

/// <summary>
/// Reads identity user events from Redis Stream and updates local replicated_users table.
/// </summary>
public sealed class IdentityUserReplicationStreamWorker : BackgroundService
{
    private const string DefaultStreamKey = "identity:user-events";
    private const int DefaultPollMilliseconds = 2000;
    private const string DefaultConsumerGroup = "core-identity-replication";
    private const string DefaultConsumerNamePrefix = "core-worker";

    private readonly ILogger<IdentityUserReplicationStreamWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public IdentityUserReplicationStreamWorker(
        ILogger<IdentityUserReplicationStreamWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!IsRedisEnabled())
        {
            _logger.LogInformation("Identity replication worker skipped because CACHE_PROVIDER is not Redis.");
            return;
        }

        var streamKey = Environment.GetEnvironmentVariable("IDENTITY_USER_STREAM_KEY") ?? DefaultStreamKey;
        var pollMs = GetIntEnv("IDENTITY_USER_STREAM_POLL_MS", DefaultPollMilliseconds);
        var groupName = Environment.GetEnvironmentVariable("IDENTITY_USER_STREAM_GROUP") ?? DefaultConsumerGroup;
        var consumerName =
            $"{Environment.GetEnvironmentVariable("IDENTITY_USER_STREAM_CONSUMER") ?? DefaultConsumerNamePrefix}-{Environment.MachineName}";

        var redisConnection = await ConnectRedisAsync(stoppingToken);
        if (redisConnection is null)
        {
            _logger.LogWarning("Identity replication worker stopped: unable to connect Redis.");
            return;
        }

        var db = redisConnection.GetDatabase();
        await EnsureConsumerGroupAsync(db, streamKey, groupName);

        _logger.LogInformation(
            "Identity replication worker started. Stream: {StreamKey}, Group: {Group}, Consumer: {Consumer}",
            streamKey,
            groupName,
            consumerName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var entries = await db.StreamReadGroupAsync(streamKey, groupName, consumerName, ">", 50);

                if (entries.Length == 0)
                {
                    // Retry pending messages assigned to this consumer.
                    entries = await db.StreamReadGroupAsync(streamKey, groupName, consumerName, "0-0", 50);
                }

                if (entries.Length == 0)
                {
                    await Task.Delay(pollMs, stoppingToken);
                    continue;
                }

                foreach (var entry in entries)
                {
                    var evt = ParseEvent(entry);
                    if (evt is null)
                    {
                        await db.StreamAcknowledgeAsync(streamKey, groupName, entry.Id);
                        continue;
                    }

                    await HandleEventAsync(evt, stoppingToken);
                    await db.StreamAcknowledgeAsync(streamKey, groupName, entry.Id);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing identity replication stream. Retrying...");
                await Task.Delay(pollMs, stoppingToken);
            }
        }

        await redisConnection.DisposeAsync();
    }

    private async Task HandleEventAsync(IdentityUserStreamEvent evt, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var replicationService = scope.ServiceProvider.GetRequiredService<IIdentityUserReplicationService>();

        if (evt.IsDelete)
        {
            await replicationService.DeleteAsync(evt.UserId, cancellationToken);
            return;
        }

        if (string.IsNullOrWhiteSpace(evt.UserName))
        {
            _logger.LogWarning("Skip identity user upsert event because username is empty. UserId: {UserId}",
                evt.UserId);
            return;
        }

        if (string.IsNullOrWhiteSpace(evt.Email))
        {
            _logger.LogWarning("Skip identity user upsert event because email is empty. UserId: {UserId}", evt.UserId);
            return;
        }

        await replicationService.UpsertAsync(
            (ReplicatedUserId)evt.UserId,
            evt.UserName!,
            evt.Email!,
            evt.FullName,
            evt.Avatar,
            cancellationToken);
    }

    private static IdentityUserStreamEvent? ParseEvent(StreamEntry entry)
    {
        var fields = entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());

        if (fields.TryGetValue("payload", out var payload) && !string.IsNullOrWhiteSpace(payload))
        {
            try
            {
                return JsonSerializer.Deserialize<IdentityUserStreamEvent>(payload);
            }
            catch
            {
                return null;
            }
        }

        if (!TryResolveUserId(fields, out var userId) || userId == Guid.Empty)
        {
            return null;
        }

        fields.TryGetValue("action", out var action);
        fields.TryGetValue("eventType", out var eventType);
        fields.TryGetValue("userName", out var userName);
        fields.TryGetValue("email", out var email);
        fields.TryGetValue("fullName", out var fullName);
        fields.TryGetValue("avatar", out var avatar);

        return new IdentityUserStreamEvent
        {
            Action = action,
            EventType = eventType,
            UserId = userId,
            UserName = userName,
            Email = email,
            FullName = fullName,
            Avatar = avatar
        };
    }

    private async Task EnsureConsumerGroupAsync(IDatabase db, string streamKey, string groupName)
    {
        try
        {
            _ = await db.StreamCreateConsumerGroupAsync(streamKey, groupName, "0-0", true);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP", StringComparison.OrdinalIgnoreCase))
        {
            // Group already exists.
        }
    }

    private static bool TryResolveUserId(IReadOnlyDictionary<string, string> fields, out Guid userId)
    {
        userId = Guid.Empty;

        // Prefer the authoritative identity Guid sent as "userGuid" over the legacy int64 "userId" field.
        if (fields.TryGetValue("userGuid", out var rawUserGuid) && Guid.TryParse(rawUserGuid, out var guidUserId)
                                                                && guidUserId != Guid.Empty)
        {
            userId = guidUserId;
            return true;
        }

        if (fields.TryGetValue("userId", out var rawUserId) && TryParseUserId(rawUserId, out userId))
        {
            return true;
        }

        return false;
    }

    private static bool TryParseUserId(string? rawUserId, out Guid userId)
    {
        userId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(rawUserId))
        {
            return false;
        }

        if (long.TryParse(rawUserId, out var legacyUserId))
        {
            if (legacyUserId <= 0)
            {
                return false;
            }

            userId = MapLegacyInt64ToGuid(legacyUserId);
            return true;
        }

        if (!Guid.TryParse(rawUserId, out var guidUserId))
        {
            return false;
        }

        if (guidUserId == Guid.Empty)
        {
            return false;
        }

        userId = guidUserId;
        return true;
    }

    private static Guid MapLegacyInt64ToGuid(long userId)
    {
        var raw = Encoding.UTF8.GetBytes($"legacy-user:{userId}");
        var hash = MD5.HashData(raw);
        return new Guid(hash);
    }

    private static bool IsRedisEnabled()
    {
        var provider = Environment.GetEnvironmentVariable("CACHE_PROVIDER") ?? "InMemory";
        return provider.Equals("Redis", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<IConnectionMultiplexer?> ConnectRedisAsync(CancellationToken cancellationToken)
    {
        var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
        var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";
        var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD");

        var config = new ConfigurationOptions
        {
            EndPoints = { $"{redisHost}:{redisPort}" },
            AbortOnConnectFail = false,
            ConnectRetry = 5,
            ConnectTimeout = 5000
        };

        if (!string.IsNullOrWhiteSpace(redisPassword))
        {
            config.Password = redisPassword;
        }

        try
        {
            return await ConnectionMultiplexer.ConnectAsync(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to connect Redis for identity replication stream.");
            return null;
        }
    }

    private static int GetIntEnv(string key, int fallback)
    {
        var raw = Environment.GetEnvironmentVariable(key);
        return int.TryParse(raw, out var value) && value > 0 ? value : fallback;
    }

    private sealed record IdentityUserStreamEvent
    {
        public string? Action { get; init; }
        public string? EventType { get; init; }
        public Guid UserId { get; init; }
        public string? UserName { get; init; }
        public string? Email { get; init; }
        public string? FullName { get; init; }
        public string? Avatar { get; init; }

        public bool IsDelete => string.Equals(Action, "DELETE", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(EventType, "UserDeleted", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(EventType, "DELETE", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(EventType, "Deleted", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(EventType, "delete", StringComparison.OrdinalIgnoreCase);
    }
}
