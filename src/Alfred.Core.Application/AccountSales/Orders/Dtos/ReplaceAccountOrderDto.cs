namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record ReplaceAccountOrderDto(
    AccountCloneId ReplacementAccountCloneId,
    string? OrderNote,
    string? WarrantyIssueNote
);
