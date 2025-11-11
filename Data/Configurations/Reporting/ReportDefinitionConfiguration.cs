using FormReporting.Models.Entities.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Reporting
{
    public class ReportDefinitionConfiguration : IEntityTypeConfiguration<ReportDefinition>
    {
        public void Configure(EntityTypeBuilder<ReportDefinition> builder)
        {
            builder.HasKey(rd => rd.ReportId);

            builder.HasOne(rd => rd.Template)
                .WithMany()
                .HasForeignKey(rd => rd.TemplateId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(rd => rd.Owner)
                .WithMany()
                .HasForeignKey(rd => rd.OwnerUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(rd => rd.ReportCode)
                .IsUnique()
                .HasDatabaseName("UQ_Report_Code");

            builder.HasIndex(rd => new { rd.TemplateId, rd.IsActive })
                .HasDatabaseName("IX_ReportDef_Template");

            builder.HasIndex(rd => new { rd.OwnerUserId, rd.IsActive })
                .HasDatabaseName("IX_ReportDef_Owner");

            builder.HasIndex(rd => new { rd.Category, rd.IsActive })
                .HasDatabaseName("IX_ReportDef_Category");

            builder.HasIndex(rd => new { rd.IsPublic, rd.IsActive })
                .HasDatabaseName("IX_ReportDef_Public")
                .HasFilter("[IsPublic] = 1");

            builder.HasIndex(rd => new { rd.RunCount, rd.LastRunDate })
                .HasDatabaseName("IX_ReportDef_Popular")
                .IsDescending();

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Report_Type",
                "[ReportType] IN ('Tabular', 'Chart', 'Pivot', 'Dashboard', 'CrossTab', 'Matrix')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Report_ChartType",
                "[ChartType] IS NULL OR [ChartType] IN ('Bar', 'Line', 'Pie', 'Doughnut', 'Area', 'Column', 'Scatter', 'Bubble', 'Radar')"
            ));
        }
    }
}
