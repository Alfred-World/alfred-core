using Alfred.Core.Application.Commodities.Dtos;
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
        .Add("assetClass", c => c.AssetClass).Filterable().Sortable()
        .Add("assetClassText", c => c.AssetClass.ToString()).Selectable()
        .Add("defaultUnitId", c => c.DefaultUnitId!).AllowAll()
        .Add("description", c => c.Description!).AllowAll()
        .Add("defaultUnitName", c => c.DefaultUnit!.Name).Selectable()
        .Add("defaultUnitCode", c => c.DefaultUnit!.Code).Selectable()
        .Add("createdAt", c => c.CreatedAt).Sortable().Selectable();

    public static ViewRegistry<Commodity, CommodityDto> Views { get; } =
        new ViewRegistry<Commodity, CommodityDto>()
            .Register("list", cfg => cfg
                .Select(x => x.Id)
                .Select(x => x.Code)
                .Select(x => x.Name)
                .SelectAs(x => x.AssetClass, "assetClassText")
                .Select(x => x.DefaultUnitId)
                .Select(x => x.DefaultUnitName)
                .Select(x => x.DefaultUnitCode)
                .Select(x => x.Description)
                .Select(x => x.CreatedAt))
            .SetDefault("list");
}
