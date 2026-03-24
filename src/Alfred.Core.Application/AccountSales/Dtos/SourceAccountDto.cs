using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record SourceAccountDto(
    Guid Id,
    AccountProductType AccountType,
    string Username,
    string Password,
    string? TwoFaSecret,
    string? RecoveryEmail,
    string? RecoveryPhone,
    string? Notes,
    bool IsActive,
    int CloneCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
