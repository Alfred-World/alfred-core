using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Entities;

public sealed class Product : BaseEntity<ProductId>, IHasCreationTime, IHasModificationTime
{
    public string Name { get; private set; } = null!;
    public AccountProductType ProductType { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ProductVariant> Variants { get; } = new List<ProductVariant>();

    private Product()
    {
        Id = ProductId.New();
    }

    public static Product Create(string name, AccountProductType productType, string? description)
    {
        return new Product
        {
            Name = name,
            ProductType = productType,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, AccountProductType productType, string? description)
    {
        Name = name;
        ProductType = productType;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
}
