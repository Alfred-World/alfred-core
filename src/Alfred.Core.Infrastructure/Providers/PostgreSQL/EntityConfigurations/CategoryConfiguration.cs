using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>(); // Store enum as string

        builder.Property(x => x.FormSchema)
            .HasColumnType("jsonb")
            .HasDefaultValue("[]");

        // Self-referencing relationship
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.SubCategories)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");
    }
}
