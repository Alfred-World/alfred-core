using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.AccessControl;

public interface IAccessPermissionService
{
    Task<PageResult<AccessPermissionDto>> SearchPermissionsAsync(SearchRequest request,
        CancellationToken cancellationToken = default);
}
