using Alfred.Core.Application.AccountSales.Dtos;
using Alfred.Core.Application.AccountSales.Shared;
using Alfred.Core.Application.Querying.Core;
using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Application.AccountSales;

public sealed partial class AccountSalesService
{
    public async Task<PageResult<ProductDto>> GetProductsAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedAsync(_unitOfWork.Products, query, ProductFieldMap.Instance,
            p => p.ToDto(),
            cancellationToken,
            includes: [p => p.Variants]);
    }

    public async Task<ProductDto?> GetProductByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Products.GetQueryable([x => x.Variants]).Where(x => x.Id == id),
            cancellationToken);

        return entity?.ToDto();
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto,
        CancellationToken cancellationToken = default)
    {
        var normalizedVariants = NormalizeCreateVariants(dto.Variants);
        var entity = Product.Create(dto.Name, dto.ProductType, dto.Description);

        await _unitOfWork.Products.AddAsync(entity, cancellationToken);

        foreach (var variant in normalizedVariants)
        {
            await _unitOfWork.ProductVariants.AddAsync(
                ProductVariant.Create(entity.Id, variant.Name, variant.Price, variant.WarrantyDays),
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Products.GetQueryable([x => x.Variants]).Where(x => x.Id == entity.Id),
            cancellationToken);

        return (created ?? entity).ToDto();
    }

    public async Task<ProductDto> UpdateProductAsync(ProductId id, UpdateProductDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Products.GetQueryable([x => x.Variants]).Where(x => x.Id == id),
            cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        }

        var normalizedVariants = NormalizeUpdateVariants(dto.Variants);

        entity.Update(dto.Name, dto.ProductType, dto.Description);

        var existingVariants = await _executor.ToListAsync(
            _unitOfWork.ProductVariants.GetQueryable().Where(x => x.ProductId == entity.Id),
            cancellationToken);

        foreach (var variant in existingVariants)
        {
            _unitOfWork.ProductVariants.Delete(variant);
        }

        foreach (var variant in normalizedVariants)
        {
            await _unitOfWork.ProductVariants.AddAsync(
                ProductVariant.Create(entity.Id, variant.Name, variant.Price, variant.WarrantyDays),
                cancellationToken);
        }

        _unitOfWork.Products.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Products.GetQueryable([x => x.Variants]).Where(x => x.Id == entity.Id),
            cancellationToken);

        return (updated ?? entity).ToDto();
    }
}
