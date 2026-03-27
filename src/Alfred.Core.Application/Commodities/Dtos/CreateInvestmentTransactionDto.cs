using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.Commodities.Dtos;

public sealed record CreateInvestmentTransactionDto(
    InvestmentTransactionType TransactionType,
    DateTimeOffset TransactionDate,
    decimal Quantity,
    UnitId UnitId,
    decimal PricePerUnit,
    decimal TotalAmount,
    decimal FeeAmount,
    Guid? FinanceTxnId,
    string? Notes
);
