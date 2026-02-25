using Alfred.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Alfred.Core.Infrastructure.Common.Abstractions;

/// <summary>
/// Abstraction for DbContext to allow swapping database providers
/// </summary>
public interface IDbContext
{
    DbSet<Unit> Units { get; }
    DbSet<Category> Categories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<BrandCategory> BrandCategories { get; }
    DbSet<Asset> Assets { get; }
    DbSet<AssetLog> AssetLogs { get; }
    DbSet<Commodity> Commodities { get; }
    DbSet<InvestmentTransaction> InvestmentTransactions { get; }
    DbSet<MarketPrice> MarketPrices { get; }
    DbSet<Attachment> Attachments { get; }
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    DatabaseFacade Database { get; }
}
