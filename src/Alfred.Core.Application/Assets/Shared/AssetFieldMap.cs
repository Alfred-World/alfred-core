using Alfred.Core.Application.Assets.Dtos;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Assets.Shared;

/// <summary>
/// Field map for Asset entity — defines fields available for filtering and sorting via DSL.
/// </summary>
public sealed class AssetFieldMap : BaseFieldMap<Asset>
{
    private static readonly Lazy<AssetFieldMap> _instance = new(() => new AssetFieldMap());

    private AssetFieldMap()
    {
    }

    public static AssetFieldMap Instance => _instance.Value;

    public override FieldMap<Asset> Fields { get; } = new FieldMap<Asset>()
        .Add("id", a => a.Id).AllowAll()
        .Add("name", a => a.Name).AllowAll()
        .Add("categoryId", a => a.CategoryId!).AllowAll()
        .Add("brandId", a => a.BrandId!).AllowAll()
        .Add("status", a => a.Status).Filterable().Sortable()
        .Add("statusText", a => a.Status.ToString()).Selectable()
        .Add("location", a => a.Location!).AllowAll()
        .Add("purchaseDate", a => a.PurchaseDate!).AllowAll()
        .Add("initialCost", a => a.InitialCost).AllowAll()
        .Add("warrantyExpiryDate", a => a.WarrantyExpiryDate!).AllowAll()
        .Add("specs", a => a.Specs).AllowAll()
        .Add("categoryName", a => a.Category!.Name).Selectable()
        .Add("brandName", a => a.Brand!.Name).Selectable()
        .Add("createdAt", a => a.CreatedAt).Sortable().Selectable()
        .Add("updatedAt", a => a.UpdatedAt!).Sortable().Selectable();

    public static ViewRegistry<Asset, AssetDto> Views { get; } =
        new ViewRegistry<Asset, AssetDto>()
            .Register("list", cfg => cfg
                .Select(x => x.Id)
                .Select(x => x.Name)
                .Select(x => x.CategoryId)
                .Select(x => x.CategoryName)
                .Select(x => x.BrandId)
                .Select(x => x.BrandName)
                .Select(x => x.PurchaseDate)
                .Select(x => x.InitialCost)
                .Select(x => x.WarrantyExpiryDate)
                .Select(x => x.Specs)
                .SelectAs(x => x.Status, "statusText")
                .Select(x => x.Location)
                .Select(x => x.CreatedAt)
                .Select(x => x.UpdatedAt))
            .SetDefault("list");
}
