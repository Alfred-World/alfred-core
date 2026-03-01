using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Alfred.Core.Domain.Abstractions.Services;
using Alfred.Core.Infrastructure.Common.Options;

using Microsoft.Extensions.Logging;

namespace Alfred.Core.Infrastructure.Services;

/// <summary>
/// Queries the Cloudflare GraphQL Analytics API to get estimated R2 bucket usage.
/// Requires a Cloudflare API token with "Account Analytics: Read" permission.
/// Data is updated periodically (every few minutes) — suitable for soft quota checks.
/// </summary>
public sealed class CloudflareR2MetricsService : IR2MetricsService
{
    private const string GraphQlEndpoint = "https://api.cloudflare.com/client/v4/graphql";

    private readonly HttpClient _httpClient;
    private readonly R2StorageOptions _options;
    private readonly ILogger<CloudflareR2MetricsService> _logger;

    public CloudflareR2MetricsService(
        HttpClient httpClient,
        R2StorageOptions options,
        ILogger<CloudflareR2MetricsService> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<long?> GetEstimatedUsageBytesAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.CloudflareApiToken))
        {
            _logger.LogDebug("CF_API_TOKEN is not configured — R2 quota check skipped.");
            return null;
        }

        try
        {
            // Query last 7 days; "max" aggregate returns the peak recorded usage
            var dateFrom = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
            var dateTo = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

            var query = $$"""
                {
                  "query": "{ viewer { accounts(filter: {accountTag: \"{{_options.AccountId}}\"}) { r2StorageUsageAdaptiveGroups(filter: {date_geq: \"{{dateFrom}}\", date_leq: \"{{dateTo}}\", bucketName: \"{{_options.BucketName}}\"}, limit: 1, orderBy: [date_DESC]) { max { payloadSize metadataSize } } } } }"
                }
                """;

            using var request = new HttpRequestMessage(HttpMethod.Post, GraphQlEndpoint)
            {
                Headers = { { "Authorization", $"Bearer {_options.CloudflareApiToken}" } },
                Content = new StringContent(query, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<CloudflareGraphQlResponse>(
                cancellationToken: cancellationToken);

            var groups = result?.Data?.Viewer?.Accounts?.FirstOrDefault()
                              ?.R2StorageUsageAdaptiveGroups;

            if (groups is null || groups.Count == 0)
            {
                _logger.LogDebug("No R2 storage usage data returned for bucket '{Bucket}'.", _options.BucketName);
                return 0L;
            }

            var max = groups[0].Max;
            var totalBytes = (max?.PayloadSize ?? 0L) + (max?.MetadataSize ?? 0L);

            _logger.LogDebug(
                "R2 bucket '{Bucket}' estimated usage: payload={Payload} B, metadata={Metadata} B, total={Total} B",
                _options.BucketName, max?.PayloadSize, max?.MetadataSize, totalBytes);

            return totalBytes;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch R2 storage metrics from Cloudflare API. Quota check skipped.");
            return null;
        }
    }

    // ── JSON deserialization models ────────────────────────────────────────────

    private sealed class CloudflareGraphQlResponse
    {
        [JsonPropertyName("data")]
        public GraphQlData? Data { get; init; }
    }

    private sealed class GraphQlData
    {
        [JsonPropertyName("viewer")]
        public Viewer? Viewer { get; init; }
    }

    private sealed class Viewer
    {
        [JsonPropertyName("accounts")]
        public List<AccountData>? Accounts { get; init; }
    }

    private sealed class AccountData
    {
        [JsonPropertyName("r2StorageUsageAdaptiveGroups")]
        public List<UsageGroup>? R2StorageUsageAdaptiveGroups { get; init; }
    }

    private sealed class UsageGroup
    {
        [JsonPropertyName("max")]
        public UsageMax? Max { get; init; }
    }

    private sealed class UsageMax
    {
        [JsonPropertyName("payloadSize")]
        public long PayloadSize { get; init; }

        [JsonPropertyName("metadataSize")]
        public long MetadataSize { get; init; }
    }
}
