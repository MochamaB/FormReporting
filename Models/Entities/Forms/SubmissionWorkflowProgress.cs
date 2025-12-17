using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Tracks multi-level approval progress for form submissions
    /// </summary>
    [Table("SubmissionWorkflowProgress")]
    public class SubmissionWorkflowProgress
    {
        [Key]
        public int ProgressId { get; set; }

        [Required]
        public int SubmissionId { get; set; }

        [Required]
        public int StepId { get; set; }

        [Required]
        public int StepOrder { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Approved, Rejected, Skipped

        // ===== NEW: Action & Target (denormalized from WorkflowStep for query performance) =====
        /// <summary>
        /// FK to WorkflowAction (denormalized from step for easier querying)
        /// </summary>
        public int? ActionId { get; set; }

        /// <summary>
        /// Target type: "Submission", "Section", "Field" (denormalized from step)
        /// </summary>
        [StringLength(20)]
        public string? TargetType { get; set; }

        /// <summary>
        /// Target ID (SectionId or ItemId) when applicable (denormalized from step)
        /// </summary>
        public int? TargetId { get; set; }

        public int? ReviewedBy { get; set; }

        public DateTime? ReviewedDate { get; set; }

        public string? Comments { get; set; }

        // ===== NEW: Signature Fields =====
        /// <summary>
        /// Type of signature: "Checkbox", "Digital", "PIN", "Biometric"
        /// </summary>
        [StringLength(20)]
        public string? SignatureType { get; set; }

        /// <summary>
        /// Signature data (base64 for digital signature image, hash for PIN, etc.)
        /// </summary>
        public string? SignatureData { get; set; }

        /// <summary>
        /// IP address when signature was captured
        /// </summary>
        [StringLength(45)]
        public string? SignatureIP { get; set; }

        /// <summary>
        /// Timestamp when signature was captured
        /// </summary>
        public DateTime? SignatureTimestamp { get; set; }

        // ===== NEW: Assignment Tracking =====
        /// <summary>
        /// When this step was assigned to the current user
        /// </summary>
        public DateTime? AssignedDate { get; set; }

        /// <summary>
        /// User ID of the assigned actor (resolved from WorkflowStep assignee configuration)
        /// </summary>
        public int? AssignedTo { get; set; }

        // Delegation and due date tracking
        public DateTime? DueDate { get; set; } // Calculated from submission date + WorkflowSteps.DueDays

        public int? DelegatedTo { get; set; } // User who received delegated approval authority

        public DateTime? DelegatedDate { get; set; } // When delegation occurred

        public int? DelegatedBy { get; set; } // Original approver who delegated

        /// <summary>
        /// Reason for delegation
        /// </summary>
        [StringLength(500)]
        public string? DelegationReason { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(SubmissionId))]
        public virtual FormTemplateSubmission Submission { get; set; } = null!;

        [ForeignKey(nameof(StepId))]
        public virtual WorkflowStep Step { get; set; } = null!;

        [ForeignKey(nameof(ActionId))]
        public virtual WorkflowAction? Action { get; set; }

        [ForeignKey(nameof(ReviewedBy))]
        public virtual User? Reviewer { get; set; }

        [ForeignKey(nameof(AssignedTo))]
        public virtual User? AssignedToUser { get; set; }

        [ForeignKey(nameof(DelegatedTo))]
        public virtual User? DelegatedToUser { get; set; }

        [ForeignKey(nameof(DelegatedBy))]
        public virtual User? DelegatedByUser { get; set; }
    }
}
