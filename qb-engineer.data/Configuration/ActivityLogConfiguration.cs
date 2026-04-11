using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.UserId);

        builder.Property(e => e.EntityType).HasMaxLength(50);
        builder.Property(e => e.Action).HasMaxLength(50);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.FieldName).HasMaxLength(100);
        builder.Property(e => e.OldValue).HasMaxLength(1000);
        builder.Property(e => e.NewValue).HasMaxLength(1000);
    }
}
