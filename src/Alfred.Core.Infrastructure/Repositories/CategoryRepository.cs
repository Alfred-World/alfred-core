using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Repositories.Base;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class CategoryRepository : BaseRepository<Category, CategoryId>, ICategoryRepository
{
    public CategoryRepository(IDbContext context) : base(context)
    {
    }
}
