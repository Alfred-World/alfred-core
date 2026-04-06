using Alfred.Core.Application.Common;
using Alfred.Core.Application.Units.Dtos;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.Units;

public sealed record UpdateUnitRequest
{
    public Optional<string> Name { get; init; }
    public Optional<string?> Symbol { get; init; }
    public Optional<UnitCategory> Category { get; init; }
    public Optional<Guid?> BaseUnitId { get; init; }
    public Optional<decimal> ConversionRate { get; init; }
    public Optional<UnitStatus> Status { get; init; }
    public Optional<string?> Description { get; init; }

    public UpdateUnitDto ToDto()
    {
        return new UpdateUnitDto
        {
            Name = Name,
            Symbol = Symbol,
            Category = Category,
            BaseUnitId = BaseUnitId.Map(id => (UnitId?)id),
            ConversionRate = ConversionRate,
            Status = Status,
            Description = Description
        };
    }
}
