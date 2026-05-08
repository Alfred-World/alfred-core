using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.Querying.JsonFilter.Inputs;

public sealed class AssetFilterInput : FilterInputBase<AssetFilterInput>
{
    public GuidFilterInput? Id { get; set; }
    public StringFilterInput? Name { get; set; }
    public GuidFilterInput? CategoryId { get; set; }
    public GuidFilterInput? BrandId { get; set; }
    public StringFilterInput? Status { get; set; }
    public StringFilterInput? Location { get; set; }
    public DateTimeFilterInput? PurchaseDate { get; set; }
    public DateTimeFilterInput? WarrantyExpiryDate { get; set; }
    public DateTimeFilterInput? CreatedAt { get; set; }
    public DateTimeFilterInput? UpdatedAt { get; set; }
}
