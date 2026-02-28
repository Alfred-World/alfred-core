using Alfred.Core.Application.Brands.Dtos;
using Alfred.Core.Application.Brands.Shared;
using Alfred.Core.Application.Common;
using Alfred.Core.Application.Querying.Core;
using Alfred.Core.Application.Querying.Filtering.Parsing;
using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Alfred.Core.Application.Brands;

public sealed class BrandService : BaseApplicationService, IBrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BrandService(
        IBrandRepository brandRepository,
        IUnitOfWork unitOfWork,
        IFilterParser filterParser) : base(filterParser)
    {
        _brandRepository = brandRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PageResult<BrandDto>> GetAllBrandsAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedAsync(
            _brandRepository,
            query,
            BrandFieldMap.Instance,
            null,
            [b => b.BrandCategories],
            b => b.ToDto(),
            cancellationToken);
    }

    public async Task<BrandDto?> GetBrandByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _brandRepository
            .GetQueryable()
            .Include(b => b.BrandCategories)
                .ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

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

        await _brandRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        return (await GetBrandByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<BrandDto> UpdateBrandAsync(Guid id, UpdateBrandDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _brandRepository
            .GetQueryable()
            .Include(b => b.BrandCategories)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (entity is null)
            throw new KeyNotFoundException($"Brand with ID {id} not found.");

        entity.Update(
            dto.Name,
            dto.Website,
            dto.SupportPhone,
            dto.Description,
            dto.LogoUrl);

        // Update categories: clear and re-add
        if (dto.CategoryIds is not null)
        {
            entity.BrandCategories.Clear();
            var brandCategories = dto.CategoryIds
                .Select(categoryId => BrandCategory.Create(entity.Id, categoryId))
                .ToList();
            entity.UpdateCategories(brandCategories);
        }

        _brandRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        return (await GetBrandByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task DeleteBrandAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _brandRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            throw new KeyNotFoundException($"Brand with ID {id} not found.");

        _brandRepository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
