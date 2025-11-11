using FormReporting.Models.Entities.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Reporting
{
    public class ReportSortingConfiguration : IEntityTypeConfiguration<ReportSorting>
    {
        public void Configure(EntityTypeBuilder<ReportSorting> builder)
        {
            builder.HasKey(rs => rs.SortId);

            builder.HasOne(rs => rs.Report)
                .WithMany(r => r.Sortings)
                .HasForeignKey(rs => rs.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rs => rs.Item)
                .WithMany()
                .HasForeignKey(rs => rs.ItemId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            builder.HasOne(rs => rs.Metric)
                .WithMany()
                .HasForeignKey(rs => rs.MetricId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            builder.HasIndex(rs => new { rs.ReportId, rs.SortOrder })
                .HasDatabaseName("IX_ReportSort_Report");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ReportSort_Direction",
                "[SortDirection] IN ('ASC', 'DESC')"
            ));
        }
    }
}
