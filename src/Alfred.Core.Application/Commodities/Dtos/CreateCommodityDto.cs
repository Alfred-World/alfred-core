using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.Commodities.Dtos;

public sealed record CreateCommodityDto(
    string Code,
    string Name,
    CommodityAssetClass AssetClass,
    Guid? DefaultUnitId,
    string? Description
);
