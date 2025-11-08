using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Organizational;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Financial
{
    /// <summary>
    /// Tenant expenses with CAPEX/OPEX classification
    /// </summary>
    [Table("TenantExpenses")]
    public class TenantExpense
    {
        [Key]
        public int ExpenseId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime ExpenseDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? ReferenceNumber { get; set; }

        // Expense Classification
        [Required]
        [StringLength(30)]
        public string ExpenseType { get; set; } = "Purchase"; // Purchase, Subscription, Maintenance, Service, Internal, Utility, Other

        public bool IsCapital { get; set; } = false; // Capital vs Recurrent (CAPEX vs OPEX)

        // Vendor Information
        [StringLength(200)]
        public string? VendorName { get; set; } // NULL for internal expenses

        // Attachments (deprecated - use MediaFiles table instead)
        [StringLength(500)]
        public string? AttachmentPath { get; set; } // Legacy field, migrate to MediaFiles

        // Approval
        public int? ApprovedBy { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ApprovedDate { get; set; }

        // Audit
        [Required]
        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant Tenant { get; set; } = null!;

        [ForeignKey(nameof(CategoryId))]
        public virtual BudgetCategory Category { get; set; } = null!;

        [ForeignKey(nameof(CreatedBy))]
        public virtual User Creator { get; set; } = null!;
    }
}
