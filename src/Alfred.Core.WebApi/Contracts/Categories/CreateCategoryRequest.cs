using Alfred.Core.Application.Categories.Dtos;

namespace Alfred.Core.WebApi.Contracts.Categories;

public sealed record CreateCategoryRequest
{
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Icon { get; init; }
    public string Type { get; init; } = null!;
    public Guid? ParentId { get; init; }
    public string FormSchema { get; init; } = "[]";

    public CreateCategoryDto ToDto() =>
        new(Code, Name, Icon, Type, ParentId, FormSchema);
}
