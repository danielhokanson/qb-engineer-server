using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class MrpSupplyConfiguration : IEntityTypeConfiguration<MrpSupply>
{
    public void Configure(EntityTypeBuilder<MrpSupply> builder)
    {
        builder.HasIndex(e => e.MrpRunId);
        builder.HasIndex(e => e.PartId);

        builder.Property(e => e.Quantity).HasPrecision(18, 4);
        builder.Property(e => e.AllocatedQuantity).HasPrecision(18, 4);

        builder.HasOne(e => e.Part)
            .WithMany()
            .HasForeignKey(e => e.PartId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
