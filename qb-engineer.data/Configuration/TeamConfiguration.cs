using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.Name).HasMaxLength(100);
        builder.Property(e => e.Color).HasMaxLength(20);
        builder.Property(e => e.Description).HasMaxLength(500);
    }
}
