using Alfred.Core.Application.AccountSales;
using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Constants;
using Alfred.Core.WebApi.Contracts.AccountSales;
using Alfred.Core.WebApi.Contracts.Common;
using Alfred.Core.WebApi.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alfred.Core.WebApi.Controllers;

[Route("api/v{version:apiVersion}/account-sales/products")]
[Authorize]
public sealed class AccountSalesProductController : BaseApiController
{
    private readonly IAccountSalesService _service;

    public AccountSalesProductController(IAccountSalesService service)
    {
        _service = service;
    }

    [HttpGet]
    [RequirePermission(PermissionCodes.AccountSales.ProductRead)]
    [ProducesResponseType(typeof(ApiPagedResponse<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] PaginationQueryParameters query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetProductsAsync(query.ToQueryRequest(), cancellationToken);
        return OkPaginatedResponse(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionCodes.AccountSales.ProductRead)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetProductByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFoundResponse("Product not found");
        }

        return OkResponse(result);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.AccountSales.ProductCreate)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateProductAsync(request.ToDto(), cancellationToken);
        return CreatedResponse(result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission(PermissionCodes.AccountSales.ProductUpdate)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateProductAsync(id, request.ToDto(), cancellationToken);
        return OkResponse(result);
    }
}
