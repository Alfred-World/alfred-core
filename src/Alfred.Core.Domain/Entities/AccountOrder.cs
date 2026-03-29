using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Entities;

public sealed class AccountOrder : BaseEntity<AccountOrderId>, IHasCreationTime, IHasModificationTime
{
    public string OrderCode { get; private set; } = string.Empty;
    public ReplicatedUserId? SoldByUserId { get; private set; }
    public MemberId MemberId { get; private set; }
    public AccountCloneId AccountCloneId { get; private set; }
    public AccountCloneId? WarrantySourceAccountCloneId { get; private set; }
    public ProductId ProductId { get; private set; }
    public ProductVariantId ProductVariantId { get; private set; }
    public string ProductVariantNameSnapshot { get; private set; } = string.Empty;
    public decimal UnitPriceSnapshot { get; private set; }
    public int WarrantyDaysSnapshot { get; private set; }
    public MemberId? ReferrerMemberId { get; private set; }
    public decimal ReferralCommissionPercentSnapshot { get; private set; }
    public decimal ReferralCommissionAmountSnapshot { get; private set; }
    public DateTime PurchaseDate { get; private set; }
    public DateTime WarrantyExpiry { get; private set; }
    public string? OrderNote { get; private set; }
    public string? WarrantyIssueNote { get; private set; }
    public AccountOrderStatus Status { get; private set; } = AccountOrderStatus.Active;
    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;
    public bool IsTrial { get; private set; }
    public decimal RefundAmount { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Member? Member { get; private set; }
    public Member? ReferrerMember { get; private set; }
    public AccountClone? AccountClone { get; private set; }
    public Product? Product { get; private set; }
    public ProductVariant? ProductVariant { get; private set; }

    private AccountOrder()
    {
        Id = AccountOrderId.New();
    }

    public static AccountOrder Create(string orderCode, MemberId memberId, AccountCloneId accountCloneId,
        ProductId productId,
        ProductVariantId productVariantId,
        string productVariantNameSnapshot,
        decimal unitPriceSnapshot,
        int warrantyDaysSnapshot,
        MemberId? referrerMemberId,
        decimal referralCommissionPercentSnapshot,
        DateTime purchaseDateUtc,
        string? orderNote,
        ReplicatedUserId? soldByUserId = null,
        bool isTrial = false)
    {
        var normalizedPrice = Math.Max(0m, decimal.Round(unitPriceSnapshot, 2, MidpointRounding.AwayFromZero));
        var normalizedWarrantyDays = Math.Max(0, warrantyDaysSnapshot);
        var normalizedReferralPercent = Math.Clamp(
            decimal.Round(referralCommissionPercentSnapshot, 2, MidpointRounding.AwayFromZero),
            0m,
            100m);
        var referralCommissionAmount = referrerMemberId.HasValue
            ? decimal.Round(normalizedPrice * normalizedReferralPercent / 100m, 2, MidpointRounding.AwayFromZero)
            : 0m;

        return new AccountOrder
        {
            OrderCode = orderCode,
            SoldByUserId = soldByUserId,
            MemberId = memberId,
            AccountCloneId = accountCloneId,
            ProductId = productId,
            ProductVariantId = productVariantId,
            ProductVariantNameSnapshot = string.IsNullOrWhiteSpace(productVariantNameSnapshot)
                ? "Default"
                : productVariantNameSnapshot.Trim(),
            UnitPriceSnapshot = normalizedPrice,
            WarrantyDaysSnapshot = normalizedWarrantyDays,
            ReferrerMemberId = referrerMemberId,
            ReferralCommissionPercentSnapshot = referrerMemberId.HasValue ? normalizedReferralPercent : 0m,
            ReferralCommissionAmountSnapshot = referralCommissionAmount,
            PurchaseDate = purchaseDateUtc,
            WarrantyExpiry = purchaseDateUtc.AddDays(normalizedWarrantyDays),
            OrderNote = orderNote,
            Status = AccountOrderStatus.Active,
            PaymentStatus = PaymentStatus.Pending,
            IsTrial = isTrial,
            RefundAmount = 0m,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void ReplaceAccount(AccountCloneId accountCloneId, DateTime replacedAtUtc, string? note,
        AccountCloneId warrantySourceAccountCloneId,
        string? warrantyIssueNote)
    {
        WarrantySourceAccountCloneId = warrantySourceAccountCloneId;
        AccountCloneId = accountCloneId;
        WarrantyExpiry = replacedAtUtc.AddDays(Math.Max(0, WarrantyDaysSnapshot));
        OrderNote = note;
        WarrantyIssueNote = string.IsNullOrWhiteSpace(warrantyIssueNote) ? null : warrantyIssueNote.Trim();
        Status = AccountOrderStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkWarrantyDone()
    {
        Status = AccountOrderStatus.WarrantyDone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkRefunded()
    {
        Status = AccountOrderStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmPayment()
    {
        PaymentStatus = PaymentStatus.Paid;
        IsTrial = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Apply a refund to this order. Returns the commission amount that should be clawed back.
    /// </summary>
    public decimal ApplyRefund(decimal refundAmount)
    {
        var normalizedRefund = Math.Max(0m, decimal.Round(refundAmount, 2, MidpointRounding.AwayFromZero));
        var maxRefundable = UnitPriceSnapshot - RefundAmount;
        normalizedRefund = Math.Min(normalizedRefund, maxRefundable);

        RefundAmount += normalizedRefund;

        if (RefundAmount >= UnitPriceSnapshot)
        {
            PaymentStatus = PaymentStatus.FullyRefunded;
            Status = AccountOrderStatus.Refunded;
        }
        else
        {
            PaymentStatus = PaymentStatus.PartiallyRefunded;
        }

        // Calculate commission to claw back proportionally
        var commissionClawback = ReferralCommissionAmountSnapshot > 0 && UnitPriceSnapshot > 0
            ? decimal.Round(normalizedRefund * ReferralCommissionPercentSnapshot / 100m, 2,
                MidpointRounding.AwayFromZero)
            : 0m;

        UpdatedAt = DateTime.UtcNow;
        return commissionClawback;
    }
}
