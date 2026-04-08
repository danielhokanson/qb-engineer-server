using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Instructions).HasMaxLength(4000);
        builder.Property(e => e.QcCriteria).HasMaxLength(1000);

        builder.HasIndex(e => e.PartId);
        builder.HasIndex(e => e.ReferencedOperationId);

        builder.HasOne(e => e.Part)
            .WithMany(p => p.Operations)
            .HasForeignKey(e => e.PartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.WorkCenter)
            .WithMany()
            .HasForeignKey(e => e.WorkCenterId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ReferencedOperation)
            .WithMany()
            .HasForeignKey(e => e.ReferencedOperationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
