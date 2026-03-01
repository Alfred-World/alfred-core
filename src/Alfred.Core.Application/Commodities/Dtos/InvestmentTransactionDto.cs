namespace Alfred.Core.Application.Commodities.Dtos;

public sealed record InvestmentTransactionDto(
    Guid Id,
    Guid CommodityId,
    string? CommodityName,
    string TransactionType,
    DateTimeOffset TransactionDate,
    decimal Quantity,
    Guid UnitId,
    string? UnitName,
    string? UnitCode,
    decimal PricePerUnit,
    decimal TotalAmount,
    decimal FeeAmount,
    Guid? FinanceTxnId,
    string? Notes,
    DateTime CreatedAt
);
