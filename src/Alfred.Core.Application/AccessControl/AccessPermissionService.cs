using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Application.AccessControl.Shared;
using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.AccessControl;

public sealed class AccessPermissionService : BaseApplicationService, IAccessPermissionService
{
    private readonly IUnitOfWork _unitOfWork;

    public AccessPermissionService(IUnitOfWork unitOfWork, IAsyncQueryExecutor executor)
        : base(executor)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PageResult<AccessPermissionDto>> SearchPermissionsAsync(SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        return await SearchWithViewAsync(
            _unitOfWork.AccessPermissions,
            request,
            AccessPermissionFieldMap.Instance,
            AccessPermissionFieldMap.Views,
            p => p.ToDto(),
            cancellationToken);
    }
}
