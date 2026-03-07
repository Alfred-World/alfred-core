using Alfred.Core.Application.Assets.Dtos;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Assets.Shared;

/// <summary>
/// Extension methods for mapping Asset domain entities to DTOs.
/// </summary>
public static class AssetMappingExtensions
{
    public static AssetDto ToDto(this Asset asset)
    {
        return new AssetDto(
            asset.Id.Value,
            asset.Name,
            asset.CategoryId?.Value,
            asset.Category?.Name,
            asset.BrandId?.Value,
            asset.Brand?.Name,
            asset.PurchaseDate,
            asset.InitialCost,
            asset.WarrantyExpiryDate,
            asset.Specs,
            asset.Status.ToString(),
            asset.Location,
            asset.CreatedAt,
            asset.UpdatedAt
        );
    }

    public static AssetLogDto ToDto(this AssetLog log)
    {
        return new AssetLogDto(
            log.Id.Value,
            log.AssetId.Value,
            log.EventType.ToString(),
            log.BrandId?.Value,
            log.Brand?.Name,
            log.PerformedAt,
            log.Cost,
            log.Quantity,
            log.Note,
            log.FinanceTxnId,
            log.NextDueDate,
            log.CreatedAt
        );
    }
}
