using Alfred.Core.Application.AccessControl.Dtos;

namespace Alfred.Core.WebApi.Contracts.AccessControl;

public sealed record CreateAccessRoleRequest(
    string Name,
    string? Icon = null,
    bool IsImmutable = false,
    bool IsSystem = false,
    List<Guid>? Permissions = null)
{
    public CreateAccessRoleDto ToDto()
    {
        return new CreateAccessRoleDto(Name, Icon, IsImmutable, IsSystem,
            Permissions?.Select(x => (AccessPermissionId)x).ToList());
    }
}

public sealed record UpdateAccessRoleRequest(
    string Name,
    string? Icon = null,
    bool IsImmutable = false,
    bool IsSystem = false,
    List<Guid>? Permissions = null)
{
    public UpdateAccessRoleDto ToDto()
    {
        return new UpdateAccessRoleDto(Name, Icon, IsImmutable, IsSystem,
            Permissions?.Select(x => (AccessPermissionId)x).ToList());
    }
}
