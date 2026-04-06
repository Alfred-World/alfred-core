using System.Linq.Expressions;

using Alfred.Core.Application.Brands.Dtos;
using Alfred.Core.Application.Brands.Shared;
using Alfred.Core.Application.Querying.Filtering.Parsing;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.Brands;

public sealed class BrandService : BaseApplicationService, IBrandService
{
    private readonly IUnitOfWork _unitOfWork;

    public BrandService(
        IUnitOfWork unitOfWork,
        IFilterParser filterParser,
        IAsyncQueryExecutor executor) : base(filterParser, executor)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PageResult<BrandDto>> GetAllBrandsAsync(QueryRequest query,
        CategoryId? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<Brand, bool>>? preFilter = categoryId.HasValue
            ? b => b.BrandCategories.Any(bc => bc.CategoryId == categoryId.Value)
            : null;

        return await GetPagedWithViewAsync(
            _unitOfWork.Brands,
            query,
            BrandFieldMap.Instance,
            BrandFieldMap.Views,
            b => b.ToDto(),
            cancellationToken,
            preFilter);
    }

    public async Task<BrandDto?> GetBrandByIdAsync(BrandId id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Brands.GetByIdWithCategoriesAsync(id, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<BrandDto> CreateBrandAsync(CreateBrandDto dto, CancellationToken cancellationToken = default)
    {
        var entity = Brand.Create(
            dto.Name,
            dto.Website,
            dto.SupportPhone,
            dto.Description,
            dto.LogoUrl);

        if (dto.CategoryIds is { Count: > 0 })
        {
            var brandCategories = dto.CategoryIds
                .Select(categoryId => BrandCategory.Create(entity.Id, categoryId))
                .ToList();
            entity.UpdateCategories(brandCategories);
        }

        await _unitOfWork.Brands.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        return (await GetBrandByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<BrandDto> UpdateBrandAsync(BrandId id, UpdateBrandDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Brands.GetByIdWithCategoriesAsync(id, cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"Brand with ID {id} not found.");
        }

        entity.Update(
            dto.Name.GetValueOrDefault(entity.Name),
            dto.Website.GetValueOrDefault(entity.Website.Value),
            dto.SupportPhone.GetValueOrDefault(entity.SupportPhone),
            dto.Description.GetValueOrDefault(entity.Description),
            dto.LogoUrl.GetValueOrDefault(entity.LogoUrl.Value));

        // Update categories only if explicitly provided
        if (dto.CategoryIds.HasValue)
        {
            var categoryIds = dto.CategoryIds.Value;
            entity.BrandCategories.Clear();
            if (categoryIds is { Count: > 0 })
            {
                var brandCategories = categoryIds
                    .Select(categoryId => BrandCategory.Create(entity.Id, categoryId))
                    .ToList();
                entity.UpdateCategories(brandCategories);
            }
        }

        _unitOfWork.Brands.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        return (await GetBrandByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task DeleteBrandAsync(BrandId id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Brands.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Brand with ID {id} not found.");
        }

        _unitOfWork.Brands.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
