using Alfred.Core.Application.AccountSales.Dtos;

namespace Alfred.Core.Application.AccountSales.Commission;

public interface ICommissionService
{
    Task<CommissionDto?> GetMemberCommissionAsync(MemberId memberId, CancellationToken cancellationToken = default);

    Task<List<CommissionDto>> GetAllCommissionsAsync(CancellationToken cancellationToken = default);

    Task<List<CommissionTransactionDto>> GetCommissionTransactionsAsync(MemberId memberId,
        CancellationToken cancellationToken = default);

    Task<PayoutResultDto> PayoutCommissionAsync(PayoutCommissionDto dto, ReplicatedUserId? processedByUserId = null,
        CancellationToken cancellationToken = default);

    Task<ReferralCommissionSettingDto?>
        GetReferralCommissionSettingAsync(CancellationToken cancellationToken = default);

    Task<List<ReferralCommissionSettingHistoryDto>> GetReferralCommissionSettingHistoryAsync(
        CancellationToken cancellationToken = default);

    Task<ReferralCommissionSettingDto> UpdateReferralCommissionSettingAsync(UpdateReferralCommissionSettingDto dto,
        ReplicatedUserId? changedByUserId = null, CancellationToken cancellationToken = default);
}
