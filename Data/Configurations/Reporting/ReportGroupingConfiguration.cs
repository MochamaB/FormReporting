using FormReporting.Models.Entities.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Reporting
{
    public class ReportGroupingConfiguration : IEntityTypeConfiguration<ReportGrouping>
    {
        public void Configure(EntityTypeBuilder<ReportGrouping> builder)
        {
            builder.HasKey(rg => rg.GroupingId);

            builder.HasOne(rg => rg.Report)
                .WithMany(r => r.Groupings)
                .HasForeignKey(rg => rg.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rg => rg.Item)
                .WithMany()
                .HasForeignKey(rg => rg.ItemId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            builder.HasOne(rg => rg.Metric)
                .WithMany()
                .HasForeignKey(rg => rg.MetricId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            builder.HasIndex(rg => new { rg.ReportId, rg.GroupOrder })
                .HasDatabaseName("IX_ReportGrouping_Report");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ReportGrouping_Type",
                "[GroupByType] IN ('Tenant', 'Region', 'Month', 'Year', 'Quarter', 'Week', 'Day', 'TenantType', 'Category', 'FieldValue', 'MetricValue')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ReportGrouping_Sort",
                "[SortDirection] IN ('ASC', 'DESC')"
            ));
        }
    }
}
