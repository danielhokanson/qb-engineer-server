using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class ShipmentLineConfiguration : IEntityTypeConfiguration<ShipmentLine>
{
    public void Configure(EntityTypeBuilder<ShipmentLine> builder)
    {
        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder.HasIndex(e => e.ShipmentId);
        builder.HasIndex(e => e.SalesOrderLineId);
    }
}
