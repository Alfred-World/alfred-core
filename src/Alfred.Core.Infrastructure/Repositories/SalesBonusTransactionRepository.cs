using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Repositories.Base;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class SalesBonusTransactionRepository
    : BaseRepository<SalesBonusTransaction, SalesBonusTransactionId>,
        ISalesBonusTransactionRepository
{
    public SalesBonusTransactionRepository(IDbContext context) : base(context)
    {
    }
}
