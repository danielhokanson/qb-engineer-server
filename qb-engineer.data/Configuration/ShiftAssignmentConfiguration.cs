using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class ShiftAssignmentConfiguration : IEntityTypeConfiguration<ShiftAssignment>
{
    public void Configure(EntityTypeBuilder<ShiftAssignment> builder)
    {
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.ShiftId);
        builder.HasIndex(e => new { e.UserId, e.EffectiveFrom });

        builder.Property(e => e.ShiftDifferentialRate).HasPrecision(10, 4);
        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder.HasOne(e => e.Shift)
            .WithMany()
            .HasForeignKey(e => e.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
