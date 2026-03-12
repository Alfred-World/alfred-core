using Alfred.Core.Application.Categories.Dtos;
using Alfred.Core.Application.Categories.Shared;
using Alfred.Core.Application.Common;
using Alfred.Core.Application.Common.Settings;
using Alfred.Core.Application.Querying.Core;
using Alfred.Core.Application.Querying.Filtering.Parsing;
using Alfred.Core.Domain.Common.Exceptions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.Categories;

public sealed class CategoryService : BaseApplicationService, ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(
        IUnitOfWork unitOfWork,
        IFilterParser filterParser,
        IAsyncQueryExecutor executor) : base(filterParser, executor)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PageResult<CategoryDto>> GetAllCategoriesAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedAsync(
            _unitOfWork.Categories,
            query,
            CategoryFieldMap.Instance,
            null,
            [c => c.Parent!, c => c.SubCategories],
            c => c.ToDto(),
            cancellationToken);
    }

    public async Task<PageResult<CategoryTreeNodeDto>> GetCategoryTreeAsync(CategoryType? type = null,
        int page = 1, int pageSize = 0, CancellationToken cancellationToken = default)
    {
        page = PaginationSettings.EnsureValidPage(page);
        pageSize = PaginationSettings.ClampPageSize(pageSize);

        var queryable = _unitOfWork.Categories.GetQueryable([c => c.SubCategories])
            .Where(c => c.ParentId == null);

        if (type.HasValue)
        {
            queryable = queryable.Where(c => c.Type == type.Value);
        }

        var total = await _executor.LongCountAsync(queryable, cancellationToken);
        var roots = await _executor.ToListAsync(
            _executor.AsNoTracking(queryable)
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize),
            cancellationToken);

        var items = roots.Select(c => new CategoryTreeNodeDto(
            c.Id.Value, c.Code, c.Name, c.Icon, c.Type,
            c.ParentId?.Value, c.SubCategories.Count, c.SubCategories.Count > 0
        )).ToList();

        return new PageResult<CategoryTreeNodeDto>(items, page, pageSize, total);
    }

    public async Task<List<CategoryTreeNodeDto>> GetChildrenAsync(Guid parentId,
        CancellationToken cancellationToken = default)
    {
        var children = await _executor.ToListAsync(
            _executor.AsNoTracking(
                _unitOfWork.Categories.GetQueryable([c => c.SubCategories])
                    .Where(c => c.ParentId == (CategoryId?)parentId)),
            cancellationToken);

        return children.Select(c => new CategoryTreeNodeDto(
            c.Id.Value, c.Code, c.Name, c.Icon, c.Type,
            c.ParentId?.Value, c.SubCategories.Count, c.SubCategories.Count > 0
        )).ToList();
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Categories.GetQueryable([c => c.Parent!, c => c.SubCategories])
                .Where(c => c.Id == (CategoryId)id),
            cancellationToken);

        return entity?.ToDto();
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        // Validate parent type match
        if (dto.ParentId.HasValue)
        {
            var parent = await _unitOfWork.Categories.GetByIdAsync((CategoryId)dto.ParentId.Value, cancellationToken);

            if (parent is null)
            {
                throw new KeyNotFoundException($"Parent category with ID {dto.ParentId.Value} not found.");
            }

            if (parent.Type != dto.Type)
            {
                throw new DomainException(
                    $"Child category type '{dto.Type}' must match parent category type '{parent.Type}'.");
            }
        }

        var entity = Category.Create(
            dto.Code,
            dto.Name,
            dto.Type,
            dto.Icon,
            dto.ParentId.HasValue ? (CategoryId?)dto.ParentId.Value : null,
            dto.FormSchema);

        await _unitOfWork.Categories.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetCategoryByIdAsync(entity.Id.Value, cancellationToken))!;
    }

    public async Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Categories.GetQueryable().Where(c => c.Id == (CategoryId)id),
            cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"Category with ID {id} not found.");
        }

        // Cannot set self as parent
        if (dto.ParentId.HasValue && dto.ParentId.Value == id)
        {
            throw new DomainException("A category cannot be its own parent.");
        }

        // Validate parent type match
        if (dto.ParentId.HasValue)
        {
            var parent = await _unitOfWork.Categories.GetByIdAsync((CategoryId)dto.ParentId.Value, cancellationToken);

            if (parent is null)
            {
                throw new KeyNotFoundException($"Parent category with ID {dto.ParentId.Value} not found.");
            }

            if (parent.Type != dto.Type)
            {
                throw new DomainException(
                    $"Child category type '{dto.Type}' must match parent category type '{parent.Type}'.");
            }

            // Prevent circular reference: parent must not be a descendant of this category
            if (await IsDescendantAsync(dto.ParentId.Value, id, cancellationToken))
            {
                throw new DomainException("Cannot set a descendant category as parent (circular reference).");
            }
        }

        // Cascade type change to all descendants if type changed
        var typeChanged = entity.Type != dto.Type;

        entity.Update(dto.Name, dto.ParentId.HasValue ? (CategoryId?)dto.ParentId.Value : null,
            dto.Type, dto.Icon, dto.FormSchema);
        _unitOfWork.Categories.Update(entity);

        if (typeChanged)
        {
            await CascadeTypeToDescendantsAsync(id, dto.Type, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetCategoryByIdAsync(entity.Id.Value, cancellationToken))!;
    }

    /// <summary>
    /// Check if <paramref name="candidateId"/> is a descendant of <paramref name="ancestorId"/>.
    /// </summary>
    private async Task<bool> IsDescendantAsync(Guid candidateId, Guid ancestorId,
        CancellationToken cancellationToken)
    {
        var visited = new HashSet<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(ancestorId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            if (!visited.Add(currentId))
            {
                continue;
            }

            var childIds = await _executor.ToListAsync(
                _unitOfWork.Categories.GetQueryable()
                    .Where(c => c.ParentId == (CategoryId?)currentId)
                    .Select(c => c.Id.Value),
                cancellationToken);

            foreach (var childId in childIds)
            {
                if (childId == candidateId)
                {
                    return true;
                }

                queue.Enqueue(childId);
            }
        }

        return false;
    }

    /// <summary>
    /// Recursively update type for all descendants of the given category.
    /// </summary>
    private async Task CascadeTypeToDescendantsAsync(Guid parentId, CategoryType newType,
        CancellationToken cancellationToken)
    {
        var children = await _executor.ToListAsync(
            _unitOfWork.Categories.GetQueryable()
                .Where(c => c.ParentId == (CategoryId?)parentId),
            cancellationToken);

        foreach (var child in children)
        {
            child.Update(child.Name, child.ParentId, newType, child.Icon, child.FormSchema);
            _unitOfWork.Categories.Update(child);
            await CascadeTypeToDescendantsAsync(child.Id.Value, newType, cancellationToken);
        }
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Categories.GetByIdAsync((CategoryId)id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Category with ID {id} not found.");
        }

        _unitOfWork.Categories.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<CategoryCountByTypeDto>> GetCategoryCountsByTypeAsync(
        CancellationToken cancellationToken = default)
    {
        return await _executor.ToListAsync(
            _executor.AsNoTracking(
                _unitOfWork.Categories.GetQueryable()
                    .GroupBy(c => c.Type)
                    .Select(g => new CategoryCountByTypeDto(g.Key, g.Count()))),
            cancellationToken);
    }
}
