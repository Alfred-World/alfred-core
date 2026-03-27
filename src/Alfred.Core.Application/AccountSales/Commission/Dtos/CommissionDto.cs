namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record CommissionDto(
    Guid Id,
    Guid MemberId,
    string? MemberDisplayName,
    decimal AvailableBalance,
    decimal TotalEarned,
    decimal TotalPaidOut,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
