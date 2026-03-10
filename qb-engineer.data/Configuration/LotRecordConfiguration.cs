using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class LotRecordConfiguration : IEntityTypeConfiguration<LotRecord>
{
    public void Configure(EntityTypeBuilder<LotRecord> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.LotNumber).HasMaxLength(100);
        builder.Property(e => e.SupplierLotNumber).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasIndex(e => e.LotNumber).IsUnique();
        builder.HasIndex(e => e.PartId);
        builder.HasIndex(e => e.JobId);
        builder.HasIndex(e => e.ProductionRunId);
        builder.HasIndex(e => e.PurchaseOrderLineId);

        builder.HasOne(e => e.Part)
            .WithMany()
            .HasForeignKey(e => e.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Job)
            .WithMany()
            .HasForeignKey(e => e.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProductionRun)
            .WithMany()
            .HasForeignKey(e => e.ProductionRunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PurchaseOrderLine)
            .WithMany()
            .HasForeignKey(e => e.PurchaseOrderLineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
