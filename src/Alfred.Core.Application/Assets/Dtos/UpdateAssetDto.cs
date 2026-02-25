namespace Alfred.Core.Application.Assets.Dtos;

public sealed record UpdateAssetDto(
    string Name,
    Guid? CategoryId,
    Guid? BrandId,
    DateTime? PurchaseDate,
    decimal InitialCost,
    DateTime? WarrantyExpiryDate,
    string? Specs,
    string? Status,
    string? Location
);
