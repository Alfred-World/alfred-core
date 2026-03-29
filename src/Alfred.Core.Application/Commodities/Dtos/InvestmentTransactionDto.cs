namespace Alfred.Core.Application.Commodities.Dtos;

public sealed class InvestmentTransactionDto
{
    public Guid Id { get; set; }
    public Guid CommodityId { get; set; }
    public string? CommodityName { get; set; }
    public string? TransactionType { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public decimal Quantity { get; set; }
    public Guid UnitId { get; set; }
    public string? UnitName { get; set; }
    public string? UnitCode { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal FeeAmount { get; set; }
    public Guid? FinanceTxnId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
