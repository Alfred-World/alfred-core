namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record ReferralCommissionSettingDto(
    Guid Id,
    decimal CommissionPercent,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
