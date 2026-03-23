using Alfred.Core.Domain.Entities;

namespace Alfred.Core.Domain.Abstractions;

public interface IAccessRoleRepository : IRepository<AccessRole, AccessRoleId>
{
    Task<bool> ExistsByNameAsync(string roleName, CancellationToken cancellationToken = default);
}
