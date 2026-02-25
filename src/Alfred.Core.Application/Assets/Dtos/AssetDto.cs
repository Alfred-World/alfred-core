namespace Alfred.Core.Application.Assets.Dtos;

public sealed record AssetDto(
    Guid Id,
    string Name,
    Guid? CategoryId,
    string? CategoryName,
    Guid? BrandId,
    string? BrandName,
    DateTime? PurchaseDate,
    decimal InitialCost,
    DateTime? WarrantyExpiryDate,
    string Specs,
    string Status,
    string? Location,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
