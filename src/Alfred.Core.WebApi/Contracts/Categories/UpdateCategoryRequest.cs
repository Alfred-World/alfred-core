using Alfred.Core.Application.Categories.Dtos;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.Categories;

public sealed record UpdateCategoryRequest
{
    public string Name { get; init; } = null!;
    public string? Icon { get; init; }
    public CategoryType Type { get; init; }
    public Guid? ParentId { get; init; }
    public string FormSchema { get; init; } = "[]";

    public UpdateCategoryDto ToDto()
    {
        return new UpdateCategoryDto(Name, Icon, Type, ParentId, FormSchema);
    }
}
