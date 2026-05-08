using Alfred.Core.Application.AccessControl;
using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Application.Querying.JsonFilter.Inputs;
using Alfred.Core.Domain.Constants;
using Alfred.Core.Domain.Querying;
using Alfred.Core.WebApi.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers;

[Route("api/v{version:apiVersion}/access-control/permissions")]
[Authorize]
public sealed class AccessPermissionsController : BaseApiController
{
    private readonly IAccessPermissionService _permissionService;

    public AccessPermissionsController(IAccessPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    [RequirePermission(PermissionCodes.AccessControl.PermissionRead)]
    [ProducesResponseType(typeof(ApiPagedResponse<AccessPermissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissions([FromQuery] PaginationQueryParameters query,
        CancellationToken cancellationToken)
    {
        var result = await _permissionService.SearchPermissionsAsync(query.ToSearchRequest(), cancellationToken);
        return OkPaginatedResponse(result);
    }

    [HttpPost("search")]
    [RequirePermission(PermissionCodes.AccessControl.PermissionRead)]
    [ProducesResponseType(typeof(ApiPagedResponse<AccessPermissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchPermissions(
        [FromBody] SearchRequest<AccessPermissionFilterInput> request,
        CancellationToken cancellationToken)
    {
        var result = await _permissionService.SearchPermissionsAsync(request.ToSearchRequest(), cancellationToken);
        return OkPaginatedResponse(result);
    }
}
