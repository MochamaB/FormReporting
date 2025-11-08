using FormReporting.Models.Entities.Software;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Software
{
    public class SoftwareInstallationHistoryConfiguration : IEntityTypeConfiguration<SoftwareInstallationHistory>
    {
        public void Configure(EntityTypeBuilder<SoftwareInstallationHistory> builder)
        {
            // Primary Key
            builder.HasKey(sih => sih.HistoryId);

            // Indexes
            builder.HasIndex(sih => new { sih.InstallationId, sih.ChangeDate })
                .IsDescending(false, true)
                .HasDatabaseName("IX_InstallHistory_Installation");

            builder.HasIndex(sih => sih.ChangeDate)
                .IsDescending()
                .HasDatabaseName("IX_InstallHistory_Date");

            builder.HasIndex(sih => new { sih.ChangeType, sih.ChangeDate })
                .IsDescending(false, true)
                .HasDatabaseName("IX_InstallHistory_Type");

            builder.HasIndex(sih => sih.ChangedBy)
                .HasDatabaseName("IX_InstallHistory_User");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_History_ChangeType",
                "ChangeType IN ('Install', 'Upgrade', 'Downgrade', 'Uninstall', 'Reinstall', 'Patch')"
            ));

            // Default Values
            builder.Property(sih => sih.ChangeDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(sih => sih.SuccessStatus).HasDefaultValue(true);

            // Relationships
            builder.HasOne(sih => sih.Installation)
                .WithMany(tsi => tsi.History)
                .HasForeignKey(sih => sih.InstallationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sih => sih.FromVersion)
                .WithMany(sv => sv.HistoryAsFromVersion)
                .HasForeignKey(sih => sih.FromVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sih => sih.ToVersion)
                .WithMany(sv => sv.HistoryAsToVersion)
                .HasForeignKey(sih => sih.ToVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sih => sih.ChangedByUser)
                .WithMany()
                .HasForeignKey(sih => sih.ChangedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
