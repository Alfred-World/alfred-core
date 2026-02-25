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

        // Polymorphic index
        builder.HasIndex(x => new { x.TargetId, x.TargetType });

        builder.Property(x => x.FileUrl)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.FileType)
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(255);

        builder.Property(x => x.UploadedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");
    }
}
