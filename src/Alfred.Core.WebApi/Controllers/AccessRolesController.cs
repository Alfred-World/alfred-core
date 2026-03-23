using Alfred.Core.Application.AccessControl;
using Alfred.Core.Application.AccessControl.Dtos;
using Alfred.Core.Domain.Constants;
using Alfred.Core.WebApi.Contracts.AccessControl;
using Alfred.Core.WebApi.Contracts.Common;
using Alfred.Core.WebApi.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers;

[Route("api/v{version:apiVersion}/access-control/roles")]
[Authorize]
public sealed class AccessRolesController : BaseApiController
{
    private readonly IAccessRoleService _roleService;

    public AccessRolesController(IAccessRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [RequirePermission(PermissionCodes.AccessControl.RoleRead)]
    [ProducesResponseType(typeof(ApiPagedResponse<AccessRoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles([FromQuery] PaginationQueryParameters query,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.GetAllRolesAsync(query.ToQueryRequest(), cancellationToken);
        return OkPaginatedResponse(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionCodes.AccessControl.RoleRead)]
    [ProducesResponseType(typeof(ApiResponse<AccessRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roleService.GetRoleByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFoundResponse("Role not found");
        }

        return OkResponse(result);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.AccessControl.RoleCreate)]
    [ProducesResponseType(typeof(ApiResponse<AccessRoleDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRole([FromBody] CreateAccessRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.CreateRoleAsync(request.ToDto(), cancellationToken);
        return CreatedResponse(result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission(PermissionCodes.AccessControl.RoleUpdate)]
    [ProducesResponseType(typeof(ApiResponse<AccessRoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateAccessRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.UpdateRoleAsync(id, request.ToDto(), cancellationToken);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(PermissionCodes.AccessControl.RoleDelete)]
    [ProducesResponseType(typeof(ApiResponse<AccessRoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteRole(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roleService.DeleteRoleAsync(id, cancellationToken);
        return OkResponse(result);
    }

    [HttpPost("{id:guid}/permissions")]
    [RequirePermission(PermissionCodes.AccessControl.RolePermissionUpdate)]
    [ProducesResponseType(typeof(ApiResponse<AccessRoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddPermissions(Guid id, [FromBody] List<Guid> permissionIds,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.AddPermissionsToRoleAsync(id, permissionIds, cancellationToken);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}/permissions")]
    [RequirePermission(PermissionCodes.AccessControl.RolePermissionUpdate)]
    [ProducesResponseType(typeof(ApiResponse<AccessRoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemovePermissions(Guid id, [FromBody] List<Guid> permissionIds,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.RemovePermissionsFromRoleAsync(id, permissionIds, cancellationToken);
        return OkResponse(result);
    }
}
