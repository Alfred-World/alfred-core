using Alfred.Core.Application.Categories.Dtos;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.Categories;

public interface ICategoryService
{
    Task<PageResult<CategoryDto>> GetAllCategoriesAsync(QueryRequest query,
        CancellationToken cancellationToken = default);

    Task<PageResult<CategoryTreeNodeDto>> GetCategoryTreeAsync(CategoryType? type = null, int page = 1,
        int pageSize = 0, CancellationToken cancellationToken = default);

    Task<List<CategoryTreeNodeDto>>
        GetChildrenAsync(CategoryId parentId, CancellationToken cancellationToken = default);

    Task<CategoryDto?> GetCategoryByIdAsync(CategoryId id, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);

    Task<CategoryDto> UpdateCategoryAsync(CategoryId id, UpdateCategoryDto dto,
        CancellationToken cancellationToken = default);

    Task DeleteCategoryAsync(CategoryId id, CancellationToken cancellationToken = default);
    Task<List<CategoryCountByTypeDto>> GetCategoryCountsByTypeAsync(CancellationToken cancellationToken = default);
}
