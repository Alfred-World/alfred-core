using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Repositories.Base;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class MemberRepository : BaseRepository<Member, MemberId>, IMemberRepository
{
    public MemberRepository(IDbContext context) : base(context)
    {
    }
}
