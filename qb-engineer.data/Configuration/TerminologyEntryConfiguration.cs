using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class TerminologyEntryConfiguration : IEntityTypeConfiguration<TerminologyEntry>
{
    public void Configure(EntityTypeBuilder<TerminologyEntry> builder)
    {
        builder.Property(e => e.Key).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Label).HasMaxLength(500).IsRequired();

        builder.HasIndex(e => e.Key).IsUnique();
    }
}
