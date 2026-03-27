using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Application.Querying.Core;

namespace Alfred.Core.Application.AccessControl;

public interface IAccessRoleService
{
    Task<PageResult<AccessRoleDto>> GetAllRolesAsync(QueryRequest query,
        CancellationToken cancellationToken = default);

    Task<AccessRoleDto?> GetRoleByIdAsync(AccessRoleId id, CancellationToken cancellationToken = default);

    Task<AccessRoleDto> CreateRoleAsync(CreateAccessRoleDto dto, CancellationToken cancellationToken = default);

    Task<AccessRoleDto> UpdateRoleAsync(AccessRoleId id, UpdateAccessRoleDto dto,
        CancellationToken cancellationToken = default);

    Task<AccessRoleDto> DeleteRoleAsync(AccessRoleId id, CancellationToken cancellationToken = default);

    Task<AccessRoleDto> AddPermissionsToRoleAsync(AccessRoleId roleId, IEnumerable<AccessPermissionId> permissionIds,
        CancellationToken cancellationToken = default);

    Task<AccessRoleDto> RemovePermissionsFromRoleAsync(AccessRoleId roleId,
        IEnumerable<AccessPermissionId> permissionIds,
        CancellationToken cancellationToken = default);
}
