using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> builder)
    {
        builder.Ignore(e => e.IsDeleted);
        // Phase 3 H2 / WU-12: IActiveAware contract member — derived from Status.
        builder.Ignore(e => e.IsActiveForNewTransactions);

        builder.HasIndex(e => e.PartNumber).IsUnique();

        builder.Property(e => e.PartNumber).HasMaxLength(50);
        // Name is the canonical short identifier (required). Indexed because
        // parts are routinely searched/sorted by name in lists, BOM rows, and
        // entity pickers.
        builder.Property(e => e.Name).IsRequired().HasMaxLength(256);
        builder.HasIndex(e => e.Name);
        builder.Property(e => e.Description).HasMaxLength(2000).IsRequired(false);
        builder.Property(e => e.Revision).HasMaxLength(10);
        builder.Property(e => e.Material).HasMaxLength(200);
        builder.Property(e => e.MoldToolRef).HasMaxLength(100);
        builder.Property(e => e.ExternalPartNumber).HasMaxLength(100);
        builder.Property(e => e.ExternalId).HasMaxLength(100);
        builder.Property(e => e.ExternalRef).HasMaxLength(100);
        builder.Property(e => e.Provider).HasMaxLength(50);
        builder.Property(e => e.CustomFieldValues).HasColumnType("jsonb");

        // MRP planning
        builder.Property(e => e.FixedOrderQuantity).HasPrecision(18, 4);
        builder.Property(e => e.MinimumOrderQuantity).HasPrecision(18, 4);
        builder.Property(e => e.OrderMultiple).HasPrecision(18, 4);
        builder.Property(e => e.IsMrpPlanned).HasDefaultValue(false);

        builder.HasIndex(e => e.PreferredVendorId);
        builder.HasOne(e => e.PreferredVendor)
            .WithMany()
            .HasForeignKey(e => e.PreferredVendorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.ToolingAssetId);
        builder.HasOne(e => e.ToolingAsset)
            .WithMany()
            .HasForeignKey(e => e.ToolingAssetId)
            .OnDelete(DeleteBehavior.SetNull);

        // Phase 3 H4 / WU-20 — pointer to active BomRevision. SetNull on
        // the FK so deleting a revision (rare; cascades from the part) does
        // not cascade-orphan-loop. Matched-side relationship is configured
        // on BomRevision.Part (WithMany BomRevisions).
        builder.HasIndex(e => e.CurrentBomRevisionId);
        builder.HasOne(e => e.CurrentBomRevision)
            .WithMany()
            .HasForeignKey(e => e.CurrentBomRevisionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Workflow Pattern Phase 2 / D3 — manual cost override + pointer at
        // active CostCalculation snapshot. SetNull because deleting a calc
        // row should leave the part intact.
        builder.Property(e => e.ManualCostOverride).HasColumnType("decimal(18,4)");
        builder.HasIndex(e => e.CurrentCostCalculationId);
        builder.HasOne(e => e.CurrentCostCalculation)
            .WithMany()
            .HasForeignKey(e => e.CurrentCostCalculationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
