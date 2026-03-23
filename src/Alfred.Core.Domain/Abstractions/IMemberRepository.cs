using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Domain.Abstractions;

public interface IMemberRepository : IRepository<Member, MemberId>
{
}
