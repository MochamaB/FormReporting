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
            _logger.LogInformation("CreateWorkflowAsync: UserId from claims = {UserId}", userId);
            
            // Fallback to a default user if claims don't have UserId (shouldn't happen in authenticated context)
            if (userId == 0)
            {
                _logger.LogWarning("UserId claim not found, attempting to get from database");
                // Try to get the first admin user as fallback - this indicates a claims configuration issue
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.IsActive);
                if (adminUser != null)
                {
                    userId = adminUser.UserId;
                    _logger.LogWarning("Using fallback user {UserId}: {UserName}", userId, adminUser.UserName);
                }
            }
            
            WorkflowDefinition workflow;

            // Check if this is for a template and if template already has a workflow
            if (dto.TemplateId.HasValue)
            {
                var template = await _context.FormTemplates
                    .Include(t => t.Workflow)
                        .ThenInclude(w => w.Steps)
                    .FirstOrDefaultAsync(t => t.TemplateId == dto.TemplateId.Value);

                if (template == null)
                {
                    throw new ArgumentException($"Template with ID {dto.TemplateId.Value} not found");
                }

                // If template already has a workflow, add steps to existing workflow
                if (template.WorkflowId.HasValue && template.Workflow != null)
                {
                    workflow = template.Workflow;
                    _logger.LogInformation("Adding steps to existing workflow {WorkflowId}: {WorkflowName}", workflow.WorkflowId, workflow.WorkflowName);
                }
                else
                {
                    // Create new workflow for template
                    workflow = new WorkflowDefinition
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

                    // Assign workflow to template
                    template.WorkflowId = workflow.WorkflowId;
                    template.ModifiedDate = DateTime.UtcNow;

                    _logger.LogInformation("Created new workflow {WorkflowId}: {WorkflowName} for template {TemplateId}", workflow.WorkflowId, workflow.WorkflowName, dto.TemplateId.Value);
                }
            }
            else
            {
                // Standalone workflow creation (no template)
                workflow = new WorkflowDefinition
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

                _logger.LogInformation("Created standalone workflow {WorkflowId}: {WorkflowName}", workflow.WorkflowId, workflow.WorkflowName);
            }

            // Add steps if provided
            if (dto.Steps.Any())
            {
                // Calculate next step order for auto-assignment
                var existingMaxOrder = workflow.Steps?.Any() == true 
                    ? workflow.Steps.Max(s => s.StepOrder) 
                    : 0;
                
                var currentOrder = existingMaxOrder;
                
                // Auto-assign StepOrder for steps that don't have one
                foreach (var stepDto in dto.Steps.OrderBy(s => s.StepOrder ?? 0))
                {
                    if (!stepDto.StepOrder.HasValue)
                    {
                        stepDto.StepOrder = ++currentOrder;
                    }
                    
                    var step = MapToStepEntity(stepDto, workflow.WorkflowId);
                    _context.WorkflowSteps.Add(step);
                }
                await _context.SaveChangesAsync();
            }

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
            if (dto.ActionId.HasValue) step.ActionId = dto.ActionId.Value;
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
                StepOrder = dto.StepOrder ?? 1, // Fallback to 1 if somehow null
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

        // ===== Dual-Mode Validation Methods =====

        public async Task<WorkflowValidationResultDto> ValidateWorkflowForModeAsync(int workflowId, int templateId)
        {
            // Get template to determine submission mode
            var template = await _context.FormTemplates.FindAsync(templateId);
            if (template == null)
            {
                return new WorkflowValidationResultDto 
                { 
                    IsValid = false, 
                    Errors = new List<string> { "Template not found" } 
                };
            }

            // Validate based on submission mode
            return template.SubmissionMode switch
            {
                Models.Common.SubmissionMode.Individual => await ValidateIndividualModeWorkflowAsync(workflowId),
                Models.Common.SubmissionMode.Collaborative => await ValidateCollaborativeModeWorkflowAsync(workflowId, templateId),
                _ => new WorkflowValidationResultDto { IsValid = false, Errors = new List<string> { "Invalid submission mode" } }
            };
        }

        public async Task<WorkflowValidationResultDto> ValidateIndividualModeWorkflowAsync(int workflowId)
        {
            var result = new WorkflowValidationResultDto { IsValid = true, Errors = new List<string>() };

            var workflow = await _context.WorkflowDefinitions
                .Include(w => w.Steps)
                    .ThenInclude(s => s.Action)
                .FirstOrDefaultAsync(w => w.WorkflowId == workflowId);

            if (workflow == null)
            {
                result.IsValid = false;
                result.Errors.Add("Workflow not found");
                return result;
            }

            // Individual Mode Rules:
            // 1. NO Fill actions allowed
            var fillSteps = workflow.Steps.Where(s => s.Action?.ActionCode == "FILL").ToList();
            if (fillSteps.Any())
            {
                result.IsValid = false;
                result.Errors.Add($"Individual mode cannot have Fill actions. Found {fillSteps.Count} Fill step(s)");
            }

            // 2. All steps must target "Submission" only (no Section/Field targeting)
            var nonSubmissionSteps = workflow.Steps.Where(s => s.TargetType != "Submission").ToList();
            if (nonSubmissionSteps.Any())
            {
                result.IsValid = false;
                result.Errors.Add($"Individual mode steps must target 'Submission' only. Found {nonSubmissionSteps.Count} step(s) targeting sections/fields");
            }

            // 3. Must have at least one approval-type action
            var approvalSteps = workflow.Steps.Where(s => 
                s.Action?.ActionCode == "APPROVE" || 
                s.Action?.ActionCode == "REVIEW" || 
                s.Action?.ActionCode == "VERIFY").ToList();
                
            if (!approvalSteps.Any())
            {
                result.IsValid = false;
                result.Errors.Add("Individual mode must have at least one approval step (Approve, Review, or Verify)");
            }

            return result;
        }

        public async Task<WorkflowValidationResultDto> ValidateCollaborativeModeWorkflowAsync(int workflowId, int templateId)
        {
            var result = new WorkflowValidationResultDto { IsValid = true, Errors = new List<string>() };

            var workflow = await _context.WorkflowDefinitions
                .Include(w => w.Steps.OrderBy(s => s.StepOrder))
                    .ThenInclude(s => s.Action)
                .FirstOrDefaultAsync(w => w.WorkflowId == workflowId);

            if (workflow == null)
            {
                result.IsValid = false;
                result.Errors.Add("Workflow not found");
                return result;
            }

            // Collaborative Mode Rules:
            // 1. Must have Fill actions
            var fillSteps = workflow.Steps.Where(s => s.Action?.ActionCode == "FILL").OrderBy(s => s.StepOrder).ToList();
            if (!fillSteps.Any())
            {
                result.IsValid = false;
                result.Errors.Add("Collaborative mode must have at least one Fill step");
            }

            // 2. Fill steps must come before approval steps
            var approvalSteps = workflow.Steps.Where(s => 
                s.Action?.ActionCode == "APPROVE" || 
                s.Action?.ActionCode == "REVIEW" || 
                s.Action?.ActionCode == "VERIFY").OrderBy(s => s.StepOrder).ToList();

            if (fillSteps.Any() && approvalSteps.Any())
            {
                var lastFillOrder = fillSteps.Max(s => s.StepOrder);
                var firstApprovalOrder = approvalSteps.Min(s => s.StepOrder);
                
                if (firstApprovalOrder <= lastFillOrder)
                {
                    result.IsValid = false;
                    result.Errors.Add("Fill steps must come before approval steps in Collaborative mode");
                }
            }

            // 3. Fill steps must target sections or fields (not just submission)
            var submissionOnlyFillSteps = fillSteps.Where(s => s.TargetType == "Submission").ToList();
            if (submissionOnlyFillSteps.Any())
            {
                result.IsValid = false;
                result.Errors.Add($"Collaborative mode Fill steps must target specific sections or fields. Found {submissionOnlyFillSteps.Count} Fill step(s) targeting entire submission");
            }

            // 4. Check section/field coverage (skip for single step workflows - allow incremental building)
            if (workflow.Steps.Count > 1)
            {
                await ValidateFormCoverageAsync(templateId, fillSteps, result);
            }

            return result;
        }

        public async Task<(bool IsValid, List<string> Errors)> ValidateWorkflowAssigneeResolutionAsync(int workflowId)
        {
            var errors = new List<string>();
            
            var workflow = await _context.WorkflowDefinitions
                .Include(w => w.Steps)
                .FirstOrDefaultAsync(w => w.WorkflowId == workflowId);

            if (workflow == null)
            {
                return (false, new List<string> { "Workflow not found" });
            }

            foreach (var step in workflow.Steps)
            {
                var stepErrors = await ValidateSingleStepAssigneeAsync(step);
                errors.AddRange(stepErrors.Select(e => $"Step {step.StepOrder} '{step.StepName}': {e}"));
            }

            return (errors.Count == 0, errors);
        }

        #endregion

        #region Private Validation Helper Methods

        private async Task ValidateFormCoverageAsync(int templateId, List<Models.Entities.Forms.WorkflowStep> fillSteps, WorkflowValidationResultDto result)
        {
            // Get all sections from the template
            var sections = await _context.FormTemplateSections
                .Where(s => s.TemplateId == templateId)
                .Select(s => new { s.SectionId, s.SectionName })
                .ToListAsync();

            var coveredSectionIds = fillSteps
                .Where(s => s.TargetType == "Section" && s.TargetId.HasValue)
                .Select(s => s.TargetId.Value)
                .Distinct()
                .ToList();

            var uncoveredSections = sections
                .Where(s => !coveredSectionIds.Contains(s.SectionId))
                .ToList();

            if (uncoveredSections.Any())
            {
                result.Errors.Add($"The following sections are not covered by Fill steps: {string.Join(", ", uncoveredSections.Select(s => s.SectionName))}");
            }
        }

        private async Task<List<string>> ValidateSingleStepAssigneeAsync(Models.Entities.Forms.WorkflowStep step)
        {
            var errors = new List<string>();

            switch (step.AssigneeType?.ToLower())
            {
                case "role":
                    if (!step.ApproverRoleId.HasValue)
                    {
                        errors.Add("Role assignee type requires ApproverRoleId");
                    }
                    else
                    {
                        var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == step.ApproverRoleId.Value);
                        if (!roleExists)
                        {
                            errors.Add($"Role {step.ApproverRoleId.Value} not found");
                        }
                    }
                    break;

                case "user":
                    if (!step.ApproverUserId.HasValue)
                    {
                        errors.Add("User assignee type requires ApproverUserId");
                    }
                    else
                    {
                        var userExists = await _context.Users.AnyAsync(u => u.UserId == step.ApproverUserId.Value);
                        if (!userExists)
                        {
                            errors.Add($"User {step.ApproverUserId.Value} not found");
                        }
                    }
                    break;

                case "department":
                    if (!step.AssigneeDepartmentId.HasValue)
                    {
                        errors.Add("Department assignee type requires AssigneeDepartmentId");
                    }
                    else
                    {
                        var deptExists = await _context.Departments.AnyAsync(d => d.DepartmentId == step.AssigneeDepartmentId.Value);
                        if (!deptExists)
                        {
                            errors.Add($"Department {step.AssigneeDepartmentId.Value} not found");
                        }
                    }
                    break;

                case "fieldvalue":
                    if (!step.AssigneeFieldId.HasValue)
                    {
                        errors.Add("FieldValue assignee type requires AssigneeFieldId");
                    }
                    else
                    {
                        var fieldExists = await _context.FormTemplateItems.AnyAsync(i => i.ItemId == step.AssigneeFieldId.Value);
                        if (!fieldExists)
                        {
                            errors.Add($"Field {step.AssigneeFieldId.Value} not found");
                        }
                    }
                    break;

                case "submitter":
                case "previousactor":
                    // These are dynamic and don't need validation at creation time
                    break;

                default:
                    errors.Add($"Unknown assignee type: {step.AssigneeType}");
                    break;
            }

            return errors;
        }

        #endregion

        #region Step-by-Step Wizard Validation

        /// <summary>
        /// Validate step data for real-time wizard validation (no database save)
        /// </summary>
        public async Task<StepValidationResultDto> ValidateStepDataAsync(StepValidationDto dto, int templateId)
        {
            var result = new StepValidationResultDto
            {
                IsValid = true,
                StepId = dto.StepId,
                Errors = new List<string>(),
                Warnings = new List<string>()
            };

            // Get template to determine submission mode
            var template = await _context.FormTemplates.FindAsync(templateId);
            if (template == null)
            {
                result.IsValid = false;
                result.Errors.Add("Template not found");
                return result;
            }

            // Validate based on current step
            switch (dto.StepId?.ToLower())
            {
                case "target":
                    ValidateTargetStep(dto, template.SubmissionMode, result);
                    break;

                case "action":
                    await ValidateActionStepAsync(dto, template.SubmissionMode, result);
                    break;

                case "assignee":
                    await ValidateAssigneeStepAsync(dto, template.SubmissionMode, result);
                    break;

                case "settings":
                    ValidateSettingsStep(dto, result);
                    break;

                default:
                    result.Warnings.Add("Unknown step ID, skipping validation");
                    break;
            }

            result.IsValid = !result.Errors.Any();
            return result;
        }

        /// <summary>
        /// Full validation before creating workflow (prevents invalid data from being saved)
        /// </summary>
        public async Task<StepValidationResultDto> ValidateBeforeCreateAsync(WorkflowStepCreateDto stepDto, int templateId)
        {
            var result = new StepValidationResultDto
            {
                IsValid = true,
                StepId = "all",
                Errors = new List<string>(),
                Warnings = new List<string>()
            };

            // Get template to determine submission mode
            var template = await _context.FormTemplates.FindAsync(templateId);
            if (template == null)
            {
                result.IsValid = false;
                result.Errors.Add("Template not found");
                return result;
            }

            // Convert to StepValidationDto for reuse
            var validationDto = new StepValidationDto
            {
                TemplateId = templateId,
                TargetType = stepDto.TargetType,
                TargetId = stepDto.TargetId,
                ActionId = stepDto.ActionId,
                StepName = stepDto.StepName,
                AssigneeType = stepDto.AssigneeType,
                ApproverRoleId = stepDto.ApproverRoleId,
                ApproverUserId = stepDto.ApproverUserId,
                AssigneeDepartmentId = stepDto.AssigneeDepartmentId,
                AssigneeFieldId = stepDto.AssigneeFieldId,
                IsMandatory = stepDto.IsMandatory,
                IsParallel = stepDto.IsParallel,
                DueDays = stepDto.DueDays,
                EscalationRoleId = stepDto.EscalationRoleId
            };

            // Validate all steps
            ValidateTargetStep(validationDto, template.SubmissionMode, result);
            await ValidateActionStepAsync(validationDto, template.SubmissionMode, result);
            await ValidateAssigneeStepAsync(validationDto, template.SubmissionMode, result);
            ValidateSettingsStep(validationDto, result);

            // Additional: Validate step name is provided
            if (string.IsNullOrWhiteSpace(stepDto.StepName))
            {
                result.Errors.Add("Step name is required");
            }

            result.IsValid = !result.Errors.Any();
            return result;
        }

        private void ValidateTargetStep(StepValidationDto dto, Models.Common.SubmissionMode mode, StepValidationResultDto result)
        {
            // Target type is required
            if (string.IsNullOrWhiteSpace(dto.TargetType))
            {
                result.Errors.Add("Target type is required");
                return;
            }

            // Individual mode: Only "Submission" target allowed
            if (mode == Models.Common.SubmissionMode.Individual)
            {
                if (!string.Equals(dto.TargetType, "Submission", StringComparison.OrdinalIgnoreCase))
                {
                    result.Errors.Add("Individual mode only allows 'Entire Submission' as target. Section/Field targeting is for Collaborative mode.");
                }
            }

            // If Section or Field target, TargetId is required
            if (string.Equals(dto.TargetType, "Section", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(dto.TargetType, "Field", StringComparison.OrdinalIgnoreCase))
            {
                if (!dto.TargetId.HasValue)
                {
                    result.Errors.Add($"Please select a {dto.TargetType.ToLower()} for this step");
                }
            }
        }

        private async Task ValidateActionStepAsync(StepValidationDto dto, Models.Common.SubmissionMode mode, StepValidationResultDto result)
        {
            // Action is required
            if (!dto.ActionId.HasValue)
            {
                result.Errors.Add("Please select an action type for this step");
                return;
            }

            // Validate action exists
            var action = await _context.WorkflowActions.FindAsync(dto.ActionId.Value);
            if (action == null)
            {
                result.Errors.Add("Selected action not found");
                return;
            }

            // Individual mode: No Fill action allowed
            if (mode == Models.Common.SubmissionMode.Individual)
            {
                if (string.Equals(action.ActionCode, "Fill", StringComparison.OrdinalIgnoreCase))
                {
                    result.Errors.Add("Individual mode cannot have Fill actions. Use Approve, Review, Sign, or Verify instead.");
                }
            }

            // Collaborative mode: Fill action should target Section/Field
            if (mode == Models.Common.SubmissionMode.Collaborative)
            {
                if (string.Equals(action.ActionCode, "Fill", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(dto.TargetType, "Submission", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Warnings.Add("Fill steps in Collaborative mode should target specific sections or fields, not the entire submission.");
                    }
                }
            }
        }

        private async Task ValidateAssigneeStepAsync(StepValidationDto dto, Models.Common.SubmissionMode mode, StepValidationResultDto result)
        {
            // Assignee type is required
            if (string.IsNullOrWhiteSpace(dto.AssigneeType))
            {
                result.Errors.Add("Please select an assignee type for this step");
                return;
            }

            // Individual mode restrictions
            if (mode == Models.Common.SubmissionMode.Individual)
            {
                if (string.Equals(dto.AssigneeType, "Submitter", StringComparison.OrdinalIgnoreCase))
                {
                    result.Errors.Add("Individual mode cannot assign to Submitter (conflict of interest)");
                }
                if (string.Equals(dto.AssigneeType, "FieldValue", StringComparison.OrdinalIgnoreCase))
                {
                    result.Errors.Add("Individual mode cannot use Field Value assignment");
                }
            }

            // Validate picker values based on assignee type
            switch (dto.AssigneeType?.ToLower())
            {
                case "role":
                    if (!dto.ApproverRoleId.HasValue)
                    {
                        result.Errors.Add("Please select a role for this step");
                    }
                    else
                    {
                        var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == dto.ApproverRoleId.Value);
                        if (!roleExists)
                        {
                            result.Errors.Add("Selected role not found");
                        }
                    }
                    break;

                case "user":
                    if (!dto.ApproverUserId.HasValue)
                    {
                        result.Errors.Add("Please select a user for this step");
                    }
                    else
                    {
                        var userExists = await _context.Users.AnyAsync(u => u.UserId == dto.ApproverUserId.Value);
                        if (!userExists)
                        {
                            result.Errors.Add("Selected user not found");
                        }
                    }
                    break;

                case "department":
                    if (!dto.AssigneeDepartmentId.HasValue)
                    {
                        result.Errors.Add("Please select a department for this step");
                    }
                    else
                    {
                        var deptExists = await _context.Departments.AnyAsync(d => d.DepartmentId == dto.AssigneeDepartmentId.Value);
                        if (!deptExists)
                        {
                            result.Errors.Add("Selected department not found");
                        }
                    }
                    break;

                case "fieldvalue":
                    if (!dto.AssigneeFieldId.HasValue)
                    {
                        result.Errors.Add("Please select a field for this step");
                    }
                    else
                    {
                        var fieldExists = await _context.FormTemplateItems.AnyAsync(i => i.ItemId == dto.AssigneeFieldId.Value);
                        if (!fieldExists)
                        {
                            result.Errors.Add("Selected field not found");
                        }
                    }
                    break;

                case "submitter":
                case "previousactor":
                    // No additional validation needed
                    break;

                default:
                    result.Errors.Add($"Unknown assignee type: {dto.AssigneeType}");
                    break;
            }
        }

        private void ValidateSettingsStep(StepValidationDto dto, StepValidationResultDto result)
        {
            // Settings are mostly optional, just validate ranges
            if (dto.DueDays.HasValue && dto.DueDays.Value < 0)
            {
                result.Errors.Add("Due days cannot be negative");
            }

            // Add warnings for best practices
            if (!dto.DueDays.HasValue)
            {
                result.Warnings.Add("Consider setting a due date for better workflow tracking");
            }
        }

        #endregion
    }
}
