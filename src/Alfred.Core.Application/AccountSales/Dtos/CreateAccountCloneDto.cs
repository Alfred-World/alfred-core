namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record CreateAccountCloneDto(
    Guid ProductId,
    string ExternalAccountId,
    string Username,
    string Password,
    string? TwoFaSecret,
    string? ExtraInfo
);
