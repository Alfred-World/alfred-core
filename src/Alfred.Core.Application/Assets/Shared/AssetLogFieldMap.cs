using System.Linq.Expressions;

using Alfred.Core.Application.Assets.Dtos;
using Alfred.Core.Application.Querying.Fields;
using Alfred.Core.Application.Querying.Projection;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Assets.Shared;

/// <summary>
/// Field map for AssetLog entity — defines fields available for filtering and sorting via DSL.
/// </summary>
public sealed class AssetLogFieldMap : BaseFieldMap<AssetLog>
{
    private static readonly Lazy<AssetLogFieldMap> _instance = new(() => new AssetLogFieldMap());

    private AssetLogFieldMap()
    {
    }

    public static AssetLogFieldMap Instance => _instance.Value;

    public override FieldMap<AssetLog> Fields { get; } = new FieldMap<AssetLog>()
        .Add("id", l => l.Id).AllowAll()
        .Add("assetId", l => l.AssetId).AllowAll()
        .Add("eventType", l => l.EventType).Filterable().Sortable()
        .Add("eventTypeText", l => l.EventType.ToString()).Selectable()
        .Add("brandId", l => l.BrandId!).AllowAll()
        .Add("performedAt", l => l.PerformedAt).AllowAll()
        .Add("cost", l => l.Cost).AllowAll()
        .Add("quantity", l => l.Quantity).AllowAll()
        .Add("note", l => l.Note!).AllowAll()
        .Add("financeTxnId", l => l.FinanceTxnId!).AllowAll()
        .Add("nextDueDate", l => l.NextDueDate!).AllowAll()
        .Add("brandName", l => l.Brand!.Name).Selectable()
        .Add("createdAt", l => l.CreatedAt).Sortable().Selectable();

    public static ViewRegistry<AssetLog, AssetLogDto> Views { get; } =
        new ViewRegistry<AssetLog, AssetLogDto>()
            .Register("list", cfg => cfg
                .Select(x => x.Id)
                .Select(x => x.AssetId)
                .SelectAs(x => x.EventType, "eventTypeText")
                .Select(x => x.BrandId)
                .Select(x => x.BrandName)
                .Select(x => x.PerformedAt)
                .Select(x => x.Cost)
                .Select(x => x.Quantity)
                .Select(x => x.Note)
                .Select(x => x.FinanceTxnId)
                .Select(x => x.NextDueDate)
                .Select(x => x.CreatedAt))
            .SetDefault("list");
}
