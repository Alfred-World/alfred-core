using Alfred.Core.Application.Categories;
using Alfred.Core.Application.Categories.Dtos;
using Alfred.Core.WebApi.Contracts.Categories;
using Alfred.Core.WebApi.Contracts.Common;

using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers;

/// <summary>
/// Manages categories with hierarchical tree support and dynamic form schemas.
/// </summary>
[Route("api/v{version:apiVersion}/categories")]
public sealed class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get paginated list of categories with optional DSL filtering and sorting.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiPagedResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCategories(
        [FromQuery] PaginationQueryParameters queryRequest,
        CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetAllCategoriesAsync(queryRequest.ToQueryRequest(), cancellationToken);
        return OkPaginatedResponse(result);
    }

    /// <summary>
    /// Get the category tree structure, optionally filtered by type (Asset, Brand, Consumable).
    /// </summary>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(ApiResponse<List<CategoryTreeNodeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoryTree(
        [FromQuery] string? type,
        CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetCategoryTreeAsync(type, cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Get a single category by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFoundResponse("Category not found");
        }

        return OkResponse(result);
    }

    /// <summary>
    /// Create a new category.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _categoryService.CreateCategoryAsync(request.ToDto(), cancellationToken);
        return CreatedResponse(result);
    }

    /// <summary>
    /// Update an existing category.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, request.ToDto(), cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Delete a category by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        await _categoryService.DeleteCategoryAsync(id, cancellationToken);
        return OkResponse("Category deleted successfully");
    }
}
