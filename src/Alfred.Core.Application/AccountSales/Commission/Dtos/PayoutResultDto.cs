namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record PayoutResultDto(
    Guid TransactionId,
    Guid MemberId,
    string? MemberDisplayName,
    decimal AmountPaid,
    decimal BalanceAfter,
    DateTime PaymentDate
);
