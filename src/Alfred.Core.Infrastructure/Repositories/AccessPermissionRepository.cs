using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Repositories.Base;

using Microsoft.EntityFrameworkCore;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class AccessPermissionRepository : BaseRepository<AccessPermission, AccessPermissionId>,
    IAccessPermissionRepository
{
    public AccessPermissionRepository(IDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = (code ?? string.Empty).Trim().ToLowerInvariant();
        return await DbSet.AnyAsync(x => x.Code == normalized, cancellationToken);
    }
}
