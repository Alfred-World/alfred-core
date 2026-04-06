using Alfred.Core.Application.Assets.Dtos;
using Alfred.Core.Application.Common;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.Assets;

public sealed record UpdateAssetRequest
{
    public Optional<string> Name { get; init; }
    public Optional<Guid?> CategoryId { get; init; }
    public Optional<Guid?> BrandId { get; init; }
    public Optional<DateTime?> PurchaseDate { get; init; }
    public Optional<decimal> InitialCost { get; init; }
    public Optional<DateTime?> WarrantyExpiryDate { get; init; }
    public Optional<string?> Specs { get; init; }
    public Optional<AssetStatus> Status { get; init; }
    public Optional<string?> Location { get; init; }

    public UpdateAssetDto ToDto()
    {
        return new UpdateAssetDto
        {
            Name = Name,
            CategoryId = CategoryId.Map(id => (CategoryId?)id),
            BrandId = BrandId.Map(id => (BrandId?)id),
            PurchaseDate = PurchaseDate,
            InitialCost = InitialCost,
            WarrantyExpiryDate = WarrantyExpiryDate,
            Specs = Specs,
            Status = Status,
            Location = Location
        };
    }
}
