using Alfred.Core.Application.AccessControl.Dtos;

namespace Alfred.Core.Application.AccessControl;

public interface IAccessPermissionService
{
    Task<PageResult<AccessPermissionDto>> GetAllPermissionsAsync(QueryRequest query,
        CancellationToken cancellationToken = default);
}
