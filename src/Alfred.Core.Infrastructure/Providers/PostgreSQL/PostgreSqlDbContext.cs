using System.Reflection;

using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.ValueObjects;
using Alfred.Core.Infrastructure.Common.Abstractions;
using Alfred.Core.Infrastructure.Common.Converters;
using Alfred.Core.Infrastructure.Common.Options;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL;

/// <summary>
/// PostgreSQL DbContext for EF Core
/// Implements IDbContext to support provider switching
/// </summary>
public class PostgreSqlDbContext : DbContext, IDbContext
{
    public DbSet<AccessRole> AccessRoles => Set<AccessRole>();
    public DbSet<AccessPermission> AccessPermissions => Set<AccessPermission>();
    public DbSet<AccessRolePermission> AccessRolePermissions => Set<AccessRolePermission>();
    public DbSet<AccessUserRole> AccessUserRoles => Set<AccessUserRole>();
    public DbSet<ReferralCommissionSetting> ReferralCommissionSettings => Set<ReferralCommissionSetting>();

    public DbSet<ReferralCommissionSettingHistory> ReferralCommissionSettingHistories =>
        Set<ReferralCommissionSettingHistory>();

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<ReplicatedUser> ReplicatedUsers => Set<ReplicatedUser>();
    public DbSet<AccountClone> AccountClones => Set<AccountClone>();
    public DbSet<AccountOrder> AccountOrders => Set<AccountOrder>();
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

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<ProductId>().HaveConversion<ProductIdConverter>();
        configurationBuilder.Properties<ProductVariantId>().HaveConversion<ProductVariantIdConverter>();
        configurationBuilder.Properties<MemberId>().HaveConversion<MemberIdConverter>();
        configurationBuilder.Properties<AccountCloneId>().HaveConversion<AccountCloneIdConverter>();
        configurationBuilder.Properties<AccountOrderId>().HaveConversion<AccountOrderIdConverter>();
        configurationBuilder.Properties<AccessRoleId>().HaveConversion<AccessRoleIdConverter>();
        configurationBuilder.Properties<AccessPermissionId>().HaveConversion<AccessPermissionIdConverter>();
        configurationBuilder.Properties<ReplicatedUserId>().HaveConversion<ReplicatedUserIdConverter>();
        configurationBuilder.Properties<ReferralCommissionSettingId>()
            .HaveConversion<ReferralCommissionSettingIdConverter>();
        configurationBuilder.Properties<ReferralCommissionSettingHistoryId>()
            .HaveConversion<ReferralCommissionSettingHistoryIdConverter>();
        configurationBuilder.Properties<AssetId>().HaveConversion<AssetIdConverter>();
        configurationBuilder.Properties<CategoryId>().HaveConversion<CategoryIdConverter>();
        configurationBuilder.Properties<BrandId>().HaveConversion<BrandIdConverter>();
        configurationBuilder.Properties<UnitId>().HaveConversion<UnitIdConverter>();
        configurationBuilder.Properties<AssetLogId>().HaveConversion<AssetLogIdConverter>();
        configurationBuilder.Properties<AttachmentId>().HaveConversion<AttachmentIdConverter>();
        configurationBuilder.Properties<CommodityId>().HaveConversion<CommodityIdConverter>();
        configurationBuilder.Properties<InvestmentTransactionId>().HaveConversion<InvestmentTransactionIdConverter>();
        configurationBuilder.Properties<Url>().HaveConversion<UrlConverter>();
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

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey is null || primaryKey.Properties.Count != 1)
            {
                continue;
            }

            var idProperty = primaryKey.Properties[0];
            if (!string.Equals(idProperty.Name, "Id", StringComparison.Ordinal))
            {
                continue;
            }

            idProperty.SetDefaultValueSql("generate_uuid_v7()");
            idProperty.ValueGenerated = ValueGenerated.OnAdd;
        }
    }
}
