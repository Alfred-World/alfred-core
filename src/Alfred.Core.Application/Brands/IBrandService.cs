using Alfred.Core.Application.Brands.Dtos;
using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.Brands;

public interface IBrandService
{
    Task<PageResult<BrandDto>> SearchBrandsAsync(SearchRequest request, CategoryId? categoryId = null,
        CancellationToken cancellationToken = default);

    Task<BrandDto?> GetBrandByIdAsync(BrandId id, CancellationToken cancellationToken = default);
    Task<BrandDto> CreateBrandAsync(CreateBrandDto dto, CancellationToken cancellationToken = default);
    Task<BrandDto> UpdateBrandAsync(BrandId id, UpdateBrandDto dto, CancellationToken cancellationToken = default);
    Task DeleteBrandAsync(BrandId id, CancellationToken cancellationToken = default);
}
