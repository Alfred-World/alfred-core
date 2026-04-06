namespace Alfred.Core.Application.Brands.Dtos;

public sealed record UpdateBrandDto
{
    public Optional<string> Name { get; init; }
    public Optional<string?> Website { get; init; }
    public Optional<string?> SupportPhone { get; init; }
    public Optional<string?> Description { get; init; }
    public Optional<string?> LogoUrl { get; init; }
    public Optional<List<CategoryId>?> CategoryIds { get; init; }
}
