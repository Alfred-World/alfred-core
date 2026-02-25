using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;

namespace Alfred.Core.Domain.Entities;

public sealed class Unit : BaseEntity, IHasCreationTime
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public Guid? BaseUnitId { get; private set; }
    public decimal ConversionRate { get; private set; }
    
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Unit? BaseUnit { get; private set; }
    public ICollection<Unit> SubUnits { get; private set; } = new List<Unit>();

    private Unit() { }

    public static Unit Create(string code, string name, Guid? baseUnitId = null, decimal conversionRate = 1)
    {
        return new Unit
        {
            Code = code,
            Name = name,
            BaseUnitId = baseUnitId,
            ConversionRate = conversionRate,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public void Update(string name, Guid? baseUnitId, decimal conversionRate)
    {
        Name = name;
        BaseUnitId = baseUnitId;
        ConversionRate = conversionRate;
    }
}
