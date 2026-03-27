using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Repositories.Base;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class CommissionTransactionRepository : BaseRepository<CommissionTransaction, CommissionTransactionId>,
    ICommissionTransactionRepository
{
    public CommissionTransactionRepository(IDbContext context) : base(context)
    {
    }
}
