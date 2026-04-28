using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class ReceivingRecordConfiguration : IEntityTypeConfiguration<ReceivingRecord>
{
    public void Configure(EntityTypeBuilder<ReceivingRecord> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.ReceivedBy).HasMaxLength(200);
        builder.Property(e => e.Notes).HasMaxLength(1000);
        // Phase 3 / WU-23 (F8-broad): decimal(18,4) for UoM-aware fractional qty.
        builder.Property(e => e.QuantityReceived).HasPrecision(18, 4);

        builder.HasIndex(e => e.PurchaseOrderLineId);

        builder.HasOne(e => e.StorageLocation)
            .WithMany()
            .HasForeignKey(e => e.StorageLocationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
