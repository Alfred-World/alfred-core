using Alfred.Core.Application.Brands;
using Alfred.Core.Application.Brands.Dtos;
using Alfred.Core.Domain.Constants;
using Alfred.Core.WebApi.Contracts.Brands;
using Alfred.Core.WebApi.Contracts.Common;
using Alfred.Core.WebApi.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers;

/// <summary>
/// Manages brands / suppliers and their category associations.
/// </summary>
[Route("api/v{version:apiVersion}/brands")]
[Authorize]
public sealed class BrandsController : BaseApiController
{
    private readonly IBrandService _brandService;

    public BrandsController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    /// <summary>
    /// Get paginated list of brands with optional DSL filtering and sorting.
    /// </summary>
    [HttpGet]
    [RequirePermission(PermissionCodes.Brand.Read)]
    [ProducesResponseType(typeof(ApiPagedResponse<BrandDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBrands(
        [FromQuery] PaginationQueryParameters queryRequest,
        [FromQuery] Guid? categoryId,
        CancellationToken cancellationToken)
    {
        var result =
            await _brandService.GetAllBrandsAsync(queryRequest.ToQueryRequest(), (CategoryId?)categoryId,
                cancellationToken);
        return OkPaginatedResponse(result);
    }

    /// <summary>
    /// Get a single brand by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionCodes.Brand.Read)]
    [ProducesResponseType(typeof(ApiResponse<BrandDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBrandById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _brandService.GetBrandByIdAsync((BrandId)id, cancellationToken);
        if (result is null)
        {
            return NotFoundResponse("Brand not found");
        }

        return OkResponse(result);
    }

    /// <summary>
    /// Create a new brand.
    /// </summary>
    [HttpPost]
    [RequirePermission(PermissionCodes.Brand.Create)]
    [ProducesResponseType(typeof(ApiResponse<BrandDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBrand(
        [FromBody] CreateBrandRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _brandService.CreateBrandAsync(request.ToDto(), cancellationToken);
        return CreatedResponse(result);
    }

    /// <summary>
    /// Update an existing brand.
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission(PermissionCodes.Brand.Update)]
    [ProducesResponseType(typeof(ApiResponse<BrandDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBrand(
        Guid id,
        [FromBody] UpdateBrandRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _brandService.UpdateBrandAsync((BrandId)id, request.ToDto(), cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Delete a brand by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(PermissionCodes.Brand.Delete)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBrand(Guid id, CancellationToken cancellationToken)
    {
        await _brandService.DeleteBrandAsync((BrandId)id, cancellationToken);
        return OkResponse("Brand deleted successfully");
    }
}
