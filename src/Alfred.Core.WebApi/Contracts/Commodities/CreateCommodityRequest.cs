using Alfred.Core.Application.Commodities.Dtos;

namespace Alfred.Core.WebApi.Contracts.Commodities;

public sealed record CreateCommodityRequest
{
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string AssetClass { get; init; } = null!;
    public Guid? DefaultUnitId { get; init; }
    public string? Description { get; init; }

    public CreateCommodityDto ToDto()
    {
        return new CreateCommodityDto(Code, Name, AssetClass, DefaultUnitId, Description);
    }
}
