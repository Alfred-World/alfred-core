namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct ProductVariantId(Guid Value)
{
    public static ProductVariantId New()
    {
        return new ProductVariantId(Guid.CreateVersion7());
    }

    public static readonly ProductVariantId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(ProductVariantId id)
    {
        return id.Value;
    }

    public static implicit operator ProductVariantId(Guid value)
    {
        return new ProductVariantId(value);
    }
}
