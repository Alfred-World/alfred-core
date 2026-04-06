using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.Assets.Dtos;

public sealed record UpdateAssetDto
{
    public Optional<string> Name { get; init; }
    public Optional<CategoryId?> CategoryId { get; init; }
    public Optional<BrandId?> BrandId { get; init; }
    public Optional<DateTime?> PurchaseDate { get; init; }
    public Optional<decimal> InitialCost { get; init; }
    public Optional<DateTime?> WarrantyExpiryDate { get; init; }
    public Optional<string?> Specs { get; init; }
    public Optional<AssetStatus> Status { get; init; }
    public Optional<string?> Location { get; init; }
}
