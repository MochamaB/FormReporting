using FormReporting.Models.Entities.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Metrics
{
    public class MetricUnitConfiguration : IEntityTypeConfiguration<MetricUnit>
    {
        public void Configure(EntityTypeBuilder<MetricUnit> builder)
        {
            builder.ToTable("MetricUnits");

            builder.HasKey(u => u.UnitId);

            builder.Property(u => u.UnitCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(u => u.UnitName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.UnitSymbol)
                .HasMaxLength(10);

            builder.Property(u => u.FormatPattern)
                .HasMaxLength(50);

            builder.Property(u => u.SuggestedAggregation)
                .HasMaxLength(20);

            builder.Property(u => u.UnitCategory)
                .HasMaxLength(30);

            builder.Property(u => u.Description)
                .HasMaxLength(200);

            // Unique constraint on UnitCode
            builder.HasIndex(u => u.UnitCode)
                .IsUnique()
                .HasDatabaseName("IX_MetricUnits_UnitCode");

            // Index for active units ordered by DisplayOrder
            builder.HasIndex(u => new { u.IsActive, u.DisplayOrder })
                .HasDatabaseName("IX_MetricUnits_Active_Order");

            // Index for filtering by UnitCategory
            builder.HasIndex(u => u.UnitCategory)
                .HasDatabaseName("IX_MetricUnits_Category");
        }
    }
}
