using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed class AccountOrderDto
{
    public Guid Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public Guid? SoldByUserId { get; set; }
    public ReplicatedUserDto? SoldByUser { get; set; }
    public Guid MemberId { get; set; }
    public string? MemberDisplayName { get; set; }
    public string? MemberSourceId { get; set; }
    public MemberDto? ReferrerMember { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid ProductVariantId { get; set; }
    public string ProductVariantNameSnapshot { get; set; } = string.Empty;
    public decimal UnitPriceSnapshot { get; set; }
    public int WarrantyDaysSnapshot { get; set; }
    public Guid? ReferrerMemberId { get; set; }
    public decimal ReferralCommissionPercentSnapshot { get; set; }
    public decimal ReferralCommissionAmountSnapshot { get; set; }
    public Guid AccountCloneId { get; set; }
    public Guid? WarrantySourceAccountCloneId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime WarrantyExpiry { get; set; }
    public string? OrderNote { get; set; }
    public string? WarrantyIssueNote { get; set; }
    public AccountOrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public bool IsTrial { get; set; }
    public decimal RefundAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
