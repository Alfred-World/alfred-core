using Alfred.Core.Application.Querying.Fields;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Brands.Shared;

public sealed class BrandFieldMap : BaseFieldMap<Brand>
{
    private static readonly Lazy<BrandFieldMap> _instance = new(() => new BrandFieldMap());

    private BrandFieldMap() { }

    public static BrandFieldMap Instance => _instance.Value;

    public override FieldMap<Brand> Fields { get; } = new FieldMap<Brand>()
        .Add("id", b => b.Id).AllowAll()
        .Add("name", b => b.Name).AllowAll()
        .Add("website", b => b.Website!).AllowAll()
        .Add("supportPhone", b => b.SupportPhone!).AllowAll()
        .Add("description", b => b.Description!).AllowAll()
        .Add("createdAt", b => b.CreatedAt).Sortable().Selectable()
        .Add("updatedAt", b => b.UpdatedAt!).Sortable().Selectable();
}
