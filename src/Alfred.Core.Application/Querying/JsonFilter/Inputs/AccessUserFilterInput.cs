using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.Querying.JsonFilter.Inputs;

/// <summary>
/// Typed filter input for AccessUser (ReplicatedUser) entity.
/// <para>
/// Usage: <c>{ "email": { "contains": "admin" } }</c>
/// Collection: <c>{ "roles": { "some": { "name": { "eq": "Admin" } } } }</c>
/// </para>
/// </summary>
public sealed class AccessUserFilterInput : FilterInputBase<AccessUserFilterInput>
{
    public GuidFilterInput? Id { get; set; }
    public StringFilterInput? UserName { get; set; }
    public StringFilterInput? Email { get; set; }
    public StringFilterInput? FullName { get; set; }
    public StringFilterInput? Avatar { get; set; }
    public DateTimeFilterInput? CreatedAt { get; set; }
    public DateTimeFilterInput? UpdatedAt { get; set; }

    /// <summary>
    /// Filter users by their roles (collection filter).
    /// Example: <c>{ "roles": { "some": { "name": { "eq": "Admin" } } } }</c>
    /// </summary>
    public CollectionFilterInput<AccessRoleFilterInput>? Roles { get; set; }
}
