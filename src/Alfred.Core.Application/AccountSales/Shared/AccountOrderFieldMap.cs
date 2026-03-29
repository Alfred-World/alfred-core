using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.AccountSales.Shared;

public sealed class AccountOrderFieldMap : BaseFieldMap<AccountOrder>
{
    private static readonly Lazy<AccountOrderFieldMap> _instance = new(() => new AccountOrderFieldMap());

    private AccountOrderFieldMap()
    {
    }

    public static AccountOrderFieldMap Instance => _instance.Value;

    public override FieldMap<AccountOrder> Fields { get; } = new FieldMap<AccountOrder>()
        // Entity scalar fields
        .Add("id", x => x.Id).AllowAll()
        .Add("orderCode", x => x.OrderCode).AllowAll()
        .Add("soldByUserId", x => x.SoldByUserId!).AllowAll()
        .Add("memberId", x => x.MemberId).AllowAll()
        .Add("referrerMemberId", x => x.ReferrerMemberId!).AllowAll()
        .Add("productId", x => x.ProductId).AllowAll()
        .Add("productVariantId", x => x.ProductVariantId).AllowAll()
        .Add("accountCloneId", x => x.AccountCloneId).AllowAll()
        .Add("warrantySourceAccountCloneId", x => x.WarrantySourceAccountCloneId!).AllowAll()
        .Add("productVariantNameSnapshot", x => x.ProductVariantNameSnapshot).AllowAll()
        .Add("unitPriceSnapshot", x => x.UnitPriceSnapshot).AllowAll()
        .Add("warrantyDaysSnapshot", x => x.WarrantyDaysSnapshot).AllowAll()
        .Add("referralCommissionPercentSnapshot", x => x.ReferralCommissionPercentSnapshot).AllowAll()
        .Add("referralCommissionAmountSnapshot", x => x.ReferralCommissionAmountSnapshot).AllowAll()
        .Add("purchaseDate", x => x.PurchaseDate).AllowAll()
        .Add("warrantyExpiry", x => x.WarrantyExpiry).AllowAll()
        .Add("orderNote", x => x.OrderNote!).AllowAll()
        .Add("warrantyIssueNote", x => x.WarrantyIssueNote!).AllowAll()
        .Add("status", x => x.Status).AllowAll()
        .Add("paymentStatus", x => x.PaymentStatus).AllowAll()
        .Add("isTrial", x => x.IsTrial).AllowAll()
        .Add("refundAmount", x => x.RefundAmount).AllowAll()
        .Add("createdAt", x => x.CreatedAt).AllowAll()
        .Add("updatedAt", x => x.UpdatedAt!).AllowAll()
        // Navigation-derived scalar fields (require Member/Product includes)
        .Add("memberDisplayName", x => x.Member!.DisplayName!).Selectable()
        .Add("memberSourceId", x => x.Member!.SourceId!).Selectable()
        .Add("productName", x => x.Product!.Name).Selectable()
        // Referrer member projection (require ReferrerMember include)
        .Add("referrerMember", x => x.ReferrerMemberId == null
            ? (MemberDto?)null
            : new MemberDto(
                x.ReferrerMember!.Id.Value,
                x.ReferrerMember.DisplayName,
                x.ReferrerMember.Source,
                x.ReferrerMember.SourceId,
                x.ReferrerMember.CustomerNote,
                x.ReferrerMember.CreatedAt)).Selectable();

    /// <summary>
    /// Available views for AccountOrder entity.
    /// </summary>
    public static ViewRegistry<AccountOrder, AccountOrderDto> Views { get; } =
        new ViewRegistry<AccountOrder, AccountOrderDto>()
            .Register("list", cfg => cfg
                .Select(x => x.Id)
                .Select(x => x.OrderCode)
                .Select(x => x.SoldByUserId)
                .SelectComputed(x => x.SoldByUser)
                .Select(x => x.MemberId)
                .Select(x => x.MemberDisplayName)
                .Select(x => x.MemberSourceId)
                .Select(x => x.ProductId)
                .Select(x => x.ProductName)
                .Select(x => x.ProductVariantId)
                .Select(x => x.ProductVariantNameSnapshot)
                .Select(x => x.UnitPriceSnapshot)
                .Select(x => x.WarrantyDaysSnapshot)
                .Select(x => x.ReferralCommissionPercentSnapshot)
                .Select(x => x.ReferralCommissionAmountSnapshot)
                .Select(x => x.ReferrerMemberId)
                .SelectAs(x => x.ReferrerMember, "referrerMember")
                .Select(x => x.AccountCloneId)
                .Select(x => x.WarrantySourceAccountCloneId)
                .Select(x => x.PurchaseDate)
                .Select(x => x.WarrantyExpiry)
                .Select(x => x.Status)
                .Select(x => x.PaymentStatus)
                .Select(x => x.IsTrial)
                .Select(x => x.RefundAmount)
                .Select(x => x.CreatedAt)
                .Select(x => x.UpdatedAt)
                .Include(o => o.Member!)
                .Include(o => o.Product!)
                .Include(o => o.ReferrerMember!))
            .Register("detail", cfg => cfg
                .Select(x => x.Id)
                .Select(x => x.OrderCode)
                .Select(x => x.SoldByUserId)
                .SelectComputed(x => x.SoldByUser)
                .Select(x => x.MemberId)
                .Select(x => x.MemberDisplayName)
                .Select(x => x.MemberSourceId)
                .Select(x => x.ProductId)
                .Select(x => x.ProductName)
                .Select(x => x.ProductVariantId)
                .Select(x => x.ProductVariantNameSnapshot)
                .Select(x => x.UnitPriceSnapshot)
                .Select(x => x.WarrantyDaysSnapshot)
                .Select(x => x.ReferralCommissionPercentSnapshot)
                .Select(x => x.ReferralCommissionAmountSnapshot)
                .Select(x => x.ReferrerMemberId)
                .SelectAs(x => x.ReferrerMember, "referrerMember")
                .Select(x => x.AccountCloneId)
                .Select(x => x.WarrantySourceAccountCloneId)
                .Select(x => x.PurchaseDate)
                .Select(x => x.WarrantyExpiry)
                .Select(x => x.OrderNote)
                .Select(x => x.WarrantyIssueNote)
                .Select(x => x.Status)
                .Select(x => x.PaymentStatus)
                .Select(x => x.IsTrial)
                .Select(x => x.RefundAmount)
                .Select(x => x.CreatedAt)
                .Select(x => x.UpdatedAt)
                .Include(o => o.Member!)
                .Include(o => o.Product!)
                .Include(o => o.ReferrerMember!))
            .SetDefault("list");
}
