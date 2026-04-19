using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class TrainingScanLogConfiguration : IEntityTypeConfiguration<TrainingScanLog>
{
    public void Configure(EntityTypeBuilder<TrainingScanLog> builder)
    {
        builder.Property(e => e.ActionType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne<Context.ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.UserId, e.ScannedAt });
        builder.HasIndex(e => e.PartId).HasFilter("part_id IS NOT NULL");
        builder.HasIndex(e => e.JobId).HasFilter("job_id IS NOT NULL");
    }
}
