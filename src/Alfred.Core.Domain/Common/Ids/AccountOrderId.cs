namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct AccountOrderId(Guid Value)
{
    public static AccountOrderId New()
    {
        return new AccountOrderId(Guid.CreateVersion7());
    }

    public static readonly AccountOrderId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(AccountOrderId id)
    {
        return id.Value;
    }

    public static implicit operator AccountOrderId(Guid value)
    {
        return new AccountOrderId(value);
    }
}
