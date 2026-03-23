using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record CreateMemberRequest
{
    public string? DisplayName { get; init; }
    public MemberSource Source { get; init; } = MemberSource.Zalo;
    public string? SourceId { get; init; }
    public string? CustomerNote { get; init; }

    public CreateMemberDto ToDto()
    {
        return new CreateMemberDto(DisplayName, Source, SourceId, CustomerNote);
    }
}
