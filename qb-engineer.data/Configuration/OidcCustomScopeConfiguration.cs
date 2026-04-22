using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class OidcCustomScopeConfiguration : IEntityTypeConfiguration<OidcCustomScope>
{
    public void Configure(EntityTypeBuilder<OidcCustomScope> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.ClaimMappingsJson).HasColumnType("jsonb");
        builder.Property(e => e.ResourcesCsv).HasMaxLength(1000);

        builder.HasIndex(e => e.Name).IsUnique();
    }
}
