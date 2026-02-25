using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;

namespace Alfred.Core.Domain.Entities;

public sealed class Brand : BaseEntity, IHasCreationTime
{
    public string Name { get; private set; } = null!;
    public string? Website { get; private set; }
    public string? SupportPhone { get; private set; }

    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<BrandCategory> BrandCategories { get; private set; } = new List<BrandCategory>();

    private Brand() { }

    public static Brand Create(string name, string? website = null, string? supportPhone = null)
    {
        return new Brand
        {
            Name = name,
            Website = website,
            SupportPhone = supportPhone,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? website, string? supportPhone)
    {
        Name = name;
        Website = website;
        SupportPhone = supportPhone;
    }
}
