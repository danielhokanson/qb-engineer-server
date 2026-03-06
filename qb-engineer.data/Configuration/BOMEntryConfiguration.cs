using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class BOMEntryConfiguration : IEntityTypeConfiguration<BOMEntry>
{
    public void Configure(EntityTypeBuilder<BOMEntry> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.Quantity).HasPrecision(18, 4);
        builder.Property(e => e.ReferenceDesignator).HasMaxLength(50);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasOne(e => e.ParentPart)
            .WithMany(p => p.BOMEntries)
            .HasForeignKey(e => e.ParentPartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ChildPart)
            .WithMany(p => p.UsedInBOM)
            .HasForeignKey(e => e.ChildPartId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
