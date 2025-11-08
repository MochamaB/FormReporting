using FormReporting.Models.Entities.Hardware;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Hardware
{
    public class HardwareCategoryConfiguration : IEntityTypeConfiguration<HardwareCategory>
    {
        public void Configure(EntityTypeBuilder<HardwareCategory> builder)
        {
            // Primary Key
            builder.HasKey(hc => hc.CategoryId);

            // Unique Constraints
            builder.HasIndex(hc => hc.CategoryCode).IsUnique();

            // Default Values
            builder.Property(hc => hc.IsActive).HasDefaultValue(true);

            // Relationships
            builder.HasOne(hc => hc.ParentCategory)
                .WithMany(pc => pc.ChildCategories)
                .HasForeignKey(hc => hc.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(hc => hc.HardwareItems)
                .WithOne(hi => hi.Category)
                .HasForeignKey(hi => hi.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
