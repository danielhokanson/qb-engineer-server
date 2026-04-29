using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class WorkflowRunEntityConfiguration : IEntityTypeConfiguration<WorkflowRunEntity>
{
    public void Configure(EntityTypeBuilder<WorkflowRunEntity> builder)
    {
        // Composite primary key per the design doc's Q3 schema.
        builder.HasKey(e => new { e.RunId, e.EntityType, e.EntityId });

        builder.Property(e => e.EntityType).HasMaxLength(64).IsRequired();
        builder.Property(e => e.EntityId).IsRequired();
        builder.Property(e => e.Role).HasMaxLength(32).IsRequired();

        builder.HasIndex(e => new { e.EntityType, e.EntityId });
    }
}
