using Alfred.Core.Application.Categories.Dtos;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.Categories;

public sealed record CreateCategoryRequest
{
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Icon { get; init; }
    public CategoryType Type { get; init; }
    public Guid? ParentId { get; init; }
    public string FormSchema { get; init; } = "[]";

    public CreateCategoryDto ToDto()
    {
        return new CreateCategoryDto(Code, Name, Icon, Type, ParentId, FormSchema);
    }
}
