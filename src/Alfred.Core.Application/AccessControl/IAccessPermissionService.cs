using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Application.Querying.Core;

namespace Alfred.Core.Application.AccessControl;

public interface IAccessPermissionService
{
    Task<PageResult<AccessPermissionDto>> GetAllPermissionsAsync(QueryRequest query,
        CancellationToken cancellationToken = default);
}
