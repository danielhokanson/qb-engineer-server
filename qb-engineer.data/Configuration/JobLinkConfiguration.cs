using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class JobLinkConfiguration : IEntityTypeConfiguration<JobLink>
{
    public void Configure(EntityTypeBuilder<JobLink> builder)
    {
        builder.HasOne(e => e.SourceJob)
            .WithMany()
            .HasForeignKey(e => e.SourceJobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TargetJob)
            .WithMany()
            .HasForeignKey(e => e.TargetJobId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
