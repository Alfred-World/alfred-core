using Alfred.Core.Domain.Abstractions.Services;

namespace Alfred.Core.Infrastructure.Common.Options;

/// <summary>
/// Configuration settings for Cloudflare R2 object storage.
/// Populated from environment variables in ServiceCollectionExtensions.
/// </summary>
public sealed class R2StorageOptions : IStorageSettings
{
    /// <summary>
    /// Cloudflare Account ID
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// R2 Access Key ID (from Cloudflare R2 API tokens)
    /// </summary>
    public string AccessKeyId { get; set; } = string.Empty;

    /// <summary>
    /// R2 Secret Access Key (from Cloudflare R2 API tokens)
    /// </summary>
    public string SecretAccessKey { get; set; } = string.Empty;

    /// <summary>
    /// R2 Bucket name
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// Default presigned URL expiration in minutes for uploads
    /// </summary>
    public int UploadUrlExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Default presigned URL expiration in minutes for downloads
    /// </summary>
    public int DownloadUrlExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Maximum allowed file size in bytes (default: 50MB)
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 50 * 1024 * 1024;

    /// <summary>
    /// Allowed MIME types for upload. Empty means all types are allowed.
    /// </summary>
    public string[] AllowedContentTypes { get; set; } = [];

    /// <summary>
    /// The S3-compatible endpoint URL for R2
    /// </summary>
    public string Endpoint => $"https://{AccountId}.r2.cloudflarestorage.com";

    /// <summary>
    /// Validate that required settings are configured
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(AccountId))
        {
            throw new InvalidOperationException("R2_ACCOUNT_ID is required for R2 storage.");
        }

        if (string.IsNullOrWhiteSpace(AccessKeyId))
        {
            throw new InvalidOperationException("R2_ACCESS_KEY_ID is required for R2 storage.");
        }

        if (string.IsNullOrWhiteSpace(SecretAccessKey))
        {
            throw new InvalidOperationException("R2_SECRET_ACCESS_KEY is required for R2 storage.");
        }

        if (string.IsNullOrWhiteSpace(BucketName))
        {
            throw new InvalidOperationException("R2_BUCKET_NAME is required for R2 storage.");
        }
    }
}
