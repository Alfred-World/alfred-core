using Alfred.Core.Application.Categories.Dtos;
using Alfred.Core.Application.Categories.Shared;
using Alfred.Core.Application.Common;
using Alfred.Core.Application.Querying.Core;
using Alfred.Core.Application.Querying.Filtering.Parsing;
using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

using Microsoft.EntityFrameworkCore;

namespace Alfred.Core.Application.Categories;

public sealed class CategoryService : BaseApplicationService, ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IFilterParser filterParser) : base(filterParser)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PageResult<CategoryDto>> GetAllCategoriesAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedAsync(
            _categoryRepository,
            query,
            CategoryFieldMap.Instance,
            null,
            [c => c.Parent!, c => c.SubCategories],
            c => c.ToDto(),
            cancellationToken);
    }

    public async Task<List<CategoryTreeNodeDto>> GetCategoryTreeAsync(string? type = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = _categoryRepository.GetQueryable()
            .Include(c => c.SubCategories)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<CategoryType>(type, true, out var categoryType))
        {
            queryable = queryable.Where(c => c.Type == categoryType);
        }

        var allCategories = await queryable.ToListAsync(cancellationToken);

        // Build tree from root nodes (no parent)
        var roots = allCategories.Where(c => c.ParentId == null).ToList();

        return roots.Select(r => BuildTreeNode(r, allCategories)).ToList();
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _categoryRepository
            .GetQueryable()
            .Include(c => c.Parent)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return entity?.ToDto();
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<CategoryType>(dto.Type, true, out var categoryType))
            throw new ArgumentException($"Invalid category type: {dto.Type}");

        var entity = Category.Create(
            dto.Code,
            dto.Name,
            categoryType,
            dto.Icon,
            dto.ParentId,
            dto.FormSchema);

        await _categoryRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetCategoryByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _categoryRepository
            .GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (entity is null)
            throw new KeyNotFoundException($"Category with ID {id} not found.");

        if (!Enum.TryParse<CategoryType>(dto.Type, true, out var categoryType))
            throw new ArgumentException($"Invalid category type: {dto.Type}");

        entity.Update(dto.Name, dto.ParentId, categoryType, dto.Icon, dto.FormSchema);

        _categoryRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetCategoryByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            throw new KeyNotFoundException($"Category with ID {id} not found.");

        _categoryRepository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static CategoryTreeNodeDto BuildTreeNode(Category category, List<Category> allCategories)
    {
        var children = allCategories
            .Where(c => c.ParentId == category.Id)
            .Select(c => BuildTreeNode(c, allCategories))
            .ToList();

        return new CategoryTreeNodeDto(
            category.Id,
            category.Code,
            category.Name,
            category.Icon,
            category.Type.ToString(),
            category.ParentId,
            children.Count,
            children);
    }
}
