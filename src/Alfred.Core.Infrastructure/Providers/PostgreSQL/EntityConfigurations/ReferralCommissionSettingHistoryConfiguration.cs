using Alfred.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class ReferralCommissionSettingHistoryConfiguration
    : IEntityTypeConfiguration<ReferralCommissionSettingHistory>
{
    public void Configure(EntityTypeBuilder<ReferralCommissionSettingHistory> builder)
    {
        builder.ToTable("referral_commission_setting_histories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PreviousCommissionPercent)
            .IsRequired()
            .HasColumnType("numeric(5,2)");

        builder.Property(x => x.NewCommissionPercent)
            .IsRequired()
            .HasColumnType("numeric(5,2)");

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        builder.HasOne(x => x.ReferralCommissionSetting)
            .WithMany(x => x.Histories)
            .HasForeignKey(x => x.ReferralCommissionSettingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ChangedByUser)
            .WithMany()
            .HasForeignKey(x => x.ChangedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.ReferralCommissionSettingId, x.CreatedAt });
        builder.HasIndex(x => x.ChangedByUserId);
    }
}
