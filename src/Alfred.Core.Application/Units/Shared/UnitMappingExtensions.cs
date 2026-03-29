using Alfred.Core.Application.Units.Dtos;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Units.Shared;

public static class UnitMappingExtensions
{
    public static UnitDto ToDto(this Unit unit)
    {
        return new UnitDto
        {
            Id = unit.Id,
            Code = unit.Code,
            Name = unit.Name,
            Symbol = unit.Symbol,
            Category = unit.Category,
            BaseUnitId = unit.BaseUnitId,
            BaseUnitName = unit.BaseUnit?.Name,
            BaseUnitCode = unit.BaseUnit?.Code,
            ConversionRate = unit.ConversionRate,
            Status = unit.Status,
            Description = unit.Description,
            DerivedUnitCount = unit.DerivedUnits.Count,
            CreatedAt = unit.CreatedAt,
            UpdatedAt = unit.UpdatedAt
        };
    }

    public static UnitTreeNodeDto ToTreeNode(this Unit unit)
    {
        return new UnitTreeNodeDto(
            unit.Id,
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
