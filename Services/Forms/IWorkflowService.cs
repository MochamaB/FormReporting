using FormReporting.Models.ViewModels.Forms;
using FormReporting.Models.Entities.Forms;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service interface for managing workflow definitions and steps
    /// </summary>
    public interface IWorkflowService
    {
        // ===== Workflow CRUD =====
        
        /// <summary>
        /// Get all workflows with optional filtering
        /// </summary>
        Task<List<WorkflowListDto>> GetWorkflowsAsync(bool? isActive = null);

        /// <summary>
        /// Get workflow by ID with all steps
        /// </summary>
        Task<WorkflowDetailDto?> GetWorkflowByIdAsync(int workflowId);

        /// <summary>
        /// Create a new workflow
        /// </summary>
        Task<WorkflowDetailDto> CreateWorkflowAsync(WorkflowCreateDto dto);

        /// <summary>
        /// Update workflow details
        /// </summary>
        Task<WorkflowDetailDto?> UpdateWorkflowAsync(int workflowId, WorkflowUpdateDto dto);

        /// <summary>
        /// Soft delete a workflow (set IsActive = false)
        /// </summary>
        Task<bool> DeleteWorkflowAsync(int workflowId);

        // ===== Step Management =====

        /// <summary>
        /// Add a step to a workflow
        /// </summary>
        Task<WorkflowStepDto> AddStepAsync(int workflowId, WorkflowStepCreateDto dto);

        /// <summary>
        /// Update a workflow step
        /// </summary>
        Task<WorkflowStepDto?> UpdateStepAsync(int stepId, WorkflowStepUpdateDto dto);

        /// <summary>
        /// Delete a workflow step
        /// </summary>
        Task<bool> DeleteStepAsync(int stepId);

        /// <summary>
        /// Reorder workflow steps
        /// </summary>
        Task<bool> ReorderStepsAsync(int workflowId, List<StepReorderDto> newOrders);

        // ===== Validation & Utilities =====

        /// <summary>
        /// Validate workflow configuration
        /// </summary>
        Task<WorkflowValidationResultDto> ValidateWorkflowAsync(int workflowId);

        /// <summary>
        /// Clone an existing workflow
        /// </summary>
        Task<WorkflowDetailDto> CloneWorkflowAsync(int workflowId, string newName);

        /// <summary>
        /// Get all available workflow actions
        /// </summary>
        Task<List<WorkflowActionDto>> GetWorkflowActionsAsync();

        /// <summary>
        /// Check if workflow can be deleted (not in use)
        /// </summary>
        Task<bool> CanDeleteWorkflowAsync(int workflowId);

        // ===== Dual-Mode Validation =====

        /// <summary>
        /// Validate workflow against specific submission mode requirements
        /// </summary>
        Task<WorkflowValidationResultDto> ValidateWorkflowForModeAsync(int workflowId, int templateId);

        /// <summary>
        /// Validate workflow steps for Individual mode (no Fill actions, submission-level only)
        /// </summary>
        Task<WorkflowValidationResultDto> ValidateIndividualModeWorkflowAsync(int workflowId);

        /// <summary>
        /// Validate workflow steps for Collaborative mode (must have Fill steps, proper sequence)
        /// </summary>
        Task<WorkflowValidationResultDto> ValidateCollaborativeModeWorkflowAsync(int workflowId, int templateId);

        /// <summary>
        /// Check if all Fill steps have valid assignee resolution
        /// </summary>
        Task<(bool IsValid, List<string> Errors)> ValidateWorkflowAssigneeResolutionAsync(int workflowId);

        /// <summary>
        /// Validate step data for real-time wizard validation (no database save)
        /// </summary>
        Task<StepValidationResultDto> ValidateStepDataAsync(StepValidationDto dto, int templateId);

        /// <summary>
        /// Full validation before creating workflow (prevents invalid data from being saved)
        /// </summary>
        Task<StepValidationResultDto> ValidateBeforeCreateAsync(WorkflowStepCreateDto stepDto, int templateId);
    }
}
