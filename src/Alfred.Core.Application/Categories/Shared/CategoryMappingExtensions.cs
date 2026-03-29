using Alfred.Core.Application.Categories.Dtos;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Categories.Shared;

public static class CategoryMappingExtensions
{
    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            Icon = category.Icon,
            Type = category.Type,
            ParentId = category.ParentId,
            ParentName = category.Parent?.Name,
            FormSchema = category.FormSchema,
            SubCategoryCount = category.SubCategories.Count,
            CreatedAt = category.CreatedAt
        };
    }

    public static CategoryTreeNodeDto ToTreeNode(this Category category)
    {
        return new CategoryTreeNodeDto(
            category.Id,
            category.Code,
            category.Name,
            category.Icon,
            category.Type,
            category.ParentId,
            category.SubCategories.Count,
            category.SubCategories.Count > 0
        );
    }
}
