using Alfred.Core.Application.Commodities.Dtos;
using Alfred.Core.Application.Commodities.Shared;
using Alfred.Core.Application.Common;
using Alfred.Core.Application.Querying.Core;
using Alfred.Core.Application.Querying.Filtering.Parsing;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Commodities;

public sealed class CommodityService : BaseApplicationService, ICommodityService
{
    private readonly IUnitOfWork _unitOfWork;

    public CommodityService(
        IUnitOfWork unitOfWork,
        IFilterParser filterParser,
        IAsyncQueryExecutor executor) : base(filterParser, executor)
    {
        _unitOfWork = unitOfWork;
    }

    #region Commodities

    public async Task<PageResult<CommodityDto>> GetAllCommoditiesAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedAsync(
            _unitOfWork.Commodities,
            query,
            CommodityFieldMap.Instance,
            null,
            [c => c.DefaultUnit!],
            c => c.ToDto(),
            cancellationToken);
    }

    public async Task<CommodityDto?> GetCommodityByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Commodities.GetQueryable([c => c.DefaultUnit!])
                .Where(c => c.Id == (CommodityId)id),
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
            dto.DefaultUnitId.HasValue ? (UnitId?)dto.DefaultUnitId.Value : null,
            dto.Description);

        await _unitOfWork.Commodities.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetCommodityByIdAsync(entity.Id.Value, cancellationToken))!;
    }

    public async Task<CommodityDto> UpdateCommodityAsync(Guid id, UpdateCommodityDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Commodities.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Commodity with ID {id} not found.");
        }

        var assetClass = dto.AssetClass;
        entity.Update(
            dto.Name,
            assetClass,
            dto.DefaultUnitId.HasValue ? (UnitId?)dto.DefaultUnitId.Value : null,
            dto.Description);

        _unitOfWork.Commodities.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetCommodityByIdAsync(entity.Id.Value, cancellationToken))!;
    }

    public async Task DeleteCommodityAsync(Guid id, CancellationToken cancellationToken = default)
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

    public async Task<PageResult<InvestmentTransactionDto>> GetTransactionsAsync(Guid commodityId, QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedAsync(
            _unitOfWork.InvestmentTransactions,
            query,
            InvestmentTransactionFieldMap.Instance,
            t => t.CommodityId == (CommodityId)commodityId,
            [t => t.Commodity!, t => t.Unit!],
            t => t.ToDto(),
            cancellationToken);
    }

    public async Task<InvestmentTransactionDto?> GetTransactionByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.InvestmentTransactions.GetQueryable([t => t.Commodity!, t => t.Unit!])
                .Where(t => t.Id == (InvestmentTransactionId)id),
            cancellationToken);

        return entity?.ToDto();
    }

    public async Task<InvestmentTransactionDto> CreateTransactionAsync(Guid commodityId,
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
            (UnitId)dto.UnitId,
            dto.PricePerUnit,
            dto.TotalAmount,
            dto.FeeAmount,
            dto.FinanceTxnId,
            dto.Notes);

        await _unitOfWork.InvestmentTransactions.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetTransactionByIdAsync(entity.Id.Value, cancellationToken))!;
    }

    public async Task DeleteTransactionAsync(Guid id, CancellationToken cancellationToken = default)
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
