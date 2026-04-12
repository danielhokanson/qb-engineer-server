using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class LaborRateConfiguration : IEntityTypeConfiguration<LaborRate>
{
    public void Configure(EntityTypeBuilder<LaborRate> builder)
    {
        builder.Property(e => e.StandardRatePerHour).HasPrecision(18, 4);
        builder.Property(e => e.OvertimeRatePerHour).HasPrecision(18, 4);
        builder.Property(e => e.DoubletimeRatePerHour).HasPrecision(18, 4);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasIndex(e => new { e.UserId, e.EffectiveFrom });

        builder.HasOne<QBEngineer.Data.Context.ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
