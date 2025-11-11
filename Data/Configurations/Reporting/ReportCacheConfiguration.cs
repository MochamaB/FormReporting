using FormReporting.Models.Entities.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Reporting
{
    public class ReportCacheConfiguration : IEntityTypeConfiguration<ReportCache>
    {
        public void Configure(EntityTypeBuilder<ReportCache> builder)
        {
            builder.HasKey(rc => rc.CacheId);

            builder.HasOne(rc => rc.Report)
                .WithMany(r => r.CacheEntries)
                .HasForeignKey(rc => rc.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rc => rc.Generator)
                .WithMany()
                .HasForeignKey(rc => rc.GeneratedBy)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(rc => rc.ExpiryDate)
                .HasDatabaseName("IX_ReportCache_Expiry");

            builder.HasIndex(rc => new { rc.ReportId, rc.GeneratedDate })
                .HasDatabaseName("IX_ReportCache_Report")
                .IsDescending(false, true);

            builder.HasIndex(rc => new { rc.HitCount, rc.LastAccessDate })
                .HasDatabaseName("IX_ReportCache_Popular")
                .IsDescending();

            builder.HasIndex(rc => new { rc.ReportId, rc.ParameterHash })
                .IsUnique()
                .HasDatabaseName("UQ_ReportCache_Hash");
        }
    }
}
