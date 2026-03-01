using Alfred.Core.Application.Commodities.Dtos;
using Alfred.Core.Application.Querying.Core;

namespace Alfred.Core.Application.Commodities;

public interface ICommodityService
{
    #region Commodities

    Task<PageResult<CommodityDto>> GetAllCommoditiesAsync(QueryRequest query,
        CancellationToken cancellationToken = default);

    Task<CommodityDto?> GetCommodityByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CommodityDto> CreateCommodityAsync(CreateCommodityDto dto, CancellationToken cancellationToken = default);

    Task<CommodityDto> UpdateCommodityAsync(Guid id, UpdateCommodityDto dto,
        CancellationToken cancellationToken = default);

    Task DeleteCommodityAsync(Guid id, CancellationToken cancellationToken = default);

    #endregion

    #region Investment Transactions

    Task<PageResult<InvestmentTransactionDto>> GetTransactionsAsync(Guid commodityId, QueryRequest query,
        CancellationToken cancellationToken = default);

    Task<InvestmentTransactionDto?> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<InvestmentTransactionDto> CreateTransactionAsync(Guid commodityId, CreateInvestmentTransactionDto dto,
        CancellationToken cancellationToken = default);

    Task DeleteTransactionAsync(Guid id, CancellationToken cancellationToken = default);

    #endregion
}
