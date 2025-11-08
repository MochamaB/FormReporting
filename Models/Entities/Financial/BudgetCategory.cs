using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Financial
{
    /// <summary>
    /// Hierarchical budget categories (Capital vs Recurrent)
    /// </summary>
    [Table("BudgetCategories")]
    public class BudgetCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        public int? ParentCategoryId { get; set; }

        public bool IsCapital { get; set; } = false; // Capital vs Recurrent

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey(nameof(ParentCategoryId))]
        public virtual BudgetCategory? ParentCategory { get; set; }

        public virtual ICollection<BudgetCategory> ChildCategories { get; set; } = new List<BudgetCategory>();
        public virtual ICollection<TenantBudget> TenantBudgets { get; set; } = new List<TenantBudget>();
        public virtual ICollection<TenantExpense> TenantExpenses { get; set; } = new List<TenantExpense>();
    }
}
