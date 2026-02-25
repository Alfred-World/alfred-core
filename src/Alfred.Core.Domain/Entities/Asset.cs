using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Entities;

public sealed class Asset : BaseEntity, IHasCreationTime, IHasModificationTime
{
    public string Name { get; private set; } = null!;
    public Guid? CategoryId { get; private set; }
    public Guid? BrandId { get; private set; }
    public DateTime? PurchaseDate { get; private set; }
    public decimal InitialCost { get; private set; }
    public DateTime? WarrantyExpiryDate { get; private set; }
    public string Specs { get; private set; } = "{}";
    public AssetStatus Status { get; private set; } = AssetStatus.Active;
    public string? Location { get; private set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Category? Category { get; private set; }
    public Brand? Brand { get; private set; }

    private Asset() { }

    public static Asset Create(string name, Guid? categoryId, Guid? brandId, DateTime? purchaseDate, decimal initialCost, DateTime? warrantyExpiryDate, string specs, AssetStatus status = AssetStatus.Active, string? location = null)
    {
        return new Asset
        {
            Name = name,
            CategoryId = categoryId,
            BrandId = brandId,
            PurchaseDate = purchaseDate,
            InitialCost = initialCost,
            WarrantyExpiryDate = warrantyExpiryDate,
            Specs = specs ?? "{}",
            Status = status,
            Location = location,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, Guid? categoryId, Guid? brandId, DateTime? purchaseDate, decimal initialCost, DateTime? warrantyExpiryDate, string specs, AssetStatus status, string? location)
    {
        Name = name;
        CategoryId = categoryId;
        BrandId = brandId;
        PurchaseDate = purchaseDate;
        InitialCost = initialCost;
        WarrantyExpiryDate = warrantyExpiryDate;
        Specs = specs ?? "{}";
        Status = status;
        Location = location;
        UpdatedAt = DateTime.UtcNow;
    }
}
