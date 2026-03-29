using Alfred.Core.Application.AccountSales;
using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Constants;
using Alfred.Core.WebApi.Contracts.AccountSales;
using Alfred.Core.WebApi.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers.AccountSales;

[Route("api/v{version:apiVersion}/account-sales/account-clones")]
[Authorize]
public sealed class AccountSalesAccountCloneController : BaseApiController
{
    private readonly IAccountSalesService _service;

    public AccountSalesAccountCloneController(IAccountSalesService service)
    {
        _service = service;
    }

    [HttpGet]
    [RequirePermission(PermissionCodes.AccountSales.AccountCloneRead)]
    [ProducesResponseType(typeof(ApiPagedResponse<AccountCloneDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountClones([FromQuery] PaginationQueryParameters query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAccountClonesAsync(query.ToQueryRequest(), cancellationToken);
        return OkPaginatedResponse(result);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.AccountSales.AccountCloneCreate)]
    [ProducesResponseType(typeof(ApiResponse<AccountCloneDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddAccountClone([FromBody] CreateAccountCloneRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.AddAccountCloneAsync(request.ToDto(), cancellationToken);
        return CreatedResponse(result);
    }

    [HttpPut("{accountCloneId:guid}")]
    [RequirePermission(PermissionCodes.AccountSales.AccountCloneStatusUpdate)]
    [ProducesResponseType(typeof(ApiResponse<AccountCloneDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAccountClone(Guid accountCloneId,
        [FromBody] UpdateAccountCloneRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _service.UpdateAccountCloneAsync((AccountCloneId)accountCloneId, request.ToDto(), cancellationToken);
        return OkResponse(result);
    }

    [HttpPost("{accountCloneId:guid}/review")]
    [RequirePermission(PermissionCodes.AccountSales.AccountCloneReview)]
    [ProducesResponseType(typeof(ApiResponse<AccountCloneDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReviewAccountClone(Guid accountCloneId,
        [FromBody] ReviewAccountCloneRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _service.ReviewAccountCloneAsync((AccountCloneId)accountCloneId, request.ToDto(), cancellationToken);
        return OkResponse(result);
    }

    [HttpPut("{accountCloneId:guid}/status")]
    [RequirePermission(PermissionCodes.AccountSales.AccountCloneStatusUpdate)]
    [ProducesResponseType(typeof(ApiResponse<AccountCloneDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAccountCloneStatus(Guid accountCloneId,
        [FromBody] UpdateAccountCloneStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _service.UpdateAccountCloneStatusAsync((AccountCloneId)accountCloneId, request.ToDto(),
                cancellationToken);
        return OkResponse(result);
    }
}
