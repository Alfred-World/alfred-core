using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Domain.Abstractions;

public interface IBrandRepository : IRepository<Brand, BrandId>
{
    /// <summary>
    /// Get Brand by id with its BrandCategories and nested Category navigation loaded.
    /// </summary>
    Task<Brand?> GetByIdWithCategoriesAsync(BrandId id, CancellationToken cancellationToken = default);
}
