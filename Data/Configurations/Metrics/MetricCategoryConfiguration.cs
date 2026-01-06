using FormReporting.Models.Entities.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Metrics
{
    public class MetricCategoryConfiguration : IEntityTypeConfiguration<MetricCategory>
    {
        public void Configure(EntityTypeBuilder<MetricCategory> builder)
        {
            builder.ToTable("MetricCategories");

            builder.HasKey(c => c.CategoryId);

            builder.Property(c => c.CategoryCode)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(c => c.CategoryName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.IconClass)
                .HasMaxLength(50);

            builder.Property(c => c.ColorHint)
                .HasMaxLength(20);

            // Unique constraint on CategoryCode
            builder.HasIndex(c => c.CategoryCode)
                .IsUnique()
                .HasDatabaseName("IX_MetricCategories_CategoryCode");

            // Index for active categories ordered by DisplayOrder
            builder.HasIndex(c => new { c.IsActive, c.DisplayOrder })
                .HasDatabaseName("IX_MetricCategories_Active_Order");
        }
    }
}
