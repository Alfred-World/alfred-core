using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Repositories.Base;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class AssetLogRepository : BaseRepository<AssetLog, AssetLogId>, IAssetLogRepository
{
    public AssetLogRepository(IDbContext context) : base(context)
    {
    }
}
