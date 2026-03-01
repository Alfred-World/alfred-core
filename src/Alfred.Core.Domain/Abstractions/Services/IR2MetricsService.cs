namespace Alfred.Core.Domain.Abstractions.Services;

/// <summary>
/// Provides storage usage metrics for Cloudflare R2 via the Cloudflare GraphQL Analytics API.
/// Data is updated periodically (typically within minutes), making it suitable for
/// soft quota enforcement without scanning the bucket.
/// </summary>
public interface IR2MetricsService
{
    /// <summary>
    /// Returns the estimated total bytes stored in the configured bucket.
    /// Calls the Cloudflare GraphQL Analytics API — O(1), no bucket scan.
    /// Returns null if the API token is not configured or the call fails gracefully.
    /// </summary>
    Task<long?> GetEstimatedUsageBytesAsync(CancellationToken cancellationToken = default);
}
