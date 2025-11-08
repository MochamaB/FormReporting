using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Organizational;
using FormReporting.Models.Entities.Identity;
using FormReporting.Models.Entities.Hardware;
using FormReporting.Models.Entities.Software;

namespace FormReporting.Models.Entities.Tickets
{
    /// <summary>
    /// Support tickets with external system integration support
    /// </summary>
    [Table("Tickets")]
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        [Required]
        [StringLength(50)]
        public string TicketNumber { get; set; } = string.Empty; // e.g., TKT-2025-00123 or synced external ID

        [Required]
        public int TenantId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        // Basic Information
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Open"; // Open, InProgress, Escalated, Resolved, Closed, Cancelled

        // People
        [Required]
        public int ReportedBy { get; set; }

        public DateTime ReportedDate { get; set; } = DateTime.Now;

        public int? AssignedTo { get; set; }

        public DateTime? AssignedDate { get; set; }

        public int? EscalatedTo { get; set; }

        public DateTime? EscalatedDate { get; set; }

        public int? ResolvedBy { get; set; }

        public DateTime? ResolvedDate { get; set; }

        public DateTime? ClosedDate { get; set; }

        // Resolution
        public string? ResolutionNotes { get; set; }

        public int? ResolutionTime { get; set; } // Minutes to resolve (calculated)

        // SLA Tracking
        public DateTime? SLADueDate { get; set; }

        public bool IsSLABreached { get; set; } = false;

        // External System Integration
        public bool IsExternal { get; set; } = false; // Is this from external ticketing system?

        [StringLength(50)]
        public string? ExternalSystem { get; set; } // 'Internal', 'Jira', 'ServiceNow', 'Zendesk', 'Freshdesk', 'BMC', 'ManageEngine', 'Other'

        [StringLength(100)]
        public string? ExternalTicketId { get; set; } // ID in external system (e.g., JIRA-12345)

        [StringLength(500)]
        public string? ExternalTicketUrl { get; set; } // Deep link to external ticket

        public DateTime? LastSyncDate { get; set; } // When was this last synced from external system?

        [StringLength(20)]
        public string SyncStatus { get; set; } = "Synced"; // 'Synced', 'Pending', 'Failed', 'NotApplicable'

        [StringLength(500)]
        public string? SyncError { get; set; } // Error message if sync failed

        // Asset Linkage
        public int? RelatedHardwareId { get; set; } // Link to specific hardware if applicable

        public int? RelatedSoftwareId { get; set; } // Link to specific software installation if applicable

        // Metadata
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant Tenant { get; set; } = null!;

        [ForeignKey(nameof(CategoryId))]
        public virtual TicketCategory Category { get; set; } = null!;

        [ForeignKey(nameof(ReportedBy))]
        public virtual User Reporter { get; set; } = null!;

        [ForeignKey(nameof(AssignedTo))]
        public virtual User? AssignedUser { get; set; }

        [ForeignKey(nameof(EscalatedTo))]
        public virtual User? EscalatedUser { get; set; }

        [ForeignKey(nameof(ResolvedBy))]
        public virtual User? Resolver { get; set; }

        [ForeignKey(nameof(RelatedHardwareId))]
        public virtual TenantHardware? RelatedHardware { get; set; }

        [ForeignKey(nameof(RelatedSoftwareId))]
        public virtual TenantSoftwareInstallation? RelatedSoftware { get; set; }

        public virtual ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    }
}
