using Alfred.Core.Application.Assets.Dtos;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.Assets;

public sealed record CreateAssetRequest
{
    public string Name { get; init; } = null!;
    public Guid? CategoryId { get; init; }
    public Guid? BrandId { get; init; }
    public DateTime? PurchaseDate { get; init; }
    public decimal InitialCost { get; init; }
    public DateTime? WarrantyExpiryDate { get; init; }
    public string Specs { get; init; } = "{}";
    public AssetStatus Status { get; init; } = AssetStatus.Active;
    public string? Location { get; init; }

    public CreateAssetDto ToDto()
    {
        return new CreateAssetDto(Name, CategoryId, BrandId, PurchaseDate, InitialCost, WarrantyExpiryDate, Specs,
            Status, Location);
    }
}
