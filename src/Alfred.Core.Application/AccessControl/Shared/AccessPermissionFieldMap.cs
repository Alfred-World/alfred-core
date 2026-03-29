using System.Linq.Expressions;

using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Application.Querying.Fields;
using Alfred.Core.Application.Querying.Projection;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.AccessControl.Shared;

public sealed class AccessPermissionFieldMap : BaseFieldMap<AccessPermission>
{
    private static readonly Lazy<AccessPermissionFieldMap> _instance = new(() => new AccessPermissionFieldMap());

    private AccessPermissionFieldMap()
    {
    }

    public static AccessPermissionFieldMap Instance => _instance.Value;

    public override FieldMap<AccessPermission> Fields { get; } = new FieldMap<AccessPermission>()
        .Add("id", x => x.Id).AllowAll()
        .Add("code", x => x.Code).AllowAll()
        .Add("name", x => x.Name).AllowAll()
        .Add("resource", x => x.Resource).AllowAll()
        .Add("action", x => x.Action).AllowAll()
        .Add("description", x => x.Description!).AllowAll()
        .Add("isActive", x => x.IsActive).AllowAll()
        .Add("createdAt", x => x.CreatedAt).AllowAll()
        .Add("updatedAt", x => x.UpdatedAt!).AllowAll();

    public static ViewRegistry<AccessPermission, AccessPermissionDto> Views { get; } =
        new ViewRegistry<AccessPermission, AccessPermissionDto>()
            .Register("list", new Expression<Func<AccessPermissionDto, object?>>[]
            {
                x => x.Id,
                x => x.Code,
                x => x.Name,
                x => x.Resource,
                x => x.Action,
                x => x.Description,
                x => x.IsActive,
                x => x.CreatedAt,
                x => x.UpdatedAt
            })
            .SetDefault("list");
}
