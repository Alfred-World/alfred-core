using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Repositories.Base;

using Microsoft.EntityFrameworkCore;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class BrandRepository : BaseRepository<Brand, BrandId>, IBrandRepository
{
    public BrandRepository(IDbContext context) : base(context)
    {
    }

    public async Task<Brand?> GetByIdWithCategoriesAsync(BrandId id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(b => b.BrandCategories)
            .ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }
}
