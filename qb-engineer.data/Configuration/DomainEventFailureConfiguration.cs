using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;

namespace QBEngineer.Data.Configuration;

public class DomainEventFailureConfiguration : IEntityTypeConfiguration<DomainEventFailure>
{
    public void Configure(EntityTypeBuilder<DomainEventFailure> builder)
    {
        builder.Property(e => e.EventType).HasMaxLength(200);
        builder.Property(e => e.EventPayload).HasColumnType("text");
        builder.Property(e => e.HandlerName).HasMaxLength(200);
        builder.Property(e => e.ErrorMessage).HasColumnType("text");

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(e => e.Status)
            .HasFilter("status != 'Resolved'");
    }
}
