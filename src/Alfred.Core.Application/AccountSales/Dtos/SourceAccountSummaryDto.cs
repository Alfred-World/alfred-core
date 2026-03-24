using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record SourceAccountSummaryDto(
    Guid Id,
    AccountProductType AccountType,
    string Username
);
