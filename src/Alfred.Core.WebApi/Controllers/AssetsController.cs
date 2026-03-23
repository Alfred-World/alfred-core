using Alfred.Core.Application.Assets;
using Alfred.Core.Application.Assets.Dtos;
using Alfred.Core.Domain.Constants;
using Alfred.Core.WebApi.Contracts.Assets;
using Alfred.Core.WebApi.Contracts.Common;
using Alfred.Core.WebApi.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers;

/// <summary>
/// Manages physical assets and their operational logs.
/// </summary>
[Route("api/v{version:apiVersion}/assets")]
[Authorize]
public sealed class AssetsController : BaseApiController
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    #region Assets

    /// <summary>
    /// Get paginated list of assets with optional DSL filtering and sorting.
    /// </summary>
    [HttpGet]
    [RequirePermission(PermissionCodes.Asset.Read)]
    [ProducesResponseType(typeof(ApiPagedResponse<AssetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAssets(
        [FromQuery] PaginationQueryParameters queryRequest,
        CancellationToken cancellationToken)
    {
        var result = await _assetService.GetAllAssetsAsync(queryRequest.ToQueryRequest(), cancellationToken);
        return OkPaginatedResponse(result);
    }

    /// <summary>
    /// Get a single asset by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionCodes.Asset.Read)]
    [ProducesResponseType(typeof(ApiResponse<AssetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _assetService.GetAssetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFoundResponse("Asset not found");
        }

        return OkResponse(result);
    }

    /// <summary>
    /// Create a new asset.
    /// </summary>
    [HttpPost]
    [RequirePermission(PermissionCodes.Asset.Create)]
    [ProducesResponseType(typeof(ApiResponse<AssetDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsset(
        [FromBody] CreateAssetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _assetService.CreateAssetAsync(request.ToDto(), cancellationToken);
        return CreatedResponse(result);
    }

    /// <summary>
    /// Update an existing asset.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(PermissionCodes.Asset.Update)]
    [ProducesResponseType(typeof(ApiResponse<AssetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsset(
        Guid id,
        [FromBody] UpdateAssetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _assetService.UpdateAssetAsync(id, request.ToDto(), cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Delete an asset by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(PermissionCodes.Asset.Delete)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsset(Guid id, CancellationToken cancellationToken)
    {
        await _assetService.DeleteAssetAsync(id, cancellationToken);
        return OkResponse("Asset deleted successfully");
    }

    #endregion

    #region Asset Logs

    /// <summary>
    /// Get paginated operational logs for a specific asset.
    /// </summary>
    [HttpGet("{assetId:guid}/logs")]
    [RequirePermission(PermissionCodes.AssetLog.Read)]
    [ProducesResponseType(typeof(ApiPagedResponse<AssetLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAssetLogs(
        Guid assetId,
        [FromQuery] PaginationQueryParameters queryRequest,
        CancellationToken cancellationToken)
    {
        var result =
            await _assetService.GetAssetLogsAsync(assetId, queryRequest.ToQueryRequest(), cancellationToken);
        return OkPaginatedResponse(result);
    }

    /// <summary>
    /// Get a single asset log entry by ID.
    /// </summary>
    [HttpGet("{assetId:guid}/logs/{logId:guid}")]
    [RequirePermission(PermissionCodes.AssetLog.Read)]
    [ProducesResponseType(typeof(ApiResponse<AssetLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssetLogById(
        Guid assetId,
        Guid logId,
        CancellationToken cancellationToken)
    {
        var result = await _assetService.GetAssetLogByIdAsync(logId, cancellationToken);
        if (result is null)
        {
            return NotFoundResponse("Asset log not found");
        }

        return OkResponse(result);
    }

    /// <summary>
    /// Record a new operational log (Refill, Repair, Maintain) for an asset.
    /// </summary>
    [HttpPost("{assetId:guid}/logs")]
    [RequirePermission(PermissionCodes.AssetLog.Create)]
    [ProducesResponseType(typeof(ApiResponse<AssetLogDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAssetLog(
        Guid assetId,
        [FromBody] CreateAssetLogRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _assetService.CreateAssetLogAsync(assetId, request.ToDto(), cancellationToken);
        return CreatedResponse(result);
    }

    /// <summary>
    /// Delete an asset log entry.
    /// </summary>
    [HttpDelete("{assetId:guid}/logs/{logId:guid}")]
    [RequirePermission(PermissionCodes.AssetLog.Delete)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAssetLog(
        Guid assetId,
        Guid logId,
        CancellationToken cancellationToken)
    {
        await _assetService.DeleteAssetLogAsync(logId, cancellationToken);
        return OkResponse("Asset log deleted successfully");
    }

    #endregion
}
