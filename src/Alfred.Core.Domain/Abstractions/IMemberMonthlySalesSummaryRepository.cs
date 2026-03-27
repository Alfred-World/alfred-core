using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Domain.Abstractions;

public interface
    IMemberMonthlySalesSummaryRepository : IRepository<MemberMonthlySalesSummary, MemberMonthlySalesSummaryId>
{
    Task<MemberMonthlySalesSummary?> GetBySellerAndPeriodAsync(MemberId soldByMemberId, int year, int month,
        CancellationToken cancellationToken = default);
}
