namespace Alfred.Core.Application.Commodities.Dtos;

public sealed record CommodityDto(
    Guid Id,
    string Code,
    string Name,
    string AssetClass,
    Guid? DefaultUnitId,
    string? DefaultUnitName,
    string? DefaultUnitCode,
    string? Description,
    DateTime CreatedAt
);
