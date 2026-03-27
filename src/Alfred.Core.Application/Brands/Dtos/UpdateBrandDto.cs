namespace Alfred.Core.Application.Brands.Dtos;

public sealed record UpdateBrandDto(
    string Name,
    string? Website,
    string? SupportPhone,
    string? Description,
    string? LogoUrl,
    List<CategoryId>? CategoryIds
);
