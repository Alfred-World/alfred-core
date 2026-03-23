namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct ReplicatedUserId(Guid Value)
{
    public static ReplicatedUserId New()
    {
        return new ReplicatedUserId(Guid.CreateVersion7());
    }

    public static readonly ReplicatedUserId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(ReplicatedUserId id)
    {
        return id.Value;
    }

    public static implicit operator ReplicatedUserId(Guid value)
    {
        return new ReplicatedUserId(value);
    }
}
