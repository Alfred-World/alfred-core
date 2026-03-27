using Alfred.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class OrderAttachmentConfiguration : IEntityTypeConfiguration<OrderAttachment>
{
    public void Configure(EntityTypeBuilder<OrderAttachment> builder)
    {
        builder.ToTable("order_attachments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileType)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.ObjectKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FileSize)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        builder.HasOne(x => x.AccountOrder)
            .WithMany()
            .HasForeignKey(x => x.AccountOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AccountOrderId);
    }
}
