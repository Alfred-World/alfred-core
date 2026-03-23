namespace Alfred.Core.Domain.Common.Ids;

public readonly record struct AccessPermissionId(Guid Value)
{
    public static AccessPermissionId New()
    {
        return new AccessPermissionId(Guid.CreateVersion7());
    }

    public static readonly AccessPermissionId Empty = new(Guid.Empty);

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(AccessPermissionId id)
    {
        return id.Value;
    }

    public static implicit operator AccessPermissionId(Guid value)
    {
        return new AccessPermissionId(value);
    }
}
