using Alfred.Core.Application.Brands.Dtos;
using Alfred.Core.Application.Common;

namespace Alfred.Core.WebApi.Contracts.Brands;

public sealed record UpdateBrandRequest
{
    public Optional<string> Name { get; init; }
    public Optional<string?> Website { get; init; }
    public Optional<string?> SupportPhone { get; init; }
    public Optional<string?> Description { get; init; }
    public Optional<string?> LogoUrl { get; init; }
    public Optional<List<Guid>?> CategoryIds { get; init; }

    public UpdateBrandDto ToDto()
    {
        return new UpdateBrandDto
        {
            Name = Name,
            Website = Website,
            SupportPhone = SupportPhone,
            Description = Description,
            LogoUrl = LogoUrl,
            CategoryIds = CategoryIds.Map(ids => ids?.Select(x => (CategoryId)x).ToList())
        };
    }
}
