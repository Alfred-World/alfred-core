using Alfred.Core.Application.AccountSales.Dtos;

namespace Alfred.Core.Application.AccountSales.Products;

public interface IProductService
{
    Task<PageResult<ProductDto>> GetProductsAsync(QueryRequest query, CancellationToken cancellationToken = default);

    Task<ProductDto?> GetProductByIdAsync(ProductId id, CancellationToken cancellationToken = default);

    Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default);

    Task<ProductDto> UpdateProductAsync(ProductId id, UpdateProductDto dto,
        CancellationToken cancellationToken = default);
}
