using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record AccountCloneDto(
    Guid Id,
    ProductSummaryDto Product,
    SourceAccountSummaryDto? SourceAccount,
    string ExternalAccountId,
    string Username,
    string Password,
    string? TwoFaSecret,
    string? ExtraInfo,
    AccountCloneStatus Status,
    DateTime CreatedAt,
    DateTime? VerifiedAt,
    DateTime? SoldAt
);
