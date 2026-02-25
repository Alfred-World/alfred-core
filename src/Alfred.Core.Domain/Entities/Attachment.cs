using Alfred.Core.Domain.Common.Base;

namespace Alfred.Core.Domain.Entities;

public sealed class Attachment : BaseEntity
{
    public Guid TargetId { get; private set; }
    public string TargetType { get; private set; } = null!; // ASSET, ASSET_LOG, INVESTMENT_TXN
    public string FileUrl { get; private set; } = null!;
    public string? FileType { get; private set; }
    public string? Description { get; private set; }
    
    public DateTimeOffset UploadedAt { get; private set; }

    private Attachment() { }

    public static Attachment Create(Guid targetId, string targetType, string fileUrl, string? fileType, string? description)
    {
        return new Attachment
        {
            TargetId = targetId,
            TargetType = targetType,
            FileUrl = fileUrl,
            FileType = fileType,
            Description = description,
            UploadedAt = DateTimeOffset.UtcNow
        };
    }
}
