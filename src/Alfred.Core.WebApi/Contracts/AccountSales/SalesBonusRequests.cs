using Alfred.Core.Application.Common;

namespace Alfred.Core.WebApi.Contracts.AccountSales;

public sealed record CreateSalesBonusTierRequest
{
    public int OrderThreshold { get; init; }
    public decimal BonusAmount { get; init; }
}

public sealed record UpdateSalesBonusTierRequest
{
    public Optional<int> OrderThreshold { get; init; }
    public Optional<decimal> BonusAmount { get; init; }
    public Optional<bool> IsActive { get; init; }
}

public sealed record MarkBonusPaidRequest
{
    public string? Note { get; init; }
}

public sealed record CancelBonusTransactionRequest
{
    public string? Note { get; init; }
}

public sealed record SettleBonusTierRequest
{
    public Guid SoldByMemberId { get; init; }
    public Guid TierId { get; init; }
    public string? Note { get; init; }
}
