using Alfred.Core.Application.Commodities.Dtos;
using Alfred.Core.Application.Commodities.Shared;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.Commodities;

public sealed class CommodityService : BaseApplicationService, ICommodityService
{
    private readonly IUnitOfWork _unitOfWork;

    public CommodityService(
        IUnitOfWork unitOfWork,
        IAsyncQueryExecutor executor) : base(executor)
    {
        _unitOfWork = unitOfWork;
    }

    #region Commodities

    public async Task<PageResult<CommodityDto>> SearchCommoditiesAsync(SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        return await SearchWithViewAsync(
            _unitOfWork.Commodities,
            request,
            CommodityFieldMap.Instance,
            CommodityFieldMap.Views,
            c => c.ToDto(),
            cancellationToken);
    }

    public async Task<CommodityDto?> GetCommodityByIdAsync(CommodityId id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Commodities.GetQueryable([c => c.DefaultUnit!])
                .Where(c => c.Id == id),
            cancellationToken);

        return entity?.ToDto();
    }

    public async Task<CommodityDto> CreateCommodityAsync(CreateCommodityDto dto,
        CancellationToken cancellationToken = default)
    {
        var assetClass = dto.AssetClass;
        var entity = Commodity.Create(
            dto.Code,
            dto.Name,
            assetClass,
            dto.DefaultUnitId,
            dto.Description);

        await _unitOfWork.Commodities.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetCommodityByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<CommodityDto> UpdateCommodityAsync(CommodityId id, UpdateCommodityDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Commodities.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Commodity with ID {id} not found.");
        }

        var assetClass = dto.AssetClass.GetValueOrDefault(entity.AssetClass);
        entity.Update(
            dto.Name.GetValueOrDefault(entity.Name),
            assetClass,
            dto.DefaultUnitId.GetValueOrDefault(entity.DefaultUnitId),
            dto.Description.GetValueOrDefault(entity.Description));

        _unitOfWork.Commodities.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetCommodityByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task DeleteCommodityAsync(CommodityId id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Commodities.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Commodity with ID {id} not found.");
        }

        _unitOfWork.Commodities.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Investment Transactions

    public async Task<PageResult<InvestmentTransactionDto>> SearchTransactionsAsync(CommodityId commodityId,
        SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        return await SearchWithViewAsync(
            _unitOfWork.InvestmentTransactions,
            request,
            InvestmentTransactionFieldMap.Instance,
            InvestmentTransactionFieldMap.Views,
            t => t.ToDto(),
            cancellationToken,
            t => t.CommodityId == commodityId);
    }

    public async Task<InvestmentTransactionDto?> GetTransactionByIdAsync(InvestmentTransactionId id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.InvestmentTransactions.GetQueryable([t => t.Commodity!, t => t.Unit!])
                .Where(t => t.Id == id),
            cancellationToken);

        return entity?.ToDto();
    }

    public async Task<InvestmentTransactionDto> CreateTransactionAsync(CommodityId commodityId,
        CreateInvestmentTransactionDto dto, CancellationToken cancellationToken = default)
    {
        var commodityExists = await _unitOfWork.Commodities.ExistsAsync(commodityId, cancellationToken);
        if (!commodityExists)
        {
            throw new KeyNotFoundException($"Commodity with ID {commodityId} not found.");
        }

        var entity = InvestmentTransaction.Create(
            commodityId,
            dto.TransactionType,
            dto.TransactionDate,
            dto.Quantity,
            dto.UnitId,
            dto.PricePerUnit,
            dto.TotalAmount,
            dto.FeeAmount,
            dto.FinanceTxnId,
            dto.Notes);

        await _unitOfWork.InvestmentTransactions.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetTransactionByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task DeleteTransactionAsync(InvestmentTransactionId id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.InvestmentTransactions.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Investment transaction with ID {id} not found.");
        }

        _unitOfWork.InvestmentTransactions.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion
}
