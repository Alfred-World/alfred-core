using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Entities;

public sealed class AssetLog : BaseEntity, IHasCreationTime
{
    public Guid AssetId { get; private set; }
    public AssetLogEventType EventType { get; private set; }
    public Guid? BrandId { get; private set; }
    public DateTimeOffset PerformedAt { get; private set; }
    public decimal Cost { get; private set; }
    public string? Note { get; private set; }
    public Guid? FinanceTxnId { get; private set; }
    public DateTime? NextDueDate { get; private set; }

    public DateTime CreatedAt { get; set; }

    // Navigation
    public Asset? Asset { get; private set; }
    public Brand? Brand { get; private set; }

    private AssetLog()
    {
    }

    public static AssetLog Create(Guid assetId, AssetLogEventType eventType, Guid? brandId, DateTimeOffset performedAt,
        decimal cost, string? note, Guid? financeTxnId, DateTime? nextDueDate)
    {
        return new AssetLog
        {
            AssetId = assetId,
            EventType = eventType,
            BrandId = brandId,
            PerformedAt = performedAt.ToUniversalTime(),
            Cost = cost,
            Note = note,
            FinanceTxnId = financeTxnId,
            NextDueDate = nextDueDate,
            CreatedAt = DateTime.UtcNow
        };
    }
}
