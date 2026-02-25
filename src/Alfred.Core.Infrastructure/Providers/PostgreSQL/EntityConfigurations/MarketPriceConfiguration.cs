using Alfred.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class MarketPriceConfiguration : IEntityTypeConfiguration<MarketPrice>
{
    public void Configure(EntityTypeBuilder<MarketPrice> builder)
    {
        builder.ToTable("market_prices");

        // Composite PK for TimescaleDB Hypertable compatibility
        builder.HasKey(x => new { x.Time, x.CommodityId });

        builder.Property(x => x.Time)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.BuyPrice)
            .HasColumnType("decimal(18, 2)")
            .IsRequired();

        builder.Property(x => x.SellPrice)
            .HasColumnType("decimal(18, 2)")
            .IsRequired();

        builder.Property(x => x.Source)
            .HasMaxLength(100);

        builder.HasOne(x => x.Commodity)
            .WithMany()
            .HasForeignKey(x => x.CommodityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
