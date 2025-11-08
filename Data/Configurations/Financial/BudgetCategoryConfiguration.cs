using FormReporting.Models.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Financial
{
    public class BudgetCategoryConfiguration : IEntityTypeConfiguration<BudgetCategory>
    {
        public void Configure(EntityTypeBuilder<BudgetCategory> builder)
        {
            // Primary Key
            builder.HasKey(bc => bc.CategoryId);

            // Unique Constraints
            builder.HasIndex(bc => bc.CategoryCode).IsUnique();

            // Default Values
            builder.Property(bc => bc.IsCapital).HasDefaultValue(false);
            builder.Property(bc => bc.IsActive).HasDefaultValue(true);

            // Relationships
            builder.HasOne(bc => bc.ParentCategory)
                .WithMany(pc => pc.ChildCategories)
                .HasForeignKey(bc => bc.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(bc => bc.TenantBudgets)
                .WithOne(tb => tb.Category)
                .HasForeignKey(tb => tb.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(bc => bc.TenantExpenses)
                .WithOne(te => te.Category)
                .HasForeignKey(te => te.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
