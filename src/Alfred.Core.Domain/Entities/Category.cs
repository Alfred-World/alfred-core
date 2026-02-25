using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Common.Interfaces;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Entities;

public sealed class Category : BaseEntity, IHasCreationTime
{
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public Guid? ParentId { get; private set; }
    public CategoryType Type { get; private set; }
    public string FormSchema { get; private set; } = "[]";

    public DateTime CreatedAt { get; set; }

    // Navigation
    public Category? Parent { get; private set; }
    public ICollection<Category> SubCategories { get; private set; } = new List<Category>();
    public ICollection<BrandCategory> BrandCategories { get; private set; } = new List<BrandCategory>();

    private Category() { }

    public static Category Create(string code, string name, CategoryType type, Guid? parentId = null, string formSchema = "[]")
    {
        return new Category
        {
            Code = code,
            Name = name,
            Type = type,
            ParentId = parentId,
            FormSchema = formSchema ?? "[]",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, Guid? parentId, CategoryType type, string formSchema)
    {
        Name = name;
        ParentId = parentId;
        Type = type;
        FormSchema = formSchema ?? "[]";
    }
}
