using Alfred.Core.Application.AccessControl;
using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Application.Querying.JsonFilter.Inputs;
using Alfred.Core.Domain.Constants;
using Alfred.Core.Domain.Querying;
using Alfred.Core.WebApi.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers;

[Route("api/v{version:apiVersion}/access-control/users")]
[Authorize]
public sealed class AccessUsersController : BaseApiController
{
    private readonly IAccessUserService _userService;

    public AccessUsersController(IAccessUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [RequirePermission(PermissionCodes.AccessControl.UserRead)]
    [ProducesResponseType(typeof(ApiPagedResponse<AccessUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] PaginationQueryParameters query,
        CancellationToken cancellationToken)
    {
        var result = await _userService.SearchUsersAsync(query.ToSearchRequest(), cancellationToken);
        return OkPaginatedResponse(result);
    }

    [HttpPost("search")]
    [RequirePermission(PermissionCodes.AccessControl.UserRead)]
    [ProducesResponseType(typeof(ApiPagedResponse<AccessUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchUsers([FromBody] SearchRequest<AccessUserFilterInput> request,
        CancellationToken cancellationToken)
    {
        var result = await _userService.SearchUsersAsync(request.ToSearchRequest(), cancellationToken);
        return OkPaginatedResponse(result);
    }

    [HttpPost("{id:guid}/roles")]
    [RequirePermission(PermissionCodes.AccessControl.UserRoleUpdate)]
    [ProducesResponseType(typeof(ApiResponse<AccessUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddRoles(Guid id, [FromBody] List<Guid> roleIds,
        CancellationToken cancellationToken)
    {
        var result = await _userService.AddRolesToUserAsync(
            (ReplicatedUserId)id,
            roleIds.Select(x => (AccessRoleId)x).ToList(),
            cancellationToken);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}/roles")]
    [RequirePermission(PermissionCodes.AccessControl.UserRoleUpdate)]
    [ProducesResponseType(typeof(ApiResponse<AccessUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveRoles(Guid id, [FromBody] List<Guid> roleIds,
        CancellationToken cancellationToken)
    {
        var result = await _userService.RemoveRolesFromUserAsync(
            (ReplicatedUserId)id,
            roleIds.Select(x => (AccessRoleId)x).ToList(),
            cancellationToken);
        return OkResponse(result);
    }
}
