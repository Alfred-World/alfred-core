using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Repositories.Base;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class ReferralCommissionSettingHistoryRepository
    : BaseRepository<ReferralCommissionSettingHistory, ReferralCommissionSettingHistoryId>,
        IReferralCommissionSettingHistoryRepository
{
    public ReferralCommissionSettingHistoryRepository(IDbContext context) : base(context)
    {
    }
}
