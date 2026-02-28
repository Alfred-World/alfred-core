namespace Alfred.Core.Application.Brands.Dtos;

public sealed record BrandDto(
    Guid Id,
    string Name,
    string? Website,
    string? SupportPhone,
    string? Description,
    string? LogoUrl,
    List<BrandCategoryDto> Categories,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record BrandCategoryDto(
    Guid Id,
    string Name,
    string Code,
    string? Icon
);
