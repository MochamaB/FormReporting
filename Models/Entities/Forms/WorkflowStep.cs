using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;
using FormReporting.Models.Entities.Organizational;

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

        // ===== NEW: Action Type =====
        /// <summary>
        /// FK to WorkflowAction (Fill, Sign, Approve, Reject, Review, Verify)
        /// </summary>
        public int? ActionId { get; set; }

        // ===== NEW: Target Scope =====
        /// <summary>
        /// What this step targets: "Submission", "Section", "Field"
        /// </summary>
        [StringLength(20)]
        public string TargetType { get; set; } = "Submission";

        /// <summary>
        /// SectionId or ItemId when TargetType is "Section" or "Field". NULL = whole submission
        /// </summary>
        public int? TargetId { get; set; }

        // ===== NEW: Enhanced Assignee Resolution =====
        /// <summary>
        /// How to resolve the assignee: "Role", "User", "Submitter", "PreviousActor", "FieldValue", "Department"
        /// </summary>
        [StringLength(20)]
        public string AssigneeType { get; set; } = "Role";

        /// <summary>
        /// Department ID when AssigneeType is "Department"
        /// </summary>
        public int? AssigneeDepartmentId { get; set; }

        /// <summary>
        /// FormTemplateItem ID when AssigneeType is "FieldValue" (user ID comes from this field's value)
        /// </summary>
        public int? AssigneeFieldId { get; set; }

        // ===== EXISTING: Approver fields (kept for backward compatibility) =====
        public int? ApproverRoleId { get; set; }

        public int? ApproverUserId { get; set; }

        public bool IsMandatory { get; set; } = true;

        public string? ConditionLogic { get; set; }

        // ===== NEW: Step Dependencies =====
        /// <summary>
        /// JSON array of StepIds that must complete before this step can start
        /// Example: "[1, 2]" means steps 1 and 2 must complete first
        /// </summary>
        public string? DependsOnStepIds { get; set; }

        // Advanced workflow features
        public bool IsParallel { get; set; } = false; // Can this step run in parallel with other steps of same StepOrder?

        public int? DueDays { get; set; } // Days to complete step from submission date

        public int? EscalationRoleId { get; set; } // Escalate to this role if overdue

        public string? AutoApproveCondition { get; set; } // JSON: {"field": "amount", "operator": "<", "value": 1000}

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(WorkflowId))]
        public virtual WorkflowDefinition Workflow { get; set; } = null!;

        [ForeignKey(nameof(ActionId))]
        public virtual WorkflowAction? Action { get; set; }

        [ForeignKey(nameof(ApproverRoleId))]
        public virtual Role? ApproverRole { get; set; }

        [ForeignKey(nameof(ApproverUserId))]
        public virtual User? ApproverUser { get; set; }

        [ForeignKey(nameof(AssigneeDepartmentId))]
        public virtual Department? AssigneeDepartment { get; set; }

        [ForeignKey(nameof(AssigneeFieldId))]
        public virtual FormTemplateItem? AssigneeField { get; set; }

        [ForeignKey(nameof(EscalationRoleId))]
        public virtual Role? EscalationRole { get; set; }

        public virtual ICollection<SubmissionWorkflowProgress> ProgressRecords { get; set; } = new List<SubmissionWorkflowProgress>();
    }
}
