using Alfred.Core.Application.Brands.Dtos;
using Alfred.Core.Application.Querying.Core;

namespace Alfred.Core.Application.Brands;

public interface IBrandService
{
    Task<PageResult<BrandDto>> GetAllBrandsAsync(QueryRequest query, Guid? categoryId = null,
        CancellationToken cancellationToken = default);

    Task<BrandDto?> GetBrandByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BrandDto> CreateBrandAsync(CreateBrandDto dto, CancellationToken cancellationToken = default);
    Task<BrandDto> UpdateBrandAsync(Guid id, UpdateBrandDto dto, CancellationToken cancellationToken = default);
    Task DeleteBrandAsync(Guid id, CancellationToken cancellationToken = default);
}
