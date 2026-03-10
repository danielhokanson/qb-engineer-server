using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class PlanningCycleEntryConfiguration : IEntityTypeConfiguration<PlanningCycleEntry>
{
    public void Configure(EntityTypeBuilder<PlanningCycleEntry> builder)
    {
        builder.HasIndex(e => new { e.PlanningCycleId, e.JobId }).IsUnique();

        builder.HasOne(e => e.Job)
            .WithMany(j => j.PlanningCycleEntries)
            .HasForeignKey(e => e.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
