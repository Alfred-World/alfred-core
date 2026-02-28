using Alfred.Core.Application.Categories.Dtos;
using Alfred.Core.Application.Querying.Core;

namespace Alfred.Core.Application.Categories;

public interface ICategoryService
{
    Task<PageResult<CategoryDto>> GetAllCategoriesAsync(QueryRequest query, CancellationToken cancellationToken = default);
    Task<List<CategoryTreeNodeDto>> GetCategoryTreeAsync(string? type = null, CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);
}
