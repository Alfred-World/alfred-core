using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Entities;

public sealed class Unit : BaseEntity, IHasCreationTime, IHasModificationTime
{
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public string? Symbol { get; private set; }
    public UnitCategory Category { get; private set; }
    public Guid? BaseUnitId { get; private set; }
    public decimal ConversionRate { get; private set; } = 1m;
    public UnitStatus Status { get; private set; } = UnitStatus.Active;
    public string? Description { get; private set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Unit? BaseUnit { get; private set; }
    public ICollection<Unit> DerivedUnits { get; private set; } = new List<Unit>();

    private Unit() { }

    public static Unit Create(
        string code,
        string name,
        UnitCategory category,
        string? symbol = null,
        Guid? baseUnitId = null,
        decimal conversionRate = 1m,
        UnitStatus status = UnitStatus.Active,
        string? description = null)
    {
        return new Unit
        {
            Code = code,
            Name = name,
            Symbol = symbol,
            Category = category,
            BaseUnitId = baseUnitId,
            ConversionRate = conversionRate,
            Status = status,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string? symbol,
        UnitCategory category,
        Guid? baseUnitId,
        decimal conversionRate,
        UnitStatus status,
        string? description)
    {
        Name = name;
        Symbol = symbol;
        Category = category;
        BaseUnitId = baseUnitId;
        ConversionRate = conversionRate;
        Status = status;
        Description = description;
    }
}
