using Alfred.Core.Application.Commodities.Dtos;
using Alfred.Core.Application.Commodities.Shared;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.Commodities;

public sealed class CommodityService : BaseApplicationService, ICommodityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CommodityService(
        IUnitOfWork unitOfWork,
        IAsyncQueryExecutor executor,
        ICurrentUser currentUser) : base(executor)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
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
        var userId = GetCurrentUserId();

        return await SearchWithViewAsync(
            _unitOfWork.InvestmentTransactions,
            request,
            InvestmentTransactionFieldMap.Instance,
            InvestmentTransactionFieldMap.Views,
            t => t.ToDto(),
            cancellationToken,
            t => t.CommodityId == commodityId && t.CreatedById == userId);
    }

    public async Task<InvestmentTransactionDto?> GetTransactionByIdAsync(CommodityId commodityId,
        InvestmentTransactionId id,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetOwnedTransactionEntityAsync(commodityId, id, cancellationToken, includeDetails: true);

        return entity?.ToDto();
    }

    public async Task<InvestmentTransactionDto> CreateTransactionAsync(CommodityId commodityId,
        CreateInvestmentTransactionDto dto, CancellationToken cancellationToken = default)
    {
        _ = GetCurrentUserId();

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

        return (await GetTransactionByIdAsync(commodityId, entity.Id, cancellationToken))!;
    }

    public async Task DeleteTransactionAsync(CommodityId commodityId, InvestmentTransactionId id,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetOwnedTransactionEntityAsync(commodityId, id, cancellationToken, includeDetails: false);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Investment transaction with ID {id} not found.");
        }

        _unitOfWork.InvestmentTransactions.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    private Guid GetCurrentUserId()
    {
        return _currentUser.UserId ?? throw new UnauthorizedAccessException("Current user id is missing.");
    }

    private async Task<InvestmentTransaction?> GetOwnedTransactionEntityAsync(CommodityId commodityId,
        InvestmentTransactionId id,
        CancellationToken cancellationToken,
        bool includeDetails)
    {
        var userId = GetCurrentUserId();
        var query = includeDetails
            ? _unitOfWork.InvestmentTransactions.GetQueryable([t => t.Commodity!, t => t.Unit!])
            : _unitOfWork.InvestmentTransactions.GetQueryable();

        return await _executor.FirstOrDefaultAsync(
            query.Where(t => t.CommodityId == commodityId && t.Id == id && t.CreatedById == userId),
            cancellationToken);
    }
}
