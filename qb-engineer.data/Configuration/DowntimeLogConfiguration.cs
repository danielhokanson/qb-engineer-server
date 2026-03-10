using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class DowntimeLogConfiguration : IEntityTypeConfiguration<DowntimeLog>
{
    public void Configure(EntityTypeBuilder<DowntimeLog> builder)
    {
        builder.Ignore(e => e.IsDeleted);
        builder.Ignore(e => e.DurationHours);

        builder.Property(e => e.Reason).HasMaxLength(500);
        builder.Property(e => e.Resolution).HasMaxLength(500);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasIndex(e => e.AssetId);
        builder.HasIndex(e => e.StartedAt);

        builder.HasOne(e => e.Asset)
            .WithMany()
            .HasForeignKey(e => e.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
