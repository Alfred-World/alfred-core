using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;

namespace Alfred.Core.Domain.Entities;

/// <summary>
/// Configurable bonus milestone tier.
/// Admin can create N tiers — e.g. "10 orders → 500k", "30 orders → 1M".
/// Thresholds are evaluated against MemberMonthlySalesSummary.OrderCount.
/// </summary>
public sealed class SalesBonusTier : BaseEntity<SalesBonusTierId>, IHasCreationTime, IHasModificationTime
{
    public int OrderThreshold { get; private set; }
    public decimal BonusAmount { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    private SalesBonusTier()
    {
        Id = SalesBonusTierId.New();
    }

    public static SalesBonusTier Create(int orderThreshold, decimal bonusAmount)
    {
        return new SalesBonusTier
        {
            OrderThreshold = Math.Max(1, orderThreshold),
            BonusAmount = Math.Max(0m, decimal.Round(bonusAmount, 2, MidpointRounding.AwayFromZero)),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(int orderThreshold, decimal bonusAmount, bool isActive)
    {
        OrderThreshold = Math.Max(1, orderThreshold);
        BonusAmount = Math.Max(0m, decimal.Round(bonusAmount, 2, MidpointRounding.AwayFromZero));
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
