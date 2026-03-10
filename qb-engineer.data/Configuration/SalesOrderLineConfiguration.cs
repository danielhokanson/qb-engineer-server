using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        builder.Ignore(e => e.LineTotal);
        builder.Ignore(e => e.RemainingQuantity);
        builder.Ignore(e => e.IsFullyShipped);

        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 4);

        builder.HasIndex(e => e.SalesOrderId);
        builder.HasIndex(e => e.PartId);

        builder.HasOne(e => e.Part)
            .WithMany()
            .HasForeignKey(e => e.PartId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Jobs)
            .WithOne(j => j.SalesOrderLine)
            .HasForeignKey(j => j.SalesOrderLineId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.ShipmentLines)
            .WithOne(sl => sl.SalesOrderLine)
            .HasForeignKey(sl => sl.SalesOrderLineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
