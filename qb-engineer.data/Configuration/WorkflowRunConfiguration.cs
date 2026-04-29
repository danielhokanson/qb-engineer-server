using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class WorkflowRunConfiguration : IEntityTypeConfiguration<WorkflowRun>
{
    public void Configure(EntityTypeBuilder<WorkflowRun> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.EntityType).HasMaxLength(64).IsRequired();
        builder.Property(e => e.EntityId).IsRequired();

        builder.Property(e => e.DefinitionId).HasMaxLength(64).IsRequired();
        builder.Property(e => e.CurrentStepId).HasMaxLength(64);

        builder.Property(e => e.Mode).HasMaxLength(16).IsRequired();
        builder.Property(e => e.AbandonedReason).HasMaxLength(64);

        builder.Property(e => e.StartedAt).IsRequired();
        builder.Property(e => e.StartedByUserId).IsRequired();
        builder.Property(e => e.LastActivityAt).IsRequired();

        builder.HasIndex(e => new { e.EntityType, e.EntityId }).IsUnique();
        builder.HasIndex(e => e.DefinitionId);
        builder.HasIndex(e => e.StartedByUserId);
        builder.HasIndex(e => e.LastActivityAt);

        // Workflow Pattern Phase 2 — uint Version for InMemory test compat,
        // bumped manually by AppDbContext.SaveChangesAsync() per IConcurrencyVersioned.
        builder.Property(e => e.Version).HasDefaultValue(1u);

        builder.HasMany(e => e.RunEntities)
            .WithOne(j => j.Run)
            .HasForeignKey(j => j.RunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
