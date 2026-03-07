using Alfred.Core.Application.Categories.Dtos;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Categories.Shared;

public static class CategoryMappingExtensions
{
    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto(
            category.Id.Value,
            category.Code,
            category.Name,
            category.Icon,
            category.Type,
            category.ParentId?.Value,
            category.Parent?.Name,
            category.FormSchema,
            category.SubCategories.Count,
            category.CreatedAt
        );
    }

    public static CategoryTreeNodeDto ToTreeNode(this Category category)
    {
        return new CategoryTreeNodeDto(
            category.Id.Value,
            category.Code,
            category.Name,
            category.Icon,
            category.Type,
            category.ParentId?.Value,
            category.SubCategories.Count,
            category.SubCategories.Count > 0
        );
    }
}
