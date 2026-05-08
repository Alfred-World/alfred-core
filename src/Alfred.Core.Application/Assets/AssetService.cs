using Alfred.Core.Application.Assets.Dtos;
using Alfred.Core.Application.Assets.Shared;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.Assets;

public sealed class AssetService : BaseApplicationService, IAssetService
{
    private readonly IUnitOfWork _unitOfWork;

    public AssetService(
        IUnitOfWork unitOfWork,
        IAsyncQueryExecutor executor) : base(executor)
    {
        _unitOfWork = unitOfWork;
    }

    #region Assets

    public async Task<PageResult<AssetDto>> SearchAssetsAsync(SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        return await SearchWithViewAsync(_unitOfWork.Assets, request, AssetFieldMap.Instance,
            AssetFieldMap.Views, a => a.ToDto(), cancellationToken);
    }

    public async Task<AssetDto?> GetAssetByIdAsync(AssetId id, CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Assets.GetQueryable([a => a.Category!, a => a.Brand!])
                .Where(a => a.Id == id),
            cancellationToken);

        return entity?.ToDto();
    }

    public async Task<AssetDto> CreateAssetAsync(CreateAssetDto dto, CancellationToken cancellationToken = default)
    {
        var status = dto.Status;
        var entity = Asset.Create(
            dto.Name,
            dto.CategoryId,
            dto.BrandId,
            dto.PurchaseDate,
            dto.InitialCost,
            dto.WarrantyExpiryDate,
            dto.Specs ?? "{}",
            status,
            dto.Location);

        await _unitOfWork.Assets.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<AssetDto> UpdateAssetAsync(AssetId id, UpdateAssetDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Assets.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Asset with ID {id} not found.");
        }

        entity.Update(
            dto.Name.GetValueOrDefault(entity.Name),
            dto.CategoryId.GetValueOrDefault(entity.CategoryId),
            dto.BrandId.GetValueOrDefault(entity.BrandId),
            dto.PurchaseDate.GetValueOrDefault(entity.PurchaseDate),
            dto.InitialCost.GetValueOrDefault(entity.InitialCost),
            dto.WarrantyExpiryDate.GetValueOrDefault(entity.WarrantyExpiryDate),
            dto.Specs.GetValueOrDefault(entity.Specs) ?? "{}",
            dto.Status.GetValueOrDefault(entity.Status),
            dto.Location.GetValueOrDefault(entity.Location));

        _unitOfWork.Assets.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task DeleteAssetAsync(AssetId id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Assets.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Asset with ID {id} not found.");
        }

        _unitOfWork.Assets.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Asset Logs

    public async Task<PageResult<AssetLogDto>> SearchAssetLogsAsync(AssetId assetId, SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        return await SearchWithViewAsync(
            _unitOfWork.AssetLogs,
            request,
            AssetLogFieldMap.Instance,
            AssetLogFieldMap.Views,
            l => l.ToDto(),
            cancellationToken,
            l => l.AssetId == assetId);
    }

    public async Task<AssetLogDto?> GetAssetLogByIdAsync(AssetLogId id, CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.AssetLogs.GetQueryable([l => l.Brand!])
                .Where(l => l.Id == id),
            cancellationToken);

        return entity?.ToDto();
    }

    public async Task<AssetLogDto> CreateAssetLogAsync(AssetId assetId, CreateAssetLogDto dto,
        CancellationToken cancellationToken = default)
    {
        var assetExists = await _unitOfWork.Assets.ExistsAsync(assetId, cancellationToken);
        if (!assetExists)
        {
            throw new KeyNotFoundException($"Asset with ID {assetId} not found.");
        }

        var entity = AssetLog.Create(
            assetId,
            dto.EventType,
            dto.BrandId,
            dto.PerformedAt,
            dto.Cost,
            dto.Quantity,
            dto.Note,
            dto.FinanceTxnId,
            dto.NextDueDate);

        await _unitOfWork.AssetLogs.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task DeleteAssetLogAsync(AssetLogId id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.AssetLogs.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Asset log with ID {id} not found.");
        }

        _unitOfWork.AssetLogs.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion
}
