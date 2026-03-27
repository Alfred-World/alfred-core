namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record PayoutCommissionDto(
    MemberId MemberId,
    string? EvidenceObjectKey,
    string? Note
);
