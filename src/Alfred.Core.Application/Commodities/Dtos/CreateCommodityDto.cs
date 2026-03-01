namespace Alfred.Core.Application.Commodities.Dtos;

public sealed record CreateCommodityDto(
    string Code,
    string Name,
    string AssetClass,
    Guid? DefaultUnitId,
    string? Description
);
