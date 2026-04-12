using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class LeavePolicyConfiguration : IEntityTypeConfiguration<LeavePolicy>
{
    public void Configure(EntityTypeBuilder<LeavePolicy> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.AccrualRatePerPayPeriod).HasPrecision(10, 4);
        builder.Property(e => e.MaxBalance).HasPrecision(10, 2);
        builder.Property(e => e.CarryOverLimit).HasPrecision(10, 2);
    }
}
