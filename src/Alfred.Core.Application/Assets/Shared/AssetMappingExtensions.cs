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
        return new AssetDto
        {
            Id = asset.Id,
            Name = asset.Name,
            CategoryId = asset.CategoryId,
            CategoryName = asset.Category?.Name,
            BrandId = asset.BrandId,
            BrandName = asset.Brand?.Name,
            PurchaseDate = asset.PurchaseDate,
            InitialCost = asset.InitialCost,
            WarrantyExpiryDate = asset.WarrantyExpiryDate,
            Specs = asset.Specs,
            Status = asset.Status.ToString(),
            Location = asset.Location,
            CreatedAt = asset.CreatedAt,
            UpdatedAt = asset.UpdatedAt
        };
    }

    public static AssetLogDto ToDto(this AssetLog log)
    {
        return new AssetLogDto
        {
            Id = log.Id,
            AssetId = log.AssetId,
            EventType = log.EventType.ToString(),
            BrandId = log.BrandId,
            BrandName = log.Brand?.Name,
            PerformedAt = log.PerformedAt,
            Cost = log.Cost,
            Quantity = log.Quantity,
            Note = log.Note,
            FinanceTxnId = log.FinanceTxnId,
            NextDueDate = log.NextDueDate,
            CreatedAt = log.CreatedAt
        };
    }
}
