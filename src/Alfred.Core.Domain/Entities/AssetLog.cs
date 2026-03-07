using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Entities;

public sealed class AssetLog : BaseEntity<AssetLogId>, IHasCreationTime
{
    public AssetId AssetId { get; private set; }
    public AssetLogEventType EventType { get; private set; }
    public BrandId? BrandId { get; private set; }
    public DateTimeOffset PerformedAt { get; private set; }
    public decimal Cost { get; private set; }
    public decimal Quantity { get; private set; } = 1m;
    public string? Note { get; private set; }
    public Guid? FinanceTxnId { get; private set; }
    public DateTime? NextDueDate { get; private set; }

    public DateTime CreatedAt { get; set; }

    // Navigation
    public Asset? Asset { get; private set; }
    public Brand? Brand { get; private set; }

    private AssetLog()
    {
        Id = AssetLogId.New();
    }

    public static AssetLog Create(AssetId assetId, AssetLogEventType eventType, BrandId? brandId,
        DateTimeOffset performedAt,
        decimal cost, decimal quantity, string? note, Guid? financeTxnId, DateTime? nextDueDate)
    {
        return new AssetLog
        {
            AssetId = assetId,
            EventType = eventType,
            BrandId = brandId,
            PerformedAt = performedAt.ToUniversalTime(),
            Cost = cost,
            Quantity = quantity > 0 ? quantity : 1m,
            Note = note,
            FinanceTxnId = financeTxnId,
            NextDueDate = nextDueDate,
            CreatedAt = DateTime.UtcNow
        };
    }
}
