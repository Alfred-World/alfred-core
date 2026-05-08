using System.Linq.Expressions;

using Alfred.Core.Domain.Common.Base;
using Alfred.Core.Domain.Querying;

namespace Alfred.Core.Application.Common;

/// <summary>
/// Base service providing common pagination and filtering helpers for all application services.
/// Uses JSON DSL filter system (SearchRequest + FilterExpressionBinder).
/// </summary>
public abstract class BaseApplicationService
{
    protected readonly IAsyncQueryExecutor _executor;

    protected BaseApplicationService(IAsyncQueryExecutor executor)
    {
        _executor = executor;
    }

    /// <summary>
    /// Execute a search query using JSON DSL with structured filter/sort.
    /// </summary>
    protected async Task<PageResult<TDto>> SearchAsync<TEntity, TId, TDto>(
        IRepository<TEntity, TId> repository,
        SearchRequest request,
        BaseFieldMap<TEntity> fieldMap,
        Func<TEntity, TDto> mapper,
        CancellationToken cancellationToken = default,
        Expression<Func<TEntity, bool>>? preFilter = null,
        Expression<Func<TEntity, object>>[]? includes = null)
        where TEntity : BaseEntity<TId>
        where TId : IEquatable<TId>
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        var fields = fieldMap.Fields;

        Expression<Func<TEntity, bool>>? jsonFilter = null;
        if (request.Filter is not null)
        {
            jsonFilter = FilterExpressionBinder<TEntity>.Bind(request.Filter, fields);
        }

        var combinedFilter = CombineFilters(preFilter, jsonFilter);
        var sortString = ConvertSortFieldsToString(request.Order);

        Func<string, (Expression<Func<TEntity, object>>? Expression, bool CanSort)> fieldSelector = fieldName =>
        {
            if (fields.TryGet(fieldName, out var expression, out _))
            {
                var canSort = fields.CanSort(fieldName);
                var objectExpression = ExpressionConverterHelper.ConvertToObjectExpression<TEntity>(expression);
                return (objectExpression, canSort);
            }

            return (null, false);
        };

        var (dbQuery, total) = await repository.BuildPagedQueryAsync(
            combinedFilter,
            sortString,
            page,
            pageSize,
            includes,
            fieldSelector,
            cancellationToken);

        var entities = await _executor.ToListAsync(_executor.AsNoTracking(dbQuery), cancellationToken);
        var items = entities.Select(mapper).ToList();

        return new PageResult<TDto>(items, page, pageSize, total);
    }

    /// <summary>
    /// Execute a search query with View/Projection support using JSON DSL.
    /// </summary>
    protected async Task<PageResult<TDto>> SearchWithViewAsync<TEntity, TId, TDto>(
        IRepository<TEntity, TId> repository,
        SearchRequest request,
        BaseFieldMap<TEntity> fieldMap,
        ViewRegistry<TEntity, TDto>? viewRegistry,
        Func<TEntity, TDto> fallbackMapper,
        CancellationToken cancellationToken = default,
        Expression<Func<TEntity, bool>>? preFilter = null)
        where TEntity : BaseEntity<TId>
        where TId : IEquatable<TId>
        where TDto : class, new()
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        var fields = fieldMap.Fields;

        Expression<Func<TEntity, bool>>? jsonFilter = null;
        if (request.Filter is not null)
        {
            jsonFilter = FilterExpressionBinder<TEntity>.Bind(request.Filter, fields);
        }

        var combinedFilter = CombineFilters(preFilter, jsonFilter);
        var sortString = ConvertSortFieldsToString(request.Order);

        // Resolve view
        ViewDefinition<TEntity, TDto>? view = null;
        if (viewRegistry != null)
        {
            try
            {
                view = viewRegistry.GetView(request.View);
            }
            catch (InvalidOperationException)
            {
                view = null;
            }
        }

        var includes = view?.Includes;

        Func<string, (Expression<Func<TEntity, object>>? Expression, bool CanSort)> fieldSelector = fieldName =>
        {
            if (fields.TryGet(fieldName, out var expression, out _))
            {
                var canSort = fields.CanSort(fieldName);
                var objectExpression = ExpressionConverterHelper.ConvertToObjectExpression<TEntity>(expression);
                return (objectExpression, canSort);
            }

            return (null, false);
        };

        var (dbQuery, total) = await repository.BuildPagedQueryAsync(
            combinedFilter,
            sortString,
            page,
            pageSize,
            includes,
            fieldSelector,
            cancellationToken);

        if (view != null)
        {
            var projected = ProjectionBinder.ApplyProjection(_executor.AsNoTracking(dbQuery), view, fields);
            var items = await _executor.ToListAsync(projected, cancellationToken);
            return new PageResult<TDto>(items, page, pageSize, total);
        }
        else
        {
            var entities = await _executor.ToListAsync(_executor.AsNoTracking(dbQuery), cancellationToken);
            var items = entities.Select(fallbackMapper).ToList();
            return new PageResult<TDto>(items, page, pageSize, total);
        }
    }

    /// <summary>
    /// Convert structured SortField list to the "field:direction,field:direction" string format
    /// used by the existing repository sorting infrastructure.
    /// </summary>
    private static string? ConvertSortFieldsToString(IReadOnlyList<SortField>? order)
    {
        if (order is null or { Count: 0 })
        {
            return null;
        }

        return string.Join(",", order.Select(s =>
            $"{s.Field} {(s.Direction == SortDirection.Desc ? "desc" : "asc")}"));
    }

    private static Expression<Func<TEntity, bool>>? CombineFilters<TEntity>(
        Expression<Func<TEntity, bool>>? left,
        Expression<Func<TEntity, bool>>? right)
    {
        if (left == null)
        {
            return right;
        }

        if (right == null)
        {
            return left;
        }

        var param = left.Parameters[0];
        var rightBody = new ParameterReplacerVisitor(right.Parameters[0], param).Visit(right.Body);
        return Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(left.Body, rightBody), param);
    }

    private sealed class ParameterReplacerVisitor(ParameterExpression oldParam, ParameterExpression newParam)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == oldParam ? newParam : base.VisitParameter(node);
        }
    }
}
