using Alfred.Core.Application.Commodities.Dtos;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.Commodities;

public sealed record UpdateCommodityRequest
{
    public string Name { get; init; } = null!;
    public CommodityAssetClass AssetClass { get; init; } = CommodityAssetClass.Metal;
    public Guid? DefaultUnitId { get; init; }
    public string? Description { get; init; }

    public UpdateCommodityDto ToDto()
    {
        return new UpdateCommodityDto(Name, AssetClass, DefaultUnitId, Description);
    }
}
