namespace Alfred.Core.Application.Commodities.Dtos;

public sealed record UpdateCommodityDto(
    string Name,
    string AssetClass,
    Guid? DefaultUnitId,
    string? Description
);
