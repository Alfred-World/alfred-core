using Alfred.Core.Application.Files.Dtos;

namespace Alfred.Core.Application.Files;

/// <summary>
/// Application service for file operations (presigned URL generation, deletion).
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Generate a presigned URL for direct file upload to R2.
    /// Validates file size, content type, and generates a unique object key.
    /// </summary>
    Task<UploadUrlResultDto> GenerateUploadUrlAsync(
        GenerateUploadUrlDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a presigned URL for downloading a file from R2.
    /// </summary>
    Task<DownloadUrlResultDto> GenerateDownloadUrlAsync(
        GenerateDownloadUrlDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a file from R2 storage.
    /// </summary>
    Task DeleteFileAsync(
        DeleteFileDto dto,
        CancellationToken cancellationToken = default);
}
