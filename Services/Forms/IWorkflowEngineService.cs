using FormReporting.Models.ViewModels.Forms;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service interface for executing workflows at runtime
    /// </summary>
    public interface IWorkflowEngineService
    {
        // ===== Workflow Initialization =====

        /// <summary>
        /// Initialize workflow progress records when a submission starts
        /// </summary>
        Task<WorkflowProgressDto> InitializeSubmissionWorkflowAsync(int submissionId);

        /// <summary>
        /// Get current workflow progress for a submission
        /// </summary>
        Task<WorkflowProgressDto?> GetSubmissionProgressAsync(int submissionId);

        // ===== Step Actions =====

        /// <summary>
        /// Complete a workflow step (approve, sign, fill, etc.)
        /// </summary>
        Task<StepProgressDto> CompleteStepAsync(StepCompleteDto dto);

        /// <summary>
        /// Reject a workflow step
        /// </summary>
        Task<StepProgressDto> RejectStepAsync(StepRejectDto dto);

        /// <summary>
        /// Delegate a workflow step to another user
        /// </summary>
        Task<StepProgressDto> DelegateStepAsync(StepDelegateDto dto);

        // ===== User Queries =====

        /// <summary>
        /// Get all pending actions for a user
        /// </summary>
        Task<List<PendingActionDto>> GetPendingActionsForUserAsync(int userId);

        /// <summary>
        /// Get pending action count for a user (for badges/notifications)
        /// </summary>
        Task<int> GetPendingActionCountAsync(int userId);

        /// <summary>
        /// Check if user can act on a specific step
        /// </summary>
        Task<bool> CanUserActOnStepAsync(int userId, int progressId);

        /// <summary>
        /// Check if user can act on a section (for section-level workflows)
        /// </summary>
        Task<bool> CanUserActOnSectionAsync(int userId, int submissionId, int sectionId);

        /// <summary>
        /// Check if user can act on a field (for field-level workflows)
        /// </summary>
        Task<bool> CanUserActOnFieldAsync(int userId, int submissionId, int fieldId);

        // ===== Step Queries =====

        /// <summary>
        /// Get the current active step(s) for a submission
        /// </summary>
        Task<List<StepProgressDto>> GetCurrentStepsAsync(int submissionId);

        /// <summary>
        /// Get step progress by ID
        /// </summary>
        Task<StepProgressDto?> GetStepProgressAsync(int progressId);

        /// <summary>
        /// Check if all step dependencies are met
        /// </summary>
        Task<bool> CheckStepDependenciesAsync(int progressId);

        // ===== Workflow State =====

        /// <summary>
        /// Check if submission workflow is complete
        /// </summary>
        Task<bool> IsWorkflowCompleteAsync(int submissionId);

        /// <summary>
        /// Get overall workflow status for a submission
        /// </summary>
        Task<string> GetWorkflowStatusAsync(int submissionId);

        // ===== Background Jobs =====

        /// <summary>
        /// Process escalations for overdue steps
        /// </summary>
        Task<int> ProcessEscalationsAsync();

        /// <summary>
        /// Auto-approve steps that meet auto-approve conditions
        /// </summary>
        Task<int> ProcessAutoApprovalsAsync();
    }
}
