using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.AccountSales.Bonus;

public interface IBonusService
{
    Task<List<SalesBonusTierDto>> GetBonusTiersAsync(CancellationToken cancellationToken = default);

    Task<SalesBonusTierDto> CreateBonusTierAsync(CreateSalesBonusTierDto dto,
        CancellationToken cancellationToken = default);

    Task<SalesBonusTierDto> UpdateBonusTierAsync(SalesBonusTierId tierId, UpdateSalesBonusTierDto dto,
        CancellationToken cancellationToken = default);

    Task DeleteBonusTierAsync(SalesBonusTierId tierId, CancellationToken cancellationToken = default);

    Task<List<MemberMonthlySalesSummaryDto>> GetSellerMonthlySummariesAsync(MemberId soldByMemberId,
        CancellationToken cancellationToken = default);

    Task<MemberMonthlySalesSummaryDto?> GetSellerMonthlySummaryAsync(MemberId soldByMemberId, int year, int month,
        CancellationToken cancellationToken = default);

    Task<List<SalesBonusTransactionDto>> GetBonusTransactionsAsync(MemberId soldByMemberId,
        CancellationToken cancellationToken = default);

    Task<List<SalesBonusTransactionDto>> GetAllBonusTransactionsAsync(int? year = null, int? month = null,
        SalesBonusTransactionStatus? status = null, MemberId? soldByMemberId = null,
        CancellationToken cancellationToken = default);

    Task<SellerBonusProgressDto> GetSellerBonusProgressAsync(MemberId soldByMemberId,
        CancellationToken cancellationToken = default);

    Task<SalesBonusTransactionDto> MarkBonusTransactionPaidAsync(SalesBonusTransactionId transactionId,
        ReplicatedUserId? processedByUserId = null, string? note = null, CancellationToken cancellationToken = default);

    Task<SalesBonusTransactionDto> CancelBonusTransactionAsync(SalesBonusTransactionId transactionId,
        ReplicatedUserId? cancelledByUserId = null, string? note = null, CancellationToken cancellationToken = default);

    Task<SalesBonusTransactionDto> SettleBonusTierAsync(MemberId soldByMemberId, SalesBonusTierId tierId,
        ReplicatedUserId? processedByUserId = null, string? note = null, CancellationToken cancellationToken = default);
}
