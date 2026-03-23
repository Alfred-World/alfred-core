using Alfred.Core.Application.Querying.Fields;
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
        .Add("isActive", x => x.IsActive).AllowAll()
        .Add("createdAt", x => x.CreatedAt).AllowAll()
        .Add("updatedAt", x => x.UpdatedAt!).AllowAll();
}
