using Alfred.Core.Application.AccountSales;
using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Constants;
using Alfred.Core.WebApi.Contracts.AccountSales;
using Alfred.Core.WebApi.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers.AccountSales;

[Route("api/v{version:apiVersion}/account-sales/source-accounts")]
[Authorize]
public sealed class AccountSalesSourceAccountController : BaseApiController
{
    private readonly ISourceAccountService _service;

    public AccountSalesSourceAccountController(ISourceAccountService service)
    {
        _service = service;
    }

    [HttpGet]
    [RequirePermission(PermissionCodes.AccountSales.SourceAccountRead)]
    [ProducesResponseType(typeof(ApiPagedResponse<SourceAccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSourceAccounts([FromQuery] PaginationQueryParameters query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetSourceAccountsAsync(query.ToQueryRequest(), cancellationToken);
        return OkPaginatedResponse(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionCodes.AccountSales.SourceAccountRead)]
    [ProducesResponseType(typeof(ApiResponse<SourceAccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSourceAccountById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetSourceAccountByIdAsync((SourceAccountId)id, cancellationToken);
        return result is null ? NotFound() : OkResponse(result);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.AccountSales.SourceAccountCreate)]
    [ProducesResponseType(typeof(ApiResponse<SourceAccountDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSourceAccount([FromBody] CreateSourceAccountRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateSourceAccountAsync(request.ToDto(), cancellationToken);
        return CreatedResponse(result);
    }

    [HttpPatch("{id:guid}")]
    [RequirePermission(PermissionCodes.AccountSales.SourceAccountUpdate)]
    [ProducesResponseType(typeof(ApiResponse<SourceAccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSourceAccount(Guid id, [FromBody] UpdateSourceAccountRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateSourceAccountAsync((SourceAccountId)id, request.ToDto(), cancellationToken);
        return OkResponse(result);
    }

    [HttpPatch("{id:guid}/active")]
    [RequirePermission(PermissionCodes.AccountSales.SourceAccountUpdate)]
    [ProducesResponseType(typeof(ApiResponse<SourceAccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetActiveStatus(Guid id, [FromBody] SetSourceAccountActiveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.SetActiveStatusAsync((SourceAccountId)id, request.IsActive, cancellationToken);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(PermissionCodes.AccountSales.SourceAccountDelete)]
    [ProducesResponseType(typeof(ApiResponse<SourceAccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSourceAccount(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteSourceAccountAsync((SourceAccountId)id, cancellationToken);
        return OkResponse(result);
    }
}
