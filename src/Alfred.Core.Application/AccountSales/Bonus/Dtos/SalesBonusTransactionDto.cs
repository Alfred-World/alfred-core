using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record SalesBonusTransactionDto(
    Guid Id,
    Guid SoldByMemberId,
    string? SoldByMemberName,
    Guid SalesBonusTierId,
    int Year,
    int Month,
    int OrderCountAtTrigger,
    int OrderThresholdSnapshot,
    decimal BonusAmountSnapshot,
    SalesBonusTransactionStatus Status,
    Guid? ProcessedByUserId,
    string? Note,
    DateTime CreatedAt
);
