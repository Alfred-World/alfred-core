using Alfred.Core.Application.Categories.Dtos;

namespace Alfred.Core.WebApi.Contracts.Categories;

public sealed record UpdateCategoryRequest
{
    public string Name { get; init; } = null!;
    public string? Icon { get; init; }
    public string Type { get; init; } = null!;
    public Guid? ParentId { get; init; }
    public string FormSchema { get; init; } = "[]";

    public UpdateCategoryDto ToDto() =>
        new(Name, Icon, Type, ParentId, FormSchema);
}
