using Alfred.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class ReferralCommissionSettingConfiguration : IEntityTypeConfiguration<ReferralCommissionSetting>
{
    public void Configure(EntityTypeBuilder<ReferralCommissionSetting> builder)
    {
        builder.ToTable("referral_commission_settings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CommissionPercent)
            .IsRequired()
            .HasColumnType("numeric(5,2)");

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => x.CreatedAt);
    }
}
