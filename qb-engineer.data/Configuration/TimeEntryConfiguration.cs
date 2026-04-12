using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    public void Configure(EntityTypeBuilder<TimeEntry> builder)
    {
        builder.HasOne(t => t.Job)
            .WithMany()
            .HasForeignKey(t => t.JobId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => new { t.UserId, t.Date });
        builder.HasIndex(t => t.JobId);
        builder.HasIndex(t => t.OperationId);

        builder.Property(t => t.LaborCost).HasPrecision(18, 4);
        builder.Property(t => t.BurdenCost).HasPrecision(18, 4);

        builder.HasOne(t => t.Operation)
            .WithMany()
            .HasForeignKey(t => t.OperationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
