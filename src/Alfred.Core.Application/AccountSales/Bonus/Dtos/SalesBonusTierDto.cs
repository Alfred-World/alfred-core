namespace Alfred.Core.Application.AccountSales.Dtos;

public sealed record SalesBonusTierDto(
    Guid Id,
    int OrderThreshold,
    decimal BonusAmount,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record CreateSalesBonusTierDto(int OrderThreshold, decimal BonusAmount);

public sealed record UpdateSalesBonusTierDto(int OrderThreshold, decimal BonusAmount, bool IsActive);
