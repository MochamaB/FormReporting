using FormReporting.Models.Entities.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Reporting
{
    public class ReportScheduleConfiguration : IEntityTypeConfiguration<ReportSchedule>
    {
        public void Configure(EntityTypeBuilder<ReportSchedule> builder)
        {
            builder.HasKey(rs => rs.ScheduleId);

            builder.HasOne(rs => rs.Report)
                .WithMany(r => r.Schedules)
                .HasForeignKey(rs => rs.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rs => rs.NotificationTemplate)
                .WithMany()
                .HasForeignKey(rs => rs.NotificationTemplateId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(rs => rs.Creator)
                .WithMany()
                .HasForeignKey(rs => rs.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(rs => new { rs.ReportId, rs.IsActive })
                .HasDatabaseName("IX_ReportSchedule_Report");

            builder.HasIndex(rs => rs.NextRunDate)
                .HasDatabaseName("IX_ReportSchedule_NextRun")
                .HasFilter("[IsActive] = 1");

            builder.HasIndex(rs => new { rs.ScheduleType, rs.IsActive })
                .HasDatabaseName("IX_ReportSchedule_Type");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ReportSchedule_Type",
                "[ScheduleType] IN ('Daily', 'Weekly', 'Monthly', 'Quarterly', 'Yearly', 'OnDemand')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ReportSchedule_Format",
                "[OutputFormat] IN ('PDF', 'Excel', 'CSV', 'JSON', 'HTML')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ReportSchedule_Orientation",
                "[PageOrientation] IN ('Portrait', 'Landscape')"
            ));
        }
    }
}
