using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Application.AccessControl.Shared;
using Alfred.Core.Application.Common;
using Alfred.Core.Application.Querying.Core;
using Alfred.Core.Application.Querying.Filtering.Parsing;

namespace Alfred.Core.Application.AccessControl;

public sealed class AccessPermissionService : BaseApplicationService, IAccessPermissionService
{
    private readonly IUnitOfWork _unitOfWork;

    public AccessPermissionService(IUnitOfWork unitOfWork, IFilterParser filterParser, IAsyncQueryExecutor executor)
        : base(filterParser, executor)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PageResult<AccessPermissionDto>> GetAllPermissionsAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedWithViewAsync(
            _unitOfWork.AccessPermissions,
            query,
            AccessPermissionFieldMap.Instance,
            AccessPermissionFieldMap.Views,
            p => p.ToDto(),
            cancellationToken);
    }
}
