using Alfred.Core.Application.Brands.Dtos;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Brands.Shared;

public static class BrandMappingExtensions
{
    public static BrandDto ToDto(this Brand brand)
    {
        return new BrandDto(
            brand.Id.Value,
            brand.Name,
            brand.Website,
            brand.SupportPhone,
            brand.Description,
            brand.LogoUrl,
            brand.BrandCategories
                .Where(bc => bc.Category is not null)
                .Select(bc =>
                    new BrandCategoryDto(bc.Category!.Id.Value, bc.Category.Name, bc.Category.Code, bc.Category.Icon))
                .ToList(),
            brand.CreatedAt,
            brand.UpdatedAt
        );
    }
}
