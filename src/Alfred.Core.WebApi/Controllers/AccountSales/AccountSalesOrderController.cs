using Alfred.Core.Application.AccountSales;
using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Constants;
using Alfred.Core.WebApi.Contracts.AccountSales;
using Alfred.Core.WebApi.Contracts.Common;
using Alfred.Core.WebApi.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers.AccountSales;

[Route("api/v{version:apiVersion}/account-sales/orders")]
[Authorize]
public sealed class AccountSalesOrderController : BaseApiController
{
    private readonly IAccountSalesService _service;

    public AccountSalesOrderController(IAccountSalesService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get paginated order list with full filter/sort/view/projection support.
    /// Filter example: memberId == '019d19ea-f221-760c-9ca2-3fb4c58230e5'
    /// Views: list (default), detail
    /// </summary>
    [HttpGet]
    [RequirePermission(PermissionCodes.AccountSales.OrderRead)]
    [ProducesResponseType(typeof(ApiPagedResponse<AccountOrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders([FromQuery] PaginationQueryParameters query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetOrdersAsync(query.ToQueryRequest(), cancellationToken);
        return OkPaginatedResponse(result);
    }

    [HttpPost("sell")]
    [RequirePermission(PermissionCodes.AccountSales.OrderSell)]
    [ProducesResponseType(typeof(ApiResponse<SellAccountResultDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> SellAccount([FromBody] CreateAccountOrderRequest request,
        CancellationToken cancellationToken)
    {
        var sellerUserId = TryGetCurrentUserId();
        var result =
            await _service.SellAccountAsync(request.ToDto(), (ReplicatedUserId?)sellerUserId, cancellationToken);
        return CreatedResponse(result);
    }

    [HttpPost("{orderId:guid}/replace")]
    [RequirePermission(PermissionCodes.AccountSales.OrderReplace)]
    [ProducesResponseType(typeof(ApiResponse<SellAccountResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReplaceAccount(Guid orderId, [FromBody] ReplaceAccountOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _service.ReplaceAccountForWarrantyAsync((AccountOrderId)orderId, request.ToDto(), cancellationToken);
        return OkResponse(result);
    }

    [HttpGet("revenue-by-seller")]
    [RequirePermission(PermissionCodes.AccountSales.RevenueRead)]
    [ProducesResponseType(typeof(ApiResponse<List<SellerRevenueDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueBySeller(CancellationToken cancellationToken)
    {
        var result = await _service.GetRevenueBySellerAsync(cancellationToken);
        return OkResponse(result);
    }

    [HttpPost("{orderId:guid}/confirm-payment")]
    [RequirePermission(PermissionCodes.AccountSales.OrderPaymentConfirm)]
    [ProducesResponseType(typeof(ApiResponse<AccountOrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfirmPayment(Guid orderId, CancellationToken cancellationToken)
    {
        var userId = TryGetCurrentUserId();
        var result =
            await _service.ConfirmOrderPaymentAsync((AccountOrderId)orderId, (ReplicatedUserId?)userId,
                cancellationToken);
        return OkResponse(result);
    }

    [HttpPost("{orderId:guid}/refund")]
    [RequirePermission(PermissionCodes.AccountSales.OrderRefund)]
    [ProducesResponseType(typeof(ApiResponse<RefundResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefundOrder(Guid orderId, [FromBody] RefundOrderRequest request,
        CancellationToken cancellationToken)
    {
        var userId = TryGetCurrentUserId();
        var dto = new RefundOrderDto((AccountOrderId)orderId, request.RefundAmount, request.Note);
        var result = await _service.RefundOrderAsync(dto, (ReplicatedUserId?)userId, cancellationToken);
        return OkResponse(result);
    }
}
