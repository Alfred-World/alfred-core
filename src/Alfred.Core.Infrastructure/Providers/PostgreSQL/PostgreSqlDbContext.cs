using System.Reflection;

using Alfred.Core.Domain.Entities;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Common.Options;

using Microsoft.EntityFrameworkCore;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL;

/// <summary>
/// PostgreSQL DbContext for EF Core
/// Implements IDbContext to support provider switching
/// </summary>
public class PostgreSqlDbContext : DbContext, IDbContext
{
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<BrandCategory> BrandCategories => Set<BrandCategory>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetLog> AssetLogs => Set<AssetLog>();
    public DbSet<Commodity> Commodities => Set<Commodity>();
    public DbSet<InvestmentTransaction> InvestmentTransactions => Set<InvestmentTransaction>();
    public DbSet<MarketPrice> MarketPrices => Set<MarketPrice>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    private readonly PostgreSqlOptions _options;

    public PostgreSqlDbContext(PostgreSqlOptions options)
    {
        _options = options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_options.ConnectionString, npgsqlOptions =>
        {
            // store migrations history table
            npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history");
        });

        if (_options.EnableDetailedErrors)
        {
            optionsBuilder.EnableDetailedErrors();
        }

        if (_options.EnableSensitiveDataLogging)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Auto-load all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
