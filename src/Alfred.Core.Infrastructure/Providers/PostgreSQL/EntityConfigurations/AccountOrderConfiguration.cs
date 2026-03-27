using Alfred.Core.Domain.Entities;
using Alfred.Core.Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class AccountOrderConfiguration : IEntityTypeConfiguration<AccountOrder>
{
    public void Configure(EntityTypeBuilder<AccountOrder> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderCode)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnType("varchar(20)");

        builder.Property(x => x.SoldByUserId)
            .HasColumnType("uuid");

        builder.Property(x => x.WarrantySourceAccountCloneId);

        builder.Property(x => x.ProductVariantNameSnapshot)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.UnitPriceSnapshot)
            .IsRequired()
            .HasColumnType("numeric(15,2)");

        builder.Property(x => x.WarrantyDaysSnapshot)
            .IsRequired();

        builder.Property(x => x.ReferrerMemberId);

        builder.Property(x => x.ReferralCommissionPercentSnapshot)
            .IsRequired()
            .HasColumnType("numeric(5,2)")
            .HasDefaultValue(0m);

        builder.Property(x => x.ReferralCommissionAmountSnapshot)
            .IsRequired()
            .HasColumnType("numeric(15,2)")
            .HasDefaultValue(0m);

        builder.Property(x => x.PurchaseDate)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.WarrantyExpiry)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.OrderNote)
            .HasColumnType("text");

        builder.Property(x => x.WarrantyIssueNote)
            .HasColumnType("text");

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>()
            .HasDefaultValue(AccountOrderStatus.Active);

        builder.Property(x => x.PaymentStatus)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>()
            .HasDefaultValue(PaymentStatus.Pending);

        builder.Property(x => x.IsTrial)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.RefundAmount)
            .IsRequired()
            .HasColumnType("numeric(15,2)")
            .HasDefaultValue(0m);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasOne(x => x.Member)
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReferrerMember)
            .WithMany()
            .HasForeignKey(x => x.ReferrerMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AccountClone)
            .WithMany()
            .HasForeignKey(x => x.AccountCloneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AccountClone>()
            .WithMany()
            .HasForeignKey(x => x.WarrantySourceAccountCloneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProductVariant)
            .WithMany()
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.MemberId);
        builder.HasIndex(x => x.ReferrerMemberId);
        builder.HasIndex(x => x.SoldByUserId);
        builder.HasIndex(x => x.AccountCloneId);
        builder.HasIndex(x => x.WarrantySourceAccountCloneId);
        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.ProductVariantId);
        builder.HasIndex(x => x.WarrantyExpiry);
        builder.HasIndex(x => x.PaymentStatus);
        builder.HasIndex(x => x.OrderCode).IsUnique();
    }
}
