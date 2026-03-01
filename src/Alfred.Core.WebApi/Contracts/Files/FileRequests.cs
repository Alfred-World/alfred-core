using Alfred.Core.Application.Files.Dtos;

namespace Alfred.Core.WebApi.Contracts.Files;

/// <summary>
/// Request to generate a presigned upload URL.
/// FE sends this before uploading directly to R2.
/// </summary>
public sealed record GenerateUploadUrlRequest
{
    /// <summary>
    /// Original file name (e.g., "anh-dep.jpg")
    /// </summary>
    public string FileName { get; init; } = null!;

    /// <summary>
    /// MIME type (e.g., "image/jpeg", "application/pdf")
    /// </summary>
    public string ContentType { get; init; } = null!;

    /// <summary>
    /// File size in bytes (for server-side validation before generating URL)
    /// </summary>
    public long FileSize { get; init; }

    /// <summary>
    /// Optional folder/prefix for organizing files (e.g., "avatars", "documents")
    /// </summary>
    public string? Folder { get; init; }

    public GenerateUploadUrlDto ToDto()
    {
        return new GenerateUploadUrlDto(FileName, ContentType, FileSize, Folder);
    }
}

/// <summary>
/// Request to generate a presigned download URL
/// </summary>
public sealed record GenerateDownloadUrlRequest
{
    /// <summary>
    /// The object key returned from the upload URL generation
    /// </summary>
    public string ObjectKey { get; init; } = null!;

    public GenerateDownloadUrlDto ToDto()
    {
        return new GenerateDownloadUrlDto(ObjectKey);
    }
}

/// <summary>
/// Request to delete a file from R2
/// </summary>
public sealed record DeleteFileRequest
{
    /// <summary>
    /// The object key of the file to delete
    /// </summary>
    public string ObjectKey { get; init; } = null!;

    public DeleteFileDto ToDto()
    {
        return new DeleteFileDto(ObjectKey);
    }
}
