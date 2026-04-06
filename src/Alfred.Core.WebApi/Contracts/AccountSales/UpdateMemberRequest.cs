using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.Common;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record UpdateMemberRequest
{
    public Optional<string?> DisplayName { get; init; }
    public Optional<MemberSource> Source { get; init; }
    public Optional<string?> SourceId { get; init; }
    public Optional<string?> CustomerNote { get; init; }

    public UpdateMemberDto ToDto()
    {
        return new UpdateMemberDto
        {
            DisplayName = DisplayName,
            Source = Source,
            SourceId = SourceId,
            CustomerNote = CustomerNote
        };
    }
}
