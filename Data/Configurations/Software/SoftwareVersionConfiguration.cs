using FormReporting.Models.Entities.Software;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Software
{
    public class SoftwareVersionConfiguration : IEntityTypeConfiguration<SoftwareVersion>
    {
        public void Configure(EntityTypeBuilder<SoftwareVersion> builder)
        {
            // Primary Key
            builder.HasKey(sv => sv.VersionId);

            // Unique Constraints
            builder.HasIndex(sv => new { sv.ProductId, sv.VersionNumber })
                .IsUnique()
                .HasDatabaseName("UQ_ProductVersion");

            // Indexes
            builder.HasIndex(sv => new { sv.ProductId, sv.MajorVersion, sv.MinorVersion, sv.PatchVersion })
                .IsDescending(false, true, true, true)
                .HasDatabaseName("IX_Versions_Product");

            builder.HasIndex(sv => new { sv.SecurityLevel, sv.EndOfLifeDate })
                .HasFilter("IsSupported = 1")
                .HasDatabaseName("IX_Versions_Security");

            builder.HasIndex(sv => sv.EndOfLifeDate)
                .HasFilter("EndOfLifeDate IS NOT NULL")
                .HasDatabaseName("IX_Versions_EOL");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Version_SecurityLevel",
                "SecurityLevel IN ('Critical', 'Stable', 'Vulnerable', 'Unsupported')"
            ));

            // Default Values
            builder.Property(sv => sv.IsCurrentVersion).HasDefaultValue(false);
            builder.Property(sv => sv.IsSupported).HasDefaultValue(true);
            builder.Property(sv => sv.SecurityLevel).HasDefaultValue("Stable");
            builder.Property(sv => sv.MinimumSupportedVersion).HasDefaultValue(false);
            builder.Property(sv => sv.CreatedDate).HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(sv => sv.Product)
                .WithMany(sp => sp.Versions)
                .HasForeignKey(sv => sv.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(sv => sv.Installations)
                .WithOne(tsi => tsi.Version)
                .HasForeignKey(tsi => tsi.VersionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(sv => sv.HistoryAsFromVersion)
                .WithOne(sih => sih.FromVersion)
                .HasForeignKey(sih => sih.FromVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(sv => sv.HistoryAsToVersion)
                .WithOne(sih => sih.ToVersion)
                .HasForeignKey(sih => sih.ToVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
