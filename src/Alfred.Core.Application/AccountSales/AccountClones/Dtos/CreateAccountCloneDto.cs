namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record CreateAccountCloneDto(
    ProductId ProductId,
    string ExternalAccountId,
    string Username,
    string Password,
    string? TwoFaSecret,
    string? ExtraInfo,
    SourceAccountId? SourceAccountId = null
);
