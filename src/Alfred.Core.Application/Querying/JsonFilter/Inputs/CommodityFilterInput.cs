using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.Querying.JsonFilter.Inputs;

public sealed class CommodityFilterInput : FilterInputBase<CommodityFilterInput>
{
    public GuidFilterInput? Id { get; set; }
    public StringFilterInput? Code { get; set; }
    public StringFilterInput? Name { get; set; }
    public StringFilterInput? AssetClass { get; set; }
    public GuidFilterInput? DefaultUnitId { get; set; }
    public StringFilterInput? Description { get; set; }
    public DateTimeFilterInput? CreatedAt { get; set; }
}
