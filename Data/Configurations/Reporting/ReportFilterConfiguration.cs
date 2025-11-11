using FormReporting.Models.Entities.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Reporting
{
    public class ReportFilterConfiguration : IEntityTypeConfiguration<ReportFilter>
    {
        public void Configure(EntityTypeBuilder<ReportFilter> builder)
        {
            builder.HasKey(rf => rf.FilterId);

            builder.HasOne(rf => rf.Report)
                .WithMany(r => r.Filters)
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
                .HasDatabaseName("IX_ReportFilter_Report");

            builder.HasIndex(rf => rf.ItemId)
                .HasDatabaseName("IX_ReportFilter_Item")
                .HasFilter("[ItemId] IS NOT NULL");

            builder.HasIndex(rf => new { rf.ReportId, rf.IsParameterized })
                .HasDatabaseName("IX_ReportFilter_Parametrized")
                .HasFilter("[IsParameterized] = 1");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ReportFilter_Type",
                "[FilterType] IN ('TenantId', 'RegionId', 'DateRange', 'Status', 'FieldValue', 'MetricValue', 'TenantType', 'Custom')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ReportFilter_Operator",
                "[Operator] IN ('Equals', 'NotEquals', 'GreaterThan', 'LessThan', 'GreaterOrEqual', 'LessOrEqual', 'Between', 'In', 'NotIn', 'Contains', 'StartsWith', 'EndsWith', 'IsNull', 'IsNotNull')"
            ));
        }
    }
}
