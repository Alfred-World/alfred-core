namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record MemberMonthlyStatsDto(
    int Year,
    int Month,
    int OrderCount,
    decimal TotalSpend,
    int ReferralCount,
    decimal TotalReferralCommission
);
