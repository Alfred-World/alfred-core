namespace Alfred.Core.Application.Categories.Dtos;

public sealed record CategoryDto(
    Guid Id,
    string Code,
    string Name,
    string? Icon,
    string Type,
    Guid? ParentId,
    string? ParentName,
    string FormSchema,
    int SubCategoryCount,
    DateTime CreatedAt
);

public sealed record CreateCategoryDto(
    string Code,
    string Name,
    string? Icon,
    string Type,
    Guid? ParentId,
    string FormSchema
);

public sealed record UpdateCategoryDto(
    string Name,
    string? Icon,
    string Type,
    Guid? ParentId,
    string FormSchema
);

public sealed record CategoryTreeNodeDto(
    Guid Id,
    string Code,
    string Name,
    string? Icon,
    string Type,
    Guid? ParentId,
    int SubCategoryCount,
    List<CategoryTreeNodeDto> Children
);
