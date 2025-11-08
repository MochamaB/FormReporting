using FormReporting.Models.Entities.Hardware;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Hardware
{
    public class HardwareMaintenanceLogConfiguration : IEntityTypeConfiguration<HardwareMaintenanceLog>
    {
        public void Configure(EntityTypeBuilder<HardwareMaintenanceLog> builder)
        {
            // Primary Key
            builder.HasKey(hml => hml.LogId);

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Maintenance_Type",
                "MaintenanceType IS NULL OR MaintenanceType IN ('Preventive', 'Corrective', 'Upgrade', 'Emergency', 'Calibration', 'Inspection')"
            ));

            // Default Values
            builder.Property(hml => hml.CreatedDate).HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(hml => hml.TenantHardware)
                .WithMany(th => th.MaintenanceLogs)
                .HasForeignKey(hml => hml.TenantHardwareId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
