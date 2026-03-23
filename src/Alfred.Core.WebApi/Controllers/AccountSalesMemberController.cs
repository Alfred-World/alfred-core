using Alfred.Core.Application.AccountSales;
using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Constants;
using Alfred.Core.WebApi.Contracts.AccountSales;
using Alfred.Core.WebApi.Contracts.Common;
using Alfred.Core.WebApi.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers;

[Route("api/v{version:apiVersion}/account-sales/members")]
[Authorize]
public sealed class AccountSalesMemberController : BaseApiController
{
    private readonly IAccountSalesService _service;

    public AccountSalesMemberController(IAccountSalesService service)
    {
        _service = service;
    }

    [HttpGet]
    [RequirePermission(PermissionCodes.AccountSales.MemberRead)]
    [ProducesResponseType(typeof(ApiPagedResponse<MemberDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembers([FromQuery] PaginationQueryParameters query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetMembersAsync(query.ToQueryRequest(), cancellationToken);
        return OkPaginatedResponse(result);
    }

    [HttpGet("search")]
    [RequirePermission(PermissionCodes.AccountSales.MemberRead)]
    [ProducesResponseType(typeof(ApiResponse<List<MemberDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchMembers([FromQuery] string keyword, [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        var result = await _service.SearchMembersAsync(keyword, take <= 0 ? 20 : take, cancellationToken);
        return OkResponse(result);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.AccountSales.MemberCreate)]
    [ProducesResponseType(typeof(ApiResponse<MemberDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateMember([FromBody] CreateMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateMemberAsync(request.ToDto(), cancellationToken);
        return CreatedResponse(result);
    }
}
