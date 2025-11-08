using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Organizational;

namespace FormReporting.Models.Entities.Financial
{
    /// <summary>
    /// Tenant budgets per fiscal year and category
    /// </summary>
    [Table("TenantBudgets")]
    public class TenantBudget
    {
        [Key]
        public int BudgetId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        public int FiscalYear { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BudgetedAmount { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ApprovedDate { get; set; }

        public int? ApprovedBy { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant Tenant { get; set; } = null!;

        [ForeignKey(nameof(CategoryId))]
        public virtual BudgetCategory Category { get; set; } = null!;
    }
}
