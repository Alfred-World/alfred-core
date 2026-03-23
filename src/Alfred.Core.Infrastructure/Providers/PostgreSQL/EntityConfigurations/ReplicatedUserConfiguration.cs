using Alfred.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alfred.Core.Infrastructure.Providers.PostgreSQL.EntityConfigurations;

internal sealed class ReplicatedUserConfiguration : IEntityTypeConfiguration<ReplicatedUser>
{
    public void Configure(EntityTypeBuilder<ReplicatedUser> builder)
    {
        builder.ToTable("replicated_users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(x => x.UserName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.FullName)
            .HasMaxLength(256);

        builder.Property(x => x.Avatar)
            .HasColumnType("text");

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(x => x.Email)
            .HasDatabaseName("IX_replicated_users_Email");

        builder.HasIndex(x => x.UserName)
            .HasDatabaseName("IX_replicated_users_UserName");
    }
}
