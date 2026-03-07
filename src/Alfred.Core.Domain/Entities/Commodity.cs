using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Entities;

public sealed class Commodity : BaseEntity<CommodityId>, IHasCreationTime
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public CommodityAssetClass AssetClass { get; private set; }
    public UnitId? DefaultUnitId { get; private set; }
    public string? Description { get; private set; }

    public DateTime CreatedAt { get; set; }

    // Navigation
    public Unit? DefaultUnit { get; private set; }

    private Commodity()
    {
        Id = CommodityId.New();
    }

    public static Commodity Create(string code, string name, CommodityAssetClass assetClass, UnitId? defaultUnitId,
        string? description)
    {
        return new Commodity
        {
            Code = code,
            Name = name,
            AssetClass = assetClass,
            DefaultUnitId = defaultUnitId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, CommodityAssetClass assetClass, UnitId? defaultUnitId, string? description)
    {
        Name = name;
        AssetClass = assetClass;
        DefaultUnitId = defaultUnitId;
        Description = description;
    }
}
