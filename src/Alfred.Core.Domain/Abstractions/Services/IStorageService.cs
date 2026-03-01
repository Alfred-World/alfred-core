namespace Alfred.Core.Domain.Abstractions.Services;

/// <summary>
/// Abstraction for object storage operations (e.g., Cloudflare R2, AWS S3).
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Generate a presigned URL for uploading a file directly to object storage.
    /// The client uses this URL to PUT the file without going through the backend.
    /// </summary>
    /// <param name="objectKey">The storage key/path for the object (e.g., "uploads/2026/02/abc.jpg")</param>
    /// <param name="contentType">The MIME type of the file (e.g., "image/jpeg")</param>
    /// <param name="expirationInMinutes">How long the presigned URL is valid</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The presigned URL for upload</returns>
    Task<string> GeneratePresignedUploadUrlAsync(
        string objectKey,
        string contentType,
        int expirationInMinutes = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload a file directly to object storage via server-side streaming.
    /// Use this to avoid browser CORS restrictions on direct R2 uploads.
    /// </summary>
    Task UploadFileAsync(
        Stream fileStream,
        string objectKey,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a presigned URL for downloading/viewing a file from object storage.
    /// </summary>
    /// <param name="objectKey">The storage key/path for the object</param>
    /// <param name="expirationInMinutes">How long the presigned URL is valid</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The presigned URL for download</returns>
    Task<string> GeneratePresignedDownloadUrlAsync(
        string objectKey,
        int expirationInMinutes = 60,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an object from storage.
    /// </summary>
    /// <param name="objectKey">The storage key/path for the object</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteObjectAsync(string objectKey, CancellationToken cancellationToken = default);
}
