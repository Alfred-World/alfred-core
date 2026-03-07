using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Entities;

public sealed class InvestmentTransaction : BaseEntity<InvestmentTransactionId>, IHasCreationTime
{
    public CommodityId CommodityId { get; private set; }
    public InvestmentTransactionType TransactionType { get; private set; }
    public DateTimeOffset TransactionDate { get; private set; }
    public decimal Quantity { get; private set; }
    public UnitId UnitId { get; private set; }
    public decimal PricePerUnit { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal FeeAmount { get; private set; }
    public Guid? FinanceTxnId { get; private set; }
    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; set; }

    // Navigation
    public Commodity? Commodity { get; private set; }
    public Unit? Unit { get; private set; }

    private InvestmentTransaction()
    {
        Id = InvestmentTransactionId.New();
    }

    public static InvestmentTransaction Create(CommodityId commodityId, InvestmentTransactionType transactionType,
        DateTimeOffset transactionDate, decimal quantity, UnitId unitId, decimal pricePerUnit, decimal totalAmount,
        decimal feeAmount, Guid? financeTxnId, string? notes)
    {
        return new InvestmentTransaction
        {
            CommodityId = commodityId,
            TransactionType = transactionType,
            TransactionDate = transactionDate,
            Quantity = quantity,
            UnitId = unitId,
            PricePerUnit = pricePerUnit,
            TotalAmount = totalAmount,
            FeeAmount = feeAmount,
            FinanceTxnId = financeTxnId,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };
    }
}
