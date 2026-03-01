using Alfred.Core.Application.Querying.Fields;
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
        .Add("createdAt", u => u.CreatedAt).Sortable().Selectable();
}
