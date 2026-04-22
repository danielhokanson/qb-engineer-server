using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class OidcClientMetadataConfiguration : IEntityTypeConfiguration<OidcClientMetadata>
{
    public void Configure(EntityTypeBuilder<OidcClientMetadata> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.ClientId).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.OwnerEmail).HasMaxLength(200);
        builder.Property(e => e.RequiredRolesCsv).HasMaxLength(1000);
        builder.Property(e => e.AllowedCustomScopesCsv).HasMaxLength(2000);
        builder.Property(e => e.Notes).HasMaxLength(4000);
        builder.Property(e => e.RegistrationAccessTokenHash).HasMaxLength(128);

        builder.HasIndex(e => e.ClientId).IsUnique();
        builder.HasIndex(e => e.Status);

        builder.HasOne(e => e.RegistrationTicket)
            .WithMany()
            .HasForeignKey(e => e.RegistrationTicketId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
