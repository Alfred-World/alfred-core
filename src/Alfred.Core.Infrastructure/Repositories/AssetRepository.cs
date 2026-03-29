using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Repositories.Base;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class AssetRepository : BaseRepository<Asset, AssetId>, IAssetRepository
{
    public AssetRepository(IDbContext context) : base(context)
    {
    }
}
