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
}
