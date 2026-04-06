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

public sealed record UpdateSalesBonusTierDto
{
    public Optional<int> OrderThreshold { get; init; }
    public Optional<decimal> BonusAmount { get; init; }
    public Optional<bool> IsActive { get; init; }
}
