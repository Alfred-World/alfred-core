namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct ProductId(Guid Value)
{
    public static ProductId New()
    {
        return new ProductId(Guid.CreateVersion7());
    }

    public static readonly ProductId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(ProductId id)
    {
        return id.Value;
    }

    public static implicit operator ProductId(Guid value)
    {
        return new ProductId(value);
    }
}
