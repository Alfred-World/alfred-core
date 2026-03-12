using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;
using Alfred.Core.Domain.ValueObjects;

namespace Alfred.Core.Domain.Entities;

public sealed class Brand : BaseEntity<BrandId>, IHasCreationTime, IHasModificationTime
{
    public string Name { get; private set; } = null!;
    public Url Website { get; private set; } = Url.Empty();
    public string? SupportPhone { get; private set; }
    public string? Description { get; private set; }
    public Url LogoUrl { get; private set; } = Url.Empty();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<BrandCategory> BrandCategories { get; private set; } = new List<BrandCategory>();

    private Brand()
    {
        Id = BrandId.New();
    }

    public static Brand Create(
        string name,
        string? website = null,
        string? supportPhone = null,
        string? description = null,
        string? logoUrl = null)
    {
        return new Brand
        {
            Name = name,
            Website = Url.Create(website),
            SupportPhone = supportPhone,
            Description = description,
            LogoUrl = Url.Create(logoUrl),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string? website,
        string? supportPhone,
        string? description,
        string? logoUrl)
    {
        Name = name;
        Website = Url.Create(website);
        SupportPhone = supportPhone;
        Description = description;
        LogoUrl = Url.Create(logoUrl);
    }

    public void UpdateCategories(ICollection<BrandCategory> brandCategories)
    {
        BrandCategories = brandCategories;
    }
}
