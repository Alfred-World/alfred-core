using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Repositories.Base;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class AccountCloneRepository : BaseRepository<AccountClone, AccountCloneId>, IAccountCloneRepository
{
    public AccountCloneRepository(IDbContext context) : base(context)
    {
    }
}
