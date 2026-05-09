using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Configuration;

/// <summary>
/// Wave 8 — EF config for <see cref="CommunicationSyncConfig"/>. Indexes the
/// (UserId, Kind, ProviderId) tuple (one connection per user per provider
/// per channel — uniqueness enforced at the application tier on connect)
/// plus UserId alone for the "list my connections" query the user-settings
/// surface needs.
/// </summary>
public class CommunicationSyncConfigConfiguration : IEntityTypeConfiguration<CommunicationSyncConfig>
{
    public void Configure(EntityTypeBuilder<CommunicationSyncConfig> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.UserId, e.Kind, e.ProviderId });

        builder.Property(e => e.Kind).HasConversion<string>().HasMaxLength(16);
        builder.Property(e => e.ProviderId).HasMaxLength(50).IsRequired();
        builder.Property(e => e.DisplayLabel).HasMaxLength(120);
        builder.Property(e => e.ExternalAccountId).HasMaxLength(200);
        builder.Property(e => e.LastSyncedExternalId).HasMaxLength(200);

        // Tokens are encrypted at the service tier (Data Protection API,
        // same pattern QuickBooks adapter uses); the column itself stores
        // the sealed envelope. 4000 is generous — Microsoft Graph access
        // tokens hover ~2.5KB, refresh tokens ~600B.
        builder.Property(e => e.AccessToken).HasMaxLength(4000);
        builder.Property(e => e.RefreshToken).HasMaxLength(4000);

        builder.Property(e => e.ConfigJson).HasColumnType("jsonb");
    }
}
