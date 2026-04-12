using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class ReportScheduleConfiguration : IEntityTypeConfiguration<ReportSchedule>
{
    public void Configure(EntityTypeBuilder<ReportSchedule> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.CronExpression).HasMaxLength(100).IsRequired();
        builder.Property(e => e.SubjectTemplate).HasMaxLength(500);

        builder.HasIndex(e => e.SavedReportId);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.NextRunAt);

        builder.HasOne(e => e.SavedReport)
            .WithMany()
            .HasForeignKey(e => e.SavedReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
