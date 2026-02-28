using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.Units.Dtos;

public sealed record UnitDto(
    Guid Id,
    string Code,
    string Name,
    string? Symbol,
    UnitCategory Category,
    Guid? BaseUnitId,
    string? BaseUnitName,
    string? BaseUnitCode,
    decimal ConversionRate,
    UnitStatus Status,
    string? Description,
    int DerivedUnitCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record CreateUnitDto(
    string Code,
    string Name,
    string? Symbol,
    UnitCategory Category,
    Guid? BaseUnitId,
    decimal ConversionRate,
    UnitStatus Status,
    string? Description
);

public sealed record UpdateUnitDto(
    string Name,
    string? Symbol,
    UnitCategory Category,
    Guid? BaseUnitId,
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
