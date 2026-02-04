using System.Linq.Expressions;

namespace Alfred.Core.Application.Querying.Projection;

/// <summary>
/// Defines a named view with its allowed fields for projection
/// </summary>
/// <typeparam name="TEntity">Source entity type</typeparam>
/// <typeparam name="TDto">Target DTO type</typeparam>
public sealed class ViewDefinition<TEntity, TDto>
    where TEntity : class
    where TDto : class, new()
{
    /// <summary>
    /// Name of the view (e.g., "list", "detail", "minimal")
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Fields allowed in this view (camelCase names matching FieldMap keys)
    /// </summary>
    public string[] Fields { get; }

    /// <summary>
    /// Navigation properties to include (for nested field access)
    /// </summary>
    public Expression<Func<TEntity, object>>[]? Includes { get; }

    /// <summary>
    /// Field aliases for mapping DTO property names to different FieldMap keys.
    /// Key = DTO property name (camelCase), Value = FieldMap key
    /// Example: "permissionsSummary" -> "permissionsSummary" (maps to separate lightweight expression)
    /// </summary>
    public Dictionary<string, string> FieldAliases { get; }

    public ViewDefinition(
        string name,
        string[] fields,
        Expression<Func<TEntity, object>>[]? includes = null,
        Dictionary<string, string>? fieldAliases = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Fields = fields ?? throw new ArgumentNullException(nameof(fields));
        Includes = includes;
        FieldAliases = fieldAliases ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Get the FieldMap key for a given DTO field.
    /// Returns the alias if defined, otherwise returns the original field name.
    /// </summary>
    public string GetFieldMapKey(string dtoFieldName)
    {
        return FieldAliases.TryGetValue(dtoFieldName, out var alias) ? alias : dtoFieldName;
    }
}
