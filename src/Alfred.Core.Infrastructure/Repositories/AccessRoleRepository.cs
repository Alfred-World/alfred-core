using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Repositories.Base;

using Microsoft.EntityFrameworkCore;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class AccessRoleRepository : BaseRepository<AccessRole, AccessRoleId>, IAccessRoleRepository
{
    public AccessRoleRepository(IDbContext context) : base(context)
    {
    }

    public override IQueryable<AccessRole> GetQueryable()
    {
        return base.GetQueryable().Include(x => x.RolePermissions).ThenInclude(x => x.Permission);
    }

    public override async Task<AccessRole?> GetByIdAsync(AccessRoleId id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var normalized = (roleName ?? string.Empty).Trim().ToUpperInvariant();
        return await DbSet.AnyAsync(x => x.NormalizedName == normalized && !x.IsDeleted, cancellationToken);
    }
}
