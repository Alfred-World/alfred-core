using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Domain.Abstractions;

public interface IInvestmentTransactionRepository : IRepository<InvestmentTransaction, Guid>
{
}
