using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class ForecastOverrideConfiguration : IEntityTypeConfiguration<ForecastOverride>
{
    public void Configure(EntityTypeBuilder<ForecastOverride> builder)
    {
        builder.HasIndex(e => e.DemandForecastId);

        builder.Property(e => e.OriginalQuantity).HasPrecision(18, 4);
        builder.Property(e => e.OverrideQuantity).HasPrecision(18, 4);
        builder.Property(e => e.Reason).HasMaxLength(1000);
    }
}
