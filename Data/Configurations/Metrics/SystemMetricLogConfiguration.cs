using FormReporting.Models.Entities.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Metrics
{
    public class SystemMetricLogConfiguration : IEntityTypeConfiguration<SystemMetricLog>
    {
        public void Configure(EntityTypeBuilder<SystemMetricLog> builder)
        {
            // Primary Key
            builder.HasKey(sml => sml.LogId);

            // Indexes
            builder.HasIndex(sml => new { sml.TenantId, sml.CheckDate })
                .IsDescending(false, true)
                .HasDatabaseName("IX_SystemMetricLogs_Tenant_Date");

            builder.HasIndex(sml => new { sml.MetricId, sml.CheckDate })
                .IsDescending(false, true)
                .HasDatabaseName("IX_SystemMetricLogs_Metric_Date");

            builder.HasIndex(sml => new { sml.Status, sml.CheckDate })
                .IsDescending(false, true)
                .HasDatabaseName("IX_SystemMetricLogs_Status");

            // Default Values
            builder.Property(sml => sml.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(sml => sml.Tenant)
                .WithMany()
                .HasForeignKey(sml => sml.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sml => sml.MetricDefinition)
                .WithMany(m => m.SystemMetricLogs)
                .HasForeignKey(sml => sml.MetricId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
