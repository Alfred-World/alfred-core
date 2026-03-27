using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class SalesBonusTransactionConfiguration : IEntityTypeConfiguration<SalesBonusTransaction>
{
    public void Configure(EntityTypeBuilder<SalesBonusTransaction> builder)
    {
        builder.ToTable("sales_bonus_transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.Month)
            .IsRequired();

        builder.Property(x => x.OrderCountAtTrigger)
            .IsRequired();

        builder.Property(x => x.OrderThresholdSnapshot)
            .IsRequired();

        builder.Property(x => x.BonusAmountSnapshot)
            .IsRequired()
            .HasColumnType("numeric(15,2)");

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(SalesBonusTransactionStatus.Pending);

        builder.Property(x => x.Note)
            .HasColumnType("text");

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        builder.HasOne(x => x.SoldByMember)
            .WithMany()
            .HasForeignKey(x => x.SoldByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SalesBonusTier)
            .WithMany()
            .HasForeignKey(x => x.SalesBonusTierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProcessedByUser)
            .WithMany()
            .HasForeignKey(x => x.ProcessedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SoldByMemberId);
        builder.HasIndex(x => new { x.SoldByMemberId, x.Year, x.Month });
        builder.HasIndex(x => x.SalesBonusTierId);
    }
}
