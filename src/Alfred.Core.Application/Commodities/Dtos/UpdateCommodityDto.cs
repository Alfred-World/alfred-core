using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.Commodities.Dtos;

public sealed record UpdateCommodityDto
{
    public Optional<string> Name { get; init; }
    public Optional<CommodityAssetClass> AssetClass { get; init; }
    public Optional<UnitId?> DefaultUnitId { get; init; }
    public Optional<string?> Description { get; init; }
}
