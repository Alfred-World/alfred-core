using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record CreateSourceAccountDto(
    AccountProductType AccountType,
    string Username,
    string Password,
    string? TwoFaSecret,
    string? RecoveryEmail,
    string? RecoveryPhone,
    string? Notes
);

public sealed record UpdateSourceAccountDto
{
    public Optional<AccountProductType> AccountType { get; init; }
    public Optional<string> Username { get; init; }
    public Optional<string> Password { get; init; }
    public Optional<string?> TwoFaSecret { get; init; }
    public Optional<string?> RecoveryEmail { get; init; }
    public Optional<string?> RecoveryPhone { get; init; }
    public Optional<string?> Notes { get; init; }
}
