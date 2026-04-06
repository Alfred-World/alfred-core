using Alfred.Core.Application.Commodities.Dtos;
using Alfred.Core.Application.Common;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.Commodities;

public sealed record UpdateCommodityRequest
{
    public Optional<string> Name { get; init; }
    public Optional<CommodityAssetClass> AssetClass { get; init; }
    public Optional<Guid?> DefaultUnitId { get; init; }
    public Optional<string?> Description { get; init; }

    public UpdateCommodityDto ToDto()
    {
        return new UpdateCommodityDto
        {
            Name = Name,
            AssetClass = AssetClass,
            DefaultUnitId = DefaultUnitId.Map(id => (UnitId?)id),
            Description = Description
        };
    }
}
