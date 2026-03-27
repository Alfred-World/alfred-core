using Alfred.Core.Application.AccountSales;
using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Constants;
using Alfred.Core.WebApi.Contracts.AccountSales;
using Alfred.Core.WebApi.Contracts.Common;
using Alfred.Core.WebApi.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers.AccountSales;

[Route("api/v{version:apiVersion}/account-sales/commissions")]
[Authorize]
public sealed class AccountSalesCommissionController : BaseApiController
{
    private readonly IAccountSalesService _service;

    public AccountSalesCommissionController(IAccountSalesService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all member commission balances.
    /// </summary>
    [HttpGet]
    [RequirePermission(PermissionCodes.AccountSales.CommissionRead)]
    [ProducesResponseType(typeof(ApiResponse<List<CommissionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCommissions(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllCommissionsAsync(cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Get commission balance for a specific member.
    /// </summary>
    [HttpGet("{memberId:guid}")]
    [RequirePermission(PermissionCodes.AccountSales.CommissionRead)]
    [ProducesResponseType(typeof(ApiResponse<CommissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMemberCommission(Guid memberId, CancellationToken cancellationToken)
    {
        var result = await _service.GetMemberCommissionAsync((MemberId)memberId, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return OkResponse(result);
    }

    /// <summary>
    /// Get commission transaction history for a member.
    /// </summary>
    [HttpGet("{memberId:guid}/transactions")]
    [RequirePermission(PermissionCodes.AccountSales.CommissionTransactionRead)]
    [ProducesResponseType(typeof(ApiResponse<List<CommissionTransactionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCommissionTransactions(Guid memberId, CancellationToken cancellationToken)
    {
        var result = await _service.GetCommissionTransactionsAsync((MemberId)memberId, cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Pay out the available commission balance to a member. Admin only.
    /// </summary>
    [HttpPost("payout")]
    [RequirePermission(PermissionCodes.AccountSales.CommissionPayout)]
    [ProducesResponseType(typeof(ApiResponse<PayoutResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> PayoutCommission([FromBody] PayoutCommissionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = TryGetCurrentUserId();
        var result =
            await _service.PayoutCommissionAsync(request.ToDto(), (ReplicatedUserId?)userId, cancellationToken);
        return OkResponse(result);
    }
}
