using Alfred.Core.Application.Brands.Dtos;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Brands.Shared;

public sealed class BrandFieldMap : BaseFieldMap<Brand>
{
    private static readonly Lazy<BrandFieldMap> _instance = new(() => new BrandFieldMap());

    private BrandFieldMap()
    {
    }

    public static BrandFieldMap Instance => _instance.Value;

    public override FieldMap<Brand> Fields { get; } = new FieldMap<Brand>()
        .Add("id", b => b.Id).AllowAll()
        .Add("name", b => b.Name).AllowAll()
        .Add("website", b => b.Website!).AllowAll()
        .Add("supportPhone", b => b.SupportPhone!).AllowAll()
        .Add("description", b => b.Description!).AllowAll()
        .Add("logoUrl", b => b.LogoUrl!).AllowAll()
        .Add("categories", b => b.BrandCategories.Select(bc => new BrandCategoryDto(
            bc.CategoryId.Value,
            bc.Category!.Name,
            bc.Category!.Code,
            bc.Category!.Icon))).Selectable()
        .Add("createdAt", b => b.CreatedAt).Sortable().Selectable()
        .Add("updatedAt", b => b.UpdatedAt!).Sortable().Selectable();

    public static ViewRegistry<Brand, BrandDto> Views { get; } =
        new ViewRegistry<Brand, BrandDto>()
            .Register("list", cfg => cfg
                .Select(x => x.Id)
                .Select(x => x.Name)
                .Select(x => x.Website)
                .Select(x => x.SupportPhone)
                .Select(x => x.Description)
                .Select(x => x.LogoUrl)
                .Select(x => x.Categories)
                .Select(x => x.CreatedAt)
                .Select(x => x.UpdatedAt))
            .SetDefault("list");
}
