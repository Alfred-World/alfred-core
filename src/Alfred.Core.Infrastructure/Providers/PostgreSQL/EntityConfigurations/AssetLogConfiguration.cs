using Alfred.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class AssetLogConfiguration : IEntityTypeConfiguration<AssetLog>
{
    public void Configure(EntityTypeBuilder<AssetLog> builder)
    {
        builder.ToTable("asset_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.Property(x => x.PerformedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.Cost)
            .HasColumnType("decimal(18, 2)")
            .HasDefaultValue(0m);

        builder.Property(x => x.Note)
            .HasColumnType("text");

        builder.Property(x => x.NextDueDate)
            .HasColumnType("date");

        builder.HasOne(x => x.Asset)
            .WithMany()
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AssetId);

        builder.HasOne(x => x.Brand)
            .WithMany()
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");
    }
}
