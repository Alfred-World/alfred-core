using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.Assets.Dtos;

public sealed record CreateAssetDto(
    string Name,
    CategoryId? CategoryId,
    BrandId? BrandId,
    DateTime? PurchaseDate,
    decimal InitialCost,
    DateTime? WarrantyExpiryDate,
    string? Specs,
    AssetStatus Status,
    string? Location
);
