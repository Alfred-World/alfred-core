using Alfred.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class BrandCategoryConfiguration : IEntityTypeConfiguration<BrandCategory>
{
    public void Configure(EntityTypeBuilder<BrandCategory> builder)
    {
        builder.ToTable("brand_categories");

        builder.HasKey(bc => new { bc.BrandId, bc.CategoryId });

        builder.HasOne(bc => bc.Brand)
            .WithMany(b => b.BrandCategories)
            .HasForeignKey(bc => bc.BrandId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bc => bc.Category)
            .WithMany(c => c.BrandCategories)
            .HasForeignKey(bc => bc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
