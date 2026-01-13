using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class FormTemplateItemConfiguration : IEntityTypeConfiguration<FormTemplateItem>
    {
        public void Configure(EntityTypeBuilder<FormTemplateItem> builder)
        {
            // Primary Key
            builder.HasKey(fti => fti.ItemId);

            // Unique Constraints
            builder.HasIndex(fti => new { fti.TemplateId, fti.ItemCode, fti.Version })
                .IsUnique()
                .HasDatabaseName("UQ_TemplateItem");

            // Indexes
            builder.HasIndex(fti => new { fti.TemplateId, fti.DisplayOrder, fti.IsActive })
                .HasDatabaseName("IX_TemplateItems_Template");

            builder.HasIndex(fti => new { fti.SectionId, fti.DisplayOrder })
                .HasDatabaseName("IX_TemplateItems_Section");

            builder.HasIndex(fti => fti.LibraryFieldId)
                .HasDatabaseName("IX_TemplateItems_Library");

            builder.HasIndex(fti => new { fti.LayoutType, fti.MatrixGroupId })
                .HasDatabaseName("IX_TemplateItems_Layout");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Item_LayoutType",
                "LayoutType IN ('Single', 'Matrix', 'Grid', 'Inline')"
            ));

            // Default Values
            builder.Property(fti => fti.DisplayOrder).HasDefaultValue(0);
            builder.Property(fti => fti.IsRequired).HasDefaultValue(false);
            builder.Property(fti => fti.LayoutType).HasDefaultValue("Single");
            builder.Property(fti => fti.IsLibraryOverride).HasDefaultValue(false);
            builder.Property(fti => fti.Version).HasDefaultValue(1);
            builder.Property(fti => fti.IsActive).HasDefaultValue(true);
            builder.Property(fti => fti.Weight).HasDefaultValue(1.0m);
            builder.Property(fti => fti.CreatedDate).HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(fti => fti.Template)
                .WithMany(ft => ft.Items)
                .HasForeignKey(fti => fti.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(fti => fti.Section)
                .WithMany(fts => fts.Items)
                .HasForeignKey(fti => fti.SectionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(fti => fti.LibraryField)
                .WithMany(fl => fl.FormTemplateItems)
                .HasForeignKey(fti => fti.LibraryFieldId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(fti => fti.Options)
                .WithOne(fio => fio.Item)
                .HasForeignKey(fio => fio.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(fti => fti.Configurations)
                .WithOne(fic => fic.Item)
                .HasForeignKey(fic => fic.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(fti => fti.Validations)
                .WithOne(fiv => fiv.Item)
                .HasForeignKey(fiv => fiv.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(fti => fti.Calculations)
                .WithOne(fic => fic.TargetItem)
                .HasForeignKey(fic => fic.TargetItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(fti => fti.MetricMappings)
                .WithOne(fimm => fimm.Item)
                .HasForeignKey(fimm => fimm.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(fti => fti.Responses)
                .WithOne(ftr => ftr.Item)
                .HasForeignKey(ftr => ftr.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(fti => fti.Routings)
                .WithOne(sr => sr.SourceItem)
                .HasForeignKey(sr => sr.SourceItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
