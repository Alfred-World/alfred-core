using Alfred.Core.Application.Units;
using Alfred.Core.Application.Units.Dtos;
using Alfred.Core.Domain.Enums;
using Alfred.Core.WebApi.Contracts.Common;
using Alfred.Core.WebApi.Contracts.Units;

using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers;

/// <summary>
/// Manages measurement units with conversion support and base unit hierarchies.
/// </summary>
[Route("api/v{version:apiVersion}/units")]
public sealed class UnitsController : BaseApiController
{
    private readonly IUnitService _unitService;

    public UnitsController(IUnitService unitService)
    {
        _unitService = unitService;
    }

    /// <summary>
    /// Get paginated list of units with optional DSL filtering and sorting.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiPagedResponse<UnitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUnits(
        [FromQuery] PaginationQueryParameters queryRequest,
        CancellationToken cancellationToken)
    {
        var result = await _unitService.GetAllUnitsAsync(queryRequest.ToQueryRequest(), cancellationToken);
        return OkPaginatedResponse(result);
    }

    /// <summary>
    /// Get a single unit by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UnitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUnitById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _unitService.GetUnitByIdAsync(id, cancellationToken);

        if (result is null)
        {
            return NotFoundResponse("Unit not found");
        }

        return OkResponse(result);
    }

    /// <summary>
    /// Create a new unit.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UnitDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUnit(
        [FromBody] CreateUnitRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _unitService.CreateUnitAsync(request.ToDto(), cancellationToken);
        return CreatedResponse(result);
    }

    /// <summary>
    /// Update an existing unit.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UnitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUnit(
        Guid id,
        [FromBody] UpdateUnitRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _unitService.UpdateUnitAsync(id, request.ToDto(), cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Delete a unit by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUnit(Guid id, CancellationToken cancellationToken)
    {
        await _unitService.DeleteUnitAsync(id, cancellationToken);
        return OkResponse("Unit deleted successfully");
    }

    /// <summary>
    /// Get base unit tree with derived units nested, optionally filtered by category.
    /// </summary>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(ApiResponse<List<UnitTreeNodeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBaseUnitTree(
        [FromQuery] UnitCategory? category,
        CancellationToken cancellationToken)
    {
        var result = await _unitService.GetBaseUnitTreeAsync(category, cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Get the count of units grouped by status.
    /// </summary>
    [HttpGet("counts-by-status")]
    [ProducesResponseType(typeof(ApiResponse<List<UnitCountByStatusDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCountsByStatus(CancellationToken cancellationToken)
    {
        var result = await _unitService.GetCountsByStatusAsync(cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Get the count of units grouped by category.
    /// </summary>
    [HttpGet("counts-by-category")]
    [ProducesResponseType(typeof(ApiResponse<List<UnitCountByCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCountsByCategory(CancellationToken cancellationToken)
    {
        var result = await _unitService.GetCountsByCategoryAsync(cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Convert a value from one unit to another.
    /// </summary>
    [HttpGet("convert")]
    [ProducesResponseType(typeof(ApiResponse<ConvertResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Convert(
        [FromQuery] Guid fromUnitId,
        [FromQuery] Guid toUnitId,
        [FromQuery] decimal value,
        CancellationToken cancellationToken)
    {
        var result = await _unitService.ConvertAsync(fromUnitId, toUnitId, value, cancellationToken);
        return OkResponse(result);
    }
}
