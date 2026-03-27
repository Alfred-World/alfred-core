using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Dtos;

/// <summary>
/// Current-month bonus progress for a single seller.
/// </summary>
public sealed record SellerBonusProgressDto(
    Guid SoldByMemberId,
    string? SoldByMemberName,
    int Year,
    int Month,
    int CurrentOrderCount,
    decimal TotalBonusEarned,
    List<BonusTierProgressDto> Tiers
);

/// <summary>
/// Per-tier progress entry — shows whether a tier is reached and payout status.
/// </summary>
public sealed record BonusTierProgressDto(
    Guid TierId,
    int OrderThreshold,
    decimal BonusAmount,
    bool IsActive,
    bool IsReached,
    /// <summary>Orders still needed to reach this tier (0 when already reached).</summary>
    int OrdersNeeded,
    SalesBonusTransactionStatus? TransactionStatus,
    Guid? TransactionId
);
