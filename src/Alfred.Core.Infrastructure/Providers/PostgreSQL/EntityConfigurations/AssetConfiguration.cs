using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("assets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.InitialCost)
            .HasColumnType("decimal(18, 2)")
            .HasDefaultValue(0m);
            
        builder.Property(x => x.PurchaseDate)
            .HasColumnType("date");

        builder.Property(x => x.WarrantyExpiryDate)
            .HasColumnType("date");

        builder.Property(x => x.Specs)
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");
            
        builder.HasIndex(x => x.Specs)
            .IsTsVectorExpressionIndex("gin"); // GIN Index for JSONB Search

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>()
            .HasDefaultValue(AssetStatus.Active);

        builder.Property(x => x.Location)
            .HasMaxLength(100);

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasIndex(x => x.CategoryId);

        builder.HasOne(x => x.Brand)
            .WithMany()
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");
    }
}
