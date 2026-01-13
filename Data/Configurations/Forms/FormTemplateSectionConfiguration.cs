using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class FormTemplateSectionConfiguration : IEntityTypeConfiguration<FormTemplateSection>
    {
        public void Configure(EntityTypeBuilder<FormTemplateSection> builder)
        {
            // Primary Key
            builder.HasKey(fts => fts.SectionId);

            // Unique Constraints
            builder.HasIndex(fts => new { fts.TemplateId, fts.SectionName })
                .IsUnique()
                .HasDatabaseName("UQ_TemplateSection_Name");

            // Indexes
            builder.HasIndex(fts => new { fts.TemplateId, fts.DisplayOrder })
                .HasDatabaseName("IX_TemplateSections_Template");

            // Default Values
            builder.Property(fts => fts.DisplayOrder).HasDefaultValue(0);
            builder.Property(fts => fts.IsCollapsible).HasDefaultValue(true);
            builder.Property(fts => fts.IsCollapsedByDefault).HasDefaultValue(false);
            builder.Property(fts => fts.ColumnLayout).HasDefaultValue(1);
            builder.Property(fts => fts.Weight).HasDefaultValue(1.0m);
            builder.Property(fts => fts.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(fts => fts.ModifiedDate).HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(fts => fts.Template)
                .WithMany(ft => ft.Sections)
                .HasForeignKey(fts => fts.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(fts => fts.Items)
                .WithOne(fti => fti.Section)
                .HasForeignKey(fti => fti.SectionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(fts => fts.SourceRoutings)
                .WithOne(sr => sr.SourceSection)
                .HasForeignKey(sr => sr.SourceSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(fts => fts.TargetRoutings)
                .WithOne(sr => sr.TargetSection)
                .HasForeignKey(sr => sr.TargetSectionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
