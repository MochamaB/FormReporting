using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FormReporting.Models.Entities.Metrics;

namespace FormReporting.Data.Configurations.Metrics
{
    public class MetricSubCategoryConfiguration : IEntityTypeConfiguration<MetricSubCategory>
    {
        public void Configure(EntityTypeBuilder<MetricSubCategory> builder)
        {
            // Primary key
            builder.HasKey(e => e.SubCategoryId);

            // Properties
            builder.Property(e => e.SubCategoryCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.SubCategoryName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.AllowedDataTypes)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.AllowedAggregationTypes)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.DefaultDataType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.DefaultAggregationType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.AllowedScopes)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.DefaultScope)
                .HasMaxLength(20);

            builder.Property(e => e.SuggestedThresholdGreen)
                .HasColumnType("decimal(18,4)");

            builder.Property(e => e.SuggestedThresholdYellow)
                .HasColumnType("decimal(18,4)");

            builder.Property(e => e.SuggestedThresholdRed)
                .HasColumnType("decimal(18,4)");

            builder.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(e => e.SubCategoryCode)
                .IsUnique()
                .HasDatabaseName("IX_MetricSubCategories_SubCategoryCode");

            builder.HasIndex(e => new { e.CategoryId, e.DisplayOrder })
                .HasDatabaseName("IX_MetricSubCategories_CategoryId_DisplayOrder");

            // Relationships
            builder.HasOne(e => e.Category)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.DefaultUnit)
                .WithMany()
                .HasForeignKey(e => e.DefaultUnitId)
                .OnDelete(DeleteBehavior.SetNull);

            // Table name
            builder.ToTable("MetricSubCategories");
        }
    }
}
