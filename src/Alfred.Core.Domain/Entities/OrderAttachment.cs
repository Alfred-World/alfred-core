using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Entities;

/// <summary>
/// File attachment for an order — payment evidence, chat logs, refund proof, etc.
/// Actual files stored in R2; only the object key is persisted here.
/// </summary>
public sealed class OrderAttachment : BaseEntity<OrderAttachmentId>, IHasCreationTime
{
    public AccountOrderId AccountOrderId { get; private set; }
    public OrderAttachmentType FileType { get; private set; }
    public string ObjectKey { get; private set; } = null!;
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long FileSize { get; private set; }
    public DateTime CreatedAt { get; set; }

    public AccountOrder? AccountOrder { get; private set; }

    private OrderAttachment()
    {
        Id = OrderAttachmentId.New();
    }

    public static OrderAttachment Create(
        AccountOrderId accountOrderId,
        OrderAttachmentType fileType,
        string objectKey,
        string fileName,
        string contentType,
        long fileSize)
    {
        return new OrderAttachment
        {
            AccountOrderId = accountOrderId,
            FileType = fileType,
            ObjectKey = objectKey,
            FileName = fileName,
            ContentType = contentType,
            FileSize = fileSize,
            CreatedAt = DateTime.UtcNow
        };
    }
}
