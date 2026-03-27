using Alfred.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class SalesBonusTierConfiguration : IEntityTypeConfiguration<SalesBonusTier>
{
    public void Configure(EntityTypeBuilder<SalesBonusTier> builder)
    {
        builder.ToTable("sales_bonus_tiers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderThreshold)
            .IsRequired();

        builder.Property(x => x.BonusAmount)
            .IsRequired()
            .HasColumnType("numeric(15,2)");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => x.OrderThreshold)
            .IsUnique();
    }
}
