using Alfred.Core.Application.Common;
using Alfred.Core.Application.Querying.Core;
using Alfred.Core.Application.Querying.Filtering.Parsing;
using Alfred.Core.Application.Units.Dtos;
using Alfred.Core.Application.Units.Shared;
using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Common.Exceptions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

using Microsoft.EntityFrameworkCore;

namespace Alfred.Core.Application.Units;

public sealed class UnitService : BaseApplicationService, IUnitService
{
    private readonly IUnitRepository _unitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnitService(
        IUnitRepository unitRepository,
        IUnitOfWork unitOfWork,
        IFilterParser filterParser) : base(filterParser)
    {
        _unitRepository = unitRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PageResult<UnitDto>> GetAllUnitsAsync(QueryRequest query,
        CancellationToken cancellationToken = default)
    {
        return await GetPagedAsync(
            _unitRepository,
            query,
            UnitFieldMap.Instance,
            null,
            [u => u.BaseUnit!, u => u.DerivedUnits],
            u => u.ToDto(),
            cancellationToken);
    }

    public async Task<UnitDto?> GetUnitByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitRepository
            .GetQueryable()
            .Include(u => u.BaseUnit)
            .Include(u => u.DerivedUnits)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return entity?.ToDto();
    }

    public async Task<UnitDto> CreateUnitAsync(CreateUnitDto dto, CancellationToken cancellationToken = default)
    {
        // Validate base unit exists
        if (dto.BaseUnitId.HasValue)
        {
            var baseUnit = await _unitRepository.GetByIdAsync(dto.BaseUnitId.Value, cancellationToken);

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

        await _unitRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetUnitByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<UnitDto> UpdateUnitAsync(Guid id, UpdateUnitDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _unitRepository
            .GetQueryable()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"Unit with ID {id} not found.");
        }

        // Cannot set self as base unit
        if (dto.BaseUnitId.HasValue && dto.BaseUnitId.Value == id)
        {
            throw new DomainException("A unit cannot be its own base unit.");
        }

        // Validate base unit exists and category matches
        if (dto.BaseUnitId.HasValue)
        {
            var baseUnit = await _unitRepository.GetByIdAsync(dto.BaseUnitId.Value, cancellationToken);

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

        entity.Update(
            dto.Name,
            dto.Symbol,
            dto.Category,
            dto.BaseUnitId,
            dto.ConversionRate,
            dto.Status,
            dto.Description);

        _unitRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetUnitByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task DeleteUnitAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitRepository
            .GetQueryable()
            .Include(u => u.DerivedUnits)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"Unit with ID {id} not found.");
        }

        if (entity.DerivedUnits.Count > 0)
        {
            throw new DomainException(
                $"Cannot delete unit '{entity.Name}' because it has {entity.DerivedUnits.Count} derived unit(s). Remove or reassign them first.");
        }

        _unitRepository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<UnitTreeNodeDto>> GetBaseUnitTreeAsync(UnitCategory? category = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = _unitRepository.GetQueryable()
            .Include(u => u.DerivedUnits)
            .ThenInclude(d => d.DerivedUnits)
            .Where(u => u.BaseUnitId == null)
            .AsNoTracking();

        if (category.HasValue)
        {
            queryable = queryable.Where(u => u.Category == category.Value);
        }

        var baseUnits = await queryable
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);

        return baseUnits.Select(u => u.ToTreeNode()).ToList();
    }

    public async Task<List<UnitCountByStatusDto>> GetCountsByStatusAsync(
        CancellationToken cancellationToken = default)
    {
        return await _unitRepository.GetQueryable()
            .AsNoTracking()
            .GroupBy(u => u.Status)
            .Select(g => new UnitCountByStatusDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UnitCountByCategoryDto>> GetCountsByCategoryAsync(
        CancellationToken cancellationToken = default)
    {
        return await _unitRepository.GetQueryable()
            .AsNoTracking()
            .GroupBy(u => u.Category)
            .Select(g => new UnitCountByCategoryDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);
    }

    public async Task<ConvertResultDto> ConvertAsync(Guid fromUnitId, Guid toUnitId, decimal value,
        CancellationToken cancellationToken = default)
    {
        var fromUnit = await _unitRepository
            .GetQueryable()
            .Include(u => u.BaseUnit)
            .FirstOrDefaultAsync(u => u.Id == fromUnitId, cancellationToken);

        var toUnit = await _unitRepository
            .GetQueryable()
            .Include(u => u.BaseUnit)
            .FirstOrDefaultAsync(u => u.Id == toUnitId, cancellationToken);

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
