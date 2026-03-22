using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.HasIndex(e => e.JobNumber).IsUnique();
        builder.HasIndex(e => new { e.TrackTypeId, e.CurrentStageId });
        builder.HasIndex(e => e.AssigneeId);
        builder.HasIndex(e => e.DueDate);

        builder.Property(e => e.JobNumber).HasMaxLength(20);
        builder.Property(e => e.Title).HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.ExternalId).HasMaxLength(100);
        builder.Property(e => e.ExternalRef).HasMaxLength(100);
        builder.Property(e => e.Provider).HasMaxLength(50);
        builder.Property(e => e.DispositionNotes).HasMaxLength(2000);
        builder.Property(e => e.CustomFieldValues).HasColumnType("jsonb");

        builder.HasOne(e => e.TrackType)
            .WithMany(t => t.Jobs)
            .HasForeignKey(e => e.TrackTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CurrentStage)
            .WithMany(s => s.Jobs)
            .HasForeignKey(e => e.CurrentStageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Jobs)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.PartId);
        builder.HasOne(e => e.Part)
            .WithMany()
            .HasForeignKey(e => e.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ParentJobId);
        builder.HasMany(j => j.ChildJobs)
            .WithOne(j => j.ParentJob)
            .HasForeignKey(j => j.ParentJobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CoverPhotoFileId);
        builder.HasOne(e => e.CoverPhotoFile)
            .WithMany()
            .HasForeignKey(e => e.CoverPhotoFileId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
