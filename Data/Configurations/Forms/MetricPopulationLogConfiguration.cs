using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class MetricPopulationLogConfiguration : IEntityTypeConfiguration<MetricPopulationLog>
    {
        public void Configure(EntityTypeBuilder<MetricPopulationLog> builder)
        {
            // Primary Key
            builder.HasKey(mpl => mpl.LogId);

            // Indexes
            builder.HasIndex(mpl => new { mpl.SubmissionId, mpl.Status })
                .HasDatabaseName("IX_MetricLog_Submission");

            builder.HasIndex(mpl => mpl.PopulatedDate)
                .IsDescending()
                .HasDatabaseName("IX_MetricLog_Date");

            builder.HasIndex(mpl => new { mpl.Status, mpl.PopulatedDate })
                .IsDescending(false, true)
                .HasDatabaseName("IX_MetricLog_Status");

            builder.HasIndex(mpl => new { mpl.MetricId, mpl.PopulatedDate })
                .IsDescending(false, true)
                .HasDatabaseName("IX_MetricLog_Metric");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_MetricLog_Status",
                "Status IN ('Success', 'Failed', 'Skipped', 'Pending')"
            ));

            // Default Values
            builder.Property(mpl => mpl.PopulatedDate).HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(mpl => mpl.Submission)
                .WithMany(fts => fts.MetricPopulationLogs)
                .HasForeignKey(mpl => mpl.SubmissionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(mpl => mpl.Metric)
                .WithMany()
                .HasForeignKey(mpl => mpl.MetricId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(mpl => mpl.Mapping)
                .WithMany(fimm => fimm.PopulationLogs)
                .HasForeignKey(mpl => mpl.MappingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(mpl => mpl.SourceItem)
                .WithMany()
                .HasForeignKey(mpl => mpl.SourceItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(mpl => mpl.PopulatedByUser)
                .WithMany()
                .HasForeignKey(mpl => mpl.PopulatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
