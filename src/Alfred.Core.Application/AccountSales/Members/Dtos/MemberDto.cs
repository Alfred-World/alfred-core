using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record MemberDto(
    Guid Id,
    string? DisplayName,
    MemberSource Source,
    string? SourceId,
    string? CustomerNote,
    DateTime CreatedAt
);
