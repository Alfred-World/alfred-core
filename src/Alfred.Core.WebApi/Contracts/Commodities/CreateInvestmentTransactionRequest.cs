using Alfred.Core.Application.Commodities.Dtos;

namespace Alfred.Core.WebApi.Contracts.Commodities;

public sealed record CreateInvestmentTransactionRequest
{
    public string TransactionType { get; init; } = null!;
    public DateTimeOffset TransactionDate { get; init; }
    public decimal Quantity { get; init; }
    public Guid UnitId { get; init; }
    public decimal PricePerUnit { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal FeeAmount { get; init; }
    public Guid? FinanceTxnId { get; init; }
    public string? Notes { get; init; }

    public CreateInvestmentTransactionDto ToDto() =>
        new(TransactionType, TransactionDate, Quantity, UnitId, PricePerUnit, TotalAmount, FeeAmount, FinanceTxnId, Notes);
}
