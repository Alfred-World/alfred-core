namespace Alfred.Core.Domain.Entities;

/// <summary>
/// Polymorphic attachment — stores file metadata for any entity type.
/// The actual file lives in R2; only the ObjectKey is stored here.
/// Download URLs are always generated as presigned (time-limited) to prevent leaks.
/// </summary>
public sealed class Attachment : BaseEntity<AttachmentId>, IHasCreationTime
{
    /// <summary>The ID of the owning entity (Asset, AssetLog, etc.)</summary>
    public Guid TargetId { get; private set; }

    /// <summary>Discriminator: "Asset", "AssetLog", "Commodity", etc.</summary>
    public string TargetType { get; private set; } = null!;

    /// <summary>R2 object key (e.g. "assets/images/2026/03/abc123_photo.png")</summary>
    public string ObjectKey { get; private set; } = null!;

    /// <summary>Original file name shown to the user</summary>
    public string FileName { get; private set; } = null!;

    /// <summary>MIME content type (e.g. "image/png", "application/pdf")</summary>
    public string ContentType { get; private set; } = null!;

    /// <summary>File size in bytes</summary>
    public long FileSize { get; private set; }

    /// <summary>Purpose tag: "PrimaryImage", "Document", "Invoice", etc.</summary>
    public string Purpose { get; private set; } = "Attachment";

    public DateTime CreatedAt { get; set; }

    private Attachment()
    {
        Id = AttachmentId.New();
    }

    public static Attachment Create(
        Guid targetId,
        string targetType,
        string objectKey,
        string fileName,
        string contentType,
        long fileSize,
        string purpose = "Attachment")
    {
        return new Attachment
        {
            TargetId = targetId,
            TargetType = targetType,
            ObjectKey = objectKey,
            FileName = fileName,
            ContentType = contentType,
            FileSize = fileSize,
            Purpose = purpose,
            CreatedAt = DateTime.UtcNow
        };
    }
}
