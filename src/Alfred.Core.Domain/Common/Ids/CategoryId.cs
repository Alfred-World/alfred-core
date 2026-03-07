namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct CategoryId(Guid Value)
{
    public static CategoryId New()
    {
        return new CategoryId(Guid.CreateVersion7());
    }

    public static readonly CategoryId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(CategoryId id)
    {
        return id.Value;
    }

    public static implicit operator CategoryId(Guid value)
    {
        return new CategoryId(value);
    }
}
