namespace Alfred.Core.Domain.Entities;

/// <summary>
/// Monthly sales tracking per seller (staff user).
/// One record per seller per (Year, Month). Serves as history after the month ends.
/// OrderCount is incremented each time the seller confirms an order payment.
/// HighestTierReachedId tracks the highest SalesBonusTier achieved this month.
/// </summary>
public sealed class MemberMonthlySalesSummary : BaseEntity<MemberMonthlySalesSummaryId>, IHasCreationTime,
    IHasModificationTime
{
    public MemberId SoldByMemberId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public int OrderCount { get; private set; }
    public SalesBonusTierId? HighestTierReachedId { get; private set; }
    public decimal TotalBonusEarned { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Member? SoldByMember { get; private set; }
    public SalesBonusTier? HighestTierReached { get; private set; }

    private MemberMonthlySalesSummary()
    {
        Id = MemberMonthlySalesSummaryId.New();
    }

    public static MemberMonthlySalesSummary Create(MemberId soldByMemberId, int year, int month)
    {
        return new MemberMonthlySalesSummary
        {
            SoldByMemberId = soldByMemberId,
            Year = year,
            Month = month,
            OrderCount = 0,
            TotalBonusEarned = 0m,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Increment order count by 1. Returns the new count for milestone evaluation.
    /// </summary>
    public int IncrementOrderCount()
    {
        OrderCount++;
        UpdatedAt = DateTime.UtcNow;
        return OrderCount;
    }

    /// <summary>
    /// Record that a new bonus tier was reached.
    /// </summary>
    public void RecordTierReached(SalesBonusTierId tierId, decimal bonusAmount)
    {
        HighestTierReachedId = tierId;
        TotalBonusEarned += Math.Max(0m, decimal.Round(bonusAmount, 2, MidpointRounding.AwayFromZero));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deduct a bonus amount from TotalBonusEarned when a transaction is cancelled.
    /// </summary>
    public void DeductBonusEarned(decimal bonusAmount)
    {
        TotalBonusEarned = Math.Max(0m,
            TotalBonusEarned - Math.Max(0m, decimal.Round(bonusAmount, 2, MidpointRounding.AwayFromZero)));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Apply a positive or negative delta to TotalBonusEarned when the admin updates a tier's BonusAmount
    /// and there are Pending transactions that need their snapshot values resynced.
    /// </summary>
    public void AdjustTotalBonusEarned(decimal delta)
    {
        TotalBonusEarned = Math.Max(0m, decimal.Round(TotalBonusEarned + delta, 2, MidpointRounding.AwayFromZero));
        UpdatedAt = DateTime.UtcNow;
    }
}
