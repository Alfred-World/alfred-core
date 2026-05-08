using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.AccessControl;

public interface IAccessUserService
{
    Task<PageResult<AccessUserDto>> SearchUsersAsync(SearchRequest request,
        CancellationToken cancellationToken = default);

    Task<AccessUserDto> AddRolesToUserAsync(ReplicatedUserId userId, IEnumerable<AccessRoleId> roleIds,
        CancellationToken cancellationToken = default);

    Task<AccessUserDto> RemoveRolesFromUserAsync(ReplicatedUserId userId, IEnumerable<AccessRoleId> roleIds,
        CancellationToken cancellationToken = default);
}
