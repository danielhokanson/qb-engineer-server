using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class OidcAuditEventConfiguration : IEntityTypeConfiguration<OidcAuditEvent>
{
    public void Configure(EntityTypeBuilder<OidcAuditEvent> builder)
    {
        builder.Property(e => e.EventType).HasConversion<string>().HasMaxLength(40);
        builder.Property(e => e.ActorIpAddress).HasMaxLength(64);
        builder.Property(e => e.ClientId).HasMaxLength(100);
        builder.Property(e => e.ScopeName).HasMaxLength(100);
        builder.Property(e => e.DetailsJson).HasColumnType("jsonb");

        builder.HasIndex(e => e.ClientId);
        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.CreatedAt);
    }
}
