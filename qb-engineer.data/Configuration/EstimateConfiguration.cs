using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Configuration;

public class EstimateConfiguration : IEntityTypeConfiguration<Estimate>
{
    public void Configure(EntityTypeBuilder<Estimate> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.EstimatedAmount).HasPrecision(18, 2);
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.Status).HasConversion<string>();

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Estimates)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ConvertedToQuote)
            .WithMany()
            .HasForeignKey(e => e.ConvertedToQuoteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.AssignedToId);
    }
}
