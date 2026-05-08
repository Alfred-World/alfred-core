using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.Querying.JsonFilter.Inputs;

public sealed class BrandFilterInput : FilterInputBase<BrandFilterInput>
{
    public GuidFilterInput? Id { get; set; }
    public StringFilterInput? Name { get; set; }
    public StringFilterInput? Website { get; set; }
    public StringFilterInput? SupportPhone { get; set; }
    public StringFilterInput? Description { get; set; }
    public StringFilterInput? LogoUrl { get; set; }
    public DateTimeFilterInput? CreatedAt { get; set; }
    public DateTimeFilterInput? UpdatedAt { get; set; }
}
