using Alfred.Core.Application.AccessControl.Dtos;

namespace Alfred.Core.Application.AccessControl;

public interface IAccessUserService
{
    Task<PageResult<AccessUserDto>> GetAllUsersAsync(QueryRequest query, CancellationToken cancellationToken = default);

    Task<AccessUserDto> AddRolesToUserAsync(ReplicatedUserId userId, IEnumerable<AccessRoleId> roleIds,
        CancellationToken cancellationToken = default);

    Task<AccessUserDto> RemoveRolesFromUserAsync(ReplicatedUserId userId, IEnumerable<AccessRoleId> roleIds,
        CancellationToken cancellationToken = default);
}
