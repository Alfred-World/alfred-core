namespace Alfred.Core.Application.Files.Dtos;

/// <summary>
/// Request DTO for generating a presigned upload URL
/// </summary>
public sealed record GenerateUploadUrlDto(
    string FileName,
    string ContentType,
    long FileSize,
    string? Folder = null);

/// <summary>
/// Response DTO containing the presigned upload URL and metadata.
/// No public URLs are exposed — all access must go through presigned URLs.
/// </summary>
public sealed record UploadUrlResultDto(
    string UploadUrl,
    string ObjectKey,
    DateTime ExpiresAt);

/// <summary>
/// Request DTO for generating a presigned download URL
/// </summary>
public sealed record GenerateDownloadUrlDto(string ObjectKey);

/// <summary>
/// Response DTO containing the presigned download URL
/// </summary>
public sealed record DownloadUrlResultDto(
    string DownloadUrl,
    DateTime ExpiresAt);

/// <summary>
/// Request DTO for deleting a file
/// </summary>
public sealed record DeleteFileDto(string ObjectKey);
