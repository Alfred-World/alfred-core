using Alfred.Core.Application.Attachments.Dtos;

namespace Alfred.Core.Application.Attachments;

/// <summary>
/// Service for managing polymorphic attachments.
/// Upload streams file to R2 and creates a DB record.
/// All returned URLs are presigned (time-limited).
/// </summary>
public interface IAttachmentService
{
    /// <summary>
    /// Upload a file to R2 and persist an Attachment record linking it to a target entity.
    /// </summary>
    Task<AttachmentDto> UploadAttachmentAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize,
        CreateAttachmentDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// List all attachments for a target entity. Download URLs are presigned.
    /// </summary>
    Task<List<AttachmentDto>> GetAttachmentsByTargetAsync(
        Guid targetId,
        string targetType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an attachment record and the underlying R2 object.
    /// </summary>
    Task DeleteAttachmentAsync(Guid attachmentId, CancellationToken cancellationToken = default);
}
