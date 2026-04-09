using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class EntityNoteConfiguration : IEntityTypeConfiguration<EntityNote>
{
    public void Configure(EntityTypeBuilder<EntityNote> builder)
    {
        builder.ToTable("entity_notes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Text).IsRequired().HasMaxLength(4000);

        builder.HasOne<QBEngineer.Data.Context.ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.EntityType, x.EntityId });
    }
}
