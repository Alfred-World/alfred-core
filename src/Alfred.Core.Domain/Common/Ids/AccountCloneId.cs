namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct AccountCloneId(Guid Value)
{
    public static AccountCloneId New()
    {
        return new AccountCloneId(Guid.CreateVersion7());
    }

    public static readonly AccountCloneId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(AccountCloneId id)
    {
        return id.Value;
    }

    public static implicit operator AccountCloneId(Guid value)
    {
        return new AccountCloneId(value);
    }
}
