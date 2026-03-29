using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Repositories.Base;

using Microsoft.EntityFrameworkCore;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class CommissionRepository : BaseRepository<Commission, CommissionId>, ICommissionRepository
{
    public CommissionRepository(IDbContext context) : base(context)
    {
    }

    public async Task<Commission?> GetByMemberIdAsync(MemberId memberId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.MemberId == memberId, cancellationToken);
    }
}
