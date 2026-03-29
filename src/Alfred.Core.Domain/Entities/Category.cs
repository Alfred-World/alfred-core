using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Entities;

public sealed class Category : BaseEntity<CategoryId>, IHasCreationTime, IHasModificationTime, IHasDeletionTime,
    IHasCreator, IHasModifier, IHasDeleter
{
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public string? Icon { get; private set; }
    public CategoryId? ParentId { get; private set; }
    public CategoryType Type { get; private set; }
    public string FormSchema { get; private set; } = "[]";

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
    public Guid? DeletedById { get; set; }

    // Navigation
    public Category? Parent { get; private set; }
    public ICollection<Category> SubCategories { get; private set; } = new List<Category>();
    public ICollection<BrandCategory> BrandCategories { get; private set; } = new List<BrandCategory>();

    private Category()
    {
        Id = CategoryId.New();
    }

    public static Category Create(string code, string name, CategoryType type, string? icon = null,
        CategoryId? parentId = null, string formSchema = "[]")
    {
        return new Category
        {
            Code = code,
            Name = name,
            Icon = icon,
            Type = type,
            ParentId = parentId,
            FormSchema = formSchema ?? "[]",
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public void Update(string name, CategoryId? parentId, CategoryType type, string? icon, string formSchema)
    {
        Name = name;
        Icon = icon;
        ParentId = parentId;
        Type = type;
        FormSchema = formSchema ?? "[]";
    }
}
