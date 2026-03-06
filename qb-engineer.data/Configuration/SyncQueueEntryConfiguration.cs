using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class SyncQueueEntryConfiguration : IEntityTypeConfiguration<SyncQueueEntry>
{
    public void Configure(EntityTypeBuilder<SyncQueueEntry> builder)
    {
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.CreatedAt);

        builder.Property(e => e.EntityType).HasMaxLength(50);
        builder.Property(e => e.Operation).HasMaxLength(50);
        builder.Property(e => e.Payload).HasColumnType("jsonb");
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);
    }
}
