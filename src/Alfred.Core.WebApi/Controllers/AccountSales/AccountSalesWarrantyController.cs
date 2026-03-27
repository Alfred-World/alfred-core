using Alfred.Core.Application.AccountSales;
using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Constants;
using Alfred.Core.WebApi.Contracts.AccountSales;
using Alfred.Core.WebApi.Contracts.Common;
using Alfred.Core.WebApi.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers.AccountSales;

[Route("api/v{version:apiVersion}/account-sales/warranty")]
[Authorize]
public sealed class AccountSalesWarrantyController : BaseApiController
{
    private readonly IAccountSalesService _service;

    public AccountSalesWarrantyController(IAccountSalesService service)
    {
        _service = service;
    }

    [HttpPost("check")]
    [RequirePermission(PermissionCodes.AccountSales.WarrantyCheck)]
    [ProducesResponseType(typeof(ApiResponse<WarrantyCheckResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckWarranty([FromBody] CheckWarrantyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CheckWarrantyAsync(request.ToDto(), cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("github-users/{username}")]
    [RequirePermission(PermissionCodes.AccountSales.GithubUserRead)]
    [ProducesResponseType(typeof(ApiResponse<GithubUserProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGithubUserProfile(string username, CancellationToken cancellationToken)
    {
        var result = await _service.GetGithubUserProfileAsync(username, cancellationToken);
        return OkResponse(result);
    }
}
