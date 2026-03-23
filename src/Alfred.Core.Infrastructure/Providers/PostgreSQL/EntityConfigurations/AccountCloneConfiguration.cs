using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class AccountCloneConfiguration : IEntityTypeConfiguration<AccountClone>
{
    public void Configure(EntityTypeBuilder<AccountClone> builder)
    {
        builder.ToTable("account_clones");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.ExternalAccountId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Password)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.TwoFaSecret)
            .HasColumnType("text");

        builder.Property(x => x.ExtraInfo)
            .HasColumnType("text");

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>()
            .HasDefaultValue(AccountCloneStatus.Init);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.VerifiedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.SoldAt)
            .HasColumnType("timestamp with time zone");

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ProductId, x.Status });
        builder.HasIndex(x => new { x.ProductId, x.Username }).IsUnique();
        builder.HasIndex(x => x.ExternalAccountId);
    }
}
