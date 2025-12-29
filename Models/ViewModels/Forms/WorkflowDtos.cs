using System.ComponentModel.DataAnnotations;

namespace FormReporting.Models.ViewModels.Forms
{
    /// <summary>
    /// DTO for creating a new workflow
    /// </summary>
    public class WorkflowCreateDto
    {
        [Required]
        [StringLength(100)]
        public string WorkflowName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int? TemplateId { get; set; }

        public List<WorkflowStepCreateDto> Steps { get; set; } = new();
    }

    /// <summary>
    /// DTO for updating a workflow
    /// </summary>
    public class WorkflowUpdateDto
    {
        [StringLength(100)]
        public string? WorkflowName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO for creating a workflow step
    /// </summary>
    public class WorkflowStepCreateDto
    {
        public int? StepOrder { get; set; }

        [Required]
        [StringLength(100)]
        public string StepName { get; set; } = string.Empty;

        public int? ActionId { get; set; }

        [StringLength(20)]
        public string TargetType { get; set; } = "Submission"; // Submission, Section, Field

        public int? TargetId { get; set; }

        [StringLength(20)]
        public string AssigneeType { get; set; } = "Role"; // Role, User, Submitter, PreviousActor, FieldValue, Department

        public int? ApproverRoleId { get; set; }
        public int? ApproverUserId { get; set; }
        public int? AssigneeDepartmentId { get; set; }
        public int? AssigneeFieldId { get; set; }

        public bool IsMandatory { get; set; } = true;
        public bool IsParallel { get; set; } = false;
        public int? DueDays { get; set; }
        public int? EscalationRoleId { get; set; }

        public string? ConditionLogic { get; set; }
        public string? AutoApproveCondition { get; set; }
        public string? DependsOnStepIds { get; set; } // JSON array: "[1, 2]"
    }

    /// <summary>
    /// DTO for updating a workflow step
    /// </summary>
    public class WorkflowStepUpdateDto
    {
        public int StepId { get; set; }
        public int? StepOrder { get; set; }
        public string StepName { get; set; } = string.Empty;
        public int? ActionId { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public int? TargetId { get; set; }
        public string AssigneeType { get; set; } = string.Empty;
        public int? ApproverRoleId { get; set; }
        public int? ApproverUserId { get; set; }
        public int? AssigneeDepartmentId { get; set; }
        public int? AssigneeFieldId { get; set; }
        public bool? IsMandatory { get; set; }
        public bool? IsParallel { get; set; }
        public int? DueDays { get; set; }
        public int? EscalationRoleId { get; set; }
        public string? ConditionLogic { get; set; }
        public string? AutoApproveCondition { get; set; }
        public string? DependsOnStepIds { get; set; }
    }

    public class StepValidationDto
    {
        public int TemplateId { get; set; }
        public string StepId { get; set; } = string.Empty; // "target", "action", "assignee", "settings"

        // Target Step Data
        public string? TargetType { get; set; }
        public int? TargetId { get; set; }

        // Action Step Data
        public int? ActionId { get; set; }
        public string? StepName { get; set; }

        // Assignee Step Data
        public string? AssigneeType { get; set; }
        public int? ApproverRoleId { get; set; }
        public int? ApproverUserId { get; set; }
        public int? AssigneeDepartmentId { get; set; }
        public int? AssigneeFieldId { get; set; }

        // Settings Step Data
        public bool IsMandatory { get; set; }
        public bool IsParallel { get; set; }
        public int? DueDays { get; set; }
        public int? EscalationRoleId { get; set; }
        public string? ConditionLogic { get; set; }
        public string? AutoApproveCondition { get; set; }
        public string? DependsOnStepIds { get; set; }
    }

