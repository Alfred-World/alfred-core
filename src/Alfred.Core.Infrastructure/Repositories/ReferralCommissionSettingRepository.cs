using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Repositories.Base;

using Microsoft.EntityFrameworkCore;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class ReferralCommissionSettingRepository
    : BaseRepository<ReferralCommissionSetting, ReferralCommissionSettingId>, IReferralCommissionSettingRepository
{
    public ReferralCommissionSettingRepository(IDbContext context) : base(context)
    {
    }

    public async Task<ReferralCommissionSetting?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.Histories.OrderByDescending(h => h.CreatedAt))
            .ThenInclude(h => h.ChangedByUser)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
