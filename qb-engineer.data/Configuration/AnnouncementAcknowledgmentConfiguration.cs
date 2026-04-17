using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class AnnouncementAcknowledgmentConfiguration : IEntityTypeConfiguration<AnnouncementAcknowledgment>
{
    public void Configure(EntityTypeBuilder<AnnouncementAcknowledgment> builder)
    {
        builder.HasOne(a => a.Announcement)
            .WithMany(a => a.Acknowledgments)
            .HasForeignKey(a => a.AnnouncementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Context.ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.AnnouncementId, a.UserId }).IsUnique();
    }
}
