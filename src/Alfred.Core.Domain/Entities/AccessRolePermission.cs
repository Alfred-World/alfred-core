using Alfred.Core.Domain.Common.Interfaces;

namespace Alfred.Core.Domain.Entities;

public sealed class AccessRolePermission : IHasCreationTime
{
    public AccessRoleId RoleId { get; private set; }
    public AccessPermissionId PermissionId { get; private set; }

    public AccessRole Role { get; private set; } = null!;
    public AccessPermission Permission { get; private set; } = null!;

    public DateTime CreatedAt { get; set; }

    private AccessRolePermission()
    {
    }

    public static AccessRolePermission Create(AccessRoleId roleId, AccessPermissionId permissionId)
    {
        return new AccessRolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
