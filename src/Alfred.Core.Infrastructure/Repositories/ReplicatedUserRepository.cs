using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Repositories.Base;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class ReplicatedUserRepository : BaseRepository<ReplicatedUser, ReplicatedUserId>,
    IReplicatedUserRepository
{
    public ReplicatedUserRepository(IDbContext context) : base(context)
    {
    }
}
