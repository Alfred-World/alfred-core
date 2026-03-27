using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.AccountSales.Shared;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales;

public sealed partial class AccountSalesService
{
    public async Task<List<SalesBonusTierDto>> GetBonusTiersAsync(CancellationToken cancellationToken = default)
    {
        var tiers = await _unitOfWork.SalesBonusTiers.GetAllAsync(cancellationToken);
        return tiers.OrderBy(t => t.OrderThreshold).Select(t => t.ToDto()).ToList();
    }

    public async Task<SalesBonusTierDto> CreateBonusTierAsync(CreateSalesBonusTierDto dto,
        CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.SalesBonusTiers.GetAllAsync(cancellationToken);

        if (existing.Any(t => t.OrderThreshold == dto.OrderThreshold))
        {
            throw new InvalidOperationException(
                $"A bonus tier with order threshold {dto.OrderThreshold} already exists.");
        }

        var tier = SalesBonusTier.Create(dto.OrderThreshold, dto.BonusAmount);
        await _unitOfWork.SalesBonusTiers.AddAsync(tier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return tier.ToDto();
    }

    public async Task<SalesBonusTierDto> UpdateBonusTierAsync(SalesBonusTierId tierId, UpdateSalesBonusTierDto dto,
        CancellationToken cancellationToken = default)
    {
        var tier = await _unitOfWork.SalesBonusTiers.GetByIdAsync(tierId, cancellationToken);
        if (tier is null)
        {
            throw new KeyNotFoundException($"Bonus tier with ID {tierId} not found.");
        }

        var existing = await _unitOfWork.SalesBonusTiers.GetAllAsync(cancellationToken);

        if (existing.Any(t => t.OrderThreshold == dto.OrderThreshold && t.Id != tierId))
        {
            throw new InvalidOperationException(
                $"A bonus tier with order threshold {dto.OrderThreshold} already exists.");
        }

        tier.Update(dto.OrderThreshold, dto.BonusAmount, dto.IsActive);
        _unitOfWork.SalesBonusTiers.Update(tier);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return tier.ToDto();
    }

    public async Task DeleteBonusTierAsync(SalesBonusTierId tierId, CancellationToken cancellationToken = default)
    {
        var tier = await _unitOfWork.SalesBonusTiers.GetByIdAsync(tierId, cancellationToken);
        if (tier is null)
        {
            throw new KeyNotFoundException($"Bonus tier with ID {tierId} not found.");
        }

        _unitOfWork.SalesBonusTiers.Delete(tier);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<MemberMonthlySalesSummaryDto>> GetSellerMonthlySummariesAsync(MemberId soldByMemberId,
        CancellationToken cancellationToken = default)
    {
        var summaries = await _executor.ToListAsync(
            _unitOfWork.MemberMonthlySalesSummaries
                .GetQueryable([s => s.SoldByMember!, s => s.HighestTierReached!])
                .Where(s => s.SoldByMemberId == soldByMemberId)
                .OrderByDescending(s => s.Year).ThenByDescending(s => s.Month),
            cancellationToken);

        return summaries.Select(s => s.ToDto()).ToList();
    }

    public async Task<MemberMonthlySalesSummaryDto?> GetSellerMonthlySummaryAsync(MemberId soldByMemberId, int year,
        int month, CancellationToken cancellationToken = default)
    {
        var summary = await _unitOfWork.MemberMonthlySalesSummaries
            .GetBySellerAndPeriodAsync(soldByMemberId, year, month, cancellationToken);
        return summary?.ToDto();
    }

    public async Task<List<SalesBonusTransactionDto>> GetBonusTransactionsAsync(MemberId soldByMemberId,
        CancellationToken cancellationToken = default)
    {
        var transactions = await _executor.ToListAsync(
            _unitOfWork.SalesBonusTransactions
                .GetQueryable([t => t.SoldByMember!, t => t.SalesBonusTier!])
                .Where(t => t.SoldByMemberId == soldByMemberId)
                .OrderByDescending(t => t.CreatedAt),
            cancellationToken);

        return transactions.Select(t => t.ToDto()).ToList();
    }

    public async Task<List<SalesBonusTransactionDto>> GetAllBonusTransactionsAsync(int? year = null,
        int? month = null, SalesBonusTransactionStatus? status = null, MemberId? soldByMemberId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.SalesBonusTransactions
            .GetQueryable([t => t.SoldByMember!, t => t.SalesBonusTier!])
            .AsQueryable();

        if (year.HasValue)
        {
            query = query.Where(t => t.Year == year.Value);
        }

        if (month.HasValue)
        {
            query = query.Where(t => t.Month == month.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (soldByMemberId.HasValue)
        {
            query = query.Where(t => t.SoldByMemberId == soldByMemberId.Value);
        }

        var transactions = await _executor.ToListAsync(
            query.OrderByDescending(t => t.Year).ThenByDescending(t => t.Month).ThenBy(t => t.SoldByMemberId),
            cancellationToken);

        return transactions.Select(t => t.ToDto()).ToList();
    }

    public async Task<SellerBonusProgressDto> GetSellerBonusProgressAsync(MemberId soldByMemberId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var year = now.Year;
        var month = now.Month;

        var member = await _unitOfWork.Members.GetByIdAsync(soldByMemberId, cancellationToken);

        var summary = await _unitOfWork.MemberMonthlySalesSummaries
            .GetBySellerAndPeriodAsync(soldByMemberId, year, month, cancellationToken);

        var currentOrderCount = summary?.OrderCount ?? 0;
        var totalBonusEarned = summary?.TotalBonusEarned ?? 0m;

        var allTiers = (await _unitOfWork.SalesBonusTiers.GetAllAsync(cancellationToken))
            .OrderBy(t => t.OrderThreshold)
            .ToList();

        var monthTransactions = await _executor.ToListAsync(
            _unitOfWork.SalesBonusTransactions.GetQueryable()
                .Where(t => t.SoldByMemberId == soldByMemberId
                            && t.Year == year
                            && t.Month == month
                            && t.Status != SalesBonusTransactionStatus.Cancelled),
            cancellationToken);

        var txByTierId = monthTransactions.ToDictionary(t => t.SalesBonusTierId, t => t);

        var tierProgressList = allTiers.Select(tier =>
        {
            txByTierId.TryGetValue(tier.Id, out var tx);
            var isReached = currentOrderCount >= tier.OrderThreshold;
            var ordersNeeded = isReached ? 0 : tier.OrderThreshold - currentOrderCount;

            return new BonusTierProgressDto(
                tier.Id.Value,
                tier.OrderThreshold,
                tier.BonusAmount,
                tier.IsActive,
                isReached,
                ordersNeeded,
                tx?.Status,
                tx?.Id.Value
            );
        }).ToList();

        return new SellerBonusProgressDto(
            soldByMemberId.Value,
            member?.DisplayName,
            year,
            month,
            currentOrderCount,
            totalBonusEarned,
            tierProgressList
        );
    }

    public async Task<SalesBonusTransactionDto> MarkBonusTransactionPaidAsync(SalesBonusTransactionId transactionId,
        ReplicatedUserId? processedByUserId = null, string? note = null,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _unitOfWork.SalesBonusTransactions.GetByIdAsync(
            transactionId, cancellationToken);
        if (transaction is null)
        {
            throw new KeyNotFoundException($"Bonus transaction with ID {transactionId} not found.");
        }

        if (transaction.Status != SalesBonusTransactionStatus.Pending)
        {
            throw new InvalidOperationException("Only pending bonus transactions can be marked as paid.");
        }

        transaction.MarkPaid(processedByUserId, note);
        _unitOfWork.SalesBonusTransactions.Update(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var loaded = await _executor.FirstOrDefaultAsync(
            _unitOfWork.SalesBonusTransactions
                .GetQueryable([t => t.SoldByMember!, t => t.SalesBonusTier!])
                .Where(t => t.Id == transaction.Id),
            cancellationToken);

        return (loaded ?? transaction).ToDto();
    }

    public async Task<SalesBonusTransactionDto> CancelBonusTransactionAsync(SalesBonusTransactionId transactionId,
        ReplicatedUserId? cancelledByUserId = null, string? note = null,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _unitOfWork.SalesBonusTransactions.GetByIdAsync(
            transactionId, cancellationToken);
        if (transaction is null)
        {
            throw new KeyNotFoundException($"Bonus transaction with ID {transactionId} not found.");
        }

        if (transaction.Status != SalesBonusTransactionStatus.Pending)
        {
            throw new InvalidOperationException("Only pending bonus transactions can be cancelled.");
        }

        transaction.Cancel(note);
        _unitOfWork.SalesBonusTransactions.Update(transaction);

        var summary = await _unitOfWork.MemberMonthlySalesSummaries
            .GetBySellerAndPeriodAsync(transaction.SoldByMemberId, transaction.Year, transaction.Month,
                cancellationToken);
        if (summary is not null)
        {
            summary.DeductBonusEarned(transaction.BonusAmountSnapshot);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var loaded = await _executor.FirstOrDefaultAsync(
            _unitOfWork.SalesBonusTransactions
                .GetQueryable([t => t.SoldByMember!, t => t.SalesBonusTier!])
                .Where(t => t.Id == transaction.Id),
            cancellationToken);

        return (loaded ?? transaction).ToDto();
    }

    public async Task<SalesBonusTransactionDto> SettleBonusTierAsync(MemberId soldByMemberId,
        SalesBonusTierId tierId,
        ReplicatedUserId? processedByUserId = null,
        string? note = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var year = now.Year;
        var month = now.Month;

        var tier = await _unitOfWork.SalesBonusTiers.GetByIdAsync(tierId, cancellationToken)
                   ?? throw new KeyNotFoundException($"Bonus tier {tierId} not found.");

        var existing = await _executor.FirstOrDefaultAsync(
            _unitOfWork.SalesBonusTransactions.GetQueryable()
                .Where(t => t.SoldByMemberId == soldByMemberId
                            && t.SalesBonusTierId == tierId
                            && t.Year == year
                            && t.Month == month
                            && t.Status != SalesBonusTransactionStatus.Cancelled),
            cancellationToken);

        if (existing is not null)
        {
            if (existing.Status == SalesBonusTransactionStatus.Paid)
            {
                throw new InvalidOperationException("This bonus tier has already been paid for the current month.");
            }

            existing.MarkPaid(processedByUserId, note);
            _unitOfWork.SalesBonusTransactions.Update(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var loaded = await _executor.FirstOrDefaultAsync(
                _unitOfWork.SalesBonusTransactions
                    .GetQueryable([t => t.SoldByMember!, t => t.SalesBonusTier!])
                    .Where(t => t.Id == existing.Id),
                cancellationToken);

            return (loaded ?? existing).ToDto();
        }

        var summary = await _unitOfWork.MemberMonthlySalesSummaries
            .GetBySellerAndPeriodAsync(soldByMemberId, year, month, cancellationToken);
        var orderCount = summary?.OrderCount ?? 0;

        var transaction = SalesBonusTransaction.Create(
            soldByMemberId,
            tierId,
            year,
            month,
            orderCount,
            tier.OrderThreshold,
            tier.BonusAmount);

        transaction.MarkPaid(processedByUserId, note);
        await _unitOfWork.SalesBonusTransactions.AddAsync(transaction, cancellationToken);

        if (summary is not null)
        {
            summary.RecordTierReached(tierId, tier.BonusAmount);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = await _executor.FirstOrDefaultAsync(
            _unitOfWork.SalesBonusTransactions
                .GetQueryable([t => t.SoldByMember!, t => t.SalesBonusTier!])
                .Where(t => t.Id == transaction.Id),
            cancellationToken);

        return (result ?? transaction).ToDto();
    }

    /// <summary>
    /// Evaluate sales bonus milestones for a seller after an order is confirmed.
    /// Increments the monthly order count and creates Pending bonus transactions for newly-crossed tiers.
    /// </summary>
    private async Task EvaluateSalesBonusAsync(MemberId soldByMemberId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var year = now.Year;
        var month = now.Month;

        var summary = await _unitOfWork.MemberMonthlySalesSummaries
            .GetBySellerAndPeriodAsync(soldByMemberId, year, month, ct);
        if (summary is null)
        {
            summary = MemberMonthlySalesSummary.Create(soldByMemberId, year, month);
            await _unitOfWork.MemberMonthlySalesSummaries.AddAsync(summary, ct);
        }

        var newOrderCount = summary.IncrementOrderCount();

        var activeTiers = (await _unitOfWork.SalesBonusTiers.GetAllAsync(ct))
            .Where(t => t.IsActive)
            .OrderBy(t => t.OrderThreshold)
            .ToList();

        if (activeTiers.Count == 0)
        {
            return;
        }

        var existingBonuses = await _executor.ToListAsync(
            _unitOfWork.SalesBonusTransactions
                .GetQueryable()
                .Where(t => t.SoldByMemberId == soldByMemberId && t.Year == year && t.Month == month
                            && t.Status != SalesBonusTransactionStatus.Cancelled),
            ct);

        var earnedTierIds = existingBonuses.Select(t => t.SalesBonusTierId).ToHashSet();

        foreach (var tier in activeTiers)
        {
            if (newOrderCount >= tier.OrderThreshold && !earnedTierIds.Contains(tier.Id))
            {
                var bonusTransaction = SalesBonusTransaction.Create(
                    soldByMemberId,
                    tier.Id,
                    year,
                    month,
                    newOrderCount,
                    tier.OrderThreshold,
                    tier.BonusAmount);
                await _unitOfWork.SalesBonusTransactions.AddAsync(bonusTransaction, ct);

                summary.RecordTierReached(tier.Id, tier.BonusAmount);
            }
        }
    }
}
