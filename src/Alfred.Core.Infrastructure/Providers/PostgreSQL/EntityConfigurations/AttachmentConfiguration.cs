using Alfred.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("attachments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TargetType)
            .IsRequired()
            .HasMaxLength(50);

        // Polymorphic index for fast lookups
        builder.HasIndex(x => new { x.TargetId, x.TargetType });

        builder.Property(x => x.ObjectKey)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FileSize)
            .IsRequired();

        builder.Property(x => x.Purpose)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Attachment");

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");
    }
}
