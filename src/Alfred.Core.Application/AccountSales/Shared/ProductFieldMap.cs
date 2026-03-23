using Alfred.Core.Application.Querying.Fields;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.AccountSales.Shared;

public sealed class ProductFieldMap : BaseFieldMap<Product>
{
    private static readonly Lazy<ProductFieldMap> _instance = new(() => new ProductFieldMap());

    private ProductFieldMap()
    {
    }

    public static ProductFieldMap Instance => _instance.Value;

    public override FieldMap<Product> Fields { get; } = new FieldMap<Product>()
        .Add("id", x => x.Id).AllowAll()
        .Add("name", x => x.Name).AllowAll()
        .Add("productType", x => x.ProductType).AllowAll()
        .Add("createdAt", x => x.CreatedAt).Sortable().Selectable()
        .Add("updatedAt", x => x.UpdatedAt!).Sortable().Selectable();
}
