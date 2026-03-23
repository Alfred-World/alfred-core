using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Repositories.Base;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class AccountOrderRepository : BaseRepository<AccountOrder, AccountOrderId>, IAccountOrderRepository
{
    public AccountOrderRepository(IDbContext context) : base(context)
    {
    }
}
