using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.Units.Dtos;

public sealed class UnitDto
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Symbol { get; set; }
    public UnitCategory Category { get; set; }
    public Guid? BaseUnitId { get; set; }
    public string? BaseUnitName { get; set; }
    public string? BaseUnitCode { get; set; }
    public decimal ConversionRate { get; set; }
    public UnitStatus Status { get; set; }
    public string? Description { get; set; }
    public int DerivedUnitCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed record CreateUnitDto(
    string Code,
    string Name,
    string? Symbol,
    UnitCategory Category,
    UnitId? BaseUnitId,
    decimal ConversionRate,
    UnitStatus Status,
    string? Description
);

public sealed record UpdateUnitDto(
    string Name,
    string? Symbol,
    UnitCategory Category,
    UnitId? BaseUnitId,
    decimal ConversionRate,
    UnitStatus Status,
    string? Description
);

public sealed record UnitTreeNodeDto(
    Guid Id,
    string Code,
    string Name,
    string? Symbol,
    UnitCategory Category,
    decimal ConversionRate,
    bool IsBaseUnit,
    List<UnitTreeNodeDto> DerivedUnits
);

public sealed record UnitCountByStatusDto(
    UnitStatus Status,
    int Count
);

public sealed record UnitCountByCategoryDto(
    UnitCategory Category,
    int Count
);

public sealed record ConvertResultDto(
    decimal FromValue,
    string FromUnitCode,
    decimal ToValue,
    string ToUnitCode,
    string Formula
);
