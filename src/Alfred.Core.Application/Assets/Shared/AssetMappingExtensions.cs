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
            asset.Id,
            asset.Name,
            asset.CategoryId,
            asset.Category?.Name,
            asset.BrandId,
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
            log.Id,
            log.AssetId,
            log.EventType.ToString(),
            log.BrandId,
            log.Brand?.Name,
            log.PerformedAt,
            log.Cost,
            log.Note,
            log.FinanceTxnId,
            log.NextDueDate,
            log.CreatedAt
        );
    }
}
