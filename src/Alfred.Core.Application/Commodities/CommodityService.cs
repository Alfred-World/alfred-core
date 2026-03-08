using Alfred.Core.Application.Commodities.Dtos;
using Alfred.Core.Application.Commodities.Shared;
using Alfred.Core.Application.Common;
using Alfred.Core.Application.Querying.Core;
using Alfred.Core.Application.Querying.Filtering.Parsing;
using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.Commodities;

public sealed class CommodityService : BaseApplicationService, ICommodityService
{
    private readonly ICommodityRepository _commodityRepository;
    private readonly IInvestmentTransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CommodityService(
        ICommodityRepository commodityRepository,
        IInvestmentTransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IFilterParser filterParser,
        IAsyncQueryExecutor executor) : base(filterParser, executor)
    {
        _commodityRepository = commodityRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    #region Commodities

    public async Task<PageResult<CommodityDto>> GetAllCommoditiesAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedAsync(
            _commodityRepository,
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
            _commodityRepository.GetQueryable([c => c.DefaultUnit!])
                .Where(c => c.Id == (CommodityId)id),
            cancellationToken);

        return entity?.ToDto();
    }

    public async Task<CommodityDto> CreateCommodityAsync(CreateCommodityDto dto,
        CancellationToken cancellationToken = default)
    {
        var assetClass = ParseAssetClass(dto.AssetClass);
        var entity = Commodity.Create(
            dto.Code,
            dto.Name,
            assetClass,
            dto.DefaultUnitId.HasValue ? (UnitId?)dto.DefaultUnitId.Value : null,
            dto.Description);

        await _commodityRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetCommodityByIdAsync(entity.Id.Value, cancellationToken))!;
    }

    public async Task<CommodityDto> UpdateCommodityAsync(Guid id, UpdateCommodityDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _commodityRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Commodity with ID {id} not found.");
        }

        var assetClass = ParseAssetClass(dto.AssetClass);
        entity.Update(
            dto.Name,
            assetClass,
            dto.DefaultUnitId.HasValue ? (UnitId?)dto.DefaultUnitId.Value : null,
            dto.Description);

        _commodityRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetCommodityByIdAsync(entity.Id.Value, cancellationToken))!;
    }

    public async Task DeleteCommodityAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _commodityRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Commodity with ID {id} not found.");
        }

        _commodityRepository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Investment Transactions

    public async Task<PageResult<InvestmentTransactionDto>> GetTransactionsAsync(Guid commodityId, QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedAsync(
            _transactionRepository,
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
            _transactionRepository.GetQueryable([t => t.Commodity!, t => t.Unit!])
                .Where(t => t.Id == (InvestmentTransactionId)id),
            cancellationToken);

        return entity?.ToDto();
    }

    public async Task<InvestmentTransactionDto> CreateTransactionAsync(Guid commodityId,
        CreateInvestmentTransactionDto dto, CancellationToken cancellationToken = default)
    {
        var commodityExists = await _commodityRepository.ExistsAsync(commodityId, cancellationToken);
        if (!commodityExists)
        {
            throw new KeyNotFoundException($"Commodity with ID {commodityId} not found.");
        }

        if (!Enum.TryParse<InvestmentTransactionType>(dto.TransactionType, true, out var transactionType))
        {
            throw new InvalidOperationException(
                $"Invalid transaction type '{dto.TransactionType}'. Valid values: {string.Join(", ", Enum.GetNames<InvestmentTransactionType>())}");
        }

        var entity = InvestmentTransaction.Create(
            commodityId,
            transactionType,
            dto.TransactionDate,
            dto.Quantity,
            (UnitId)dto.UnitId,
            dto.PricePerUnit,
            dto.TotalAmount,
            dto.FeeAmount,
            dto.FinanceTxnId,
            dto.Notes);

        await _transactionRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetTransactionByIdAsync(entity.Id.Value, cancellationToken))!;
    }

    public async Task DeleteTransactionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _transactionRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Investment transaction with ID {id} not found.");
        }

        _transactionRepository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    private static CommodityAssetClass ParseAssetClass(string? assetClass)
    {
        if (string.IsNullOrWhiteSpace(assetClass))
        {
            return CommodityAssetClass.Metal;
        }

        if (!Enum.TryParse<CommodityAssetClass>(assetClass, true, out var parsed))
        {
            throw new InvalidOperationException(
                $"Invalid asset class '{assetClass}'. Valid values: {string.Join(", ", Enum.GetNames<CommodityAssetClass>())}");
        }

        return parsed;
    }
}
