using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Application.Querying.Core;

namespace Alfred.Core.Application.AccessControl;

public interface IAccessRoleService
{
    Task<PageResult<AccessRoleDto>> GetAllRolesAsync(QueryRequest query,
        CancellationToken cancellationToken = default);

    Task<AccessRoleDto?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AccessRoleDto> CreateRoleAsync(CreateAccessRoleDto dto, CancellationToken cancellationToken = default);

    Task<AccessRoleDto> UpdateRoleAsync(Guid id, UpdateAccessRoleDto dto,
        CancellationToken cancellationToken = default);

    Task<AccessRoleDto> DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AccessRoleDto> AddPermissionsToRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds,
        CancellationToken cancellationToken = default);

    Task<AccessRoleDto> RemovePermissionsFromRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds,
        CancellationToken cancellationToken = default);
}
