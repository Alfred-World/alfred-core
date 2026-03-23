using Alfred.Core.Application.Commodities;
using Alfred.Core.Application.Commodities.Dtos;
using Alfred.Core.Domain.Constants;
using Alfred.Core.WebApi.Contracts.Commodities;
using Alfred.Core.WebApi.Contracts.Common;
using Alfred.Core.WebApi.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers;

/// <summary>
/// Manages commodities (investment assets) and their transactions.
/// </summary>
[Route("api/v{version:apiVersion}/commodities")]
[Authorize]
public sealed class CommoditiesController : BaseApiController
{
    private readonly ICommodityService _commodityService;

    public CommoditiesController(ICommodityService commodityService)
    {
        _commodityService = commodityService;
    }

    #region Commodities

    /// <summary>
    /// Get paginated list of commodities with optional DSL filtering and sorting.
    /// </summary>
    [HttpGet]
    [RequirePermission(PermissionCodes.Commodity.Read)]
    [ProducesResponseType(typeof(ApiPagedResponse<CommodityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCommodities(
        [FromQuery] PaginationQueryParameters queryRequest,
        CancellationToken cancellationToken)
    {
        var result = await _commodityService.GetAllCommoditiesAsync(queryRequest.ToQueryRequest(), cancellationToken);
        return OkPaginatedResponse(result);
    }

    /// <summary>
    /// Get a single commodity by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionCodes.Commodity.Read)]
    [ProducesResponseType(typeof(ApiResponse<CommodityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommodityById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _commodityService.GetCommodityByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFoundResponse("Commodity not found");
        }

        return OkResponse(result);
    }

    /// <summary>
    /// Create a new commodity.
    /// </summary>
    [HttpPost]
    [RequirePermission(PermissionCodes.Commodity.Create)]
    [ProducesResponseType(typeof(ApiResponse<CommodityDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCommodity(
        [FromBody] CreateCommodityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _commodityService.CreateCommodityAsync(request.ToDto(), cancellationToken);
        return CreatedResponse(result);
    }

    /// <summary>
    /// Update an existing commodity.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(PermissionCodes.Commodity.Update)]
    [ProducesResponseType(typeof(ApiResponse<CommodityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCommodity(
        Guid id,
        [FromBody] UpdateCommodityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _commodityService.UpdateCommodityAsync(id, request.ToDto(), cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Delete a commodity by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(PermissionCodes.Commodity.Delete)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCommodity(Guid id, CancellationToken cancellationToken)
    {
        await _commodityService.DeleteCommodityAsync(id, cancellationToken);
        return OkResponse("Commodity deleted successfully");
    }

    #endregion

    #region Investment Transactions

    /// <summary>
    /// Get paginated list of investment transactions for a commodity.
    /// </summary>
    [HttpGet("{commodityId:guid}/transactions")]
    [RequirePermission(PermissionCodes.InvestmentTransaction.Read)]
    [ProducesResponseType(typeof(ApiPagedResponse<InvestmentTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTransactions(
        Guid commodityId,
        [FromQuery] PaginationQueryParameters queryRequest,
        CancellationToken cancellationToken)
    {
        var result =
            await _commodityService.GetTransactionsAsync(commodityId, queryRequest.ToQueryRequest(), cancellationToken);
        return OkPaginatedResponse(result);
    }

    /// <summary>
    /// Get a single investment transaction by ID.
    /// </summary>
    [HttpGet("{commodityId:guid}/transactions/{transactionId:guid}")]
    [RequirePermission(PermissionCodes.InvestmentTransaction.Read)]
    [ProducesResponseType(typeof(ApiResponse<InvestmentTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactionById(Guid commodityId, Guid transactionId,
        CancellationToken cancellationToken)
    {
        var result = await _commodityService.GetTransactionByIdAsync(transactionId, cancellationToken);
        if (result is null)
        {
            return NotFoundResponse("Transaction not found");
        }

        return OkResponse(result);
    }

    /// <summary>
    /// Create a new investment transaction for a commodity.
    /// </summary>
    [HttpPost("{commodityId:guid}/transactions")]
    [RequirePermission(PermissionCodes.InvestmentTransaction.Create)]
    [ProducesResponseType(typeof(ApiResponse<InvestmentTransactionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTransaction(
        Guid commodityId,
        [FromBody] CreateInvestmentTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _commodityService.CreateTransactionAsync(commodityId, request.ToDto(), cancellationToken);
        return CreatedResponse(result);
    }

    /// <summary>
    /// Delete an investment transaction by ID.
    /// </summary>
    [HttpDelete("{commodityId:guid}/transactions/{transactionId:guid}")]
    [RequirePermission(PermissionCodes.InvestmentTransaction.Delete)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTransaction(Guid commodityId, Guid transactionId,
        CancellationToken cancellationToken)
    {
        await _commodityService.DeleteTransactionAsync(transactionId, cancellationToken);
        return OkResponse("Transaction deleted successfully");
    }

    #endregion
}
