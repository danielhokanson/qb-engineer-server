using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class JobSubtaskConfiguration : IEntityTypeConfiguration<JobSubtask>
{
    public void Configure(EntityTypeBuilder<JobSubtask> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.Text).HasMaxLength(500);

        builder.HasOne(e => e.Job)
            .WithMany(j => j.Subtasks)
            .HasForeignKey(e => e.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
