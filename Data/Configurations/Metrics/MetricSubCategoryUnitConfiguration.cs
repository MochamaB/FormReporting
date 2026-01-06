using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FormReporting.Models.Entities.Metrics;

namespace FormReporting.Data.Configurations.Metrics
{
    public class MetricSubCategoryUnitConfiguration : IEntityTypeConfiguration<MetricSubCategoryUnit>
    {
        public void Configure(EntityTypeBuilder<MetricSubCategoryUnit> builder)
        {
            // Primary key
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(e => new { e.SubCategoryId, e.UnitId })
                .IsUnique()
                .HasDatabaseName("IX_MetricSubCategoryUnits_SubCategoryId_UnitId");

            builder.HasIndex(e => new { e.SubCategoryId, e.DisplayOrder })
                .HasDatabaseName("IX_MetricSubCategoryUnits_SubCategoryId_DisplayOrder");

            // Relationships
            builder.HasOne(e => e.SubCategory)
                .WithMany(sc => sc.AllowedUnits)
                .HasForeignKey(e => e.SubCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Unit)
                .WithMany()
                .HasForeignKey(e => e.UnitId)
                .OnDelete(DeleteBehavior.Cascade);

            // Table name
            builder.ToTable("MetricSubCategoryUnits");
        }
    }
}
