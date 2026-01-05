using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    /// <summary>
    /// Entity Framework configuration for FormSectionMetricMapping
    /// </summary>
    public class FormSectionMetricMappingConfiguration : IEntityTypeConfiguration<FormSectionMetricMapping>
    {
        public void Configure(EntityTypeBuilder<FormSectionMetricMapping> builder)
        {
            // Primary key
            builder.HasKey(x => x.MappingId);

            // Properties
            builder.Property(x => x.MappingId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.SectionId)
                .IsRequired();

            builder.Property(x => x.MetricId)
                .IsRequired(false);

            builder.Property(x => x.MappingName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.MappingType)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(x => x.AggregationType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(x => x.Section)
                .WithMany()
                .HasForeignKey(x => x.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Metric)
                .WithMany()
                .HasForeignKey(x => x.MetricId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(x => x.SectionId);
            builder.HasIndex(x => x.MetricId);
            builder.HasIndex(x => new { x.SectionId, x.MappingName })
                .IsUnique();
        }
    }
}
