namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record MemberDetailDto(
    MemberDto Member,
    MemberStatsDto Stats
);
