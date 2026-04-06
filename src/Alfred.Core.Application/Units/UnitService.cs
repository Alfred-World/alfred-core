using Alfred.Core.Application.Querying.Filtering.Parsing;
using Alfred.Core.Application.Units.Dtos;
using Alfred.Core.Application.Units.Shared;
using Alfred.Core.Domain.Common.Exceptions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.Units;

public sealed class UnitService : BaseApplicationService, IUnitService
{
    private readonly IUnitOfWork _unitOfWork;

    public UnitService(
        IUnitOfWork unitOfWork,
        IFilterParser filterParser,
        IAsyncQueryExecutor executor) : base(filterParser, executor)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PageResult<UnitDto>> GetAllUnitsAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedWithViewAsync(
            _unitOfWork.Units,
            query,
            UnitFieldMap.Instance,
            UnitFieldMap.Views,
            u => u.ToDto(),
            cancellationToken);
    }

    public async Task<UnitDto?> GetUnitByIdAsync(UnitId id, CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Units.GetQueryable([u => u.BaseUnit!, u => u.DerivedUnits])
                .Where(u => u.Id == id),
            cancellationToken);

        return entity?.ToDto();
    }

    public async Task<UnitDto> CreateUnitAsync(CreateUnitDto dto, CancellationToken cancellationToken = default)
    {
        // Validate base unit exists
        if (dto.BaseUnitId.HasValue)
        {
            var baseUnit = await _unitOfWork.Units.GetByIdAsync(dto.BaseUnitId.Value, cancellationToken);

            if (baseUnit is null)
            {
                throw new KeyNotFoundException($"Base unit with ID {dto.BaseUnitId.Value} not found.");
            }

            if (baseUnit.Category != dto.Category)
            {
                throw new DomainException(
                    $"Unit category '{dto.Category}' must match base unit category '{baseUnit.Category}'.");
            }
        }

        var entity = Unit.Create(
            dto.Code,
            dto.Name,
            dto.Category,
            dto.Symbol,
            dto.BaseUnitId,
            dto.ConversionRate,
            dto.Status,
            dto.Description);

        await _unitOfWork.Units.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetUnitByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<UnitDto> UpdateUnitAsync(UnitId id, UpdateUnitDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Units.GetQueryable().Where(u => u.Id == id),
            cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"Unit with ID {id} not found.");
        }

        var mergedBaseUnitId = dto.BaseUnitId.GetValueOrDefault(entity.BaseUnitId);
        var mergedCategory = dto.Category.GetValueOrDefault(entity.Category);

        // Cannot set self as base unit
        if (mergedBaseUnitId.HasValue && mergedBaseUnitId.Value == id)
        {
            throw new DomainException("A unit cannot be its own base unit.");
        }

        // Validate base unit exists and category matches
        if (mergedBaseUnitId.HasValue)
        {
            var baseUnit = await _unitOfWork.Units.GetByIdAsync(mergedBaseUnitId.Value, cancellationToken);

            if (baseUnit is null)
            {
                throw new KeyNotFoundException($"Base unit with ID {mergedBaseUnitId.Value} not found.");
            }

            if (baseUnit.Category != mergedCategory)
            {
                throw new DomainException(
                    $"Unit category '{mergedCategory}' must match base unit category '{baseUnit.Category}'.");
            }
        }

        entity.Update(
            dto.Name.GetValueOrDefault(entity.Name),
            dto.Symbol.GetValueOrDefault(entity.Symbol),
            mergedCategory,
            mergedBaseUnitId,
            dto.ConversionRate.GetValueOrDefault(entity.ConversionRate),
            dto.Status.GetValueOrDefault(entity.Status),
            dto.Description.GetValueOrDefault(entity.Description));

        _unitOfWork.Units.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetUnitByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task DeleteUnitAsync(UnitId id, CancellationToken cancellationToken = default)
    {
        var entity = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Units.GetQueryable([u => u.DerivedUnits])
                .Where(u => u.Id == id),
            cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"Unit with ID {id} not found.");
        }

        if (entity.DerivedUnits.Count > 0)
        {
            throw new DomainException(
                $"Cannot delete unit '{entity.Name}' because it has {entity.DerivedUnits.Count} derived unit(s). Remove or reassign them first.");
        }

        _unitOfWork.Units.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<UnitTreeNodeDto>> GetBaseUnitTreeAsync(UnitCategory? category = null,
        CancellationToken cancellationToken = default)
    {
        var baseUnits = await _unitOfWork.Units.GetBaseUnitsWithDerivedAsync(category, cancellationToken);
        return baseUnits.Select(u => u.ToTreeNode()).ToList();
    }

    public async Task<List<UnitCountByStatusDto>> GetCountsByStatusAsync(
        CancellationToken cancellationToken = default)
    {
        return await _executor.ToListAsync(
            _executor.AsNoTracking(_unitOfWork.Units.GetQueryable())
                .GroupBy(u => u.Status)
                .Select(g => new UnitCountByStatusDto(g.Key, g.Count())),
            cancellationToken);
    }

    public async Task<List<UnitCountByCategoryDto>> GetCountsByCategoryAsync(
        CancellationToken cancellationToken = default)
    {
        return await _executor.ToListAsync(
            _executor.AsNoTracking(_unitOfWork.Units.GetQueryable())
                .GroupBy(u => u.Category)
                .Select(g => new UnitCountByCategoryDto(g.Key, g.Count())),
            cancellationToken);
    }

    public async Task<ConvertResultDto> ConvertAsync(UnitId fromUnitId, UnitId toUnitId, decimal value,
        CancellationToken cancellationToken = default)
    {
        var fromUnit = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Units.GetQueryable([u => u.BaseUnit!])
                .Where(u => u.Id == fromUnitId),
            cancellationToken);

        var toUnit = await _executor.FirstOrDefaultAsync(
            _unitOfWork.Units.GetQueryable([u => u.BaseUnit!])
                .Where(u => u.Id == toUnitId),
            cancellationToken);

        if (fromUnit is null)
        {
            throw new KeyNotFoundException($"From unit with ID {fromUnitId} not found.");
        }

        if (toUnit is null)
        {
            throw new KeyNotFoundException($"To unit with ID {toUnitId} not found.");
        }

        // Resolve to common base: both must share the same base unit lineage
        var fromBase = ResolveBaseUnit(fromUnit);
        var toBase = ResolveBaseUnit(toUnit);

        if (fromBase.Id != toBase.Id)
        {
            throw new DomainException(
                $"Cannot convert between '{fromUnit.Name}' and '{toUnit.Name}' — they do not share a common base unit.");
        }

        // Convert: fromValue * fromRate / toRate
        var fromRate = fromUnit.BaseUnitId == null ? 1m : fromUnit.ConversionRate;
        var toRate = toUnit.BaseUnitId == null ? 1m : toUnit.ConversionRate;
        var result = value * fromRate / toRate;

        var formula = $"1 {fromUnit.Code} = {fromRate / toRate:G} {toUnit.Code}";

        return new ConvertResultDto(value, fromUnit.Code, result, toUnit.Code, formula);
    }

    private static Unit ResolveBaseUnit(Unit unit)
    {
        var current = unit;

        while (current.BaseUnit != null)
        {
            current = current.BaseUnit;
        }

        return current;
    }
}
