using Alfred.Core.Application.Commodities.Dtos;

namespace Alfred.Core.Application.Commodities;

public interface ICommodityService
{
    #region Commodities

    Task<PageResult<CommodityDto>> GetAllCommoditiesAsync(QueryRequest query,
        CancellationToken cancellationToken = default);

    Task<CommodityDto?> GetCommodityByIdAsync(CommodityId id, CancellationToken cancellationToken = default);

    Task<CommodityDto> CreateCommodityAsync(CreateCommodityDto dto, CancellationToken cancellationToken = default);

    Task<CommodityDto> UpdateCommodityAsync(CommodityId id, UpdateCommodityDto dto,
        CancellationToken cancellationToken = default);

    Task DeleteCommodityAsync(CommodityId id, CancellationToken cancellationToken = default);

    #endregion

    #region Investment Transactions

    Task<PageResult<InvestmentTransactionDto>> GetTransactionsAsync(CommodityId commodityId, QueryRequest query,
        CancellationToken cancellationToken = default);

    Task<InvestmentTransactionDto?> GetTransactionByIdAsync(InvestmentTransactionId id,
        CancellationToken cancellationToken = default);

    Task<InvestmentTransactionDto> CreateTransactionAsync(CommodityId commodityId, CreateInvestmentTransactionDto dto,
        CancellationToken cancellationToken = default);

    Task DeleteTransactionAsync(InvestmentTransactionId id, CancellationToken cancellationToken = default);

    #endregion
}
