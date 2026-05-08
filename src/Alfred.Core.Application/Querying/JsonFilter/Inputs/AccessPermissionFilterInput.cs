using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.Querying.JsonFilter.Inputs;

/// <summary>
/// Typed filter input for AccessPermission entity.
/// <para>
/// Usage: <c>{ "resource": { "eq": "users" }, "action": { "eq": "read" } }</c>
/// </para>
/// </summary>
public sealed class AccessPermissionFilterInput : FilterInputBase<AccessPermissionFilterInput>
{
    public GuidFilterInput? Id { get; set; }
    public StringFilterInput? Code { get; set; }
    public StringFilterInput? Name { get; set; }
    public StringFilterInput? Resource { get; set; }
    public StringFilterInput? Action { get; set; }
    public StringFilterInput? Description { get; set; }
    public BoolFilterInput? IsActive { get; set; }
    public DateTimeFilterInput? CreatedAt { get; set; }
    public DateTimeFilterInput? UpdatedAt { get; set; }
}
