using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Shared;

public static class AccountSalesMappingExtensions
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto(
            product.Id.Value,
            product.Name,
            product.ProductType,
            product.Variants
                .OrderBy(x => x.CreatedAt)
                .Select(x => x.ToDto())
                .ToList(),
            product.Description,
            product.CreatedAt,
            product.UpdatedAt);
    }

    public static ProductVariantDto ToDto(this ProductVariant variant)
    {
        return new ProductVariantDto(
            variant.Id.Value,
            variant.Name,
            variant.Price,
            variant.WarrantyDays,
            variant.CreatedAt,
            variant.UpdatedAt);
    }

    public static MemberDto ToDto(this Member member)
    {
        return new MemberDto(
            member.Id.Value,
            member.DisplayName,
            member.Source,
            member.SourceId,
            member.CustomerNote,
            member.CreatedAt);
    }

    public static AccountCloneDto ToDto(this AccountClone accountClone)
    {
        return new AccountCloneDto(
            accountClone.Id.Value,
            new ProductSummaryDto(
                accountClone.ProductId.Value,
                accountClone.Product?.Name ?? string.Empty,
                accountClone.Product?.ProductType ?? AccountProductType.Other),
            accountClone.SourceAccount != null
                ? new SourceAccountSummaryDto(
                    accountClone.SourceAccount.Id.Value,
                    accountClone.SourceAccount.AccountType,
                    accountClone.SourceAccount.Username)
                : null,
            accountClone.ExternalAccountId,
            accountClone.Username,
            accountClone.Password,
            accountClone.TwoFaSecret,
            accountClone.ExtraInfo,
            accountClone.Status,
            accountClone.CreatedAt,
            accountClone.VerifiedAt,
            accountClone.SoldAt);
    }

    public static AccountOrderDto ToDto(this AccountOrder accountOrder)
    {
        return new AccountOrderDto
        {
            Id = accountOrder.Id.Value,
            OrderCode = accountOrder.OrderCode,
            SoldByUserId = accountOrder.SoldByUserId?.Value,
            SoldByUser = null,
            MemberId = accountOrder.MemberId.Value,
            MemberDisplayName = accountOrder.Member?.DisplayName,
            MemberSourceId = accountOrder.Member?.SourceId,
            ReferrerMemberId = accountOrder.ReferrerMemberId?.Value,
            ReferrerMember = accountOrder.ReferrerMember?.ToDto(),
            ProductId = accountOrder.ProductId.Value,
            ProductName = accountOrder.Product?.Name ?? string.Empty,
            ProductVariantId = accountOrder.ProductVariantId.Value,
            ProductVariantNameSnapshot = accountOrder.ProductVariantNameSnapshot,
            UnitPriceSnapshot = accountOrder.UnitPriceSnapshot,
            WarrantyDaysSnapshot = accountOrder.WarrantyDaysSnapshot,
            ReferralCommissionPercentSnapshot = accountOrder.ReferralCommissionPercentSnapshot,
            ReferralCommissionAmountSnapshot = accountOrder.ReferralCommissionAmountSnapshot,
            AccountCloneId = accountOrder.AccountCloneId.Value,
            WarrantySourceAccountCloneId = accountOrder.WarrantySourceAccountCloneId?.Value,
            PurchaseDate = accountOrder.PurchaseDate,
            WarrantyExpiry = accountOrder.WarrantyExpiry,
            OrderNote = accountOrder.OrderNote,
            WarrantyIssueNote = accountOrder.WarrantyIssueNote,
            Status = accountOrder.Status,
            CreatedAt = accountOrder.CreatedAt,
            UpdatedAt = accountOrder.UpdatedAt
        };
    }

    public static SourceAccountDto ToDto(this SourceAccount sourceAccount)
    {
        return new SourceAccountDto(
            sourceAccount.Id.Value,
            sourceAccount.AccountType,
            sourceAccount.Username,
            sourceAccount.Password,
            sourceAccount.TwoFaSecret,
            sourceAccount.RecoveryEmail,
            sourceAccount.RecoveryPhone,
            sourceAccount.Notes,
            sourceAccount.IsActive,
            sourceAccount.Clones.Count,
            sourceAccount.CreatedAt,
            sourceAccount.UpdatedAt);
    }
}
