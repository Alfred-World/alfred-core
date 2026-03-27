using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Entities;

/// <summary>
/// Immutable audit record for every bonus tier achievement.
/// Created when a seller's monthly order count crosses a tier threshold.
/// Snapshot fields freeze the tier values at trigger time.
/// </summary>
public sealed class SalesBonusTransaction : BaseEntity<SalesBonusTransactionId>, IHasCreationTime
{
    public MemberId SoldByMemberId { get; private set; }
    public SalesBonusTierId SalesBonusTierId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public int OrderCountAtTrigger { get; private set; }
    public int OrderThresholdSnapshot { get; private set; }
    public decimal BonusAmountSnapshot { get; private set; }
    public SalesBonusTransactionStatus Status { get; private set; }
    public ReplicatedUserId? ProcessedByUserId { get; private set; }
    public string? Note { get; private set; }
    public DateTime CreatedAt { get; set; }

    public Member? SoldByMember { get; private set; }
    public SalesBonusTier? SalesBonusTier { get; private set; }
    public ReplicatedUser? ProcessedByUser { get; private set; }

    private SalesBonusTransaction()
    {
        Id = SalesBonusTransactionId.New();
    }

    public static SalesBonusTransaction Create(
        MemberId soldByMemberId,
        SalesBonusTierId tierId,
        int year,
        int month,
        int orderCountAtTrigger,
        int orderThresholdSnapshot,
        decimal bonusAmountSnapshot)
    {
        return new SalesBonusTransaction
        {
            SoldByMemberId = soldByMemberId,
            SalesBonusTierId = tierId,
            Year = year,
            Month = month,
            OrderCountAtTrigger = orderCountAtTrigger,
            OrderThresholdSnapshot = orderThresholdSnapshot,
            BonusAmountSnapshot = Math.Max(0m, decimal.Round(bonusAmountSnapshot, 2, MidpointRounding.AwayFromZero)),
            Status = SalesBonusTransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkPaid(ReplicatedUserId? processedByUserId, string? note = null)
    {
        Status = SalesBonusTransactionStatus.Paid;
        ProcessedByUserId = processedByUserId;
        Note = note;
    }

    public void Cancel(string? note = null)
    {
        Status = SalesBonusTransactionStatus.Cancelled;
        Note = note;
    }

    /// <summary>
    /// Update the frozen snapshot values for a Pending transaction when the admin changes the tier config.
    /// Only Pending transactions are eligible; Paid/Cancelled ones remain immutable.
    /// </summary>
    public void UpdatePendingSnapshot(int orderThreshold, decimal bonusAmount)
    {
        if (Status != SalesBonusTransactionStatus.Pending)
        {
            throw new InvalidOperationException("Only pending transactions can have their snapshot updated.");
        }

        OrderThresholdSnapshot = Math.Max(1, orderThreshold);
        BonusAmountSnapshot = Math.Max(0m, decimal.Round(bonusAmount, 2, MidpointRounding.AwayFromZero));
    }
}
