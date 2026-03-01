using Alfred.Core.Application.Units.Dtos;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.Units;

public sealed record CreateUnitRequest
{
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Symbol { get; init; }
    public UnitCategory Category { get; init; }
    public Guid? BaseUnitId { get; init; }
    public decimal ConversionRate { get; init; } = 1m;
    public UnitStatus Status { get; init; } = UnitStatus.Active;
    public string? Description { get; init; }

    public CreateUnitDto ToDto()
    {
        return new CreateUnitDto(Code, Name, Symbol, Category, BaseUnitId, ConversionRate, Status, Description);
    }
}
