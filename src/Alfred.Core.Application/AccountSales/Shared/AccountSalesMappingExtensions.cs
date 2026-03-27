using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Shared;

public static class AccountSalesMappingExtensions
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto(
            product.Id,
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
            variant.Id,
            variant.Name,
            variant.Price,
            variant.WarrantyDays,
            variant.CreatedAt,
            variant.UpdatedAt);
    }

    public static MemberDto ToDto(this Member member)
    {
        return new MemberDto(
            member.Id,
            member.DisplayName,
            member.Source,
            member.SourceId,
            member.CustomerNote,
            member.CreatedAt);
    }

    public static AccountCloneDto ToDto(this AccountClone accountClone)
    {
        return new AccountCloneDto(
            accountClone.Id,
            new ProductSummaryDto(
                accountClone.ProductId,
                accountClone.Product?.Name ?? string.Empty,
                accountClone.Product?.ProductType ?? AccountProductType.Other),
            accountClone.SourceAccount != null
                ? new SourceAccountSummaryDto(
                    accountClone.SourceAccount.Id,
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
            Id = accountOrder.Id,
            OrderCode = accountOrder.OrderCode,
            SoldByUserId = accountOrder.SoldByUserId,
            SoldByUser = null,
            MemberId = accountOrder.MemberId,
            MemberDisplayName = accountOrder.Member?.DisplayName,
            MemberSourceId = accountOrder.Member?.SourceId,
            ReferrerMemberId = accountOrder.ReferrerMemberId,
            ReferrerMember = accountOrder.ReferrerMember?.ToDto(),
            ProductId = accountOrder.ProductId,
            ProductName = accountOrder.Product?.Name ?? string.Empty,
            ProductVariantId = accountOrder.ProductVariantId,
            ProductVariantNameSnapshot = accountOrder.ProductVariantNameSnapshot,
            UnitPriceSnapshot = accountOrder.UnitPriceSnapshot,
            WarrantyDaysSnapshot = accountOrder.WarrantyDaysSnapshot,
            ReferralCommissionPercentSnapshot = accountOrder.ReferralCommissionPercentSnapshot,
            ReferralCommissionAmountSnapshot = accountOrder.ReferralCommissionAmountSnapshot,
            AccountCloneId = accountOrder.AccountCloneId,
            WarrantySourceAccountCloneId = accountOrder.WarrantySourceAccountCloneId,
            PurchaseDate = accountOrder.PurchaseDate,
            WarrantyExpiry = accountOrder.WarrantyExpiry,
            OrderNote = accountOrder.OrderNote,
            WarrantyIssueNote = accountOrder.WarrantyIssueNote,
            Status = accountOrder.Status,
            PaymentStatus = accountOrder.PaymentStatus,
            IsTrial = accountOrder.IsTrial,
            RefundAmount = accountOrder.RefundAmount,
            CreatedAt = accountOrder.CreatedAt,
            UpdatedAt = accountOrder.UpdatedAt
        };
    }

    public static SourceAccountDto ToDto(this SourceAccount sourceAccount)
    {
        return new SourceAccountDto(
            sourceAccount.Id,
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

    public static SalesBonusTierDto ToDto(this SalesBonusTier tier)
    {
        return new SalesBonusTierDto(
            tier.Id,
            tier.OrderThreshold,
            tier.BonusAmount,
            tier.IsActive,
            tier.CreatedAt,
            tier.UpdatedAt);
    }

    public static MemberMonthlySalesSummaryDto ToDto(this MemberMonthlySalesSummary summary)
    {
        return new MemberMonthlySalesSummaryDto(
            summary.Id,
            summary.SoldByMemberId,
            summary.SoldByMember?.DisplayName,
            summary.Year,
            summary.Month,
            summary.OrderCount,
            summary.HighestTierReachedId,
            summary.HighestTierReached?.OrderThreshold,
            summary.TotalBonusEarned,
            summary.CreatedAt,
            summary.UpdatedAt);
    }

    public static SalesBonusTransactionDto ToDto(this SalesBonusTransaction transaction)
    {
        return new SalesBonusTransactionDto(
            transaction.Id,
            transaction.SoldByMemberId,
            transaction.SoldByMember?.DisplayName,
            transaction.SalesBonusTierId,
            transaction.Year,
            transaction.Month,
            transaction.OrderCountAtTrigger,
            transaction.OrderThresholdSnapshot,
            transaction.BonusAmountSnapshot,
            transaction.Status,
            transaction.ProcessedByUserId,
            transaction.Note,
            transaction.CreatedAt);
    }
}
