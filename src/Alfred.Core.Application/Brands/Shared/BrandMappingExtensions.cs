using Alfred.Core.Application.Brands.Dtos;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Brands.Shared;

public static class BrandMappingExtensions
{
    public static BrandDto ToDto(this Brand brand)
    {
        return new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            Website = brand.Website,
            SupportPhone = brand.SupportPhone,
            Description = brand.Description,
            LogoUrl = brand.LogoUrl,
            Categories = brand.BrandCategories
                .Where(bc => bc.Category is not null)
                .Select(bc =>
                    new BrandCategoryDto(bc.Category!.Id, bc.Category.Name, bc.Category.Code, bc.Category.Icon)),
            CreatedAt = brand.CreatedAt,
            UpdatedAt = brand.UpdatedAt
        };
    }
}
