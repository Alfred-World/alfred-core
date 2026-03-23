using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;

namespace Alfred.Core.Domain.Entities;

public sealed class AccessRole : BaseEntity<AccessRoleId>, IHasCreationTime, IHasModificationTime, IHasDeletionTime
{
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public string? Icon { get; private set; }
    public bool IsImmutable { get; private set; }
    public bool IsSystem { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<AccessRolePermission> RolePermissions { get; private set; } = new List<AccessRolePermission>();
    public ICollection<AccessUserRole> UserRoles { get; private set; } = new List<AccessUserRole>();

    private AccessRole()
    {
        Id = AccessRoleId.New();
    }

    public static AccessRole Create(string name, string? icon = null, bool isImmutable = false, bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Role name is required.");
        }

        return new AccessRole
        {
            Name = name.Trim(),
            NormalizedName = name.Trim().ToUpperInvariant(),
            Icon = string.IsNullOrWhiteSpace(icon) ? null : icon.Trim(),
            IsImmutable = isImmutable,
            IsSystem = isSystem,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? icon, bool isImmutable, bool isSystem)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Role name is required.");
        }

        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
        Icon = string.IsNullOrWhiteSpace(icon) ? null : icon.Trim();
        IsImmutable = isImmutable;
        IsSystem = isSystem;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SyncPermissions(IEnumerable<AccessPermissionId> permissionIds)
    {
        var desired = permissionIds.Distinct().ToHashSet();
        var current = RolePermissions.Select(x => x.PermissionId).ToHashSet();

        var toAdd = desired.Except(current);
        foreach (var permissionId in toAdd)
        {
            RolePermissions.Add(AccessRolePermission.Create(Id, permissionId));
        }

        var toRemove = current.Except(desired).ToHashSet();
        var removingItems = RolePermissions.Where(x => toRemove.Contains(x.PermissionId)).ToList();
        foreach (var item in removingItems)
        {
            RolePermissions.Remove(item);
        }

        UpdatedAt = DateTime.UtcNow;
    }
}
