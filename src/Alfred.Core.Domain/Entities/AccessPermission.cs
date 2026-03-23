using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;

namespace Alfred.Core.Domain.Entities;

public sealed class AccessPermission : BaseEntity<AccessPermissionId>, IHasCreationTime, IHasModificationTime
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Resource { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<AccessRolePermission> RolePermissions { get; private set; } = new List<AccessRolePermission>();

    private AccessPermission()
    {
        Id = AccessPermissionId.New();
    }

    public static AccessPermission Create(string code, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new InvalidOperationException("Permission code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Permission name is required.");
        }

        var normalizedCode = code.Trim().ToLowerInvariant();
        var parts = normalizedCode.Split(':', StringSplitOptions.RemoveEmptyEntries);
        var resource = parts.Length > 0 ? parts[0] : normalizedCode;
        var action = parts.Length > 1 ? parts[1] : "access";

        return new AccessPermission
        {
            Code = normalizedCode,
            Name = name.Trim(),
            Resource = resource,
            Action = action,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Permission name is required.");
        }

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
}
