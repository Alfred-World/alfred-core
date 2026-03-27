namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record MemberMonthlySalesSummaryDto(
    Guid Id,
    Guid SoldByMemberId,
    string? SoldByMemberName,
    int Year,
    int Month,
    int OrderCount,
    Guid? HighestTierReachedId,
    int? HighestTierOrderThreshold,
    decimal TotalBonusEarned,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
