namespace Alfred.Core.Domain.Entities;

public sealed class BrandCategory
{
    public BrandId BrandId { get; private set; }
    public CategoryId CategoryId { get; private set; }

    // Navigation
    public Brand? Brand { get; private set; }
    public Category? Category { get; private set; }

    private BrandCategory()
    {
    }

    public static BrandCategory Create(BrandId brandId, CategoryId categoryId)
    {
        return new BrandCategory
        {
            BrandId = brandId,
            CategoryId = categoryId
        };
    }
}
