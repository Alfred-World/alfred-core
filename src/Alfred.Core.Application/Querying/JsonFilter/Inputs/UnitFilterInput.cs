using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.Querying.JsonFilter.Inputs;

public sealed class UnitFilterInput : FilterInputBase<UnitFilterInput>
{
    public GuidFilterInput? Id { get; set; }
    public StringFilterInput? Code { get; set; }
    public StringFilterInput? Name { get; set; }
    public StringFilterInput? Symbol { get; set; }
    public StringFilterInput? Category { get; set; }
    public StringFilterInput? Status { get; set; }
    public GuidFilterInput? BaseUnitId { get; set; }
    public StringFilterInput? Description { get; set; }
    public DateTimeFilterInput? CreatedAt { get; set; }
    public DateTimeFilterInput? UpdatedAt { get; set; }
}
