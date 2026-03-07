using Alfred.Core.Application.Units.Dtos;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Units.Shared;

public static class UnitMappingExtensions
{
    public static UnitDto ToDto(this Unit unit)
    {
        return new UnitDto(
            unit.Id.Value,
            unit.Code,
            unit.Name,
            unit.Symbol,
            unit.Category,
            unit.BaseUnitId?.Value,
            unit.BaseUnit?.Name,
            unit.BaseUnit?.Code,
            unit.ConversionRate,
            unit.Status,
            unit.Description,
            unit.DerivedUnits.Count,
            unit.CreatedAt,
            unit.UpdatedAt
        );
    }

    public static UnitTreeNodeDto ToTreeNode(this Unit unit)
    {
        return new UnitTreeNodeDto(
            unit.Id.Value,
            unit.Code,
            unit.Name,
            unit.Symbol,
            unit.Category,
            unit.ConversionRate,
            unit.BaseUnitId == null,
            unit.DerivedUnits
                .OrderBy(d => d.Name)
                .Select(d => d.ToTreeNode())
                .ToList()
        );
    }
}
