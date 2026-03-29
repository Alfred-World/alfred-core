namespace Alfred.Core.Domain.Entities;

public sealed class ProductVariant : BaseEntity<ProductVariantId>, IHasCreationTime, IHasModificationTime
{
    public ProductId ProductId { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal Price { get; private set; }
    public int WarrantyDays { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Product? Product { get; private set; }

    private ProductVariant()
    {
        Id = ProductVariantId.New();
    }

    public static ProductVariant Create(ProductId productId, string name, decimal price, int warrantyDays)
    {
        return new ProductVariant
        {
            ProductId = productId,
            Name = name,
            Price = Math.Max(0m, decimal.Round(price, 2, MidpointRounding.AwayFromZero)),
            WarrantyDays = Math.Max(0, warrantyDays),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, decimal price, int warrantyDays)
    {
        Name = name;
        Price = Math.Max(0m, decimal.Round(price, 2, MidpointRounding.AwayFromZero));
        WarrantyDays = Math.Max(0, warrantyDays);
        UpdatedAt = DateTime.UtcNow;
    }
}
