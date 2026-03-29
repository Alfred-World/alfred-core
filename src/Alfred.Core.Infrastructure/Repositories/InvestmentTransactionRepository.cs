using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Repositories.Base;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class InvestmentTransactionRepository : BaseRepository<InvestmentTransaction, InvestmentTransactionId>,
    IInvestmentTransactionRepository
{
    public InvestmentTransactionRepository(IDbContext context) : base(context)
    {
    }
}
