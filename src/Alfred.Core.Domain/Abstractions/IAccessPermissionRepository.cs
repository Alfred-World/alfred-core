using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Domain.Abstractions;

public interface IAccessPermissionRepository : IRepository<AccessPermission, AccessPermissionId>
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
}
