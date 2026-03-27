namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct SalesBonusTransactionId(Guid Value)
{
    public static SalesBonusTransactionId New()
    {
        return new SalesBonusTransactionId(Guid.CreateVersion7());
    }

    public static readonly SalesBonusTransactionId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(SalesBonusTransactionId id)
    {
        return id.Value;
    }

    public static implicit operator SalesBonusTransactionId(Guid value)
    {
        return new SalesBonusTransactionId(value);
    }
}
