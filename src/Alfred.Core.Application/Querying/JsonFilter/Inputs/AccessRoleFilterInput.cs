using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.Querying.JsonFilter.Inputs;

/// <summary>
/// Typed filter input for AccessRole entity.
/// <para>
/// Usage: <c>{ "name": { "contains": "admin" }, "isSystem": { "eq": true } }</c>
/// Collection: <c>{ "permissions": { "some": { "resource": { "eq": "users" } } } }</c>
/// </para>
/// </summary>
public sealed class AccessRoleFilterInput : FilterInputBase<AccessRoleFilterInput>
{
    public GuidFilterInput? Id { get; set; }
    public StringFilterInput? Name { get; set; }
    public StringFilterInput? NormalizedName { get; set; }
    public BoolFilterInput? IsImmutable { get; set; }
    public BoolFilterInput? IsSystem { get; set; }
    public StringFilterInput? Icon { get; set; }
    public BoolFilterInput? IsDeleted { get; set; }
    public DateTimeFilterInput? CreatedAt { get; set; }
    public DateTimeFilterInput? UpdatedAt { get; set; }

    /// <summary>
    /// Filter roles by their permissions (collection filter).
    /// Example: <c>{ "permissions": { "some": { "code": { "eq": "users.read" } } } }</c>
    /// </summary>
    public CollectionFilterInput<AccessPermissionFilterInput>? Permissions { get; set; }
}
