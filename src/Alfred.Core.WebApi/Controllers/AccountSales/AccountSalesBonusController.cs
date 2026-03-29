using Alfred.Core.Application.AccountSales;
using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Constants;
using Alfred.Core.Domain.Enums;
using Alfred.Core.WebApi.Contracts.AccountSales;
using Alfred.Core.WebApi.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers.AccountSales;

[Route("api/v{version:apiVersion}/account-sales/bonus")]
[Authorize]
public sealed class AccountSalesBonusController : BaseApiController
{
    private readonly IAccountSalesService _service;

    public AccountSalesBonusController(IAccountSalesService service)
    {
        _service = service;
    }

    // ========== Bonus Tiers (Admin Config) ==========

    /// <summary>
    /// Get all bonus tier configurations.
    /// </summary>
    [HttpGet("tiers")]
    [RequirePermission(PermissionCodes.AccountSales.BonusTierRead)]
    [ProducesResponseType(typeof(ApiResponse<List<SalesBonusTierDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBonusTiers(CancellationToken cancellationToken)
    {
        var result = await _service.GetBonusTiersAsync(cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Create a new bonus tier milestone.
    /// </summary>
    [HttpPost("tiers")]
    [RequirePermission(PermissionCodes.AccountSales.BonusTierCreate)]
    [ProducesResponseType(typeof(ApiResponse<SalesBonusTierDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateBonusTier([FromBody] CreateSalesBonusTierRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateBonusTierAsync(
            new CreateSalesBonusTierDto(request.OrderThreshold, request.BonusAmount),
            cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Update an existing bonus tier.
    /// </summary>
    [HttpPut("tiers/{tierId:guid}")]
    [RequirePermission(PermissionCodes.AccountSales.BonusTierUpdate)]
    [ProducesResponseType(typeof(ApiResponse<SalesBonusTierDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateBonusTier(Guid tierId, [FromBody] UpdateSalesBonusTierRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateBonusTierAsync((SalesBonusTierId)tierId,
            new UpdateSalesBonusTierDto(request.OrderThreshold, request.BonusAmount, request.IsActive),
            cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Delete a bonus tier (hard delete).
    /// </summary>
    [HttpDelete("tiers/{tierId:guid}")]
    [RequirePermission(PermissionCodes.AccountSales.BonusTierDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteBonusTier(Guid tierId, CancellationToken cancellationToken)
    {
        await _service.DeleteBonusTierAsync((SalesBonusTierId)tierId, cancellationToken);
        return NoContent();
    }

    // ========== Seller Monthly Summaries ==========

    /// <summary>
    /// Get all monthly sales summaries for a seller (history across all months).
    /// </summary>
    [HttpGet("summaries/{soldByMemberId:guid}")]
    [RequirePermission(PermissionCodes.AccountSales.BonusSummaryRead)]
    [ProducesResponseType(typeof(ApiResponse<List<MemberMonthlySalesSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSellerMonthlySummaries(Guid soldByMemberId, CancellationToken cancellationToken)
    {
        var result = await _service.GetSellerMonthlySummariesAsync(new MemberId(soldByMemberId), cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Get the monthly sales summary for a referrer member for a specific month.
    /// </summary>
    [HttpGet("summaries/{soldByMemberId:guid}/{year:int}/{month:int}")]
    [RequirePermission(PermissionCodes.AccountSales.BonusSummaryRead)]
    [ProducesResponseType(typeof(ApiResponse<MemberMonthlySalesSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSellerMonthlySummary(Guid soldByMemberId, int year, int month,
        CancellationToken cancellationToken)
    {
        var result =
            await _service.GetSellerMonthlySummaryAsync(new MemberId(soldByMemberId), year, month, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return OkResponse(result);
    }

    // ========== Bonus Transactions ==========

    /// <summary>
    /// Get all bonus transactions for a referrer member.
    /// </summary>
    [HttpGet("transactions/{soldByMemberId:guid}")]
    [RequirePermission(PermissionCodes.AccountSales.BonusTransactionRead)]
    [ProducesResponseType(typeof(ApiResponse<List<SalesBonusTransactionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBonusTransactions(Guid soldByMemberId, CancellationToken cancellationToken)
    {
        var result = await _service.GetBonusTransactionsAsync(new MemberId(soldByMemberId), cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Get all bonus transactions across all sellers (admin). Filter by year, month, or status.
    /// </summary>
    [HttpGet("transactions")]
    [RequirePermission(PermissionCodes.AccountSales.BonusTransactionRead)]
    [ProducesResponseType(typeof(ApiResponse<List<SalesBonusTransactionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllBonusTransactions(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] SalesBonusTransactionStatus? status,
        [FromQuery] Guid? soldByMemberId,
        CancellationToken cancellationToken)
    {
        var memberId = soldByMemberId.HasValue ? new MemberId(soldByMemberId.Value) : (MemberId?)null;
        var result = await _service.GetAllBonusTransactionsAsync(year, month, status, memberId, cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Get current-month bonus progress for a seller (order count vs tier thresholds).
    /// </summary>
    [HttpGet("progress/{soldByMemberId:guid}")]
    [RequirePermission(PermissionCodes.AccountSales.BonusSummaryProgressRead)]
    [ProducesResponseType(typeof(ApiResponse<SellerBonusProgressDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSellerBonusProgress(Guid soldByMemberId, CancellationToken cancellationToken)
    {
        var result = await _service.GetSellerBonusProgressAsync(new MemberId(soldByMemberId), cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Mark a pending bonus transaction as paid. Admin only.
    /// </summary>
    [HttpPost("transactions/{transactionId:guid}/pay")]
    [RequirePermission(PermissionCodes.AccountSales.BonusTransactionPay)]
    [ProducesResponseType(typeof(ApiResponse<SalesBonusTransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkBonusPaid(Guid transactionId, [FromBody] MarkBonusPaidRequest request,
        CancellationToken cancellationToken)
    {
        var userId = TryGetCurrentUserId();
        var result = await _service.MarkBonusTransactionPaidAsync((SalesBonusTransactionId)transactionId,
            (ReplicatedUserId?)userId, request.Note,
            cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Cancel a pending bonus transaction. Admin only.
    /// </summary>
    [HttpPost("transactions/{transactionId:guid}/cancel")]
    [RequirePermission(PermissionCodes.AccountSales.BonusTransactionCancel)]
    [ProducesResponseType(typeof(ApiResponse<SalesBonusTransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelBonusTransaction(Guid transactionId,
        [FromBody] CancelBonusTransactionRequest request, CancellationToken cancellationToken)
    {
        var userId = TryGetCurrentUserId();
        var result = await _service.CancelBonusTransactionAsync((SalesBonusTransactionId)transactionId,
            (ReplicatedUserId?)userId, request.Note, cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Manually settle (create + pay) a bonus tier for a member for the current month. 
    /// Creates a Paid transaction if none exists, or marks an existing Pending one as Paid.
    /// </summary>
    [HttpPost("transactions/settle-tier")]
    [RequirePermission(PermissionCodes.AccountSales.BonusTransactionPay)]
    [ProducesResponseType(typeof(ApiResponse<SalesBonusTransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SettleBonusTier([FromBody] SettleBonusTierRequest request,
        CancellationToken cancellationToken)
    {
        var userId = TryGetCurrentUserId();
        var result = await _service.SettleBonusTierAsync(
            (MemberId)request.SoldByMemberId,
            (SalesBonusTierId)request.TierId,
            (ReplicatedUserId?)userId,
            request.Note,
            cancellationToken);
        return OkResponse(result);
    }
}
