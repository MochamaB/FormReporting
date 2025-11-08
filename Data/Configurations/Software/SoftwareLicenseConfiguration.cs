using FormReporting.Models.Entities.Software;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Software
{
    public class SoftwareLicenseConfiguration : IEntityTypeConfiguration<SoftwareLicense>
    {
        public void Configure(EntityTypeBuilder<SoftwareLicense> builder)
        {
            // Primary Key
            builder.HasKey(sl => sl.LicenseId);

            // Indexes
            builder.HasIndex(sl => new { sl.ProductId, sl.IsActive })
                .HasDatabaseName("IX_Licenses_Product");

            builder.HasIndex(sl => sl.ExpiryDate)
                .HasFilter("IsActive = 1 AND ExpiryDate IS NOT NULL")
                .HasDatabaseName("IX_Licenses_Expiry");

            // Note: Cannot use computed expression in filtered index
            // Use a regular index instead for available licenses query
            builder.HasIndex(sl => new { sl.ProductId, sl.QuantityPurchased, sl.QuantityUsed, sl.IsActive })
                .HasDatabaseName("IX_Licenses_Available");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_License_Type",
                "LicenseType IN ('Perpetual', 'Subscription', 'Trial', 'Volume', 'Academic', 'OEM')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_License_Quantity",
                "QuantityUsed <= QuantityPurchased"
            ));

            // Default Values
            builder.Property(sl => sl.QuantityPurchased).HasDefaultValue(1);
            builder.Property(sl => sl.QuantityUsed).HasDefaultValue(0);
            builder.Property(sl => sl.Currency).HasDefaultValue("KES");
            builder.Property(sl => sl.IsActive).HasDefaultValue(true);
            builder.Property(sl => sl.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(sl => sl.ModifiedDate).HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(sl => sl.Product)
                .WithMany(sp => sp.Licenses)
                .HasForeignKey(sl => sl.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(sl => sl.Installations)
                .WithOne(tsi => tsi.License)
                .HasForeignKey(tsi => tsi.LicenseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
