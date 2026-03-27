using Alfred.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class MemberMonthlySalesSummaryConfiguration : IEntityTypeConfiguration<MemberMonthlySalesSummary>
{
    public void Configure(EntityTypeBuilder<MemberMonthlySalesSummary> builder)
    {
        builder.ToTable("member_monthly_sales_summaries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.Month)
            .IsRequired();

        builder.Property(x => x.OrderCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.TotalBonusEarned)
            .IsRequired()
            .HasColumnType("numeric(15,2)")
            .HasDefaultValue(0m);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasOne(x => x.SoldByMember)
            .WithMany()
            .HasForeignKey(x => x.SoldByMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.HighestTierReached)
            .WithMany()
            .HasForeignKey(x => x.HighestTierReachedId)
            .OnDelete(DeleteBehavior.Restrict);

        // One record per referrer member per month
        builder.HasIndex(x => new { x.SoldByMemberId, x.Year, x.Month })
            .IsUnique();
    }
}
