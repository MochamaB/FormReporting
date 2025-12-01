using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FormReporting.Models.Entities.Forms;

namespace FormReporting.Data.Configurations.Forms
{
    /// <summary>
    /// Entity Framework configuration for FormItemOptionTemplate
    /// </summary>
    public class FormItemOptionTemplateConfiguration : IEntityTypeConfiguration<FormItemOptionTemplate>
    {
        public void Configure(EntityTypeBuilder<FormItemOptionTemplate> builder)
        {
            builder.ToTable("FormItemOptionTemplates");

            builder.HasKey(e => e.TemplateId);

            // Properties
            builder.Property(e => e.TemplateName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.TemplateCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Category)
                .HasMaxLength(50);

            builder.Property(e => e.SubCategory)
                .HasMaxLength(50);

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.ApplicableFieldTypes)
                .HasMaxLength(200);

            builder.Property(e => e.RecommendedFor)
                .HasMaxLength(500);

            builder.Property(e => e.ScoringType)
                .HasMaxLength(30);

            builder.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(e => e.TemplateCode)
                .IsUnique()
                .HasDatabaseName("IX_FormItemOptionTemplates_TemplateCode");

            builder.HasIndex(e => new { e.Category, e.IsActive })
                .HasDatabaseName("IX_FormItemOptionTemplates_Category_IsActive");

            builder.HasIndex(e => new { e.TenantId, e.IsActive })
                .HasDatabaseName("IX_FormItemOptionTemplates_Tenant_IsActive");

            builder.HasIndex(e => e.DisplayOrder)
                .HasDatabaseName("IX_FormItemOptionTemplates_DisplayOrder");

            // Relationships
            builder.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Modifier)
                .WithMany()
                .HasForeignKey(e => e.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Items)
                .WithOne(e => e.Template)
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// Entity Framework configuration for FormItemOptionTemplateItem
    /// </summary>
    public class FormItemOptionTemplateItemConfiguration : IEntityTypeConfiguration<FormItemOptionTemplateItem>
    {
        public void Configure(EntityTypeBuilder<FormItemOptionTemplateItem> builder)
        {
            builder.ToTable("FormItemOptionTemplateItems");

            builder.HasKey(e => e.TemplateItemId);

            // Properties
            builder.Property(e => e.OptionValue)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.OptionLabel)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.ScoreValue)
                .HasColumnType("decimal(10,2)");

            builder.Property(e => e.ScoreWeight)
                .HasColumnType("decimal(10,2)");

            builder.Property(e => e.IconClass)
                .HasMaxLength(50);

            builder.Property(e => e.ColorHint)
                .HasMaxLength(7);

            // Indexes
            builder.HasIndex(e => new { e.TemplateId, e.DisplayOrder })
                .IsUnique()
                .HasDatabaseName("IX_FormItemOptionTemplateItems_Template_Order");

            // Relationships
            builder.HasOne(e => e.Template)
                .WithMany(t => t.Items)
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// Entity Framework configuration for FormItemOptionTemplateCategory
    /// </summary>
    public class FormItemOptionTemplateCategoryConfiguration : IEntityTypeConfiguration<FormItemOptionTemplateCategory>
    {
        public void Configure(EntityTypeBuilder<FormItemOptionTemplateCategory> builder)
        {
            builder.ToTable("FormItemOptionTemplateCategories");

            builder.HasKey(e => e.CategoryId);

            // Properties
            builder.Property(e => e.CategoryName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.IconClass)
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(e => e.CategoryName)
                .IsUnique()
                .HasDatabaseName("IX_FormItemOptionTemplateCategories_CategoryName");

            builder.HasIndex(e => e.DisplayOrder)
                .HasDatabaseName("IX_FormItemOptionTemplateCategories_DisplayOrder");

            // Relationships
            builder.HasMany(e => e.Templates)
                .WithMany()
                .UsingEntity(j => j.ToTable("FormItemOptionTemplateCategoryMappings"));
        }
    }
}
