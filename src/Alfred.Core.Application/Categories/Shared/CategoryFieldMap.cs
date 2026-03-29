using System.Linq.Expressions;

using Alfred.Core.Application.Categories.Dtos;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Categories.Shared;

public sealed class CategoryFieldMap : BaseFieldMap<Category>
{
    private static readonly Lazy<CategoryFieldMap> _instance = new(() => new CategoryFieldMap());

    private CategoryFieldMap()
    {
    }

    public static CategoryFieldMap Instance => _instance.Value;

    public override FieldMap<Category> Fields { get; } = new FieldMap<Category>()
        .Add("id", c => c.Id).AllowAll()
        .Add("code", c => c.Code).AllowAll()
        .Add("name", c => c.Name).AllowAll()
        .Add("type", c => c.Type).AllowAll()
        .Add("parentId", c => c.ParentId!).AllowAll()
        .Add("icon", c => c.Icon!).AllowAll()
        .Add("formSchema", c => c.FormSchema).AllowAll()
        .Add("parentName", c => c.Parent!.Name).Selectable()
        .Add("subCategoryCount", c => c.SubCategories.Count()).Selectable()
        .Add("createdAt", c => c.CreatedAt).Sortable().Selectable();

    public static ViewRegistry<Category, CategoryDto> Views { get; } =
        new ViewRegistry<Category, CategoryDto>()
            .Register("list", new Expression<Func<CategoryDto, object?>>[]
            {
                x => x.Id,
                x => x.Code,
                x => x.Name,
                x => x.Icon,
                x => x.Type,
                x => x.ParentId,
                x => x.ParentName,
                x => x.FormSchema,
                x => x.SubCategoryCount,
                x => x.CreatedAt
            })
            .SetDefault("list");
}
