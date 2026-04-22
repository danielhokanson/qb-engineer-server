using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class OidcRegistrationTicketConfiguration : IEntityTypeConfiguration<OidcRegistrationTicket>
{
    public void Configure(EntityTypeBuilder<OidcRegistrationTicket> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.TicketPrefix).HasMaxLength(16).IsRequired();
        builder.Property(e => e.TicketHash).HasMaxLength(128).IsRequired();
        builder.Property(e => e.AllowedRedirectUriPrefix).HasMaxLength(500).IsRequired();
        builder.Property(e => e.AllowedPostLogoutRedirectUriPrefix).HasMaxLength(500);
        builder.Property(e => e.AllowedScopesCsv).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.RequiredRolesForUsersCsv).HasMaxLength(1000);
        builder.Property(e => e.TrustedPublisherKeyIdsCsv).HasMaxLength(1000);
        builder.Property(e => e.ExpectedClientName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.RedeemedFromIp).HasMaxLength(64);
        builder.Property(e => e.ResultingClientId).HasMaxLength(100);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(e => e.TicketHash).IsUnique();
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ExpiresAt);
    }
}
