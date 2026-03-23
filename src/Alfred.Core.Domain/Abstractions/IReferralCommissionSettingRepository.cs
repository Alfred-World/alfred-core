using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Domain.Abstractions;

public interface IReferralCommissionSettingRepository
    : IRepository<ReferralCommissionSetting, ReferralCommissionSettingId>
{
    Task<ReferralCommissionSetting?> GetCurrentAsync(CancellationToken cancellationToken = default);
}
