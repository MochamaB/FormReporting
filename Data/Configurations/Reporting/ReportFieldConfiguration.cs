using FormReporting.Models.Entities.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Reporting
{
    public class ReportFieldConfiguration : IEntityTypeConfiguration<ReportField>
    {
        public void Configure(EntityTypeBuilder<ReportField> builder)
        {
            builder.HasKey(rf => rf.FieldId);

            builder.HasOne(rf => rf.Report)
                .WithMany(r => r.Fields)
                .HasForeignKey(rf => rf.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rf => rf.Item)
                .WithMany()
                .HasForeignKey(rf => rf.ItemId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            builder.HasOne(rf => rf.Metric)
                .WithMany()
                .HasForeignKey(rf => rf.MetricId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            builder.HasIndex(rf => new { rf.ReportId, rf.DisplayOrder })
                .HasDatabaseName("IX_ReportField_Report");

            builder.HasIndex(rf => rf.ItemId)
                .HasDatabaseName("IX_ReportField_Item")
                .HasFilter("[ItemId] IS NOT NULL");

            builder.HasIndex(rf => rf.MetricId)
                .HasDatabaseName("IX_ReportField_Metric")
                .HasFilter("[MetricId] IS NOT NULL");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ReportField_Source",
                "[SourceType] IN ('FormItem', 'Metric', 'Computed', 'SystemField')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ReportField_Aggregation",
                "[AggregationType] IS NULL OR [AggregationType] IN ('Sum', 'Avg', 'Count', 'Min', 'Max', 'CountDistinct', 'First', 'Last', 'None')"
            ));
        }
    }
}
