namespace Alfred.Core.Application.Brands.Dtos;

public sealed record CreateBrandDto(
    string Name,
    string? Website,
    string? SupportPhone,
    string? Description,
    string? LogoUrl,
    List<Guid>? CategoryIds
);
