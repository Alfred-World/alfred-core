using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Application.Querying.Core;

namespace Alfred.Core.Application.AccessControl;

public interface IAccessUserService
{
    Task<PageResult<AccessUserDto>> GetAllUsersAsync(QueryRequest query, CancellationToken cancellationToken = default);

    Task<AccessUserDto> AddRolesToUserAsync(Guid userId, IEnumerable<Guid> roleIds,
        CancellationToken cancellationToken = default);

    Task<AccessUserDto> RemoveRolesFromUserAsync(Guid userId, IEnumerable<Guid> roleIds,
        CancellationToken cancellationToken = default);
}
