using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record UpdateMemberDto
{
    public Optional<string?> DisplayName { get; init; }
    public Optional<MemberSource> Source { get; init; }
    public Optional<string?> SourceId { get; init; }
    public Optional<string?> CustomerNote { get; init; }
}
