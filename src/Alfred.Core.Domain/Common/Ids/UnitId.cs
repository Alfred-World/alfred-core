namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct UnitId(Guid Value)
{
    public static UnitId New()
    {
        return new UnitId(Guid.CreateVersion7());
    }

    public static readonly UnitId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(UnitId id)
    {
        return id.Value;
    }

    public static implicit operator UnitId(Guid value)
    {
        return new UnitId(value);
    }
}
