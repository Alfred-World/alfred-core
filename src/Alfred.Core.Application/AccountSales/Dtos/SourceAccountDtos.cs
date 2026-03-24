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

public sealed record UpdateSourceAccountDto(
    AccountProductType AccountType,
    string Username,
    string Password,
    string? TwoFaSecret,
    string? RecoveryEmail,
    string? RecoveryPhone,
    string? Notes
);
