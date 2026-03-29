using Alfred.Core.Application.AccessControl;
using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Domain.Constants;
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
        var result = await _permissionService.GetAllPermissionsAsync(query.ToQueryRequest(), cancellationToken);
        return OkPaginatedResponse(result);
    }
}
