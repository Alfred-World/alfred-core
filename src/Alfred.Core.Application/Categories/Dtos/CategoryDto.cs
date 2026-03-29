using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.Categories.Dtos;

public sealed class CategoryDto
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Icon { get; set; }
    public CategoryType Type { get; set; }
    public Guid? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string? FormSchema { get; set; }
    public int SubCategoryCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed record CreateCategoryDto(
    string Code,
    string Name,
    string? Icon,
    CategoryType Type,
    CategoryId? ParentId,
    string FormSchema
);

public sealed record UpdateCategoryDto(
    string Name,
    string? Icon,
    CategoryType Type,
    CategoryId? ParentId,
    string FormSchema
);

public sealed record CategoryTreeNodeDto(
    Guid Id,
    string Code,
    string Name,
    string? Icon,
    CategoryType Type,
    Guid? ParentId,
    int SubCategoryCount,
    bool HasChildren
);

public sealed record CategoryCountByTypeDto(
    CategoryType Type,
    int Count
);
