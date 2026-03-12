using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class KioskTerminalConfiguration : IEntityTypeConfiguration<KioskTerminal>
{
    public void Configure(EntityTypeBuilder<KioskTerminal> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.Name).HasMaxLength(100);
        builder.Property(e => e.DeviceToken).HasMaxLength(100);

        builder.HasIndex(e => e.DeviceToken).IsUnique();
        builder.HasIndex(e => e.TeamId);

        builder.HasOne(e => e.Team)
            .WithMany()
            .HasForeignKey(e => e.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
