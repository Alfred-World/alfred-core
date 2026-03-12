using Alfred.Core.Application.Assets.Dtos;
using Alfred.Core.Application.Assets.Shared;
using Alfred.Core.Application.Common;
using Alfred.Core.Application.Querying.Core;
using Alfred.Core.Application.Querying.Filtering.Parsing;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.Assets;

public sealed class AssetService : BaseApplicationService, IAssetService
{
    private readonly IUnitOfWork _unitOfWork;

    public AssetService(
        IUnitOfWork unitOfWork,
        IFilterParser filterParser,
        IAsyncQueryExecutor executor) : base(filterParser, executor)
    {
        _unitOfWork = unitOfWork;
    }

    #region Assets

    public async Task<PageResult<AssetDto>> GetAllAssetsAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedAsync(_unitOfWork.Assets, query, AssetFieldMap.Instance,
            a => a.ToDto(), cancellationToken);
    }

    public async Task<AssetDto?> GetAssetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Assets.GetQueryable([a => a.Category!, a => a.Brand!])
                .Where(a => a.Id == (AssetId)id),
            cancellationToken);

        return entity?.ToDto();
    }

    public async Task<AssetDto> CreateAssetAsync(CreateAssetDto dto, CancellationToken cancellationToken = default)
    {
        var status = ParseStatus(dto.Status);
        var entity = Asset.Create(
            dto.Name,
            dto.CategoryId.HasValue ? (CategoryId?)dto.CategoryId.Value : null,
            dto.BrandId.HasValue ? (BrandId?)dto.BrandId.Value : null,
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

    public async Task<AssetDto> UpdateAssetAsync(Guid id, UpdateAssetDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Assets.GetByIdAsync((AssetId)id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Asset with ID {id} not found.");
        }

        var status = ParseStatus(dto.Status);
        entity.Update(
            dto.Name,
            dto.CategoryId.HasValue ? (CategoryId?)dto.CategoryId.Value : null,
            dto.BrandId.HasValue ? (BrandId?)dto.BrandId.Value : null,
            dto.PurchaseDate,
            dto.InitialCost,
            dto.WarrantyExpiryDate,
            dto.Specs ?? "{}",
            status,
            dto.Location);

        _unitOfWork.Assets.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task DeleteAssetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Assets.GetByIdAsync((AssetId)id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Asset with ID {id} not found.");
        }

        _unitOfWork.Assets.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Asset Logs

    public async Task<PageResult<AssetLogDto>> GetAssetLogsAsync(Guid assetId, QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        // Pre-filter by assetId; combined with DSL filter inside GetPagedAsync
        return await GetPagedAsync(
            _unitOfWork.AssetLogs,
            query,
            AssetLogFieldMap.Instance,
            l => l.AssetId == (AssetId)assetId,
            l => l.ToDto(),
            cancellationToken);
    }

    public async Task<AssetLogDto?> GetAssetLogByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.AssetLogs.GetQueryable([l => l.Brand!])
                .Where(l => l.Id == (AssetLogId)id),
            cancellationToken);

        return entity?.ToDto();
    }

    public async Task<AssetLogDto> CreateAssetLogAsync(Guid assetId, CreateAssetLogDto dto,
        CancellationToken cancellationToken = default)
    {
        var assetExists = await _unitOfWork.Assets.ExistsAsync((AssetId)assetId, cancellationToken);
        if (!assetExists)
        {
            throw new KeyNotFoundException($"Asset with ID {assetId} not found.");
        }

        if (!Enum.TryParse<AssetLogEventType>(dto.EventType, true, out var eventType))
        {
            throw new InvalidOperationException(
                $"Invalid event type '{dto.EventType}'. Valid values: {string.Join(", ", Enum.GetNames<AssetLogEventType>())}");
        }

        var entity = AssetLog.Create(
            assetId,
            eventType,
            dto.BrandId.HasValue ? (BrandId?)dto.BrandId.Value : null,
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

    public async Task DeleteAssetLogAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.AssetLogs.GetByIdAsync((AssetLogId)id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Asset log with ID {id} not found.");
        }

        _unitOfWork.AssetLogs.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    private static AssetStatus ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return AssetStatus.Active;
        }

        if (!Enum.TryParse<AssetStatus>(status, true, out var parsed))
        {
            throw new InvalidOperationException(
                $"Invalid asset status '{status}'. Valid values: {string.Join(", ", Enum.GetNames<AssetStatus>())}");
        }

        return parsed;
    }
}
