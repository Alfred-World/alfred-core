using Alfred.Core.Application.Querying.Fields;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Assets.Shared;

/// <summary>
/// Field map for Asset entity — defines fields available for filtering and sorting via DSL.
/// </summary>
public sealed class AssetFieldMap : BaseFieldMap<Asset>
{
    private static readonly Lazy<AssetFieldMap> _instance = new(() => new AssetFieldMap());

    private AssetFieldMap() { }

    public static AssetFieldMap Instance => _instance.Value;

    public override FieldMap<Asset> Fields { get; } = new FieldMap<Asset>()
        .Add("id", a => a.Id).AllowAll()
        .Add("name", a => a.Name).AllowAll()
        .Add("categoryId", a => a.CategoryId!).AllowAll()
        .Add("brandId", a => a.BrandId!).AllowAll()
        .Add("status", a => a.Status).AllowAll()
        .Add("location", a => a.Location!).AllowAll()
        .Add("purchaseDate", a => a.PurchaseDate!).AllowAll()
        .Add("initialCost", a => a.InitialCost).AllowAll()
        .Add("warrantyExpiryDate", a => a.WarrantyExpiryDate!).AllowAll()
        .Add("createdAt", a => a.CreatedAt).Sortable().Selectable()
        .Add("updatedAt", a => a.UpdatedAt!).Sortable().Selectable();
}
