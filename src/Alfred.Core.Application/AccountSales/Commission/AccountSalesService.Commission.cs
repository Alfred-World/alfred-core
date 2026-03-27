using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.AccountSales;

public sealed partial class AccountSalesService
{
    public async Task<CommissionDto?> GetMemberCommissionAsync(MemberId memberId,
        CancellationToken cancellationToken = default)
    {
        var commission = await _unitOfWork.Commissions.GetByMemberIdAsync(memberId, cancellationToken);
        if (commission is null)
        {
            return null;
        }

        var member = await _unitOfWork.Members.GetByIdAsync(memberId, cancellationToken);
        return new CommissionDto(
            commission.Id,
            commission.MemberId,
            member?.DisplayName,
            commission.AvailableBalance,
            commission.TotalEarned,
            commission.TotalPaidOut,
            commission.CreatedAt,
            commission.UpdatedAt);
    }

    public async Task<List<CommissionDto>> GetAllCommissionsAsync(CancellationToken cancellationToken = default)
    {
        var commissions = await _executor.ToListAsync(
            _unitOfWork.Commissions.GetQueryable([c => c.Member!]), cancellationToken);

        return commissions.Select(c => new CommissionDto(
            c.Id,
            c.MemberId,
            c.Member?.DisplayName,
            c.AvailableBalance,
            c.TotalEarned,
            c.TotalPaidOut,
            c.CreatedAt,
            c.UpdatedAt)).ToList();
    }

    public async Task<List<CommissionTransactionDto>> GetCommissionTransactionsAsync(MemberId memberId,
        CancellationToken cancellationToken = default)
    {
        var transactions = await _executor.ToListAsync(
            _unitOfWork.CommissionTransactions
                .GetQueryable([t => t.Member!, t => t.AccountOrder!])
                .Where(t => t.MemberId == memberId)
                .OrderByDescending(t => t.CreatedAt),
            cancellationToken);

        return transactions.Select(t => new CommissionTransactionDto(
            t.Id,
            t.MemberId,
            t.Member?.DisplayName,
            t.AccountOrderId,
            t.AccountOrder?.OrderCode,
            t.TransactionType,
            t.Amount,
            t.BalanceAfter,
            t.Note,
            t.EvidenceObjectKey,
            t.Status,
            t.ProcessedByUserId,
            t.CreatedAt)).ToList();
    }

    public async Task<PayoutResultDto> PayoutCommissionAsync(PayoutCommissionDto dto,
        ReplicatedUserId? processedByUserId = null,
        CancellationToken cancellationToken = default)
    {
        PayoutResultDto? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var commission = await _unitOfWork.Commissions.GetByMemberIdAsync(dto.MemberId, ct);
            if (commission is null)
            {
                throw new KeyNotFoundException($"No commission record found for member {dto.MemberId}.");
            }

            if (commission.AvailableBalance <= 0)
            {
                throw new InvalidOperationException("No available commission balance to pay out.");
            }

            var member = await _unitOfWork.Members.GetByIdAsync(dto.MemberId, ct);
            var paidAmount = commission.Payout();
            _unitOfWork.Commissions.Update(commission);

            var transaction = CommissionTransaction.CreatePayout(
                commission.MemberId,
                paidAmount,
                commission.AvailableBalance,
                dto.EvidenceObjectKey,
                dto.Note,
                processedByUserId);
            await _unitOfWork.CommissionTransactions.AddAsync(transaction, ct);

            await _unitOfWork.SaveChangesAsync(ct);

            result = new PayoutResultDto(
                transaction.Id,
                commission.MemberId,
                member?.DisplayName,
                paidAmount,
                commission.AvailableBalance,
                transaction.CreatedAt);
        }, cancellationToken);

        return result!;
    }

    public async Task<ReferralCommissionSettingDto?> GetReferralCommissionSettingAsync(
        CancellationToken cancellationToken = default)
    {
        var setting = await _unitOfWork.ReferralCommissionSettings.GetCurrentAsync(cancellationToken);
        if (setting is null)
        {
            return null;
        }

        var history = setting.Histories
            .Select(h => new ReferralCommissionSettingHistoryDto(
                h.Id,
                h.PreviousCommissionPercent,
                h.NewCommissionPercent,
                h.CreatedAt,
                h.ChangedByUser is not null
                    ? ToReplicatedUserDto(h.ChangedByUser)
                    : h.ChangedByUserId.HasValue
                        ? new ReplicatedUserDto { Id = h.ChangedByUserId.Value }
                        : null))
            .ToList();

        return new ReferralCommissionSettingDto(
            setting.Id,
            setting.CommissionPercent,
            setting.CreatedAt,
            setting.UpdatedAt,
            history);
    }

    public async Task<List<ReferralCommissionSettingHistoryDto>> GetReferralCommissionSettingHistoryAsync(
        CancellationToken cancellationToken = default)
    {
        var setting = await _unitOfWork.ReferralCommissionSettings.GetCurrentAsync(cancellationToken);
        if (setting is null)
        {
            return [];
        }

        return setting.Histories
            .Select(history => new ReferralCommissionSettingHistoryDto(
                history.Id,
                history.PreviousCommissionPercent,
                history.NewCommissionPercent,
                history.CreatedAt,
                history.ChangedByUser is not null
                    ? ToReplicatedUserDto(history.ChangedByUser)
                    : history.ChangedByUserId.HasValue
                        ? new ReplicatedUserDto { Id = history.ChangedByUserId.Value }
                        : null))
            .ToList();
    }

    public async Task<ReferralCommissionSettingDto> UpdateReferralCommissionSettingAsync(
        UpdateReferralCommissionSettingDto dto,
        ReplicatedUserId? changedByUserId = null,
        CancellationToken cancellationToken = default)
    {
        var changedAt = DateTime.UtcNow;
        var setting = await _unitOfWork.ReferralCommissionSettings.GetCurrentAsync(cancellationToken);
        var isNewSetting = setting is null;
        var previousPercent = setting?.CommissionPercent ?? 0m;

        if (setting is null)
        {
            setting = ReferralCommissionSetting.Create(dto.CommissionPercent);
            await _unitOfWork.ReferralCommissionSettings.AddAsync(setting, cancellationToken);
        }
        else
        {
            setting.UpdatePercent(dto.CommissionPercent);
            _unitOfWork.ReferralCommissionSettings.Update(setting);
        }

        if (isNewSetting || setting.CommissionPercent != previousPercent)
        {
            var history = ReferralCommissionSettingHistory.Create(
                setting.Id,
                previousPercent,
                setting.CommissionPercent,
                changedByUserId,
                changedAt);

            await _unitOfWork.ReferralCommissionSettingHistories.AddAsync(history, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReferralCommissionSettingDto(
            setting.Id,
            setting.CommissionPercent,
            setting.CreatedAt,
            setting.UpdatedAt);
    }
}
