namespace Alfred.Core.Application.Querying.Fields;

/// <summary>
/// Base class for field maps with common querying logic.
/// Navigation includes are now handled via ViewRegistry/ViewDefinition.
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public abstract class BaseFieldMap<TEntity> where TEntity : class
{
    /// <summary>
    /// Field map instance for filtering, sorting, and querying
    /// </summary>
    public abstract FieldMap<TEntity> Fields { get; }
}
