using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;

namespace Alfred.Core.Domain.Entities;

public sealed class Brand : BaseEntity<BrandId>, IHasCreationTime, IHasModificationTime
{
    public string Name { get; private set; } = null!;
    public string? Website { get; private set; }
    public string? SupportPhone { get; private set; }
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }

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
            Website = website,
            SupportPhone = supportPhone,
            Description = description,
            LogoUrl = logoUrl,
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
        Website = website;
        SupportPhone = supportPhone;
        Description = description;
        LogoUrl = logoUrl;
    }

    public void UpdateCategories(ICollection<BrandCategory> brandCategories)
    {
        BrandCategories = brandCategories;
    }
}
