using FormReporting.Models.Entities.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Reporting
{
    public class RegionalMonthlySnapshotConfiguration : IEntityTypeConfiguration<RegionalMonthlySnapshot>
    {
        public void Configure(EntityTypeBuilder<RegionalMonthlySnapshot> builder)
        {
            builder.HasKey(rms => rms.SnapshotId);

            builder.HasOne(rms => rms.Region)
                .WithMany()
                .HasForeignKey(rms => rms.RegionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(rms => new { rms.RegionId, rms.YearMonth })
                .IsUnique()
                .HasDatabaseName("UQ_RegionMonth");
        }
    }
}
