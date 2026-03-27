using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.Categories.Dtos;

public sealed record CategoryDto(
    Guid Id,
    string Code,
    string Name,
    string? Icon,
    CategoryType Type,
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
