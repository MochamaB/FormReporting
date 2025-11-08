using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class FormCategoryConfiguration : IEntityTypeConfiguration<FormCategory>
    {
        public void Configure(EntityTypeBuilder<FormCategory> builder)
        {
            // Primary Key
            builder.HasKey(fc => fc.CategoryId);

            // Unique Constraints
            builder.HasIndex(fc => fc.CategoryName).IsUnique();
            builder.HasIndex(fc => fc.CategoryCode).IsUnique();

            // Indexes
            builder.HasIndex(fc => new { fc.IsActive, fc.DisplayOrder })
                .HasDatabaseName("IX_Categories_Active");

            // Default Values
            builder.Property(fc => fc.DisplayOrder).HasDefaultValue(0);
            builder.Property(fc => fc.IsActive).HasDefaultValue(true);
            builder.Property(fc => fc.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(fc => fc.ModifiedDate).HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasMany(fc => fc.FormTemplates)
                .WithOne(ft => ft.Category)
                .HasForeignKey(ft => ft.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
