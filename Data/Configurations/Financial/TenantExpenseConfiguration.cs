using FormReporting.Models.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Financial
{
    public class TenantExpenseConfiguration : IEntityTypeConfiguration<TenantExpense>
    {
        public void Configure(EntityTypeBuilder<TenantExpense> builder)
        {
            // Primary Key
            builder.HasKey(te => te.ExpenseId);

            // Indexes
            builder.HasIndex(te => new { te.TenantId, te.ExpenseDate })
                .IsDescending(false, true)
                .HasDatabaseName("IX_Expenses_Tenant");

            builder.HasIndex(te => new { te.ExpenseDate, te.TenantId })
                .HasDatabaseName("IX_Expenses_Date");

            builder.HasIndex(te => new { te.ExpenseType, te.IsCapital })
                .HasDatabaseName("IX_Expenses_Type");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Expense_Type",
                "ExpenseType IN ('Purchase', 'Subscription', 'Maintenance', 'Service', 'Internal', 'Utility', 'Other')"
            ));

            // Default Values
            builder.Property(te => te.ExpenseType).HasDefaultValue("Purchase");
            builder.Property(te => te.IsCapital).HasDefaultValue(false);
            builder.Property(te => te.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(te => te.ModifiedDate).HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(te => te.Tenant)
                .WithMany()
                .HasForeignKey(te => te.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(te => te.Category)
                .WithMany(bc => bc.TenantExpenses)
                .HasForeignKey(te => te.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(te => te.Creator)
                .WithMany()
                .HasForeignKey(te => te.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
