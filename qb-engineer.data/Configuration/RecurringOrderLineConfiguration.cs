using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class RecurringOrderLineConfiguration : IEntityTypeConfiguration<RecurringOrderLine>
{
    public void Configure(EntityTypeBuilder<RecurringOrderLine> builder)
    {
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 4);

        builder.HasIndex(e => e.RecurringOrderId);
        builder.HasIndex(e => e.PartId);

        builder.HasOne(e => e.Part)
            .WithMany()
            .HasForeignKey(e => e.PartId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
