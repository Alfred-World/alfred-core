namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record ReplaceAccountOrderDto(
    Guid ReplacementAccountCloneId,
    string? OrderNote,
    string? WarrantyIssueNote
);
