using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class InvestmentTransactionConfiguration : IEntityTypeConfiguration<InvestmentTransaction>
{
    public void Configure(EntityTypeBuilder<InvestmentTransaction> builder)
    {
        builder.ToTable("investment_transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransactionType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(x => x.TransactionDate)
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.Quantity)
            .HasColumnType("decimal(15, 4)")
            .IsRequired();

        builder.Property(x => x.PricePerUnit)
            .HasColumnType("decimal(18, 2)")
            .IsRequired();

        builder.Property(x => x.TotalAmount)
            .HasColumnType("decimal(18, 2)")
            .IsRequired();

        builder.Property(x => x.FeeAmount)
            .HasColumnType("decimal(18, 2)")
            .HasDefaultValue(0m);

        builder.Property(x => x.Notes)
            .HasColumnType("text");

        builder.HasOne(x => x.Commodity)
            .WithMany()
            .HasForeignKey(x => x.CommodityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CommodityId);

        builder.HasOne(x => x.Unit)
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");
    }
}
