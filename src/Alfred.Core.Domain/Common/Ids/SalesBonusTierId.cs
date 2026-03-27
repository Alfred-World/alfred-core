namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct SalesBonusTierId(Guid Value)
{
    public static SalesBonusTierId New()
    {
        return new SalesBonusTierId(Guid.CreateVersion7());
    }

    public static readonly SalesBonusTierId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(SalesBonusTierId id)
    {
        return id.Value;
    }

    public static implicit operator SalesBonusTierId(Guid value)
    {
        return new SalesBonusTierId(value);
    }
}
