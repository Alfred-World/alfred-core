namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct InvestmentTransactionId(Guid Value)
{
    public static InvestmentTransactionId New()
    {
        return new InvestmentTransactionId(Guid.CreateVersion7());
    }

    public static readonly InvestmentTransactionId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(InvestmentTransactionId id)
    {
        return id.Value;
    }

    public static implicit operator InvestmentTransactionId(Guid value)
    {
        return new InvestmentTransactionId(value);
    }
}