    /// <summary>
    /// Result of step-by-step validation for wizard
    /// </summary>
    public class StepValidationResultDto
    {
        public bool IsValid { get; set; }
        public string StepId { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for editing workflow basic details
    /// </summary>
    public class WorkflowEditViewModel
    {
        public int WorkflowId { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Workflow Name")]
        public string WorkflowName { get; set; } = string.Empty;
        
        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for workflow list item
    /// </summary>
    public class WorkflowListDto
    {
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int StepCount { get; set; }
        public int TemplateCount { get; set; } // Number of templates using this workflow
        public DateTime CreatedDate { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime ModifiedDate { get; set; }
    }

    /// <summary>
    /// DTO for workflow details with steps
    /// </summary>
    public class WorkflowDetailDto : WorkflowListDto
    {
        public List<WorkflowStepDto> Steps { get; set; } = new();
        public List<WorkflowTemplateDto> Templates { get; set; } = new();
    }

    /// <summary>
    /// DTO for workflow step details
    /// </summary>
    public class WorkflowStepDto
    {
        public int StepId { get; set; }
        public int WorkflowId { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; } = string.Empty;

        public int? ActionId { get; set; }
        public string? ActionCode { get; set; }
        public string? ActionName { get; set; }
        public string? ActionIconClass { get; set; }
        public string? ActionCssClass { get; set; }
        public bool RequiresSignature { get; set; }
        public bool RequiresComment { get; set; }

        public string TargetType { get; set; } = "Submission";
        public int? TargetId { get; set; }
        public string? TargetName { get; set; } // Resolved section/field name

        public string AssigneeType { get; set; } = "Role";
        public int? ApproverRoleId { get; set; }
        public string? ApproverRoleName { get; set; }
        public int? ApproverUserId { get; set; }
        public string? ApproverUserName { get; set; }
        public int? AssigneeDepartmentId { get; set; }
        public string? AssigneeDepartmentName { get; set; }
        public int? AssigneeFieldId { get; set; }
        public string? AssigneeFieldLabel { get; set; }

        public bool IsMandatory { get; set; }
        public bool IsParallel { get; set; }
        public int? DueDays { get; set; }
        public int? EscalationRoleId { get; set; }
        public string? EscalationRoleName { get; set; }

        public string? ConditionLogic { get; set; }
        public string? AutoApproveCondition { get; set; }
        public List<int>? DependsOnStepIds { get; set; }

        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// DTO for templates using a workflow
    /// </summary>
    public class WorkflowTemplateDto
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateCode { get; set; } = string.Empty;
        public string PublishStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for workflow action (lookup)
    /// </summary>
    public class WorkflowActionDto
    {
        public int ActionId { get; set; }
        public string ActionCode { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool RequiresSignature { get; set; }
        public bool RequiresComment { get; set; }
        public bool AllowDelegate { get; set; }
        public string? IconClass { get; set; }
        public string? CssClass { get; set; }
    }

    /// <summary>
    /// DTO for step reordering
    /// </summary>
    public class StepReorderDto
    {
        public int StepId { get; set; }
        public int NewOrder { get; set; }
    }

    /// <summary>
    /// DTO for completing a workflow step
    /// </summary>
    public class StepCompleteDto
    {
        [Required]
        public int ProgressId { get; set; }

        public string? Comments { get; set; }

        // Signature data (if required)
        public string? SignatureType { get; set; } // Checkbox, Digital, PIN
        public string? SignatureData { get; set; } // Base64 for digital, hash for PIN
    }

    /// <summary>
    /// DTO for rejecting a workflow step
    /// </summary>
    public class StepRejectDto
    {
        [Required]
        public int ProgressId { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for delegating a workflow step
    /// </summary>
    public class StepDelegateDto
    {
        [Required]
        public int ProgressId { get; set; }

        [Required]
        public int DelegateToUserId { get; set; }

        public string? Reason { get; set; }
    }

    /// <summary>
    /// DTO for submission workflow progress
    /// </summary>
    public class WorkflowProgressDto
    {
        public int SubmissionId { get; set; }
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public string OverallStatus { get; set; } = string.Empty; // Pending, InProgress, Completed, Rejected
        public int TotalSteps { get; set; }
        public int CompletedSteps { get; set; }
        public int CurrentStepOrder { get; set; }
        public decimal ProgressPercentage => TotalSteps > 0 
            ? Math.Round((decimal)CompletedSteps / TotalSteps * 100, 0) 
            : 0;

        public List<StepProgressDto> Steps { get; set; } = new();
    }

    /// <summary>
    /// DTO for individual step progress
    /// </summary>
    public class StepProgressDto
    {
        public int ProgressId { get; set; }
        public int StepId { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; } = string.Empty;

        public string? ActionCode { get; set; }
        public string? ActionName { get; set; }
        public string? ActionIconClass { get; set; }
        public string? ActionCssClass { get; set; }
        public bool RequiresSignature { get; set; }
        public bool RequiresComment { get; set; }

        public string TargetType { get; set; } = "Submission";
        public int? TargetId { get; set; }
        public string? TargetName { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Approved, Rejected, Skipped
        public bool IsCurrent { get; set; }
        public bool CanAct { get; set; } // Whether current user can act on this step

        public int? AssignedToId { get; set; }
        public string? AssignedToName { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status == "Pending";

        public int? ReviewedById { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string? Comments { get; set; }

        public bool HasSignature { get; set; }
        public string? SignatureType { get; set; }
        public DateTime? SignatureTimestamp { get; set; }

        public int? DelegatedToId { get; set; }
        public string? DelegatedToName { get; set; }
        public DateTime? DelegatedDate { get; set; }
        public string? DelegationReason { get; set; }
    }

    /// <summary>
    /// DTO for user's pending actions
    /// </summary>
    public class PendingActionDto
    {
        public int ProgressId { get; set; }
        public int SubmissionId { get; set; }
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateCode { get; set; } = string.Empty;

        public int StepId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string? ActionCode { get; set; }
        public string? ActionName { get; set; }
        public string? ActionIconClass { get; set; }
        public bool RequiresSignature { get; set; }

        public string TargetType { get; set; } = "Submission";
        public string? TargetName { get; set; }

        public string SubmittedByName { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow;
        public int DaysUntilDue => DueDate.HasValue ? (int)(DueDate.Value - DateTime.UtcNow).TotalDays : 0;

        public bool IsDelegated { get; set; }
        public string? DelegatedFromName { get; set; }
    }

    /// <summary>
    /// DTO for workflow validation result
    /// </summary>
    public class WorkflowValidationResultDto
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// DTO for template readiness validation
    /// </summary>
    public class TemplateReadinessDto
    {
        public int TemplateId { get; set; }
        public bool IsReady { get; set; }
        public string SubmissionMode { get; set; } = string.Empty;
        public List<string> BlockingIssues { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public TemplateConfigurationStatusDto Configuration { get; set; } = new();
    }

    /// <summary>
    /// DTO for detailed template configuration status
    /// </summary>
    public class TemplateConfigurationStatusDto
    {
        // Form Structure
        public bool HasFormStructure { get; set; }
        public int SectionCount { get; set; }
        public int FieldCount { get; set; }
        
        // Assignments
        public bool HasAssignments { get; set; }
        public int ActiveAssignmentCount { get; set; }
        public bool AssignmentsCoverUsers { get; set; }
        
        // Workflow
        public bool HasWorkflow { get; set; }
        public string? WorkflowName { get; set; }
        public int WorkflowStepCount { get; set; }
        public bool WorkflowValidForMode { get; set; }
        public List<string> WorkflowIssues { get; set; } = new();
        
        // Submission Rules (if applicable)
        public bool HasSubmissionRules { get; set; }
        public bool SubmissionRulesValid { get; set; }
        
        // Overall Status
        public string TemplateStatus { get; set; } = string.Empty; // Draft, Published, etc.
        public bool ReadyForSubmissions { get; set; }
        public DateTime? LastValidated { get; set; }
    }
}
