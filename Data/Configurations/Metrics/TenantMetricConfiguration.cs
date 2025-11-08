using FormReporting.Models.Entities.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Metrics
{
    public class TenantMetricConfiguration : IEntityTypeConfiguration<TenantMetric>
    {
        public void Configure(EntityTypeBuilder<TenantMetric> builder)
        {
            // Primary Key
            builder.HasKey(tm => tm.MetricValueId);

            // Unique Constraint
            builder.HasIndex(tm => new { tm.TenantId, tm.MetricId, tm.ReportingPeriod })
                .IsUnique()
                .HasDatabaseName("UQ_TenantMetricPeriod");

            // Indexes - Critical for time-series queries
            builder.HasIndex(tm => new { tm.ReportingPeriod, tm.TenantId, tm.MetricId })
                .IsDescending(true, false, false)
                .IncludeProperties(tm => new { tm.NumericValue, tm.TextValue })
                .HasDatabaseName("IX_TenantMetrics_TimeSeries");

            builder.HasIndex(tm => new { tm.TenantId, tm.MetricId, tm.ReportingPeriod })
                .IsDescending(false, false, true)
                .HasDatabaseName("IX_TenantMetrics_Tenant");

            builder.HasIndex(tm => tm.ReportingPeriod)
                .IsDescending()
                .IncludeProperties(tm => new { tm.TenantId, tm.NumericValue })
                .HasDatabaseName("IX_TenantMetrics_Period");

            builder.HasIndex(tm => new { tm.SourceType, tm.SourceReferenceId })
                .HasDatabaseName("IX_TenantMetrics_Source");

            // Check Constraint
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_TenantMetric_SourceType",
                "SourceType IS NULL OR SourceType IN ('UserInput', 'SystemCalculated', 'HangfireJob', 'ExternalAPI', 'Manual', 'Import')"
            ));

            // Default Values
            builder.Property(tm => tm.CapturedDate).HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(tm => tm.Tenant)
                .WithMany()
                .HasForeignKey(tm => tm.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(tm => tm.MetricDefinition)
                .WithMany(m => m.TenantMetrics)
                .HasForeignKey(tm => tm.MetricId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
