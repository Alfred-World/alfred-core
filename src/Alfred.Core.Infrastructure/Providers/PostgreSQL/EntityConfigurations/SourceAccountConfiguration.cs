using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class SourceAccountConfiguration : IEntityTypeConfiguration<SourceAccount>
{
    public void Configure(EntityTypeBuilder<SourceAccount> builder)
    {
        builder.ToTable("source_accounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccountType)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>()
            .HasDefaultValue(AccountProductType.Other);

        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Password)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.TwoFaSecret)
            .HasColumnType("text");

        builder.Property(x => x.RecoveryEmail)
            .HasMaxLength(255);

        builder.Property(x => x.RecoveryPhone)
            .HasMaxLength(50);

        builder.Property(x => x.Notes)
            .HasColumnType("text");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasMany(x => x.Clones)
            .WithOne(x => x.SourceAccount)
            .HasForeignKey(x => x.SourceAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.AccountType);
        builder.HasIndex(x => new { x.AccountType, x.Username });
    }
}
