using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class JobActivityLogConfiguration : IEntityTypeConfiguration<JobActivityLog>
{
    public void Configure(EntityTypeBuilder<JobActivityLog> builder)
    {
        builder.HasIndex(e => e.JobId);
        builder.HasIndex(e => e.CreatedAt);

        builder.Property(e => e.FieldName).HasMaxLength(100);
        builder.Property(e => e.OldValue).HasMaxLength(1000);
        builder.Property(e => e.NewValue).HasMaxLength(1000);
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasOne(e => e.Job)
            .WithMany(j => j.ActivityLogs)
            .HasForeignKey(e => e.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
