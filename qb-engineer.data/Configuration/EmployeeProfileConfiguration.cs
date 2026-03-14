using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using QBEngineer.Core.Entities;

namespace QBEngineer.Data.Configuration;

public class EmployeeProfileConfiguration : IEntityTypeConfiguration<EmployeeProfile>
{
    public void Configure(EntityTypeBuilder<EmployeeProfile> builder)
    {
        builder.Ignore(e => e.IsDeleted);

        builder.HasIndex(e => e.UserId).IsUnique();

        // Personal
        builder.Property(e => e.Gender).HasMaxLength(50);

        // Address
        builder.Property(e => e.Street1).HasMaxLength(200);
        builder.Property(e => e.Street2).HasMaxLength(200);
        builder.Property(e => e.City).HasMaxLength(100);
        builder.Property(e => e.State).HasMaxLength(100);
        builder.Property(e => e.ZipCode).HasMaxLength(20);
        builder.Property(e => e.Country).HasMaxLength(100);

        // Contact
        builder.Property(e => e.PhoneNumber).HasMaxLength(50);
        builder.Property(e => e.PersonalEmail).HasMaxLength(200);

        // Emergency
        builder.Property(e => e.EmergencyContactName).HasMaxLength(200);
        builder.Property(e => e.EmergencyContactPhone).HasMaxLength(50);
        builder.Property(e => e.EmergencyContactRelationship).HasMaxLength(100);

        // Employment
        builder.Property(e => e.Department).HasMaxLength(200);
        builder.Property(e => e.JobTitle).HasMaxLength(200);
        builder.Property(e => e.EmployeeNumber).HasMaxLength(50);
        builder.Property(e => e.HourlyRate).HasPrecision(10, 2);
        builder.Property(e => e.SalaryAmount).HasPrecision(12, 2);
    }
}
