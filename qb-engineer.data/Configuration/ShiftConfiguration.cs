using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.NetHours).HasPrecision(8, 2);
    }
}
