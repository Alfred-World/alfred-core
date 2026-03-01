using Alfred.Core.Application.Querying.Fields;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Commodities.Shared;

/// <summary>
/// Field map for InvestmentTransaction entity — defines fields available for filtering and sorting via DSL.
/// </summary>
public sealed class InvestmentTransactionFieldMap : BaseFieldMap<InvestmentTransaction>
{
    private static readonly Lazy<InvestmentTransactionFieldMap> _instance = new(() => new InvestmentTransactionFieldMap());

    private InvestmentTransactionFieldMap() { }

    public static InvestmentTransactionFieldMap Instance => _instance.Value;

    public override FieldMap<InvestmentTransaction> Fields { get; } = new FieldMap<InvestmentTransaction>()
        .Add("id", t => t.Id).AllowAll()
        .Add("commodityId", t => t.CommodityId).AllowAll()
        .Add("transactionType", t => t.TransactionType).AllowAll()
        .Add("transactionDate", t => t.TransactionDate).AllowAll()
        .Add("quantity", t => t.Quantity).AllowAll()
        .Add("pricePerUnit", t => t.PricePerUnit).AllowAll()
        .Add("totalAmount", t => t.TotalAmount).AllowAll()
        .Add("feeAmount", t => t.FeeAmount).AllowAll()
        .Add("createdAt", t => t.CreatedAt).Sortable().Selectable();
}
