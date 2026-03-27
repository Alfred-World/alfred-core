using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record CreateMemberDto(
    string? DisplayName,
    MemberSource Source,
    string? SourceId,
    string? CustomerNote
);
