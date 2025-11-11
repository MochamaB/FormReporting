using FormReporting.Models.Entities.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Reporting
{
    public class ReportExecutionLogConfiguration : IEntityTypeConfiguration<ReportExecutionLog>
    {
        public void Configure(EntityTypeBuilder<ReportExecutionLog> builder)
        {
            builder.HasKey(rel => rel.ExecutionId);

            builder.HasOne(rel => rel.Report)
                .WithMany(r => r.ExecutionLogs)
                .HasForeignKey(rel => rel.ReportId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(rel => rel.Executor)
                .WithMany()
                .HasForeignKey(rel => rel.ExecutedBy)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(rel => rel.Schedule)
                .WithMany(s => s.ExecutionLogs)
                .HasForeignKey(rel => rel.ScheduleId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(rel => new { rel.ReportId, rel.ExecutionDate })
                .HasDatabaseName("IX_ReportExec_Report")
                .IsDescending(false, true);

            builder.HasIndex(rel => new { rel.ExecutedBy, rel.ExecutionDate })
                .HasDatabaseName("IX_ReportExec_User")
                .IsDescending(false, true);

            builder.HasIndex(rel => rel.ExecutionDate)
                .HasDatabaseName("IX_ReportExec_Date")
                .IsDescending();

            builder.HasIndex(rel => rel.ExecutionTimeMs)
                .HasDatabaseName("IX_ReportExec_Performance")
                .IsDescending();

            builder.HasIndex(rel => new { rel.Status, rel.ExecutionDate })
                .HasDatabaseName("IX_ReportExec_Failed")
                .HasFilter("[Status] = 'Failed'")
                .IsDescending(false, true);

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ReportExec_Type",
                "[ExecutionType] IN ('Manual', 'Scheduled', 'Cached', 'API')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ReportExec_Status",
                "[Status] IN ('Success', 'Failed', 'Timeout', 'Cancelled', 'Pending')"
            ));
        }
    }
}
