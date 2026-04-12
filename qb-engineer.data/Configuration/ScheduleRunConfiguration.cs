using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class ScheduleRunConfiguration : IEntityTypeConfiguration<ScheduleRun>
{
    public void Configure(EntityTypeBuilder<ScheduleRun> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.ParametersJson).HasColumnType("jsonb");
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);
    }
}
