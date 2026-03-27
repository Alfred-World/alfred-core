using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record CommissionTransactionDto(
    Guid Id,
    Guid MemberId,
    string? MemberDisplayName,
    Guid? AccountOrderId,
    string? OrderCode,
    CommissionTransactionType TransactionType,
    decimal Amount,
    decimal BalanceAfter,
    string? Note,
    string? EvidenceObjectKey,
    CommissionTransactionStatus Status,
    Guid? ProcessedByUserId,
    DateTime CreatedAt
);
