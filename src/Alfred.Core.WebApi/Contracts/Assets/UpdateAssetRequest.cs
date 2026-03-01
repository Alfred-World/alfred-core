using Alfred.Core.Application.Assets.Dtos;

namespace Alfred.Core.WebApi.Contracts.Assets;

public sealed record UpdateAssetRequest
{
    public string Name { get; init; } = null!;
    public Guid? CategoryId { get; init; }
    public Guid? BrandId { get; init; }
    public DateTime? PurchaseDate { get; init; }
    public decimal InitialCost { get; init; }
    public DateTime? WarrantyExpiryDate { get; init; }
    public string Specs { get; init; } = "{}";
    public string? Status { get; init; }
    public string? Location { get; init; }

    public UpdateAssetDto ToDto()
    {
        return new UpdateAssetDto(Name, CategoryId, BrandId, PurchaseDate, InitialCost, WarrantyExpiryDate, Specs,
            Status, Location);
    }
}
