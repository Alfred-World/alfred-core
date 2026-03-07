using Alfred.Core.Domain.Abstractions;
using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Repositories.Base;

namespace Alfred.Core.Infrastructure.Repositories;

public sealed class CommodityRepository : BaseRepository<Commodity, CommodityId>, ICommodityRepository
{
    public CommodityRepository(IDbContext context) : base(context)
    {
    }
}
