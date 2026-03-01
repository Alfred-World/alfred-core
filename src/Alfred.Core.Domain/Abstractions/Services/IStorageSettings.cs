namespace Alfred.Core.Domain.Abstractions.Services;

/// <summary>
/// Configuration contract for object storage.
/// Infrastructure provides the concrete implementation with actual values.
/// </summary>
public interface IStorageSettings
{
    /// <summary>
    /// Maximum allowed file size in bytes
    /// </summary>
    long MaxFileSizeBytes { get; }

    /// <summary>
    /// Allowed MIME types for upload. Empty means all types are allowed.
    /// </summary>
    string[] AllowedContentTypes { get; }

    /// <summary>
    /// Default presigned URL expiration in minutes for uploads
    /// </summary>
    int UploadUrlExpirationMinutes { get; }

    /// <summary>
    /// Default presigned URL expiration in minutes for downloads
    /// </summary>
    int DownloadUrlExpirationMinutes { get; }

    /// <summary>
    /// Cloudflare API token with Account Analytics: Read permission.
    /// Used to query the Cloudflare GraphQL Analytics API for bucket storage metrics.
    /// Leave empty to disable quota checking.
    /// </summary>
    string CloudflareApiToken { get; }

    /// <summary>
    /// Total storage quota in bytes. Default 10 GB (Cloudflare R2 free tier).
    /// Set to 0 to disable quota checking.
    /// </summary>
    long StorageQuotaBytes { get; }
}
