using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class UserIntegrationConfiguration : IEntityTypeConfiguration<UserIntegration>
{
    public void Configure(EntityTypeBuilder<UserIntegration> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.Category).HasMaxLength(50);
        builder.Property(e => e.ProviderId).HasMaxLength(100);
        builder.Property(e => e.DisplayName).HasMaxLength(200);
        builder.Property(e => e.EncryptedCredentials).HasMaxLength(4000);
        builder.Property(e => e.LastError).HasMaxLength(2000);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.UserId, e.ProviderId }).IsUnique()
            .HasFilter("deleted_at IS NULL");
    }
}
