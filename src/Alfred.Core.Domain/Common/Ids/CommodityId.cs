namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct CommodityId(Guid Value)
{
    public static CommodityId New()
    {
        return new CommodityId(Guid.CreateVersion7());
    }

    public static readonly CommodityId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(CommodityId id)
    {
        return id.Value;
    }

    public static implicit operator CommodityId(Guid value)
    {
        return new CommodityId(value);
    }
}
