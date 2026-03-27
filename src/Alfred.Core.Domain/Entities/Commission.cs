using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;

namespace Alfred.Core.Domain.Entities;

/// <summary>
/// Running commission balance for a Member (Sale).
/// When an order completes, commission is accrued here.
/// When admin pays out, the balance is reset to 0.
/// </summary>
public sealed class Commission : BaseEntity<CommissionId>, IHasCreationTime, IHasModificationTime
{
    public MemberId MemberId { get; private set; }
    public decimal AvailableBalance { get; private set; }
    public decimal TotalEarned { get; private set; }
    public decimal TotalPaidOut { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Member? Member { get; private set; }

    private Commission()
    {
        Id = CommissionId.New();
    }

    public static Commission Create(MemberId memberId)
    {
        return new Commission
        {
            MemberId = memberId,
            AvailableBalance = 0m,
            TotalEarned = 0m,
            TotalPaidOut = 0m,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Accrue commission from a completed order.
    /// </summary>
    public void AccrueCommission(decimal amount)
    {
        var normalized = Math.Max(0m, decimal.Round(amount, 2, MidpointRounding.AwayFromZero));
        AvailableBalance += normalized;
        TotalEarned += normalized;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deduct commission (e.g. due to refund). Cannot go below 0.
    /// Returns actual amount deducted.
    /// </summary>
    public decimal DeductCommission(decimal amount)
    {
        var normalized = Math.Max(0m, decimal.Round(amount, 2, MidpointRounding.AwayFromZero));
        var actualDeduction = Math.Min(normalized, AvailableBalance);
        AvailableBalance -= actualDeduction;
        TotalEarned -= actualDeduction;
        UpdatedAt = DateTime.UtcNow;
        return actualDeduction;
    }

    /// <summary>
    /// Pay out all available balance. Returns the amount paid out.
    /// </summary>
    public decimal Payout()
    {
        var paidAmount = AvailableBalance;
        TotalPaidOut += paidAmount;
        AvailableBalance = 0m;
        UpdatedAt = DateTime.UtcNow;
        return paidAmount;
    }
}
