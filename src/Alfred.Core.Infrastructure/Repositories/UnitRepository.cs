using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Repositories.Base;

using Microsoft.EntityFrameworkCore;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class UnitRepository : BaseRepository<Unit, UnitId>, IUnitRepository
{
    public UnitRepository(IDbContext context) : base(context)
    {
    }

    public async Task<List<Unit>> GetBaseUnitsWithDerivedAsync(UnitCategory? category, CancellationToken ct = default)
    {
        var query = DbSet
            .Include(u => u.DerivedUnits)
            .ThenInclude(d => d.DerivedUnits)
            .Where(u => u.BaseUnitId == null)
            .AsNoTracking();

        if (category.HasValue)
        {
            query = query.Where(u => u.Category == category.Value);
        }

        return await query.OrderBy(u => u.Name).ToListAsync(ct);
    }
}
