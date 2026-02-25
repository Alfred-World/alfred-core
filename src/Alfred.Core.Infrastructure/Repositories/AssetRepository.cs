using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Repositories.Base;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class AssetRepository : BaseRepository<Asset, Guid>, IAssetRepository
{
    public AssetRepository(IDbContext context) : base(context)
    {
    }
}
