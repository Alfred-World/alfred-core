namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record ReferralCommissionSettingHistoryDto(
    Guid Id,
    decimal PreviousCommissionPercent,
    decimal NewCommissionPercent,
    DateTime ChangedAt,
    ReplicatedUserDto? ChangedByUser
);
