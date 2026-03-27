using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Entities;

/// <summary>
/// Immutable audit log for every commission change:
/// - OrderCommission: when an order's payment is confirmed
/// - Refund: when an order is refunded and commission is clawed back
/// - Payout: when admin pays out the member's balance
/// </summary>
public sealed class CommissionTransaction : BaseEntity<CommissionTransactionId>, IHasCreationTime
{
    public MemberId MemberId { get; private set; }
    public AccountOrderId? AccountOrderId { get; private set; }
    public CommissionTransactionType TransactionType { get; private set; }
    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public string? Note { get; private set; }
    public string? EvidenceObjectKey { get; private set; }
    public CommissionTransactionStatus Status { get; private set; }
    public ReplicatedUserId? ProcessedByUserId { get; private set; }
    public DateTime CreatedAt { get; set; }

    public Member? Member { get; private set; }
    public AccountOrder? AccountOrder { get; private set; }
    public ReplicatedUser? ProcessedByUser { get; private set; }

    private CommissionTransaction()
    {
        Id = CommissionTransactionId.New();
    }

    public static CommissionTransaction CreateOrderCommission(
        MemberId memberId,
        AccountOrderId accountOrderId,
        decimal amount,
        decimal balanceAfter,
        ReplicatedUserId? processedByUserId = null)
    {
        return new CommissionTransaction
        {
            MemberId = memberId,
            AccountOrderId = accountOrderId,
            TransactionType = CommissionTransactionType.OrderCommission,
            Amount = Math.Max(0m, decimal.Round(amount, 2, MidpointRounding.AwayFromZero)),
            BalanceAfter = decimal.Round(balanceAfter, 2, MidpointRounding.AwayFromZero),
            Status = CommissionTransactionStatus.Completed,
            ProcessedByUserId = processedByUserId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static CommissionTransaction CreateRefundDeduction(
        MemberId memberId,
        AccountOrderId accountOrderId,
        decimal amount,
        decimal balanceAfter,
        string? note = null)
    {
        return new CommissionTransaction
        {
            MemberId = memberId,
            AccountOrderId = accountOrderId,
            TransactionType = CommissionTransactionType.Refund,
            Amount = -Math.Abs(decimal.Round(amount, 2, MidpointRounding.AwayFromZero)),
            BalanceAfter = decimal.Round(balanceAfter, 2, MidpointRounding.AwayFromZero),
            Note = note,
            Status = CommissionTransactionStatus.Completed,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static CommissionTransaction CreatePayout(
        MemberId memberId,
        decimal amount,
        decimal balanceAfter,
        string? evidenceObjectKey,
        string? note,
        ReplicatedUserId? processedByUserId)
    {
        return new CommissionTransaction
        {
            MemberId = memberId,
            TransactionType = CommissionTransactionType.Payout,
            Amount = -Math.Abs(decimal.Round(amount, 2, MidpointRounding.AwayFromZero)),
            BalanceAfter = decimal.Round(balanceAfter, 2, MidpointRounding.AwayFromZero),
            EvidenceObjectKey = evidenceObjectKey,
            Note = note,
            Status = CommissionTransactionStatus.Completed,
            ProcessedByUserId = processedByUserId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
