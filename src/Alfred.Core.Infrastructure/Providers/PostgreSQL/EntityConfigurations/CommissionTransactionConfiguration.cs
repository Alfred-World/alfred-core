using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class CommissionTransactionConfiguration : IEntityTypeConfiguration<CommissionTransaction>
{
    public void Configure(EntityTypeBuilder<CommissionTransaction> builder)
    {
        builder.ToTable("commission_transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransactionType)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.Amount)
            .IsRequired()
            .HasColumnType("numeric(15,2)");

        builder.Property(x => x.BalanceAfter)
            .IsRequired()
            .HasColumnType("numeric(15,2)");

        builder.Property(x => x.Note)
            .HasColumnType("text");

        builder.Property(x => x.EvidenceObjectKey)
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(CommissionTransactionStatus.Pending);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        builder.HasOne(x => x.Member)
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AccountOrder)
            .WithMany()
            .HasForeignKey(x => x.AccountOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProcessedByUser)
            .WithMany()
            .HasForeignKey(x => x.ProcessedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.MemberId);
        builder.HasIndex(x => x.AccountOrderId);
        builder.HasIndex(x => x.TransactionType);
    }
}
