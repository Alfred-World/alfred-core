namespace Alfred.Core.Domain.Common.Events;

/// <summary>
/// Raised when a SalesBonusTier's OrderThreshold or BonusAmount is changed.
/// Pending transactions for this tier need their snapshot values re-synced.
/// </summary>
public sealed record SalesBonusTierUpdatedDomainEvent(
    SalesBonusTierId TierId,
    int NewOrderThreshold,
    decimal NewBonusAmount) : DomainEvent;
