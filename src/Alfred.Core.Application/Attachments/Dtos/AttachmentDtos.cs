namespace Alfred.Core.Application.Attachments.Dtos;

/// <summary>
/// Attachment returned to the client — never exposes raw ObjectKey or direct R2 URLs.
/// DownloadUrl is always a time-limited presigned URL.
/// </summary>
public sealed record AttachmentDto(
    Guid Id,
    Guid TargetId,
    string TargetType,
    string FileName,
    string ContentType,
    long FileSize,
    string Purpose,
    string DownloadUrl,
    DateTime CreatedAt);

/// <summary>
/// Input DTO for creating an attachment record after a file is uploaded to R2.
/// </summary>
public sealed record CreateAttachmentDto(
    Guid TargetId,
    string TargetType,
    string Purpose);
