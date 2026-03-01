using Alfred.Core.Application.Querying.Fields;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Commodities.Shared;

/// <summary>
/// Field map for Commodity entity — defines fields available for filtering and sorting via DSL.
/// </summary>
public sealed class CommodityFieldMap : BaseFieldMap<Commodity>
{
    private static readonly Lazy<CommodityFieldMap> _instance = new(() => new CommodityFieldMap());

    private CommodityFieldMap()
    {
    }

    public static CommodityFieldMap Instance => _instance.Value;

    public override FieldMap<Commodity> Fields { get; } = new FieldMap<Commodity>()
        .Add("id", c => c.Id).AllowAll()
        .Add("code", c => c.Code).AllowAll()
        .Add("name", c => c.Name).AllowAll()
        .Add("assetClass", c => c.AssetClass).AllowAll()
        .Add("defaultUnitId", c => c.DefaultUnitId!).AllowAll()
        .Add("description", c => c.Description!).AllowAll()
        .Add("createdAt", c => c.CreatedAt).Sortable().Selectable();
}
