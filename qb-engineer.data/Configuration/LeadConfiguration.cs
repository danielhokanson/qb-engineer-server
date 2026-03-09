using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.Property(e => e.CompanyName).HasMaxLength(200);
        builder.Property(e => e.ContactName).HasMaxLength(200);
        builder.Property(e => e.Email).HasMaxLength(200);
        builder.Property(e => e.Phone).HasMaxLength(50);
        builder.Property(e => e.Source).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.LostReason).HasMaxLength(500);
        builder.Property(e => e.CustomFieldValues).HasColumnType("jsonb");

        builder.HasOne(e => e.ConvertedCustomer)
            .WithMany()
            .HasForeignKey(e => e.ConvertedCustomerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
