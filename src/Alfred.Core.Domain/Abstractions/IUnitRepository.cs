using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

namespace Alfred.Core.Domain.Abstractions;

public interface IUnitRepository : IRepository<Unit, UnitId>
{
    Task<List<Unit>> GetBaseUnitsWithDerivedAsync(UnitCategory? category, CancellationToken ct = default);
}
