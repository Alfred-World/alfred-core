namespace Alfred.Core.Application.Commodities.Dtos;

public sealed record CreateInvestmentTransactionDto(
    string TransactionType,
    DateTimeOffset TransactionDate,
    decimal Quantity,
    Guid UnitId,
    decimal PricePerUnit,
    decimal TotalAmount,
    decimal FeeAmount,
    Guid? FinanceTxnId,
    string? Notes
);
