using System.Linq.Expressions;

using Alfred.Core.Application.Common.Exceptions;
using Alfred.Core.Application.Querying.Core;
using Alfred.Core.Application.Querying.Fields;
using Alfred.Core.Application.Querying.Filtering.Binding;
using Alfred.Core.Application.Querying.Filtering.Parsing;
using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Common.Base;

using Microsoft.EntityFrameworkCore;

namespace Alfred.Core.Application.Common;

/// <summary>
/// Base service providing common pagination and filtering helpers for all application services.
/// Eliminates boilerplate filter/sort/page logic across service implementations.
/// </summary>
public abstract class BaseApplicationService
{
    private readonly IFilterParser _filterParser;

    protected BaseApplicationService(IFilterParser filterParser)
    {
        _filterParser = filterParser;
    }

    /// <summary>
    /// Execute a paginated query with DSL filtering and dynamic sorting.
    /// </summary>
    protected async Task<PageResult<TDto>> GetPagedAsync<TEntity, TId, TDto>(
        IRepository<TEntity, TId> repository,
        QueryRequest query,
        BaseFieldMap<TEntity> fieldMap,
        Func<TEntity, TDto> mapper,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity<TId>
        where TId : IEquatable<TId>
    {
        return await GetPagedAsync(repository, query, fieldMap, null, null, mapper, cancellationToken);
    }

    /// <summary>
    /// Execute a paginated query with an optional pre-filter (e.g. FK constraint), DSL filtering, and sorting.
    /// Pre-filter is combined with DSL filter using AND.
    /// </summary>
    protected async Task<PageResult<TDto>> GetPagedAsync<TEntity, TId, TDto>(
        IRepository<TEntity, TId> repository,
        QueryRequest query,
        BaseFieldMap<TEntity> fieldMap,
        Expression<Func<TEntity, bool>>? preFilter,
        Func<TEntity, TDto> mapper,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity<TId>
        where TId : IEquatable<TId>
    {
        return await GetPagedAsync(repository, query, fieldMap, preFilter, null, mapper, cancellationToken);
    }

    /// <summary>
    /// Full overload with pre-filter + includes support.
    /// </summary>
    protected async Task<PageResult<TDto>> GetPagedAsync<TEntity, TId, TDto>(
        IRepository<TEntity, TId> repository,
        QueryRequest query,
        BaseFieldMap<TEntity> fieldMap,
        Expression<Func<TEntity, bool>>? preFilter,
        Expression<Func<TEntity, object>>[]? includes,
        Func<TEntity, TDto> mapper,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity<TId>
        where TId : IEquatable<TId>
    {
        var page = query.GetEffectivePage();
        var pageSize = query.GetEffectivePageSize();
        var fields = fieldMap.Fields;

        Expression<Func<TEntity, bool>>? dslFilter = null;
        if (!string.IsNullOrWhiteSpace(query.Filter))
        {
            try
            {
                var ast = _filterParser.Parse(query.Filter);
                dslFilter = EfFilterBinder<TEntity>.Bind(ast, fields);
            }
            catch (InvalidOperationException ex)
            {
                throw FilterExceptionHelper.CreateFilterException(ex, query.Filter, fields);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Invalid filter syntax: {ex.Message}", ex);
            }
        }

        var combinedFilter = CombineFilters(preFilter, dslFilter);

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
            query.Sort,
            page,
            pageSize,
            includes,
            fieldSelector,
            cancellationToken);

        var entities = await dbQuery.AsNoTracking().ToListAsync(cancellationToken);
        var items = entities.Select(mapper).ToList();

        return new PageResult<TDto>(items, page, pageSize, total);
    }

    /// <summary>
    /// Parse a DSL filter string into an expression for a given entity type.
    /// Returns null if the filter string is empty.
    /// </summary>
    protected Expression<Func<TEntity, bool>>? ParseFilter<TEntity>(string? filter, FieldMap<TEntity> fieldMap)
        where TEntity : class
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return null;
        }

        try
        {
            var ast = _filterParser.Parse(filter);
            return EfFilterBinder<TEntity>.Bind(ast, fieldMap);
        }
        catch (InvalidOperationException ex)
        {
            throw FilterExceptionHelper.CreateFilterException(ex, filter, fieldMap);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Invalid filter syntax: {ex.Message}", ex);
        }
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
            => node == oldParam ? newParam : base.VisitParameter(node);
    }
}
