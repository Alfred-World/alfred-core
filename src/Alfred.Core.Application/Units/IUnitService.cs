using Alfred.Core.Application.Units.Dtos;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Application.Units;

public interface IUnitService
{
    Task<PageResult<UnitDto>> GetAllUnitsAsync(QueryRequest query, CancellationToken cancellationToken = default);
    Task<UnitDto?> GetUnitByIdAsync(UnitId id, CancellationToken cancellationToken = default);
    Task<UnitDto> CreateUnitAsync(CreateUnitDto dto, CancellationToken cancellationToken = default);
    Task<UnitDto> UpdateUnitAsync(UnitId id, UpdateUnitDto dto, CancellationToken cancellationToken = default);
    Task DeleteUnitAsync(UnitId id, CancellationToken cancellationToken = default);

    Task<List<UnitTreeNodeDto>> GetBaseUnitTreeAsync(UnitCategory? category = null,
        CancellationToken cancellationToken = default);

    Task<List<UnitCountByStatusDto>> GetCountsByStatusAsync(CancellationToken cancellationToken = default);
    Task<List<UnitCountByCategoryDto>> GetCountsByCategoryAsync(CancellationToken cancellationToken = default);

    Task<ConvertResultDto> ConvertAsync(UnitId fromUnitId, UnitId toUnitId, decimal value,
        CancellationToken cancellationToken = default);
}
