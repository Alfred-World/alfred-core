using Alfred.Core.Application.Brands.Dtos;

namespace Alfred.Core.WebApi.Contracts.Brands;

public sealed record UpdateBrandRequest
{
    public string Name { get; init; } = null!;
    public string? Website { get; init; }
    public string? SupportPhone { get; init; }
    public string? Description { get; init; }
    public string? LogoUrl { get; init; }
    public List<Guid>? CategoryIds { get; init; }

    public UpdateBrandDto ToDto()
    {
        return new UpdateBrandDto(Name, Website, SupportPhone, Description, LogoUrl, CategoryIds);
    }
}
