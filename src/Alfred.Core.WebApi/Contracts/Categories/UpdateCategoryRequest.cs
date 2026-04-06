using Alfred.Core.Application.Categories.Dtos;
using Alfred.Core.Application.Common;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.WebApi.Contracts.Categories;

public sealed record UpdateCategoryRequest
{
    public Optional<string> Name { get; init; }
    public Optional<string?> Icon { get; init; }
    public Optional<CategoryType> Type { get; init; }
    public Optional<Guid?> ParentId { get; init; }
    public Optional<string> FormSchema { get; init; }

    public UpdateCategoryDto ToDto()
    {
        return new UpdateCategoryDto
        {
            Name = Name,
            Icon = Icon,
            Type = Type,
            ParentId = ParentId.Map(id => (CategoryId?)id),
            FormSchema = FormSchema
        };
    }
}
