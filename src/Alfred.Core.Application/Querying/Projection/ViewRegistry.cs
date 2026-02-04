using System.Linq.Expressions;

using Alfred.Core.Application.Querying.Fields;

namespace Alfred.Core.Application.Querying.Projection;

/// <summary>
/// Registry of available views for an entity/DTO pair
/// Provides fluent API to register and lookup views
/// </summary>
/// <typeparam name="TEntity">Source entity type</typeparam>
/// <typeparam name="TDto">Target DTO type</typeparam>
public sealed class ViewRegistry<TEntity, TDto>
    where TEntity : class
    where TDto : class, new()
{
    private readonly Dictionary<string, ViewDefinition<TEntity, TDto>> _views = new(StringComparer.OrdinalIgnoreCase);
    private string? _defaultViewName;

    /// <summary>
    /// Register a view with its fields
    /// </summary>
    public ViewRegistry<TEntity, TDto> Register(
        string name,
        string[] fields,
        Expression<Func<TEntity, object>>[]? includes = null,
        Dictionary<string, string>? fieldAliases = null)
    {
        _views[name] = new ViewDefinition<TEntity, TDto>(name, fields, includes, fieldAliases);
        return this;
    }

    /// <summary>
    /// Register a view with its fields using strongly-typed expressions
    /// </summary>
    public ViewRegistry<TEntity, TDto> Register(
        string name,
        Expression<Func<TDto, object?>>[] fields,
        Expression<Func<TEntity, object>>[]? includes = null)
    {
        var fieldNames = FieldExpressionHelper.GetFieldNames(fields);
        return Register(name, fieldNames, includes);
    }

    /// <summary>
    /// Begin building a view with fluent API
    /// </summary>
    public ViewBuilder View(string name)
    {
        return new ViewBuilder(this, name);
    }

    /// <summary>
    /// Set the default view to use when no view is specified
    /// </summary>
    public ViewRegistry<TEntity, TDto> SetDefault(string viewName)
    {
        if (!_views.ContainsKey(viewName))
        {
            throw new InvalidOperationException($"View '{viewName}' not found. Register it first.");
        }

        _defaultViewName = viewName;
        return this;
    }

    /// <summary>
    /// Get a view by name, or the default view if name is null/empty
    /// </summary>
    public ViewDefinition<TEntity, TDto> GetView(string? viewName)
    {
        if (string.IsNullOrWhiteSpace(viewName))
        {
            return GetDefaultView();
        }

        if (_views.TryGetValue(viewName, out var view))
        {
            return view;
        }

        throw new InvalidOperationException(
            $"View '{viewName}' not found. Available views: {string.Join(", ", _views.Keys)}");
    }

    /// <summary>
    /// Get the default view
    /// </summary>
    public ViewDefinition<TEntity, TDto> GetDefaultView()
    {
        if (_defaultViewName == null)
        {
            throw new InvalidOperationException("No default view set. Call SetDefault() first.");
        }

        return _views[_defaultViewName];
    }

    /// <summary>
    /// Check if a view exists
    /// </summary>
    public bool HasView(string viewName)
    {
        return _views.ContainsKey(viewName);
    }

    /// <summary>
    /// Get all registered view names
    /// </summary>
    public IEnumerable<string> GetViewNames()
    {
        return _views.Keys;
    }

    /// <summary>
    /// Fluent builder for constructing views with field aliases
    /// </summary>
    public sealed class ViewBuilder
    {
        private readonly ViewRegistry<TEntity, TDto> _registry;
        private readonly string _name;
        private readonly List<string> _fields = new();
        private readonly Dictionary<string, string> _fieldAliases = new(StringComparer.OrdinalIgnoreCase);
        private List<Expression<Func<TEntity, object>>>? _includes;

        internal ViewBuilder(ViewRegistry<TEntity, TDto> registry, string name)
        {
            _registry = registry;
            _name = name;
        }

        /// <summary>
        /// Add fields to the view
        /// </summary>
        public ViewBuilder Select(params string[] fields)
        {
            _fields.AddRange(fields);
            return this;
        }

        /// <summary>
        /// Add fields to the view using strongly-typed expressions
        /// </summary>
        public ViewBuilder Select(params Expression<Func<TDto, object?>>[] fields)
        {
            _fields.AddRange(FieldExpressionHelper.GetFieldNames(fields));
            return this;
        }

        /// <summary>
        /// Add a field with an alias (maps DTO property to different FieldMap key)
        /// </summary>
        public ViewBuilder SelectAs(string dtoPropertyName, string fieldMapKey)
        {
            _fields.Add(dtoPropertyName);
            _fieldAliases[dtoPropertyName] = fieldMapKey;
            return this;
        }

        /// <summary>
        /// Add navigation includes
        /// </summary>
        public ViewBuilder Include(params Expression<Func<TEntity, object>>[] includes)
        {
            _includes ??= new List<Expression<Func<TEntity, object>>>();
            _includes.AddRange(includes);
            return this;
        }

        /// <summary>
        /// Build and register the view
        /// </summary>
        public ViewRegistry<TEntity, TDto> Build()
        {
            return _registry.Register(
                _name,
                _fields.ToArray(),
                _includes?.ToArray(),
                _fieldAliases.Count > 0 ? _fieldAliases : null);
        }
    }
}
