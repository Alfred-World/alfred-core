using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Domain.Abstractions;

public interface ICommissionRepository : IRepository<Commission, CommissionId>
{
    Task<Commission?> GetByMemberIdAsync(MemberId memberId, CancellationToken cancellationToken = default);
}
