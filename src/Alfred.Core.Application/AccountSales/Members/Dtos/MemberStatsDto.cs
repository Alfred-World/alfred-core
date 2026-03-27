namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record MemberStatsDto(
    int TotalOrders,
    decimal TotalSpend,
    int TotalReferrals,
    decimal TotalReferralCommission,
    IReadOnlyList<MemberMonthlyStatsDto> Monthly
);
