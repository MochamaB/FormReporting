using FormReporting.Models.Entities.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Metrics
{
    public class MetricDefinitionConfiguration : IEntityTypeConfiguration<MetricDefinition>
    {
        public void Configure(EntityTypeBuilder<MetricDefinition> builder)
        {
            // Primary Key
            builder.HasKey(m => m.MetricId);

            // Unique Constraints
            builder.HasIndex(m => m.MetricCode).IsUnique();

            // Indexes
            builder.HasIndex(m => new { m.SubCategoryId, m.IsKPI, m.IsActive })
                .HasDatabaseName("IX_Metrics_SubCategory");

            builder.HasIndex(m => new { m.SourceType, m.IsActive })
                .HasDatabaseName("IX_Metrics_SourceType");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Metric_SourceType",
                "SourceType IN ('UserInput', 'SystemCalculated', 'ExternalSystem', 'ComplianceTracking', 'AutomatedCheck')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Metric_DataType",
                "DataType IN ('Integer', 'Decimal', 'Percentage', 'Boolean', 'Text', 'Duration', 'Date', 'DateTime')"
            ));

            // Unit is now a foreign key to MetricUnits table - no check constraint needed

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Metric_AggregationType",
                "AggregationType IS NULL OR AggregationType IN ('SUM', 'AVG', 'MAX', 'MIN', 'LAST_VALUE', 'COUNT', 'NONE')"
            ));

            // Default Values
            builder.Property(m => m.IsKPI).HasDefaultValue(false);
            builder.Property(m => m.IsActive).HasDefaultValue(true);
            builder.Property(m => m.CreatedDate).HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(m => m.SubCategory)
                .WithMany(sc => sc.MetricDefinitions)
                .HasForeignKey(m => m.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Unit)
                .WithMany()
                .HasForeignKey(m => m.UnitId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(m => m.TenantMetrics)
                .WithOne(tm => tm.MetricDefinition)
                .HasForeignKey(tm => tm.MetricId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(m => m.SystemMetricLogs)
                .WithOne(sml => sml.MetricDefinition)
                .HasForeignKey(sml => sml.MetricId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
