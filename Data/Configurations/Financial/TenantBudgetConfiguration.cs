using FormReporting.Models.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Financial
{
    public class TenantBudgetConfiguration : IEntityTypeConfiguration<TenantBudget>
    {
        public void Configure(EntityTypeBuilder<TenantBudget> builder)
        {
            // Primary Key
            builder.HasKey(tb => tb.BudgetId);

            // Unique Constraints
            builder.HasIndex(tb => new { tb.TenantId, tb.FiscalYear, tb.CategoryId })
                .IsUnique()
                .HasDatabaseName("UQ_TenantBudget");

            // Default Values
            builder.Property(tb => tb.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(tb => tb.ModifiedDate).HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(tb => tb.Tenant)
                .WithMany()
                .HasForeignKey(tb => tb.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(tb => tb.Category)
                .WithMany(bc => bc.TenantBudgets)
                .HasForeignKey(tb => tb.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
