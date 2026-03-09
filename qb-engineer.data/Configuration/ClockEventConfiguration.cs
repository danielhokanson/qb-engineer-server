using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class ClockEventConfiguration : IEntityTypeConfiguration<ClockEvent>
{
    public void Configure(EntityTypeBuilder<ClockEvent> builder)
    {
        builder.Property(c => c.Reason).HasMaxLength(100);
        builder.Property(c => c.ScanMethod).HasMaxLength(50);
        builder.Property(c => c.Source).HasMaxLength(50);

        builder.HasIndex(c => new { c.UserId, c.Timestamp });
    }
}
