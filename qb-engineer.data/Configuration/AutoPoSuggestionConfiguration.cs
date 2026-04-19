using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class AutoPoSuggestionConfiguration : IEntityTypeConfiguration<AutoPoSuggestion>
{
    public void Configure(EntityTypeBuilder<AutoPoSuggestion> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.SourceSalesOrderIds).HasColumnType("jsonb");

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(e => e.Part)
            .WithMany()
            .HasForeignKey(e => e.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Vendor)
            .WithMany()
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ConvertedPurchaseOrder)
            .WithMany()
            .HasForeignKey(e => e.ConvertedPurchaseOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.PartId, e.Status });
        builder.HasIndex(e => e.VendorId);
        builder.HasIndex(e => e.ConvertedPurchaseOrderId)
            .HasFilter("converted_purchase_order_id IS NOT NULL");
    }
}
