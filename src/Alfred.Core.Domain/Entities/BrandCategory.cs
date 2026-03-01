namespace Alfred.Core.Domain.Entities;

public sealed class BrandCategory
{
    public Guid BrandId { get; private set; }
    public Guid CategoryId { get; private set; }

    // Navigation
    public Brand? Brand { get; private set; }
    public Category? Category { get; private set; }

    private BrandCategory()
    {
    }

    public static BrandCategory Create(Guid brandId, Guid categoryId)
    {
        return new BrandCategory
        {
            BrandId = brandId,
            CategoryId = categoryId
        };
    }
}
