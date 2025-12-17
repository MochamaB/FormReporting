using FormReporting.Data;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Services.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service for managing workflow definitions and steps
    /// </summary>
    public class WorkflowService : IWorkflowService
    {
        private readonly ApplicationDbContext _context;
        private readonly IClaimsService _claimsService;
        private readonly ILogger<WorkflowService> _logger;

        public WorkflowService(
            ApplicationDbContext context,
            IClaimsService claimsService,
            ILogger<WorkflowService> logger)
        {
            _context = context;
            _claimsService = claimsService;
            _logger = logger;
        }

        #region Workflow CRUD

        public async Task<List<WorkflowListDto>> GetWorkflowsAsync(bool? isActive = null)
        {
            var query = _context.WorkflowDefinitions
                .Include(w => w.Steps)
                .Include(w => w.FormTemplates)
                .Include(w => w.Creator)
                .AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(w => w.IsActive == isActive.Value);
            }

            var workflows = await query
                .OrderByDescending(w => w.ModifiedDate)
                .Select(w => new WorkflowListDto
                {
                    WorkflowId = w.WorkflowId,
                    WorkflowName = w.WorkflowName,
                    Description = w.Description,
                    IsActive = w.IsActive,
                    StepCount = w.Steps.Count,
                    TemplateCount = w.FormTemplates.Count,
                    CreatedDate = w.CreatedDate,
                    CreatedByName = w.Creator.FullName,
                    ModifiedDate = w.ModifiedDate
                })
                .ToListAsync();

            return workflows;
        }

        public async Task<WorkflowDetailDto?> GetWorkflowByIdAsync(int workflowId)
        {
            var workflow = await _context.WorkflowDefinitions
                .Include(w => w.Steps.OrderBy(s => s.StepOrder))
                    .ThenInclude(s => s.Action)
                .Include(w => w.Steps)
                    .ThenInclude(s => s.ApproverRole)
                .Include(w => w.Steps)
                    .ThenInclude(s => s.ApproverUser)
                .Include(w => w.Steps)
                    .ThenInclude(s => s.AssigneeDepartment)
                .Include(w => w.Steps)
                    .ThenInclude(s => s.EscalationRole)
                .Include(w => w.FormTemplates)
                .Include(w => w.Creator)
                .FirstOrDefaultAsync(w => w.WorkflowId == workflowId);

            if (workflow == null)
                return null;

            return MapToDetailDto(workflow);
        }

        public async Task<WorkflowDetailDto> CreateWorkflowAsync(WorkflowCreateDto dto)
        {
            var userId = _claimsService.GetUserId();

            var workflow = new WorkflowDefinition
            {
                WorkflowName = dto.WorkflowName,
                Description = dto.Description,
                IsActive = true,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.WorkflowDefinitions.Add(workflow);
            await _context.SaveChangesAsync();

            // Add steps if provided
            if (dto.Steps.Any())
            {
                foreach (var stepDto in dto.Steps.OrderBy(s => s.StepOrder))
                {
                    var step = MapToStepEntity(stepDto, workflow.WorkflowId);
                    _context.WorkflowSteps.Add(step);
                }
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Created workflow {WorkflowId}: {WorkflowName}", workflow.WorkflowId, workflow.WorkflowName);

            return (await GetWorkflowByIdAsync(workflow.WorkflowId))!;
        }

        public async Task<WorkflowDetailDto?> UpdateWorkflowAsync(int workflowId, WorkflowUpdateDto dto)
        {
            var workflow = await _context.WorkflowDefinitions.FindAsync(workflowId);
            if (workflow == null)
                return null;

            if (!string.IsNullOrEmpty(dto.WorkflowName))
                workflow.WorkflowName = dto.WorkflowName;

            if (dto.Description != null)
                workflow.Description = dto.Description;

            if (dto.IsActive.HasValue)
                workflow.IsActive = dto.IsActive.Value;

            workflow.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated workflow {WorkflowId}", workflowId);

            return await GetWorkflowByIdAsync(workflowId);
        }

        public async Task<bool> DeleteWorkflowAsync(int workflowId)
        {
            var workflow = await _context.WorkflowDefinitions.FindAsync(workflowId);
            if (workflow == null)
                return false;

            // Check if workflow is in use
            var inUse = await _context.FormTemplates.AnyAsync(t => t.WorkflowId == workflowId);
            if (inUse)
            {
                // Soft delete
                workflow.IsActive = false;
                workflow.ModifiedDate = DateTime.UtcNow;
            }
            else
            {
                // Hard delete if not in use
                _context.WorkflowDefinitions.Remove(workflow);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted workflow {WorkflowId} (soft: {SoftDelete})", workflowId, inUse);

            return true;
        }

        #endregion

        #region Step Management

        public async Task<WorkflowStepDto> AddStepAsync(int workflowId, WorkflowStepCreateDto dto)
        {
            var workflow = await _context.WorkflowDefinitions.FindAsync(workflowId);
            if (workflow == null)
                throw new ArgumentException($"Workflow {workflowId} not found");

            var step = MapToStepEntity(dto, workflowId);
            _context.WorkflowSteps.Add(step);
            await _context.SaveChangesAsync();

            workflow.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Added step {StepId} to workflow {WorkflowId}", step.StepId, workflowId);

            return await GetStepDtoAsync(step.StepId);
        }

        public async Task<WorkflowStepDto?> UpdateStepAsync(int stepId, WorkflowStepUpdateDto dto)
        {
            var step = await _context.WorkflowSteps
                .Include(s => s.Workflow)
                .FirstOrDefaultAsync(s => s.StepId == stepId);

            if (step == null)
                return null;

            // Update fields if provided
            if (dto.StepOrder.HasValue) step.StepOrder = dto.StepOrder.Value;
            if (!string.IsNullOrEmpty(dto.StepName)) step.StepName = dto.StepName;
            if (dto.ActionId.HasValue) step.ActionId = dto.ActionId;
            if (!string.IsNullOrEmpty(dto.TargetType)) step.TargetType = dto.TargetType;
            if (dto.TargetId.HasValue) step.TargetId = dto.TargetId;
            if (!string.IsNullOrEmpty(dto.AssigneeType)) step.AssigneeType = dto.AssigneeType;
            if (dto.ApproverRoleId.HasValue) step.ApproverRoleId = dto.ApproverRoleId;
            if (dto.ApproverUserId.HasValue) step.ApproverUserId = dto.ApproverUserId;
            if (dto.AssigneeDepartmentId.HasValue) step.AssigneeDepartmentId = dto.AssigneeDepartmentId;
            if (dto.AssigneeFieldId.HasValue) step.AssigneeFieldId = dto.AssigneeFieldId;
            if (dto.IsMandatory.HasValue) step.IsMandatory = dto.IsMandatory.Value;
            if (dto.IsParallel.HasValue) step.IsParallel = dto.IsParallel.Value;
            if (dto.DueDays.HasValue) step.DueDays = dto.DueDays;
            if (dto.EscalationRoleId.HasValue) step.EscalationRoleId = dto.EscalationRoleId;
            if (dto.ConditionLogic != null) step.ConditionLogic = dto.ConditionLogic;
            if (dto.AutoApproveCondition != null) step.AutoApproveCondition = dto.AutoApproveCondition;
            if (dto.DependsOnStepIds != null) step.DependsOnStepIds = dto.DependsOnStepIds;

            step.Workflow.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated step {StepId}", stepId);

            return await GetStepDtoAsync(stepId);
        }

        public async Task<bool> DeleteStepAsync(int stepId)
        {
            var step = await _context.WorkflowSteps
                .Include(s => s.Workflow)
                .FirstOrDefaultAsync(s => s.StepId == stepId);

            if (step == null)
                return false;

            // Check if step has progress records
            var hasProgress = await _context.SubmissionWorkflowProgresses.AnyAsync(p => p.StepId == stepId);
            if (hasProgress)
            {
                _logger.LogWarning("Cannot delete step {StepId} - has progress records", stepId);
                return false;
            }

            _context.WorkflowSteps.Remove(step);
            step.Workflow.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted step {StepId} from workflow {WorkflowId}", stepId, step.WorkflowId);

            return true;
        }

        public async Task<bool> ReorderStepsAsync(int workflowId, List<StepReorderDto> newOrders)
        {
            var steps = await _context.WorkflowSteps
                .Where(s => s.WorkflowId == workflowId)
                .ToListAsync();

            foreach (var order in newOrders)
            {
                var step = steps.FirstOrDefault(s => s.StepId == order.StepId);
                if (step != null)
                {
                    step.StepOrder = order.NewOrder;
                }
            }

            var workflow = await _context.WorkflowDefinitions.FindAsync(workflowId);
            if (workflow != null)
            {
                workflow.ModifiedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reordered steps for workflow {WorkflowId}", workflowId);

            return true;
        }

        #endregion

        #region Validation & Utilities

        public async Task<WorkflowValidationResultDto> ValidateWorkflowAsync(int workflowId)
        {
            var result = new WorkflowValidationResultDto { IsValid = true };

            var workflow = await _context.WorkflowDefinitions
                .Include(w => w.Steps)
                .FirstOrDefaultAsync(w => w.WorkflowId == workflowId);

            if (workflow == null)
            {
                result.IsValid = false;
                result.Errors.Add("Workflow not found");
                return result;
            }

            // Check for at least one step
            if (!workflow.Steps.Any())
            {
                result.IsValid = false;
                result.Errors.Add("Workflow must have at least one step");
            }

            // Check for duplicate step orders
            var duplicateOrders = workflow.Steps
                .GroupBy(s => s.StepOrder)
                .Where(g => g.Count() > 1 && !g.All(s => s.IsParallel))
                .Select(g => g.Key)
                .ToList();

            if (duplicateOrders.Any())
            {
                result.IsValid = false;
                result.Errors.Add($"Duplicate step orders found (non-parallel): {string.Join(", ", duplicateOrders)}");
            }

            // Check for circular dependencies
            foreach (var step in workflow.Steps.Where(s => !string.IsNullOrEmpty(s.DependsOnStepIds)))
            {
                try
                {
                    var dependsOn = JsonSerializer.Deserialize<List<int>>(step.DependsOnStepIds!);
                    if (dependsOn != null)
                    {
                        // Check if dependencies exist
                        var missingDeps = dependsOn.Where(d => !workflow.Steps.Any(s => s.StepId == d)).ToList();
                        if (missingDeps.Any())
                        {
                            result.Warnings.Add($"Step {step.StepId} references non-existent dependencies: {string.Join(", ", missingDeps)}");
                        }

                        // Check for self-reference
                        if (dependsOn.Contains(step.StepId))
                        {
                            result.IsValid = false;
                            result.Errors.Add($"Step {step.StepId} cannot depend on itself");
                        }
                    }
                }
                catch
                {
                    result.Warnings.Add($"Step {step.StepId} has invalid DependsOnStepIds JSON");
                }
            }

            // Check assignee configuration
            foreach (var step in workflow.Steps)
            {
                var hasAssignee = step.AssigneeType switch
                {
                    "Role" => step.ApproverRoleId.HasValue,
                    "User" => step.ApproverUserId.HasValue,
                    "Department" => step.AssigneeDepartmentId.HasValue,
                    "FieldValue" => step.AssigneeFieldId.HasValue,
                    "Submitter" => true,
                    "PreviousActor" => true,
                    _ => false
                };

                if (!hasAssignee)
                {
                    result.Warnings.Add($"Step {step.StepId} ({step.StepName}) has no assignee configured for type '{step.AssigneeType}'");
                }
            }

            return result;
        }

        public async Task<WorkflowDetailDto> CloneWorkflowAsync(int workflowId, string newName)
        {
            var source = await _context.WorkflowDefinitions
                .Include(w => w.Steps)
                .FirstOrDefaultAsync(w => w.WorkflowId == workflowId);

            if (source == null)
                throw new ArgumentException($"Workflow {workflowId} not found");

            var userId = _claimsService.GetUserId();

            var clone = new WorkflowDefinition
            {
                WorkflowName = newName,
                Description = source.Description,
                IsActive = true,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.WorkflowDefinitions.Add(clone);
            await _context.SaveChangesAsync();

            // Clone steps
            foreach (var sourceStep in source.Steps.OrderBy(s => s.StepOrder))
            {
                var clonedStep = new WorkflowStep
                {
                    WorkflowId = clone.WorkflowId,
                    StepOrder = sourceStep.StepOrder,
                    StepName = sourceStep.StepName,
                    ActionId = sourceStep.ActionId,
                    TargetType = sourceStep.TargetType,
                    TargetId = sourceStep.TargetId,
                    AssigneeType = sourceStep.AssigneeType,
                    ApproverRoleId = sourceStep.ApproverRoleId,
                    ApproverUserId = sourceStep.ApproverUserId,
                    AssigneeDepartmentId = sourceStep.AssigneeDepartmentId,
                    AssigneeFieldId = sourceStep.AssigneeFieldId,
                    IsMandatory = sourceStep.IsMandatory,
                    IsParallel = sourceStep.IsParallel,
                    DueDays = sourceStep.DueDays,
                    EscalationRoleId = sourceStep.EscalationRoleId,
                    ConditionLogic = sourceStep.ConditionLogic,
                    AutoApproveCondition = sourceStep.AutoApproveCondition,
                    // Note: DependsOnStepIds will need to be remapped if we want to preserve dependencies
                    CreatedDate = DateTime.UtcNow
                };

                _context.WorkflowSteps.Add(clonedStep);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cloned workflow {SourceId} to {CloneId}: {CloneName}", workflowId, clone.WorkflowId, newName);

            return (await GetWorkflowByIdAsync(clone.WorkflowId))!;
        }

        public async Task<List<WorkflowActionDto>> GetWorkflowActionsAsync()
        {
            return await _context.WorkflowActions
                .Where(a => a.IsActive)
                .OrderBy(a => a.DisplayOrder)
                .Select(a => new WorkflowActionDto
                {
                    ActionId = a.ActionId,
                    ActionCode = a.ActionCode,
                    ActionName = a.ActionName,
                    Description = a.Description,
                    RequiresSignature = a.RequiresSignature,
                    RequiresComment = a.RequiresComment,
                    AllowDelegate = a.AllowDelegate,
                    IconClass = a.IconClass,
                    CssClass = a.CssClass
                })
                .ToListAsync();
        }

        public async Task<bool> CanDeleteWorkflowAsync(int workflowId)
        {
            return !await _context.FormTemplates.AnyAsync(t => t.WorkflowId == workflowId);
        }

        #endregion

        #region Private Helpers

        private WorkflowDetailDto MapToDetailDto(WorkflowDefinition workflow)
        {
            return new WorkflowDetailDto
            {
                WorkflowId = workflow.WorkflowId,
                WorkflowName = workflow.WorkflowName,
                Description = workflow.Description,
                IsActive = workflow.IsActive,
                StepCount = workflow.Steps.Count,
                TemplateCount = workflow.FormTemplates.Count,
                CreatedDate = workflow.CreatedDate,
                CreatedByName = workflow.Creator?.FullName ?? "Unknown",
                ModifiedDate = workflow.ModifiedDate,
                Steps = workflow.Steps.OrderBy(s => s.StepOrder).Select(MapToStepDto).ToList(),
                Templates = workflow.FormTemplates.Select(t => new WorkflowTemplateDto
                {
                    TemplateId = t.TemplateId,
                    TemplateName = t.TemplateName,
                    TemplateCode = t.TemplateCode,
                    PublishStatus = t.PublishStatus
                }).ToList()
            };
        }

        private WorkflowStepDto MapToStepDto(WorkflowStep step)
        {
            List<int>? dependsOnIds = null;
            if (!string.IsNullOrEmpty(step.DependsOnStepIds))
            {
                try
                {
                    dependsOnIds = JsonSerializer.Deserialize<List<int>>(step.DependsOnStepIds);
                }
                catch { }
            }

            return new WorkflowStepDto
            {
                StepId = step.StepId,
                WorkflowId = step.WorkflowId,
                StepOrder = step.StepOrder,
                StepName = step.StepName,
                ActionId = step.ActionId,
                ActionCode = step.Action?.ActionCode,
                ActionName = step.Action?.ActionName,
                ActionIconClass = step.Action?.IconClass,
                ActionCssClass = step.Action?.CssClass,
                RequiresSignature = step.Action?.RequiresSignature ?? false,
                RequiresComment = step.Action?.RequiresComment ?? false,
                TargetType = step.TargetType,
                TargetId = step.TargetId,
                AssigneeType = step.AssigneeType,
                ApproverRoleId = step.ApproverRoleId,
                ApproverRoleName = step.ApproverRole?.RoleName,
                ApproverUserId = step.ApproverUserId,
                ApproverUserName = step.ApproverUser?.FullName,
                AssigneeDepartmentId = step.AssigneeDepartmentId,
                AssigneeDepartmentName = step.AssigneeDepartment?.DepartmentName,
                AssigneeFieldId = step.AssigneeFieldId,
                IsMandatory = step.IsMandatory,
                IsParallel = step.IsParallel,
                DueDays = step.DueDays,
                EscalationRoleId = step.EscalationRoleId,
                EscalationRoleName = step.EscalationRole?.RoleName,
                ConditionLogic = step.ConditionLogic,
                AutoApproveCondition = step.AutoApproveCondition,
                DependsOnStepIds = dependsOnIds,
                CreatedDate = step.CreatedDate
            };
        }

        private async Task<WorkflowStepDto> GetStepDtoAsync(int stepId)
        {
            var step = await _context.WorkflowSteps
                .Include(s => s.Action)
                .Include(s => s.ApproverRole)
                .Include(s => s.ApproverUser)
                .Include(s => s.AssigneeDepartment)
                .Include(s => s.EscalationRole)
                .FirstAsync(s => s.StepId == stepId);

            return MapToStepDto(step);
        }

        private WorkflowStep MapToStepEntity(WorkflowStepCreateDto dto, int workflowId)
        {
            return new WorkflowStep
            {
                WorkflowId = workflowId,
                StepOrder = dto.StepOrder,
                StepName = dto.StepName,
                ActionId = dto.ActionId,
                TargetType = dto.TargetType,
                TargetId = dto.TargetId,
                AssigneeType = dto.AssigneeType,
                ApproverRoleId = dto.ApproverRoleId,
                ApproverUserId = dto.ApproverUserId,
                AssigneeDepartmentId = dto.AssigneeDepartmentId,
                AssigneeFieldId = dto.AssigneeFieldId,
                IsMandatory = dto.IsMandatory,
                IsParallel = dto.IsParallel,
                DueDays = dto.DueDays,
                EscalationRoleId = dto.EscalationRoleId,
                ConditionLogic = dto.ConditionLogic,
                AutoApproveCondition = dto.AutoApproveCondition,
                DependsOnStepIds = dto.DependsOnStepIds,
                CreatedDate = DateTime.UtcNow
            };
        }

        #endregion
    }
}
