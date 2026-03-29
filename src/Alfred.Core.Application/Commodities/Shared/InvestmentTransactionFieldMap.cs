using Alfred.Core.Application.Commodities.Dtos;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Commodities.Shared;

/// <summary>
/// Field map for InvestmentTransaction entity — defines fields available for filtering and sorting via DSL.
/// </summary>
public sealed class InvestmentTransactionFieldMap : BaseFieldMap<InvestmentTransaction>
{
    private static readonly Lazy<InvestmentTransactionFieldMap> _instance = new(() =>
        new InvestmentTransactionFieldMap());

    private InvestmentTransactionFieldMap()
    {
    }

    public static InvestmentTransactionFieldMap Instance => _instance.Value;

    public override FieldMap<InvestmentTransaction> Fields { get; } = new FieldMap<InvestmentTransaction>()
        .Add("id", t => t.Id).AllowAll()
        .Add("commodityId", t => t.CommodityId).AllowAll()
        .Add("transactionType", t => t.TransactionType).Filterable().Sortable()
        .Add("transactionTypeText", t => t.TransactionType.ToString()).Selectable()
        .Add("transactionDate", t => t.TransactionDate).AllowAll()
        .Add("quantity", t => t.Quantity).AllowAll()
        .Add("unitId", t => t.UnitId).AllowAll()
        .Add("pricePerUnit", t => t.PricePerUnit).AllowAll()
        .Add("totalAmount", t => t.TotalAmount).AllowAll()
        .Add("feeAmount", t => t.FeeAmount).AllowAll()
        .Add("financeTxnId", t => t.FinanceTxnId!).AllowAll()
        .Add("notes", t => t.Notes!).AllowAll()
        .Add("commodityName", t => t.Commodity!.Name).Selectable()
        .Add("unitName", t => t.Unit!.Name).Selectable()
        .Add("unitCode", t => t.Unit!.Code).Selectable()
        .Add("createdAt", t => t.CreatedAt).Sortable().Selectable();

    public static ViewRegistry<InvestmentTransaction, InvestmentTransactionDto> Views { get; } =
        new ViewRegistry<InvestmentTransaction, InvestmentTransactionDto>()
            .Register("list", cfg => cfg
                .Select(x => x.Id)
                .Select(x => x.CommodityId)
                .Select(x => x.CommodityName)
                .SelectAs(x => x.TransactionType, "transactionTypeText")
                .Select(x => x.TransactionDate)
                .Select(x => x.Quantity)
                .Select(x => x.UnitId)
                .Select(x => x.UnitName)
                .Select(x => x.UnitCode)
                .Select(x => x.PricePerUnit)
                .Select(x => x.TotalAmount)
                .Select(x => x.FeeAmount)
                .Select(x => x.FinanceTxnId)
                .Select(x => x.Notes)
                .Select(x => x.CreatedAt))
            .SetDefault("list");
}
