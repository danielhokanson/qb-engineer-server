using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class TrainingPathEnrollmentConfiguration : IEntityTypeConfiguration<TrainingPathEnrollment>
{
    public void Configure(EntityTypeBuilder<TrainingPathEnrollment> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.HasIndex(e => new { e.UserId, e.PathId }).IsUnique();
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.AssignedByUserId);

        builder.HasOne<QBEngineer.Data.Context.ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<QBEngineer.Data.Context.ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.AssignedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
