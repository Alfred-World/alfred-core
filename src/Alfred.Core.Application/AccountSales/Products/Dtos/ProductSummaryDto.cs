using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record ProductSummaryDto(
    Guid Id,
    string Name,
    AccountProductType ProductType
);
