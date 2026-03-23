using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record UpdateAccountCloneStatusDto(
    AccountCloneStatus Status
);
