namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct BrandId(Guid Value)
{
    public static BrandId New()
    {
        return new BrandId(Guid.CreateVersion7());
    }

    public static readonly BrandId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(BrandId id)
    {
        return id.Value;
    }

    public static implicit operator BrandId(Guid value)
    {
        return new BrandId(value);
    }
}
