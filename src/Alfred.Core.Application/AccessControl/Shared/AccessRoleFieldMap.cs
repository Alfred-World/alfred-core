using System.Linq.Expressions;

using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Application.Querying.Fields;
using Alfred.Core.Application.Querying.Projection;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.AccessControl.Shared;

/// <summary>
/// FieldMap for Role entity - defines filterable, sortable fields and views.
/// Optimized for database-level projection to minimize memory usage.
/// </summary>
public class AccessRoleFieldMap : BaseFieldMap<AccessRole>
{
    private static readonly Lazy<AccessRoleFieldMap> _instance = new(() => new AccessRoleFieldMap());

    private AccessRoleFieldMap()
    {
    }

    public static AccessRoleFieldMap Instance => _instance.Value;

    public override FieldMap<AccessRole> Fields { get; } = new FieldMap<AccessRole>()
        .Add("id", r => r.Id).AllowAll()
        .Add("name", r => r.Name).AllowAll()
        .Add("normalizedName", r => r.NormalizedName).AllowAll()
        .Add("isImmutable", r => r.IsImmutable).AllowAll()
        .Add("isSystem", r => r.IsSystem).AllowAll()
        .Add("icon", r => r.Icon!).AllowAll()
        .Add("isDeleted", r => r.IsDeleted).AllowAll()
        .Add("createdAt", r => r.CreatedAt).AllowAll()
        .Add("updatedAt", r => r.UpdatedAt!).AllowAll()

        // Full permission projection - all fields
        .Add("permissions", r => r.RolePermissions.Select(rp => new AccessPermissionDto(
            rp.Permission.Id.Value,
            rp.Permission.Code,
            rp.Permission.Name,
            rp.Permission.Resource,
            rp.Permission.Action,
            rp.Permission.Description,
            rp.Permission.IsActive,
            rp.Permission.CreatedAt,
            rp.Permission.UpdatedAt
        ))).AllowAll()

        // Lightweight permission projection - only id, code, name (for list views)
        // Returns AccessPermissionDto with null for non-essential fields (skipped in JSON)
        .Add("permissionsSummary", r => r.RolePermissions.Select(rp => new AccessPermissionDto(
            rp.Permission.Id.Value,
            rp.Permission.Code,
            rp.Permission.Name,
            string.Empty, // Resource - intentionally omitted in summary view
            string.Empty, // Action - intentionally omitted in summary view
            null, // Description - intentionally omitted in summary view
            rp.Permission.IsActive,
            rp.Permission.CreatedAt,
            rp.Permission.UpdatedAt
        ))).Selectable();

    /// <summary>
    /// Available views for Role entity.
    /// Each view defines which fields are projected at database level.
    /// </summary>
    public static ViewRegistry<AccessRole, AccessRoleDto> Views { get; } = new ViewRegistry<AccessRole, AccessRoleDto>()
        .Register("list", new Expression<Func<AccessRoleDto, object?>>[]
        {
            r => r.Id,
            r => r.Name,
            r => r.NormalizedName,
            r => r.IsImmutable,
            r => r.IsSystem,
            r => r.Icon,
            r => r.IsDeleted,
            r => r.CreatedAt,
            r => r.UpdatedAt
        })
        // Detail view with lightweight permissions (only id, code, name)
        .Register("detail", cfg => cfg
            .Select(r => r.Id)
            .Select(r => r.Name)
            .Select(r => r.NormalizedName)
            .Select(r => r.IsImmutable)
            .Select(r => r.IsSystem)
            .Select(r => r.Icon)
            .Select(r => r.IsDeleted)
            .Select(r => r.CreatedAt)
            .Select(r => r.UpdatedAt)
            // Map DTO's Permissions property to use permissionsSummary from FieldMap
            .SelectAs(r => r.Permissions, "permissionsSummary"))
        // Detail view with full permissions (all fields)
        .Register("detail.full", new Expression<Func<AccessRoleDto, object?>>[]
        {
            r => r.Id,
            r => r.Name,
            r => r.NormalizedName,
            r => r.IsImmutable,
            r => r.IsSystem,
            r => r.Icon,
            r => r.IsDeleted,
            r => r.CreatedAt,
            r => r.UpdatedAt,
            r => r.Permissions
        })
        .Register("summary", new Expression<Func<AccessRoleDto, object?>>[]
        {
            r => r.Id,
            r => r.Name,
            r => r.Icon
        })
        .SetDefault("list");
}
