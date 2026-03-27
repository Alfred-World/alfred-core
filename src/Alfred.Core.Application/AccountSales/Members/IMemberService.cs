using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.Querying.Core;

namespace Alfred.Core.Application.AccountSales.Members;

public interface IMemberService
{
    Task<MemberDto?> GetMemberByIdAsync(MemberId id, CancellationToken cancellationToken = default);

    Task<MemberDetailDto?> GetMemberDetailAsync(MemberId id, CancellationToken cancellationToken = default);

    Task<PageResult<MemberDto>> GetMembersAsync(QueryRequest query, CancellationToken cancellationToken = default);

    Task<List<MemberDto>> SearchMembersAsync(string keyword, int take = 20,
        CancellationToken cancellationToken = default);

    Task<MemberDto> CreateMemberAsync(CreateMemberDto dto, CancellationToken cancellationToken = default);

    Task<MemberDto?> UpdateMemberAsync(MemberId id, UpdateMemberDto dto, CancellationToken cancellationToken = default);

    Task<MemberStatsDto?> GetMemberStatsAsync(MemberId id, CancellationToken cancellationToken = default);
}
