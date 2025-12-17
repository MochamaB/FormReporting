using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Organizational;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Form template assignments - defines WHO can access/submit a form template.
    /// Supports 8 assignment types (tenant-based + user-based).
    /// Submission rules (WHEN/HOW) are managed separately in FormTemplateSubmissionRule.
    /// </summary>
    [Table("FormTemplateAssignments")]
    public class FormTemplateAssignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        public int TemplateId { get; set; }

        // ===== ASSIGNMENT TYPE (8 types total) =====
        /// <summary>
        /// Tenant-based: 'All', 'TenantType', 'TenantGroup', 'SpecificTenant'
        /// User-based: 'Role', 'Department', 'UserGroup', 'SpecificUser'
        /// </summary>
        [Required]
        [StringLength(50)]
        public string AssignmentType { get; set; } = string.Empty;

        // ===== TENANT-BASED TARGETS =====
        [StringLength(20)]
        public string? TenantType { get; set; }

        public int? TenantGroupId { get; set; }

        public int? TenantId { get; set; }

        // ===== USER-BASED TARGETS =====
        public int? RoleId { get; set; }

        public int? DepartmentId { get; set; }

        public int? UserGroupId { get; set; }

        public int? UserId { get; set; }

        // ===== ACCESS PERIOD =====
        /// <summary>
        /// When the assignment becomes effective (access starts)
        /// </summary>
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the assignment expires (NULL = indefinite access)
        /// </summary>
        public DateTime? EffectiveUntil { get; set; }

        // ===== ACCESS OPTIONS =====
        /// <summary>
        /// Allow anonymous submissions (no login required)
        /// </summary>
        public bool AllowAnonymous { get; set; } = false;

        // ===== ASSIGNMENT STATUS =====
        /// <summary>
        /// Status: Active, Suspended, Revoked
        /// </summary>
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        // ===== CANCELLATION =====
        /// <summary>
        /// User who cancelled/revoked this assignment
        /// </summary>
        public int? CancelledBy { get; set; }

        /// <summary>
        /// When the assignment was cancelled
        /// </summary>
        public DateTime? CancelledDate { get; set; }

        /// <summary>
        /// Reason for cancellation
        /// </summary>
        [StringLength(500)]
        public string? CancellationReason { get; set; }

        // ===== METADATA =====
        [Required]
        public int AssignedBy { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Notes { get; set; }

        // ===== NAVIGATION PROPERTIES =====
        [ForeignKey(nameof(TemplateId))]
        public virtual FormTemplate Template { get; set; } = null!;

        [ForeignKey(nameof(TenantGroupId))]
        public virtual TenantGroup? TenantGroup { get; set; }

        [ForeignKey(nameof(TenantId))]
        public virtual Tenant? Tenant { get; set; }

        [ForeignKey(nameof(RoleId))]
        public virtual Role? Role { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department? Department { get; set; }

        [ForeignKey(nameof(UserGroupId))]
        public virtual UserGroup? UserGroup { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(AssignedBy))]
        public virtual User AssignedByUser { get; set; } = null!;

        [ForeignKey(nameof(CancelledBy))]
        public virtual User? CancelledByUser { get; set; }
    }
}
