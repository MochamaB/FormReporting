using FormReporting.Models.Entities.Hardware;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Hardware
{
    public class TenantHardwareConfiguration : IEntityTypeConfiguration<TenantHardware>
    {
        public void Configure(EntityTypeBuilder<TenantHardware> builder)
        {
            // Primary Key
            builder.HasKey(th => th.TenantHardwareId);

            // Indexes
            builder.HasIndex(th => new { th.TenantId, th.Status })
                .HasDatabaseName("IX_TenantHw_Tenant");

            builder.HasIndex(th => th.Status)
                .HasFilter("Status != 'Retired'")
                .HasDatabaseName("IX_TenantHw_Status");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_TenantHw_Status",
                "Status IS NULL OR Status IN ('Operational', 'Faulty', 'UnderRepair', 'Retired', 'InStorage', 'PendingDeployment', 'Disposed')"
            ));

            // Default Values
            builder.Property(th => th.Quantity).HasDefaultValue(1);
            builder.Property(th => th.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(th => th.ModifiedDate).HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(th => th.Tenant)
                .WithMany()
                .HasForeignKey(th => th.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(th => th.HardwareItem)
                .WithMany(hi => hi.TenantHardware)
                .HasForeignKey(th => th.HardwareItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(th => th.MaintenanceLogs)
                .WithOne(hml => hml.TenantHardware)
                .HasForeignKey(hml => hml.TenantHardwareId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
