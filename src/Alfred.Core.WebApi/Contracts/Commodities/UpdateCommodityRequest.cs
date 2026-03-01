using Alfred.Core.Application.Commodities.Dtos;

namespace Alfred.Core.WebApi.Contracts.Commodities;

public sealed record UpdateCommodityRequest
{
    public string Name { get; init; } = null!;
    public string AssetClass { get; init; } = null!;
    public Guid? DefaultUnitId { get; init; }
    public string? Description { get; init; }

    public UpdateCommodityDto ToDto()
    {
        return new UpdateCommodityDto(Name, AssetClass, DefaultUnitId, Description);
    }
}
