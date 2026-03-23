namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct MemberId(Guid Value)
{
    public static MemberId New()
    {
        return new MemberId(Guid.CreateVersion7());
    }

    public static readonly MemberId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(MemberId id)
    {
        return id.Value;
    }

    public static implicit operator MemberId(Guid value)
    {
        return new MemberId(value);
    }
}
