using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    /// <summary>
    /// Entity Framework configuration for FormTemplateMetricMapping
    /// </summary>
    public class FormTemplateMetricMappingConfiguration : IEntityTypeConfiguration<FormTemplateMetricMapping>
    {
        public void Configure(EntityTypeBuilder<FormTemplateMetricMapping> builder)
        {
            // Primary key
            builder.HasKey(x => x.MappingId);

            // Properties
            builder.Property(x => x.MappingId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.TemplateId)
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
            builder.HasOne(x => x.Template)
                .WithMany()
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Metric)
                .WithMany()
                .HasForeignKey(x => x.MetricId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(x => x.TemplateId);
            builder.HasIndex(x => x.MetricId);
            builder.HasIndex(x => new { x.TemplateId, x.MappingName })
                .IsUnique();
        }
    }
}
