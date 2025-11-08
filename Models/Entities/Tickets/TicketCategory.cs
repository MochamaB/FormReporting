using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Tickets
{
    /// <summary>
    /// Hierarchical ticket categories with SLA configuration
    /// </summary>
    [Table("TicketCategories")]
    public class TicketCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int? ParentCategoryId { get; set; }

        // SLA Configuration
        public int? SLAHours { get; set; } // Resolution time target

        public int? EscalationHours { get; set; } // When to escalate if not resolved

        public string? GenericCategoryMapping { get; set; } // JSON: {"system": "category"} for other systems

        // Display and Organization
        public int DisplayOrder { get; set; } = 0;

        [StringLength(50)]
        public string? IconClass { get; set; } // UI icon class

        [StringLength(20)]
        public string? ColorCode { get; set; } // Hex color for UI

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey(nameof(ParentCategoryId))]
        public virtual TicketCategory? ParentCategory { get; set; }

        public virtual ICollection<TicketCategory> ChildCategories { get; set; } = new List<TicketCategory>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
