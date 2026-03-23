namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct AccessRoleId(Guid Value)
{
    public static AccessRoleId New()
    {
        return new AccessRoleId(Guid.CreateVersion7());
    }

    public static readonly AccessRoleId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(AccessRoleId id)
    {
        return id.Value;
    }

    public static implicit operator AccessRoleId(Guid value)
    {
        return new AccessRoleId(value);
    }
}
