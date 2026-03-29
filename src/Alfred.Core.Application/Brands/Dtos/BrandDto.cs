namespace Alfred.Core.Application.Brands.Dtos;

public sealed class BrandDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Website { get; set; }
    public string? SupportPhone { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public IEnumerable<BrandCategoryDto>? Categories { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed record BrandCategoryDto(
    Guid Id,
    string Name,
    string Code,
    string? Icon
);
