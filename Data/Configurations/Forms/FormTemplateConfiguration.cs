using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class FormTemplateConfiguration : IEntityTypeConfiguration<FormTemplate>
    {
        public void Configure(EntityTypeBuilder<FormTemplate> builder)
        {
            // Primary Key
            builder.HasKey(ft => ft.TemplateId);

            // Unique Constraints
            builder.HasIndex(ft => ft.TemplateCode).IsUnique();

            // Indexes
            builder.HasIndex(ft => new { ft.CategoryId, ft.IsActive })
                .HasDatabaseName("IX_Templates_Category");

            builder.HasIndex(ft => ft.WorkflowId)
                .HasDatabaseName("IX_Template_Workflow");

            builder.HasIndex(ft => new { ft.PublishStatus, ft.IsActive })
                .HasDatabaseName("IX_Templates_PublishStatus");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Template_Approval",
                "(RequiresApproval = 0 AND WorkflowId IS NULL) OR (RequiresApproval = 1 AND WorkflowId IS NOT NULL) OR (PublishStatus = 'Draft')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Template_PublishStatus",
                "PublishStatus IN ('Draft', 'Published', 'Archived', 'Deprecated')"
            ));

            // Default Values
            builder.Property(ft => ft.Version).HasDefaultValue(1);
            builder.Property(ft => ft.IsActive).HasDefaultValue(true);
            builder.Property(ft => ft.RequiresApproval).HasDefaultValue(true);
            builder.Property(ft => ft.PublishStatus).HasDefaultValue("Draft");
            builder.Property(ft => ft.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(ft => ft.ModifiedDate).HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(ft => ft.Category)
                .WithMany(fc => fc.FormTemplates)
                .HasForeignKey(ft => ft.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ft => ft.Workflow)
                .WithMany(wd => wd.FormTemplates)
                .HasForeignKey(ft => ft.WorkflowId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ft => ft.Creator)
                .WithMany()
                .HasForeignKey(ft => ft.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ft => ft.Modifier)
                .WithMany()
                .HasForeignKey(ft => ft.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ft => ft.Publisher)
                .WithMany()
                .HasForeignKey(ft => ft.PublishedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ft => ft.Archiver)
                .WithMany()
                .HasForeignKey(ft => ft.ArchivedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(ft => ft.Sections)
                .WithOne(fts => fts.Template)
                .HasForeignKey(fts => fts.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(ft => ft.Items)
                .WithOne(fti => fti.Template)
                .HasForeignKey(fti => fti.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(ft => ft.Submissions)
                .WithOne(fts => fts.Template)
                .HasForeignKey(fts => fts.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(ft => ft.Assignments)
                .WithOne(fta => fta.Template)
                .HasForeignKey(fta => fta.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(ft => ft.Analytics)
                .WithOne(fa => fa.Template)
                .HasForeignKey(fa => fa.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
