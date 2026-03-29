namespace Alfred.Core.Domain.Entities;

public sealed class AccessUserRole : IHasCreationTime
{
    public ReplicatedUserId UserId { get; private set; }
    public AccessRoleId RoleId { get; private set; }

    public ReplicatedUser User { get; private set; } = null!;
    public AccessRole Role { get; private set; } = null!;

    public DateTime CreatedAt { get; set; }

    private AccessUserRole()
    {
    }

    public static AccessUserRole Create(ReplicatedUserId userId, AccessRoleId roleId)
    {
        return new AccessUserRole
        {
            UserId = userId,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
