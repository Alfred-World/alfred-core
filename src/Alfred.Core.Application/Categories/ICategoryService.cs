using Alfred.Core.Application.Categories.Dtos;
using Alfred.Core.Application.Querying.Core;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.Categories;

public interface ICategoryService
{
    Task<PageResult<CategoryDto>> GetAllCategoriesAsync(QueryRequest query,
        CancellationToken cancellationToken = default);

    Task<PageResult<CategoryTreeNodeDto>> GetCategoryTreeAsync(CategoryType? type = null, int page = 1,
        int pageSize = 0, CancellationToken cancellationToken = default);

    Task<List<CategoryTreeNodeDto>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);

    Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto,
        CancellationToken cancellationToken = default);

    Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<CategoryCountByTypeDto>> GetCategoryCountsByTypeAsync(CancellationToken cancellationToken = default);
}
