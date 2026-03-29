using System.Linq.Expressions;

using Alfred.Core.Application.Units.Dtos;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Units.Shared;

public sealed class UnitFieldMap : BaseFieldMap<Unit>
{
    private static readonly Lazy<UnitFieldMap> _instance = new(() => new UnitFieldMap());

    private UnitFieldMap()
    {
    }

    public static UnitFieldMap Instance => _instance.Value;

    public override FieldMap<Unit> Fields { get; } = new FieldMap<Unit>()
        .Add("id", u => u.Id).AllowAll()
        .Add("code", u => u.Code).AllowAll()
        .Add("name", u => u.Name).AllowAll()
        .Add("symbol", u => u.Symbol!).AllowAll()
        .Add("category", u => u.Category).AllowAll()
        .Add("status", u => u.Status).AllowAll()
        .Add("conversionRate", u => u.ConversionRate).Sortable().Selectable()
        .Add("baseUnitId", u => u.BaseUnitId!).AllowAll()
        .Add("description", u => u.Description!).AllowAll()
        .Add("updatedAt", u => u.UpdatedAt!).Sortable().Selectable()
        .Add("baseUnitName", u => u.BaseUnit!.Name).Selectable()
        .Add("baseUnitCode", u => u.BaseUnit!.Code).Selectable()
        .Add("derivedUnitCount", u => u.DerivedUnits.Count()).Selectable()
        .Add("createdAt", u => u.CreatedAt).Sortable().Selectable();

    public static ViewRegistry<Unit, UnitDto> Views { get; } =
        new ViewRegistry<Unit, UnitDto>()
            .Register("list", new Expression<Func<UnitDto, object?>>[]
            {
                x => x.Id,
                x => x.Code,
                x => x.Name,
                x => x.Symbol,
                x => x.Category,
                x => x.BaseUnitId,
                x => x.BaseUnitName,
                x => x.BaseUnitCode,
                x => x.ConversionRate,
                x => x.Status,
                x => x.Description,
                x => x.DerivedUnitCount,
                x => x.CreatedAt,
                x => x.UpdatedAt
            })
            .SetDefault("list");
}
