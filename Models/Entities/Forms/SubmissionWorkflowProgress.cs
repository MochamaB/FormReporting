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
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Skipped

        public int? ReviewedBy { get; set; }

        public DateTime? ReviewedDate { get; set; }

        public string? Comments { get; set; }

        // Delegation and due date tracking
        public DateTime? DueDate { get; set; } // Calculated from submission date + WorkflowSteps.DueDays

        public int? DelegatedTo { get; set; } // User who received delegated approval authority

        public DateTime? DelegatedDate { get; set; } // When delegation occurred

        public int? DelegatedBy { get; set; } // Original approver who delegated

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(SubmissionId))]
        public virtual FormTemplateSubmission Submission { get; set; } = null!;

        [ForeignKey(nameof(StepId))]
        public virtual WorkflowStep Step { get; set; } = null!;

        [ForeignKey(nameof(ReviewedBy))]
        public virtual User? Reviewer { get; set; }

        [ForeignKey(nameof(DelegatedTo))]
        public virtual User? DelegatedToUser { get; set; }

        [ForeignKey(nameof(DelegatedBy))]
        public virtual User? DelegatedByUser { get; set; }
    }
}
