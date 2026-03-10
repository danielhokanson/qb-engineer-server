using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class QcChecklistItemConfiguration : IEntityTypeConfiguration<QcChecklistItem>
{
    public void Configure(EntityTypeBuilder<QcChecklistItem> builder)
    {
        builder.Property(e => e.Description).HasMaxLength(200);
        builder.Property(e => e.Specification).HasMaxLength(500);

        builder.HasIndex(e => e.TemplateId);
    }
}
