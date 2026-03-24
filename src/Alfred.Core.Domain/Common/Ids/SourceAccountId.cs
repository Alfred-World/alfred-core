namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct SourceAccountId(Guid Value)
{
    public static SourceAccountId New()
    {
        return new SourceAccountId(Guid.CreateVersion7());
    }

    public static readonly SourceAccountId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(SourceAccountId id)
    {
        return id.Value;
    }

    public static implicit operator SourceAccountId(Guid value)
    {
        return new SourceAccountId(value);
    }
}
