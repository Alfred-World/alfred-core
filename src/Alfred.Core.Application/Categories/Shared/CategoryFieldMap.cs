using Alfred.Core.Application.Querying.Fields;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Categories.Shared;

public sealed class CategoryFieldMap : BaseFieldMap<Category>
{
    private static readonly Lazy<CategoryFieldMap> _instance = new(() => new CategoryFieldMap());

    private CategoryFieldMap() { }

    public static CategoryFieldMap Instance => _instance.Value;

    public override FieldMap<Category> Fields { get; } = new FieldMap<Category>()
        .Add("id", c => c.Id).AllowAll()
        .Add("code", c => c.Code).AllowAll()
        .Add("name", c => c.Name).AllowAll()
        .Add("type", c => c.Type).AllowAll()
        .Add("parentId", c => c.ParentId!).AllowAll()
        .Add("createdAt", c => c.CreatedAt).Sortable().Selectable();
}
