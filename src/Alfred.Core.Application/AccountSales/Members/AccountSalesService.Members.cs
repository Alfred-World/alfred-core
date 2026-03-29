using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.AccountSales.Shared;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.AccountSales;

public sealed partial class AccountSalesService
{
    public async Task<MemberDto?> GetMemberByIdAsync(MemberId id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Members.GetByIdAsync(id, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<MemberDetailDto?> GetMemberDetailAsync(MemberId id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Members.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var purchaseAggs = await _executor.ToListAsync(
            _unitOfWork.AccountOrders.GetQueryable()
                .Where(o => o.MemberId == id)
                .GroupBy(o => new { o.PurchaseDate.Year, o.PurchaseDate.Month })
                .Select(g => new MonthAgg(g.Key.Year, g.Key.Month, g.Count(), g.Sum(o => o.UnitPriceSnapshot))),
            cancellationToken);

        var referralAggs = await _executor.ToListAsync(
            _unitOfWork.AccountOrders.GetQueryable()
                .Where(o => o.ReferrerMemberId == id)
                .GroupBy(o => new { o.PurchaseDate.Year, o.PurchaseDate.Month })
                .Select(g =>
                    new MonthAgg(g.Key.Year, g.Key.Month, g.Count(), g.Sum(o => o.ReferralCommissionAmountSnapshot))),
            cancellationToken);

        var allMonths = purchaseAggs.Select(p => (p.Year, p.Month))
            .Concat(referralAggs.Select(r => (r.Year, r.Month)))
            .Distinct()
            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month);

        var monthly = allMonths.Select(m =>
        {
            var p = purchaseAggs.Find(x => x.Year == m.Year && x.Month == m.Month);
            var r = referralAggs.Find(x => x.Year == m.Year && x.Month == m.Month);
            return new MemberMonthlyStatsDto(m.Year, m.Month, p?.Count ?? 0, p?.Amount ?? 0m, r?.Count ?? 0,
                r?.Amount ?? 0m);
        }).ToList();

        var stats = new MemberStatsDto(
            purchaseAggs.Sum(p => p.Count),
            purchaseAggs.Sum(p => p.Amount),
            referralAggs.Sum(r => r.Count),
            referralAggs.Sum(r => r.Amount),
            monthly);

        return new MemberDetailDto(entity.ToDto(), stats);
    }

    public async Task<PageResult<MemberDto>> GetMembersAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedAsync(_unitOfWork.Members, query, MemberFieldMap.Instance, m => m.ToDto(),
            cancellationToken);
    }

    public async Task<List<MemberDto>> SearchMembersAsync(string keyword, int take = 20,
        CancellationToken cancellationToken = default)
    {
        var normalized = (keyword ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return [];
        }

        var query = _unitOfWork.Members.GetQueryable()
            .Where(m => (m.DisplayName != null && m.DisplayName.Contains(normalized))
                        || (m.SourceId != null && m.SourceId.Contains(normalized)))
            .OrderByDescending(m => m.CreatedAt)
            .Take(Math.Clamp(take, 1, 100));

        var entities = await _executor.ToListAsync(_executor.AsNoTracking(query), cancellationToken);
        return entities.Select(x => x.ToDto()).ToList();
    }

    public async Task<MemberDto> CreateMemberAsync(CreateMemberDto dto, CancellationToken cancellationToken = default)
    {
        var entity = Member.Create(dto.DisplayName, dto.Source, dto.SourceId, dto.CustomerNote);
        await _unitOfWork.Members.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<MemberDto?> UpdateMemberAsync(MemberId id, UpdateMemberDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Members.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Update(dto.DisplayName, dto.Source, dto.SourceId, dto.CustomerNote);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<MemberStatsDto?> GetMemberStatsAsync(MemberId id, CancellationToken cancellationToken = default)
    {
        var exists = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Members.GetQueryable().Where(m => m.Id == id),
            cancellationToken);

        if (exists is null)
        {
            return null;
        }

        var purchaseAggs = await _executor.ToListAsync(
            _unitOfWork.AccountOrders.GetQueryable()
                .Where(o => o.MemberId == id)
                .GroupBy(o => new { o.PurchaseDate.Year, o.PurchaseDate.Month })
                .Select(g => new MonthAgg(g.Key.Year, g.Key.Month, g.Count(), g.Sum(o => o.UnitPriceSnapshot))),
            cancellationToken);

        var referralAggs = await _executor.ToListAsync(
            _unitOfWork.AccountOrders.GetQueryable()
                .Where(o => o.ReferrerMemberId == id)
                .GroupBy(o => new { o.PurchaseDate.Year, o.PurchaseDate.Month })
                .Select(g =>
                    new MonthAgg(g.Key.Year, g.Key.Month, g.Count(), g.Sum(o => o.ReferralCommissionAmountSnapshot))),
            cancellationToken);

        var allMonths = purchaseAggs.Select(p => (p.Year, p.Month))
            .Concat(referralAggs.Select(r => (r.Year, r.Month)))
            .Distinct()
            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month);

        var monthly = allMonths.Select(m =>
        {
            var p = purchaseAggs.Find(x => x.Year == m.Year && x.Month == m.Month);
            var r = referralAggs.Find(x => x.Year == m.Year && x.Month == m.Month);

            return new MemberMonthlyStatsDto(m.Year, m.Month, p?.Count ?? 0, p?.Amount ?? 0m, r?.Count ?? 0,
                r?.Amount ?? 0m);
        }).ToList();

        return new MemberStatsDto(
            purchaseAggs.Sum(p => p.Count),
            purchaseAggs.Sum(p => p.Amount),
            referralAggs.Sum(r => r.Count),
            referralAggs.Sum(r => r.Amount),
            monthly);
    }

    private sealed record MonthAgg(int Year, int Month, int Count, decimal Amount);
}
