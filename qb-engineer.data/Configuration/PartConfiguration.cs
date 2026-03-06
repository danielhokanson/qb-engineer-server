using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.HasIndex(e => e.PartNumber).IsUnique();

        builder.Property(e => e.PartNumber).HasMaxLength(50);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Revision).HasMaxLength(10);
        builder.Property(e => e.Material).HasMaxLength(200);
        builder.Property(e => e.MoldToolRef).HasMaxLength(100);
        builder.Property(e => e.ExternalId).HasMaxLength(100);
        builder.Property(e => e.ExternalRef).HasMaxLength(100);
        builder.Property(e => e.Provider).HasMaxLength(50);
        builder.Property(e => e.CustomFieldValues).HasColumnType("jsonb");
    }
}
