using Alfred.Core.Application.Commodities.Dtos;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.Commodities;

public sealed record CreateInvestmentTransactionRequest
{
    public InvestmentTransactionType TransactionType { get; init; }
    public DateTimeOffset TransactionDate { get; init; }
    public decimal Quantity { get; init; }
    public Guid UnitId { get; init; }
    public decimal PricePerUnit { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal FeeAmount { get; init; }
    public Guid? FinanceTxnId { get; init; }
    public string? Notes { get; init; }

    public CreateInvestmentTransactionDto ToDto()
    {
        return new CreateInvestmentTransactionDto(TransactionType, TransactionDate, Quantity, (UnitId)UnitId,
            PricePerUnit,
            TotalAmount, FeeAmount,
            FinanceTxnId, Notes);
    }
}
