using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Repositories.Base;

using Microsoft.EntityFrameworkCore;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class MemberMonthlySalesSummaryRepository
    : BaseRepository<MemberMonthlySalesSummary, MemberMonthlySalesSummaryId>,
        IMemberMonthlySalesSummaryRepository
{
    public MemberMonthlySalesSummaryRepository(IDbContext context) : base(context)
    {
    }

    public async Task<MemberMonthlySalesSummary?> GetBySellerAndPeriodAsync(MemberId soldByMemberId, int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            s => s.SoldByMemberId == soldByMemberId && s.Year == year && s.Month == month,
            cancellationToken);
    }
}
