using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.AccessControl.Shared;

public static class AccessControlMappingExtensions
{
    public static AccessPermissionDto ToDto(this AccessPermission entity)
    {
        return new AccessPermissionDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Resource = entity.Resource,
            Action = entity.Action,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static AccessRoleDto ToDto(this AccessRole entity)
    {
        return new AccessRoleDto
        {
            Id = entity.Id,
            Name = entity.Name,
            NormalizedName = entity.NormalizedName,
            Icon = entity.Icon,
            IsImmutable = entity.IsImmutable,
            IsSystem = entity.IsSystem,
            IsDeleted = entity.IsDeleted,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Permissions = entity.RolePermissions
                .Where(x => x.Permission != null)
                .Select(x => x.Permission.ToDto())
                .ToList()
        };
    }
}
