using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Configuration;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasIndex(e => e.WorkLocationId);

        builder.HasOne(e => e.WorkLocation)
            .WithMany()
            .HasForeignKey(e => e.WorkLocationId)
            .OnDelete(DeleteBehavior.SetNull);

        // Phase 3 / WU-06 / C1 — optional rollup role template assignment.
        builder.HasIndex(e => e.RoleTemplateId);

        builder.HasOne(e => e.RoleTemplate)
            .WithMany()
            .HasForeignKey(e => e.RoleTemplateId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
