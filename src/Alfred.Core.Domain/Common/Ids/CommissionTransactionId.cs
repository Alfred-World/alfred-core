namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct CommissionTransactionId(Guid Value)
{
    public static CommissionTransactionId New()
    {
        return new CommissionTransactionId(Guid.CreateVersion7());
    }

    public static readonly CommissionTransactionId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(CommissionTransactionId id)
    {
        return id.Value;
    }

    public static implicit operator CommissionTransactionId(Guid value)
    {
        return new CommissionTransactionId(value);
    }
}
