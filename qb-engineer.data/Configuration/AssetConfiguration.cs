using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.Name).HasMaxLength(200);
        builder.Property(e => e.Location).HasMaxLength(200);
        builder.Property(e => e.Manufacturer).HasMaxLength(200);
        builder.Property(e => e.Model).HasMaxLength(200);
        builder.Property(e => e.SerialNumber).HasMaxLength(100);
        builder.Property(e => e.PhotoFileId).HasMaxLength(200);
        builder.Property(e => e.CurrentHours).HasPrecision(18, 2);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasIndex(e => e.SerialNumber);
        builder.HasIndex(e => e.SourceJobId);
        builder.HasIndex(e => e.SourcePartId);

        builder.HasOne(e => e.SourceJob)
            .WithMany()
            .HasForeignKey(e => e.SourceJobId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.SourcePart)
            .WithMany()
            .HasForeignKey(e => e.SourcePartId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
