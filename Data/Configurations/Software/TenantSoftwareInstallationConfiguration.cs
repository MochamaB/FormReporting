using FormReporting.Models.Entities.Software;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Software
{
    public class TenantSoftwareInstallationConfiguration : IEntityTypeConfiguration<TenantSoftwareInstallation>
    {
        public void Configure(EntityTypeBuilder<TenantSoftwareInstallation> builder)
        {
            // Primary Key
            builder.HasKey(tsi => tsi.InstallationId);

            // Unique Constraints
            builder.HasIndex(tsi => new { tsi.TenantId, tsi.ProductId, tsi.InstallationType, tsi.MachineName })
                .IsUnique()
                .HasDatabaseName("UQ_Install_Tenant_Product_Type");

            // Indexes
            builder.HasIndex(tsi => new { tsi.TenantId, tsi.Status })
                .HasDatabaseName("IX_Installations_Tenant");

            builder.HasIndex(tsi => new { tsi.ProductId, tsi.Status })
                .HasDatabaseName("IX_Installations_Product");

            builder.HasIndex(tsi => tsi.VersionId)
                .HasDatabaseName("IX_Installations_Version");

            builder.HasIndex(tsi => tsi.LicenseId)
                .HasFilter("LicenseId IS NOT NULL")
                .HasDatabaseName("IX_Installations_License");

            builder.HasIndex(tsi => tsi.LastVerifiedDate)
                .HasFilter("Status = 'Active'")
                .HasDatabaseName("IX_Installations_Verification");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Install_Status",
                "Status IN ('Active', 'Deprecated', 'NeedsUpgrade', 'EndOfLife', 'Uninstalled')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Install_Type",
                "InstallationType IN ('Server', 'Workstation', 'Cloud', 'Virtual', 'Container') OR InstallationType IS NULL"
            ));

            // Default Values
            builder.Property(tsi => tsi.Status).HasDefaultValue("Active");
            builder.Property(tsi => tsi.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(tsi => tsi.ModifiedDate).HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(tsi => tsi.Tenant)
                .WithMany()
                .HasForeignKey(tsi => tsi.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(tsi => tsi.Product)
                .WithMany(sp => sp.Installations)
                .HasForeignKey(tsi => tsi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(tsi => tsi.Version)
                .WithMany(sv => sv.Installations)
                .HasForeignKey(tsi => tsi.VersionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(tsi => tsi.License)
                .WithMany(sl => sl.Installations)
                .HasForeignKey(tsi => tsi.LicenseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(tsi => tsi.Verifier)
                .WithMany()
                .HasForeignKey(tsi => tsi.VerifiedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(tsi => tsi.Modifier)
                .WithMany()
                .HasForeignKey(tsi => tsi.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(tsi => tsi.History)
                .WithOne(sih => sih.Installation)
                .HasForeignKey(sih => sih.InstallationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
