using Alfred.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class CommissionConfiguration : IEntityTypeConfiguration<Commission>
{
    public void Configure(EntityTypeBuilder<Commission> builder)
    {
        builder.ToTable("commissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AvailableBalance)
            .IsRequired()
            .HasColumnType("numeric(15,2)")
            .HasDefaultValue(0m);

        builder.Property(x => x.TotalEarned)
            .IsRequired()
            .HasColumnType("numeric(15,2)")
            .HasDefaultValue(0m);

        builder.Property(x => x.TotalPaidOut)
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

        builder.HasIndex(x => x.MemberId)
            .IsUnique();
    }
}
