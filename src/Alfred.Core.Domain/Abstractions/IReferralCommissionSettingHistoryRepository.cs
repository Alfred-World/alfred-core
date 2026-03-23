using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Domain.Abstractions;

public interface IReferralCommissionSettingHistoryRepository
    : IRepository<ReferralCommissionSettingHistory, ReferralCommissionSettingHistoryId>
{
}
