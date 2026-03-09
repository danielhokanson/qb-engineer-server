using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class BinMovementConfiguration : IEntityTypeConfiguration<BinMovement>
{
    public void Configure(EntityTypeBuilder<BinMovement> builder)
    {
        builder.Property(e => e.EntityType).HasMaxLength(50);
        builder.Property(e => e.LotNumber).HasMaxLength(100);
        builder.Property(e => e.Quantity).HasPrecision(18, 4);

        builder.HasOne(e => e.FromLocation)
            .WithMany()
            .HasForeignKey(e => e.FromLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ToLocation)
            .WithMany()
            .HasForeignKey(e => e.ToLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => e.MovedAt);
    }
}
