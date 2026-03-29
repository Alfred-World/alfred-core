using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Repositories.Base;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class SourceAccountRepository : BaseRepository<SourceAccount, SourceAccountId>, ISourceAccountRepository
{
    public SourceAccountRepository(IDbContext context) : base(context)
    {
    }
}
