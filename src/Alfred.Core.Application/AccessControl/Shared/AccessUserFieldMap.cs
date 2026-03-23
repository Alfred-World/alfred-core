using System.Linq.Expressions;

using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Application.Querying.Fields;
using Alfred.Core.Application.Querying.Projection;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.AccessControl.Shared;

public sealed class AccessUserFieldMap : BaseFieldMap<ReplicatedUser>
{
    private static readonly Lazy<AccessUserFieldMap> _instance = new(() => new AccessUserFieldMap());

    private AccessUserFieldMap()
    {
    }

    public static AccessUserFieldMap Instance => _instance.Value;

    public override FieldMap<ReplicatedUser> Fields { get; } = new FieldMap<ReplicatedUser>()
        .Add("id", x => x.Id).AllowAll()
        .Add("userName", x => x.UserName).AllowAll()
        .Add("email", x => x.Email).AllowAll()
        .Add("fullName", x => x.FullName!).AllowAll()
        .Add("avatar", x => x.Avatar!).AllowAll()
        .Add("createdAt", x => x.CreatedAt).AllowAll()
        .Add("updatedAt", x => x.UpdatedAt).AllowAll()
        .Add("roles", x => x.UserRoles.Select(ur => new AccessRoleDto
        {
            Id = ur.Role.Id.Value,
            Name = ur.Role.Name,
            NormalizedName = ur.Role.NormalizedName,
            Icon = ur.Role.Icon,
            IsImmutable = ur.Role.IsImmutable,
            IsSystem = ur.Role.IsSystem,
            IsDeleted = ur.Role.IsDeleted,
            CreatedAt = ur.Role.CreatedAt,
            UpdatedAt = ur.Role.UpdatedAt,
            Permissions = null
        })).Selectable()
        .Add("rolesSummary", x => x.UserRoles.Select(ur => new AccessRoleDto
        {
            Id = ur.Role.Id.Value,
            Name = ur.Role.Name,
            Icon = ur.Role.Icon,
            Permissions = null
        })).Selectable();

    public static ViewRegistry<ReplicatedUser, AccessUserDto> Views { get; } =
        new ViewRegistry<ReplicatedUser, AccessUserDto>()
            .Register("list", cfg => cfg
                .Select(x => x.Id)
                .Select(x => x.UserName)
                .Select(x => x.Email)
                .Select(x => x.FullName)
                .Select(x => x.Avatar)
                .Select(x => x.CreatedAt)
                .Select(x => x.UpdatedAt)
                .SelectAs(x => x.Roles, "rolesSummary"))
            .Register("detail", cfg => cfg
                .Select(x => x.Id)
                .Select(x => x.UserName)
                .Select(x => x.Email)
                .Select(x => x.FullName)
                .Select(x => x.Avatar)
                .Select(x => x.CreatedAt)
                .Select(x => x.UpdatedAt)
                .SelectAs(x => x.Roles, "roles"))
            .Register("summary", new Expression<Func<AccessUserDto, object?>>[]
            {
                x => x.Id,
                x => x.UserName,
                x => x.FullName,
                x => x.Email
            })
            .SetDefault("list");
}
