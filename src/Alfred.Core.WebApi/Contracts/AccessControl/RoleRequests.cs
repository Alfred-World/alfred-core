using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Application.Common;

namespace Alfred.Core.WebApi.Contracts.AccessControl;

public sealed record CreateAccessRoleRequest(
    string Name,
    string? Icon = null,
    bool IsSystem = false,
    List<Guid>? Permissions = null)
{
    public CreateAccessRoleDto ToDto()
    {
        return new CreateAccessRoleDto(Name, Icon, IsSystem,
            Permissions?.Select(x => (AccessPermissionId)x).ToList());
    }
}

public sealed record UpdateAccessRoleRequest
{
    public Optional<string> Name { get; init; }
    public Optional<string?> Icon { get; init; }
    public Optional<bool> IsSystem { get; init; }
    public Optional<List<Guid>?> Permissions { get; init; }

    public UpdateAccessRoleDto ToDto()
    {
        return new UpdateAccessRoleDto
        {
            Name = Name,
            Icon = Icon,
            IsSystem = IsSystem,
            Permissions = Permissions.Map(ids => ids?.Select(x => (AccessPermissionId)x).ToList())
        };
    }
}
