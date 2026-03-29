using Alfred.Core.Application.Brands.Dtos;

namespace Alfred.Core.Application.Brands;

public interface IBrandService
{
    Task<PageResult<BrandDto>> GetAllBrandsAsync(QueryRequest query, CategoryId? categoryId = null,
        CancellationToken cancellationToken = default);

    Task<BrandDto?> GetBrandByIdAsync(BrandId id, CancellationToken cancellationToken = default);
    Task<BrandDto> CreateBrandAsync(CreateBrandDto dto, CancellationToken cancellationToken = default);
    Task<BrandDto> UpdateBrandAsync(BrandId id, UpdateBrandDto dto, CancellationToken cancellationToken = default);
    Task DeleteBrandAsync(BrandId id, CancellationToken cancellationToken = default);
}
