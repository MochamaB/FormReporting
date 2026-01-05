using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    /// <summary>
    /// Entity Framework configuration for FormTemplateMetricSource
    /// </summary>
    public class FormTemplateMetricSourceConfiguration : IEntityTypeConfiguration<FormTemplateMetricSource>
    {
        public void Configure(EntityTypeBuilder<FormTemplateMetricSource> builder)
        {
            // Primary key
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.TemplateMappingId)
                .IsRequired();

            builder.Property(x => x.SectionMappingId)
                .IsRequired();

            builder.Property(x => x.Weight)
                .HasColumnType("decimal(5,4)")
                .IsRequired(false);

            builder.Property(x => x.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            // Relationships
            builder.HasOne(x => x.TemplateMapping)
                .WithMany(x => x.Sources)
                .HasForeignKey(x => x.TemplateMappingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SectionMapping)
                .WithMany()
                .HasForeignKey(x => x.SectionMappingId)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes
            builder.HasIndex(x => x.TemplateMappingId);
            builder.HasIndex(x => x.SectionMappingId);
            builder.HasIndex(x => new { x.TemplateMappingId, x.SectionMappingId })
                .IsUnique();
        }
    }
}
