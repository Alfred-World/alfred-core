using Alfred.Core.Application.Categories.Dtos;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Categories.Shared;

public static class CategoryMappingExtensions
{
    public static CategoryDto ToDto(this Category category) => new(
        category.Id,
        category.Code,
        category.Name,
        category.Icon,
        category.Type.ToString(),
        category.ParentId,
        category.Parent?.Name,
        category.FormSchema,
        category.SubCategories.Count,
        category.CreatedAt
    );

    public static CategoryTreeNodeDto ToTreeNode(this Category category) => new(
        category.Id,
        category.Code,
        category.Name,
        category.Icon,
        category.Type.ToString(),
        category.ParentId,
        category.SubCategories.Count,
        category.SubCategories.Select(c => c.ToTreeNode()).ToList()
    );
}
