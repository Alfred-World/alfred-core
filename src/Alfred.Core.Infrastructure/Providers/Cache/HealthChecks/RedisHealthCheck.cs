using System.Diagnostics;

using Alfred.Core.Infrastructure.Common.HealthChecks;

using StackExchange.Redis;

namespace Alfred.Core.Infrastructure.Providers.Cache.HealthChecks;

/// <summary>
/// Health check for Redis connectivity and authentication.
/// Only runs when CACHE_PROVIDER=Redis; returns Healthy immediately otherwise.
/// Creates a dedicated short-lived connection to validate credentials before the
/// application singleton ICacheProvider is constructed.
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    public string ServiceName => "Redis";

    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var provider = Environment.GetEnvironmentVariable("CACHE_PROVIDER") ?? "InMemory";

        // Redis is only required when explicitly configured
        if (!provider.Equals("Redis", StringComparison.OrdinalIgnoreCase))
        {
            return HealthCheckResult.Healthy(
                ServiceName,
                $"Redis check skipped (CACHE_PROVIDER={provider})");
        }

        var stopwatch = Stopwatch.StartNew();

        var host = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
        var port = int.TryParse(Environment.GetEnvironmentVariable("REDIS_PORT"), out var p) ? p : 6379;
        var password = Environment.GetEnvironmentVariable("REDIS_PASSWORD") ?? string.Empty;

        try
        {
            var options = new ConfigurationOptions
            {
                EndPoints = { { host, port } },
                AbortOnConnectFail = true,
                ConnectTimeout = 5_000,
                SyncTimeout = 5_000,
                ReconnectRetryPolicy = new LinearRetry(0),
                ConnectRetry = 1
            };

            if (!string.IsNullOrEmpty(password))
            {
                options.Password = password;
            }

            using var mux = await ConnectionMultiplexer.ConnectAsync(options);

            if (!mux.IsConnected)
            {
                stopwatch.Stop();
                return HealthCheckResult.Unhealthy(
                    ServiceName,
                    $"Redis at {host}:{port} is not connected",
                    stopwatch.Elapsed);
            }

            var db = mux.GetDatabase();
            var latency = await db.PingAsync();

            stopwatch.Stop();
            return HealthCheckResult.Healthy(
                ServiceName,
                $"Redis at {host}:{port} is operational (latency: {latency.TotalMilliseconds:F1}ms)",
                stopwatch.Elapsed);
        }
        catch (RedisConnectionException ex)
        {
            stopwatch.Stop();
            return HealthCheckResult.Unhealthy(
                ServiceName,
                $"Redis connection failed at {host}:{port} — {ex.InnerException?.Message ?? ex.Message}",
                stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return HealthCheckResult.Unhealthy(
                ServiceName,
                $"Redis health check failed: {ex.Message}",
                stopwatch.Elapsed);
        }
    }
}
