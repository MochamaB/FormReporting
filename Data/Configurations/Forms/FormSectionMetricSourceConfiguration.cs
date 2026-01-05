using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    /// <summary>
    /// Entity Framework configuration for FormSectionMetricSource
    /// </summary>
    public class FormSectionMetricSourceConfiguration : IEntityTypeConfiguration<FormSectionMetricSource>
    {
        public void Configure(EntityTypeBuilder<FormSectionMetricSource> builder)
        {
            // Primary key
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.SectionMappingId)
                .IsRequired();

            builder.Property(x => x.ItemMappingId)
                .IsRequired();

            builder.Property(x => x.Weight)
                .HasColumnType("decimal(5,4)")
                .IsRequired(false);

            builder.Property(x => x.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            // Relationships
            builder.HasOne(x => x.SectionMapping)
                .WithMany(x => x.Sources)
                .HasForeignKey(x => x.SectionMappingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ItemMapping)
                .WithMany()
                .HasForeignKey(x => x.ItemMappingId)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes
            builder.HasIndex(x => x.SectionMappingId);
            builder.HasIndex(x => x.ItemMappingId);
            builder.HasIndex(x => new { x.SectionMappingId, x.ItemMappingId })
                .IsUnique();
        }
    }
}
