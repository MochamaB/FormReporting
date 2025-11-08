using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Individual approval steps within a workflow
    /// </summary>
    [Table("WorkflowSteps")]
    public class WorkflowStep
    {
        [Key]
        public int StepId { get; set; }

        [Required]
        public int WorkflowId { get; set; }

        [Required]
        public int StepOrder { get; set; }

        [Required]
        [StringLength(100)]
        public string StepName { get; set; } = string.Empty;

        public int? ApproverRoleId { get; set; }

        public int? ApproverUserId { get; set; }

        public bool IsMandatory { get; set; } = true;

        public string? ConditionLogic { get; set; }

        // Advanced workflow features
        public bool IsParallel { get; set; } = false; // Can this step run in parallel with other steps of same StepOrder?

        public int? DueDays { get; set; } // Days to complete step from submission date

        public int? EscalationRoleId { get; set; } // Escalate to this role if overdue

        public string? AutoApproveCondition { get; set; } // JSON: {"field": "amount", "operator": "<", "value": 1000}

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(WorkflowId))]
        public virtual WorkflowDefinition Workflow { get; set; } = null!;

        [ForeignKey(nameof(ApproverRoleId))]
        public virtual Role? ApproverRole { get; set; }

        [ForeignKey(nameof(ApproverUserId))]
        public virtual User? ApproverUser { get; set; }

        [ForeignKey(nameof(EscalationRoleId))]
        public virtual Role? EscalationRole { get; set; }

        public virtual ICollection<SubmissionWorkflowProgress> ProgressRecords { get; set; } = new List<SubmissionWorkflowProgress>();
    }
}
