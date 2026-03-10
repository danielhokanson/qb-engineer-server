using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class JobPartConfiguration : IEntityTypeConfiguration<JobPart>
{
    public void Configure(EntityTypeBuilder<JobPart> builder)
    {
        builder.HasOne(jp => jp.Job)
            .WithMany()
            .HasForeignKey(jp => jp.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(jp => jp.Part)
            .WithMany()
            .HasForeignKey(jp => jp.PartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(jp => new { jp.JobId, jp.PartId }).IsUnique();

        builder.Property(jp => jp.Notes).HasMaxLength(500);
        builder.Property(jp => jp.Quantity).HasPrecision(10, 2);
    }
}
