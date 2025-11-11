using FormReporting.Models.Entities.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Reporting
{
    public class TenantPerformanceSnapshotConfiguration : IEntityTypeConfiguration<TenantPerformanceSnapshot>
    {
        public void Configure(EntityTypeBuilder<TenantPerformanceSnapshot> builder)
        {
            builder.HasKey(tps => tps.SnapshotId);

            builder.HasOne(tps => tps.Tenant)
                .WithMany()
                .HasForeignKey(tps => tps.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(tps => new { tps.TenantId, tps.SnapshotDate })
                .HasDatabaseName("IX_PerfSnapshot_Tenant_Date")
                .IsDescending(false, true);

            builder.HasIndex(tps => new { tps.SnapshotType, tps.SnapshotDate })
                .HasDatabaseName("IX_PerfSnapshot_Type_Date")
                .IsDescending(false, true);

            builder.HasIndex(tps => tps.GeneratedDate)
                .HasDatabaseName("IX_PerfSnapshot_Generated")
                .IsDescending();

            builder.HasIndex(tps => new { tps.TenantId, tps.SnapshotDate, tps.SnapshotType })
                .IsUnique()
                .HasDatabaseName("UQ_PerfSnapshot");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_SnapshotType",
                "[SnapshotType] IN ('Daily', 'Weekly', 'Monthly', 'Quarterly')"
            ));
        }
    }
}
