using FormReporting.Models.Entities.Software;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Software
{
    public class SoftwareProductConfiguration : IEntityTypeConfiguration<SoftwareProduct>
    {
        public void Configure(EntityTypeBuilder<SoftwareProduct> builder)
        {
            // Primary Key
            builder.HasKey(sp => sp.ProductId);

            // Unique Constraints
            builder.HasIndex(sp => sp.ProductCode).IsUnique();

            // Indexes
            builder.HasIndex(sp => new { sp.ProductCategory, sp.IsActive })
                .HasDatabaseName("IX_Products_Category");

            builder.HasIndex(sp => sp.Vendor)
                .HasDatabaseName("IX_Products_Vendor");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Product_LicenseModel",
                "LicenseModel IN ('PerUser', 'PerDevice', 'Enterprise', 'Subscription', 'OpenSource', 'Concurrent') OR LicenseModel IS NULL"
            ));

            // Default Values
            builder.Property(sp => sp.IsKTDAProduct).HasDefaultValue(false);
            builder.Property(sp => sp.RequiresLicense).HasDefaultValue(false);
            builder.Property(sp => sp.IsActive).HasDefaultValue(true);
            builder.Property(sp => sp.CreatedDate).HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasMany(sp => sp.Versions)
                .WithOne(sv => sv.Product)
                .HasForeignKey(sv => sv.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(sp => sp.Licenses)
                .WithOne(sl => sl.Product)
                .HasForeignKey(sl => sl.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(sp => sp.Installations)
                .WithOne(tsi => tsi.Product)
                .HasForeignKey(tsi => tsi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
