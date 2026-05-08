using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.Querying.JsonFilter.Inputs;

public sealed class CategoryFilterInput : FilterInputBase<CategoryFilterInput>
{
    public GuidFilterInput? Id { get; set; }
    public StringFilterInput? Code { get; set; }
    public StringFilterInput? Name { get; set; }
    public StringFilterInput? Type { get; set; }
    public GuidFilterInput? ParentId { get; set; }
    public StringFilterInput? Icon { get; set; }
    public DateTimeFilterInput? CreatedAt { get; set; }
}
