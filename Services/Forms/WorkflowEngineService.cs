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
    /// Service for executing workflows at runtime
    /// </summary>
    public class WorkflowEngineService : IWorkflowEngineService
    {
        private readonly ApplicationDbContext _context;
        private readonly IClaimsService _claimsService;
        private readonly ILogger<WorkflowEngineService> _logger;

        public WorkflowEngineService(
            ApplicationDbContext context,
            IClaimsService claimsService,
            ILogger<WorkflowEngineService> logger)
        {
            _context = context;
            _claimsService = claimsService;
            _logger = logger;
        }

        #region Workflow Initialization

        public async Task<WorkflowProgressDto> InitializeSubmissionWorkflowAsync(int submissionId)
        {
            var submission = await _context.FormTemplateSubmissions
                .Include(s => s.Template)
                    .ThenInclude(t => t.Workflow)
                        .ThenInclude(w => w!.Steps.OrderBy(st => st.StepOrder))
                            .ThenInclude(st => st.Action)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
                throw new ArgumentException($"Submission {submissionId} not found");

            if (submission.Template.WorkflowId == null)
                throw new InvalidOperationException($"Template {submission.TemplateId} has no workflow configured");

            var workflow = submission.Template.Workflow!;

            // Check if progress already exists
            var existingProgress = await _context.SubmissionWorkflowProgresses
                .AnyAsync(p => p.SubmissionId == submissionId);

            if (existingProgress)
            {
                _logger.LogWarning("Workflow progress already exists for submission {SubmissionId}", submissionId);
                return (await GetSubmissionProgressAsync(submissionId))!;
            }

            // Create progress records for each step
            foreach (var step in workflow.Steps.OrderBy(s => s.StepOrder))
            {
                var assignedTo = await ResolveAssigneeAsync(step, submission);
                var dueDate = step.DueDays.HasValue
                    ? DateTime.UtcNow.AddDays(step.DueDays.Value)
                    : (DateTime?)null;

                var progress = new SubmissionWorkflowProgress
                {
                    SubmissionId = submissionId,
                    StepId = step.StepId,
                    StepOrder = step.StepOrder,
                    Status = step.StepOrder == 1 ? "Pending" : "Pending", // First step is immediately pending
                    ActionId = step.ActionId,
                    TargetType = step.TargetType,
                    TargetId = step.TargetId,
                    AssignedTo = assignedTo,
                    AssignedDate = step.StepOrder == 1 ? DateTime.UtcNow : null,
                    DueDate = step.StepOrder == 1 ? dueDate : null,
                    CreatedDate = DateTime.UtcNow
                };

                _context.SubmissionWorkflowProgresses.Add(progress);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Initialized workflow for submission {SubmissionId} with {StepCount} steps",
                submissionId, workflow.Steps.Count);

            return (await GetSubmissionProgressAsync(submissionId))!;
        }

        public async Task<WorkflowProgressDto?> GetSubmissionProgressAsync(int submissionId)
        {
            var submission = await _context.FormTemplateSubmissions
                .Include(s => s.Template)
                    .ThenInclude(t => t.Workflow)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission?.Template.Workflow == null)
                return null;

            var progressRecords = await _context.SubmissionWorkflowProgresses
                .Include(p => p.Step)
                    .ThenInclude(s => s.Action)
                .Include(p => p.AssignedToUser)
                .Include(p => p.Reviewer)
                .Include(p => p.DelegatedToUser)
                .Where(p => p.SubmissionId == submissionId)
                .OrderBy(p => p.StepOrder)
                .ToListAsync();

            if (!progressRecords.Any())
                return null;

            var currentUserId = _claimsService.GetUserId();
            var completedCount = progressRecords.Count(p => p.Status == "Completed" || p.Status == "Approved");
            var currentStepOrder = progressRecords
                .Where(p => p.Status == "Pending" || p.Status == "InProgress")
                .Min(p => (int?)p.StepOrder) ?? progressRecords.Max(p => p.StepOrder);

            return new WorkflowProgressDto
            {
                SubmissionId = submissionId,
                WorkflowId = submission.Template.WorkflowId!.Value,
                WorkflowName = submission.Template.Workflow.WorkflowName,
                OverallStatus = await GetWorkflowStatusAsync(submissionId),
                TotalSteps = progressRecords.Count,
                CompletedSteps = completedCount,
                CurrentStepOrder = currentStepOrder,
                Steps = progressRecords.Select(p => MapToStepProgressDto(p, currentUserId, currentStepOrder)).ToList()
            };
        }

        #endregion

        #region Step Actions

        public async Task<StepProgressDto> CompleteStepAsync(StepCompleteDto dto)
        {
            var progress = await _context.SubmissionWorkflowProgresses
                .Include(p => p.Step)
                    .ThenInclude(s => s.Action)
                .Include(p => p.Submission)
                .FirstOrDefaultAsync(p => p.ProgressId == dto.ProgressId);

            if (progress == null)
                throw new ArgumentException($"Progress record {dto.ProgressId} not found");

            var userId = _claimsService.GetUserId();

            // Verify user can act on this step
            if (!await CanUserActOnStepAsync(userId, dto.ProgressId))
                throw new UnauthorizedAccessException("User is not authorized to complete this step");

            // Check dependencies
            if (!await CheckStepDependenciesAsync(dto.ProgressId))
                throw new InvalidOperationException("Step dependencies are not met");

            // Update progress
            progress.Status = progress.Step.Action?.ActionCode == "Approve" ? "Approved" : "Completed";
            progress.ReviewedBy = userId;
            progress.ReviewedDate = DateTime.UtcNow;
            progress.Comments = dto.Comments;

            // Handle signature if required
            if (progress.Step.Action?.RequiresSignature == true && !string.IsNullOrEmpty(dto.SignatureData))
            {
                progress.SignatureType = dto.SignatureType;
                progress.SignatureData = dto.SignatureData;
                progress.SignatureIP = _claimsService.GetClientIP();
                progress.SignatureTimestamp = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Activate next step(s)
            await ActivateNextStepsAsync(progress.SubmissionId, progress.StepOrder);

            // Check if workflow is complete
            if (await IsWorkflowCompleteAsync(progress.SubmissionId))
            {
                progress.Submission.Status = "Approved";
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Completed step {StepId} for submission {SubmissionId} by user {UserId}",
                progress.StepId, progress.SubmissionId, userId);

            return (await GetStepProgressAsync(dto.ProgressId))!;
        }

        public async Task<StepProgressDto> RejectStepAsync(StepRejectDto dto)
        {
            var progress = await _context.SubmissionWorkflowProgresses
                .Include(p => p.Step)
                .Include(p => p.Submission)
                .FirstOrDefaultAsync(p => p.ProgressId == dto.ProgressId);

            if (progress == null)
                throw new ArgumentException($"Progress record {dto.ProgressId} not found");

            var userId = _claimsService.GetUserId();

            // Verify user can act on this step
            if (!await CanUserActOnStepAsync(userId, dto.ProgressId))
                throw new UnauthorizedAccessException("User is not authorized to reject this step");

            progress.Status = "Rejected";
            progress.ReviewedBy = userId;
            progress.ReviewedDate = DateTime.UtcNow;
            progress.Comments = dto.Reason;

            // Update submission status
            progress.Submission.Status = "Rejected";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Rejected step {StepId} for submission {SubmissionId} by user {UserId}: {Reason}",
                progress.StepId, progress.SubmissionId, userId, dto.Reason);

            return (await GetStepProgressAsync(dto.ProgressId))!;
        }

        public async Task<StepProgressDto> DelegateStepAsync(StepDelegateDto dto)
        {
            var progress = await _context.SubmissionWorkflowProgresses
                .Include(p => p.Step)
                    .ThenInclude(s => s.Action)
                .FirstOrDefaultAsync(p => p.ProgressId == dto.ProgressId);

            if (progress == null)
                throw new ArgumentException($"Progress record {dto.ProgressId} not found");

            // Check if action allows delegation
            if (progress.Step.Action?.AllowDelegate == false)
                throw new InvalidOperationException("This step cannot be delegated");

            var userId = _claimsService.GetUserId();

            // Verify user can act on this step
            if (!await CanUserActOnStepAsync(userId, dto.ProgressId))
                throw new UnauthorizedAccessException("User is not authorized to delegate this step");

            progress.DelegatedBy = userId;
            progress.DelegatedTo = dto.DelegateToUserId;
            progress.DelegatedDate = DateTime.UtcNow;
            progress.DelegationReason = dto.Reason;
            progress.AssignedTo = dto.DelegateToUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Delegated step {StepId} from user {FromUserId} to user {ToUserId}",
                progress.StepId, userId, dto.DelegateToUserId);

            return (await GetStepProgressAsync(dto.ProgressId))!;
        }

        #endregion

        #region User Queries

        public async Task<List<PendingActionDto>> GetPendingActionsForUserAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return new List<PendingActionDto>();

            var userRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();

            var pendingProgress = await _context.SubmissionWorkflowProgresses
                .Include(p => p.Step)
                    .ThenInclude(s => s.Action)
                .Include(p => p.Submission)
                    .ThenInclude(s => s.Template)
                .Include(p => p.Submission)
                    .ThenInclude(s => s.Submitter)
                .Include(p => p.DelegatedByUser)
                .Where(p => (p.Status == "Pending" || p.Status == "InProgress") &&
                           (p.AssignedTo == userId ||
                            p.DelegatedTo == userId ||
                            (p.Step.AssigneeType == "Role" && p.Step.ApproverRoleId.HasValue && userRoleIds.Contains(p.Step.ApproverRoleId.Value))))
                .OrderBy(p => p.DueDate)
                .ToListAsync();

            return pendingProgress.Select(p => new PendingActionDto
            {
                ProgressId = p.ProgressId,
                SubmissionId = p.SubmissionId,
                TemplateId = p.Submission.TemplateId,
                TemplateName = p.Submission.Template.TemplateName,
                TemplateCode = p.Submission.Template.TemplateCode,
                StepId = p.StepId,
                StepName = p.Step.StepName,
                ActionCode = p.Step.Action?.ActionCode,
                ActionName = p.Step.Action?.ActionName,
                ActionIconClass = p.Step.Action?.IconClass,
                RequiresSignature = p.Step.Action?.RequiresSignature ?? false,
                TargetType = p.TargetType ?? "Submission",
                SubmittedByName = p.Submission.Submitter.FullName,
                SubmittedDate = p.Submission.SubmittedDate ?? p.Submission.CreatedDate,
                DueDate = p.DueDate,
                IsDelegated = p.DelegatedTo == userId,
                DelegatedFromName = p.DelegatedByUser?.FullName
            }).ToList();
        }

        public async Task<int> GetPendingActionCountAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return 0;

            var userRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();

            return await _context.SubmissionWorkflowProgresses
                .Where(p => (p.Status == "Pending" || p.Status == "InProgress") &&
                           (p.AssignedTo == userId ||
                            p.DelegatedTo == userId ||
                            (p.Step.AssigneeType == "Role" && p.Step.ApproverRoleId.HasValue && userRoleIds.Contains(p.Step.ApproverRoleId.Value))))
                .CountAsync();
        }

        public async Task<bool> CanUserActOnStepAsync(int userId, int progressId)
        {
            var progress = await _context.SubmissionWorkflowProgresses
                .Include(p => p.Step)
                .FirstOrDefaultAsync(p => p.ProgressId == progressId);

            if (progress == null)
                return false;

            // Check if step is actionable
            if (progress.Status != "Pending" && progress.Status != "InProgress")
                return false;

            // Check direct assignment
            if (progress.AssignedTo == userId || progress.DelegatedTo == userId)
                return true;

            // Check role-based assignment
            if (progress.Step.AssigneeType == "Role" && progress.Step.ApproverRoleId.HasValue)
            {
                return await _context.UserRoles.AnyAsync(ur =>
                    ur.UserId == userId && ur.RoleId == progress.Step.ApproverRoleId.Value);
            }

            // Check department-based assignment
            if (progress.Step.AssigneeType == "Department" && progress.Step.AssigneeDepartmentId.HasValue)
            {
                var user = await _context.Users.FindAsync(userId);
                return user?.DepartmentId == progress.Step.AssigneeDepartmentId;
            }

            return false;
        }

        public async Task<bool> CanUserActOnSectionAsync(int userId, int submissionId, int sectionId)
        {
            var sectionProgress = await _context.SubmissionWorkflowProgresses
                .Where(p => p.SubmissionId == submissionId &&
                           p.TargetType == "Section" &&
                           p.TargetId == sectionId &&
                           (p.Status == "Pending" || p.Status == "InProgress"))
                .ToListAsync();

            foreach (var progress in sectionProgress)
            {
                if (await CanUserActOnStepAsync(userId, progress.ProgressId))
                    return true;
            }

            return false;
        }

        public async Task<bool> CanUserActOnFieldAsync(int userId, int submissionId, int fieldId)
        {
            var fieldProgress = await _context.SubmissionWorkflowProgresses
                .Where(p => p.SubmissionId == submissionId &&
                           p.TargetType == "Field" &&
                           p.TargetId == fieldId &&
                           (p.Status == "Pending" || p.Status == "InProgress"))
                .ToListAsync();

            foreach (var progress in fieldProgress)
            {
                if (await CanUserActOnStepAsync(userId, progress.ProgressId))
                    return true;
            }

            return false;
        }

        #endregion

        #region Step Queries

        public async Task<List<StepProgressDto>> GetCurrentStepsAsync(int submissionId)
        {
            var currentUserId = _claimsService.GetUserId();

            var allProgress = await _context.SubmissionWorkflowProgresses
                .Include(p => p.Step)
                    .ThenInclude(s => s.Action)
                .Include(p => p.AssignedToUser)
                .Where(p => p.SubmissionId == submissionId)
                .OrderBy(p => p.StepOrder)
                .ToListAsync();

            var currentStepOrder = allProgress
                .Where(p => p.Status == "Pending" || p.Status == "InProgress")
                .Min(p => (int?)p.StepOrder) ?? 0;

            var currentSteps = allProgress
                .Where(p => p.StepOrder == currentStepOrder &&
                           (p.Status == "Pending" || p.Status == "InProgress"))
                .ToList();

            return currentSteps.Select(p => MapToStepProgressDto(p, currentUserId, currentStepOrder)).ToList();
        }

        public async Task<StepProgressDto?> GetStepProgressAsync(int progressId)
        {
            var progress = await _context.SubmissionWorkflowProgresses
                .Include(p => p.Step)
                    .ThenInclude(s => s.Action)
                .Include(p => p.AssignedToUser)
                .Include(p => p.Reviewer)
                .Include(p => p.DelegatedToUser)
                .Include(p => p.DelegatedByUser)
                .FirstOrDefaultAsync(p => p.ProgressId == progressId);

            if (progress == null)
                return null;

            var currentUserId = _claimsService.GetUserId();
            var currentStepOrder = await _context.SubmissionWorkflowProgresses
                .Where(p => p.SubmissionId == progress.SubmissionId &&
                           (p.Status == "Pending" || p.Status == "InProgress"))
                .MinAsync(p => (int?)p.StepOrder) ?? 0;

            return MapToStepProgressDto(progress, currentUserId, currentStepOrder);
        }

        public async Task<bool> CheckStepDependenciesAsync(int progressId)
        {
            var progress = await _context.SubmissionWorkflowProgresses
                .Include(p => p.Step)
                .FirstOrDefaultAsync(p => p.ProgressId == progressId);

            if (progress == null)
                return false;

            // If no dependencies, return true
            if (string.IsNullOrEmpty(progress.Step.DependsOnStepIds))
                return true;

            try
            {
                var dependsOnIds = JsonSerializer.Deserialize<List<int>>(progress.Step.DependsOnStepIds);
                if (dependsOnIds == null || !dependsOnIds.Any())
                    return true;

                // Check if all dependent steps are completed
                var dependentProgress = await _context.SubmissionWorkflowProgresses
                    .Where(p => p.SubmissionId == progress.SubmissionId &&
                               dependsOnIds.Contains(p.StepId))
                    .ToListAsync();

                return dependentProgress.All(p => p.Status == "Completed" || p.Status == "Approved" || p.Status == "Skipped");
            }
            catch
            {
                return true; // If parsing fails, assume no dependencies
            }
        }

        #endregion

        #region Workflow State

        public async Task<bool> IsWorkflowCompleteAsync(int submissionId)
        {
            var mandatorySteps = await _context.SubmissionWorkflowProgresses
                .Include(p => p.Step)
                .Where(p => p.SubmissionId == submissionId && p.Step.IsMandatory)
                .ToListAsync();

            return mandatorySteps.All(p => p.Status == "Completed" || p.Status == "Approved" || p.Status == "Skipped");
        }

        public async Task<string> GetWorkflowStatusAsync(int submissionId)
        {
            var progress = await _context.SubmissionWorkflowProgresses
                .Where(p => p.SubmissionId == submissionId)
                .ToListAsync();

            if (!progress.Any())
                return "NotStarted";

            if (progress.Any(p => p.Status == "Rejected"))
                return "Rejected";

            if (progress.All(p => p.Status == "Completed" || p.Status == "Approved" || p.Status == "Skipped"))
                return "Completed";

            if (progress.Any(p => p.Status == "InProgress"))
                return "InProgress";

            return "Pending";
        }

        #endregion

        #region Background Jobs

        public async Task<int> ProcessEscalationsAsync()
        {
            var overdueSteps = await _context.SubmissionWorkflowProgresses
                .Include(p => p.Step)
                .Where(p => (p.Status == "Pending" || p.Status == "InProgress") &&
                           p.DueDate.HasValue &&
                           p.DueDate.Value < DateTime.UtcNow &&
                           p.Step.EscalationRoleId.HasValue)
                .ToListAsync();

            var escalated = 0;
            foreach (var progress in overdueSteps)
            {
                // Find a user with the escalation role
                var escalationUser = await _context.UserRoles
                    .Include(ur => ur.User)
                    .Where(ur => ur.RoleId == progress.Step.EscalationRoleId && ur.User.IsActive)
                    .Select(ur => ur.User)
                    .FirstOrDefaultAsync();

                if (escalationUser != null)
                {
                    progress.DelegatedBy = progress.AssignedTo;
                    progress.DelegatedTo = escalationUser.UserId;
                    progress.DelegatedDate = DateTime.UtcNow;
                    progress.DelegationReason = "Auto-escalated due to overdue";
                    progress.AssignedTo = escalationUser.UserId;
                    escalated++;

                    _logger.LogInformation("Escalated step {StepId} to user {UserId} due to overdue",
                        progress.StepId, escalationUser.UserId);
                }
            }

            await _context.SaveChangesAsync();

            return escalated;
        }

        public async Task<int> ProcessAutoApprovalsAsync()
        {
            var stepsWithAutoApprove = await _context.SubmissionWorkflowProgresses
                .Include(p => p.Step)
                .Include(p => p.Submission)
                    .ThenInclude(s => s.Responses)
                .Where(p => (p.Status == "Pending" || p.Status == "InProgress") &&
                           !string.IsNullOrEmpty(p.Step.AutoApproveCondition))
                .ToListAsync();

            var autoApproved = 0;
            foreach (var progress in stepsWithAutoApprove)
            {
                if (EvaluateAutoApproveCondition(progress))
                {
                    progress.Status = "Approved";
                    progress.ReviewedDate = DateTime.UtcNow;
                    progress.Comments = "Auto-approved based on condition";
                    autoApproved++;

                    _logger.LogInformation("Auto-approved step {StepId} for submission {SubmissionId}",
                        progress.StepId, progress.SubmissionId);

                    // Activate next steps
                    await ActivateNextStepsAsync(progress.SubmissionId, progress.StepOrder);
                }
            }

            await _context.SaveChangesAsync();

            return autoApproved;
        }

        #endregion

        #region Private Helpers

        private async Task<int?> ResolveAssigneeAsync(WorkflowStep step, FormTemplateSubmission submission)
        {
            return step.AssigneeType switch
            {
                "User" => step.ApproverUserId,
                "Submitter" => submission.SubmittedBy,
                "PreviousActor" => await GetPreviousActorAsync(submission.SubmissionId, step.StepOrder),
                "FieldValue" => await GetAssigneeFromFieldAsync(submission.SubmissionId, step.AssigneeFieldId),
                // Role and Department are resolved at action time, not assignment time
                _ => null
            };
        }

        private async Task<int?> GetPreviousActorAsync(int submissionId, int currentStepOrder)
        {
            var previousProgress = await _context.SubmissionWorkflowProgresses
                .Where(p => p.SubmissionId == submissionId && p.StepOrder < currentStepOrder)
                .OrderByDescending(p => p.StepOrder)
                .FirstOrDefaultAsync();

            return previousProgress?.ReviewedBy;
        }

        private async Task<int?> GetAssigneeFromFieldAsync(int submissionId, int? fieldId)
        {
            if (!fieldId.HasValue)
                return null;

            var response = await _context.FormTemplateResponses
                .FirstOrDefaultAsync(r => r.SubmissionId == submissionId && r.ItemId == fieldId.Value);

            if (response != null && int.TryParse(response.TextValue, out var userId))
                return userId;

            return null;
        }

        private async Task ActivateNextStepsAsync(int submissionId, int completedStepOrder)
        {
            var nextSteps = await _context.SubmissionWorkflowProgresses
                .Include(p => p.Step)
                .Where(p => p.SubmissionId == submissionId &&
                           p.StepOrder == completedStepOrder + 1 &&
                           p.Status == "Pending")
                .ToListAsync();

            foreach (var nextStep in nextSteps)
            {
                // Check dependencies
                if (await CheckStepDependenciesAsync(nextStep.ProgressId))
                {
                    nextStep.AssignedDate = DateTime.UtcNow;
                    if (nextStep.Step.DueDays.HasValue)
                    {
                        nextStep.DueDate = DateTime.UtcNow.AddDays(nextStep.Step.DueDays.Value);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private bool EvaluateAutoApproveCondition(SubmissionWorkflowProgress progress)
        {
            if (string.IsNullOrEmpty(progress.Step.AutoApproveCondition))
                return false;

            try
            {
                // Parse condition: {"field": "amount", "operator": "<", "value": 1000}
                var condition = JsonSerializer.Deserialize<AutoApproveCondition>(progress.Step.AutoApproveCondition);
                if (condition == null)
                    return false;

                // Find the field response
                var response = progress.Submission.Responses
                    .FirstOrDefault(r => r.Item?.ItemName?.ToLower() == condition.Field?.ToLower() ||
                                        r.Item?.ItemCode?.ToLower() == condition.Field?.ToLower());

                if (response == null)
                    return false;

                // Get value from appropriate field based on data type
                decimal fieldValue;
                if (response.NumericValue.HasValue)
                    fieldValue = response.NumericValue.Value;
                else if (!string.IsNullOrEmpty(response.TextValue) && decimal.TryParse(response.TextValue, out var parsed))
                    fieldValue = parsed;
                else
                    return false;

                return condition.Operator switch
                {
                    "<" => fieldValue < condition.Value,
                    "<=" => fieldValue <= condition.Value,
                    ">" => fieldValue > condition.Value,
                    ">=" => fieldValue >= condition.Value,
                    "==" => fieldValue == condition.Value,
                    "!=" => fieldValue != condition.Value,
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        private StepProgressDto MapToStepProgressDto(SubmissionWorkflowProgress progress, int currentUserId, int currentStepOrder)
        {
            var canAct = (progress.Status == "Pending" || progress.Status == "InProgress") &&
                        progress.StepOrder == currentStepOrder &&
                        (progress.AssignedTo == currentUserId || progress.DelegatedTo == currentUserId);

            return new StepProgressDto
            {
                ProgressId = progress.ProgressId,
                StepId = progress.StepId,
                StepOrder = progress.StepOrder,
                StepName = progress.Step.StepName,
                ActionCode = progress.Step.Action?.ActionCode,
                ActionName = progress.Step.Action?.ActionName,
                ActionIconClass = progress.Step.Action?.IconClass,
                ActionCssClass = progress.Step.Action?.CssClass,
                RequiresSignature = progress.Step.Action?.RequiresSignature ?? false,
                RequiresComment = progress.Step.Action?.RequiresComment ?? false,
                TargetType = progress.TargetType ?? "Submission",
                TargetId = progress.TargetId,
                Status = progress.Status,
                IsCurrent = progress.StepOrder == currentStepOrder && (progress.Status == "Pending" || progress.Status == "InProgress"),
                CanAct = canAct,
                AssignedToId = progress.AssignedTo,
                AssignedToName = progress.AssignedToUser?.FullName,
                AssignedDate = progress.AssignedDate,
                DueDate = progress.DueDate,
                ReviewedById = progress.ReviewedBy,
                ReviewedByName = progress.Reviewer?.FullName,
                ReviewedDate = progress.ReviewedDate,
                Comments = progress.Comments,
                HasSignature = !string.IsNullOrEmpty(progress.SignatureData),
                SignatureType = progress.SignatureType,
                SignatureTimestamp = progress.SignatureTimestamp,
                DelegatedToId = progress.DelegatedTo,
                DelegatedToName = progress.DelegatedToUser?.FullName,
                DelegatedDate = progress.DelegatedDate,
                DelegationReason = progress.DelegationReason
            };
        }

        private class AutoApproveCondition
        {
            public string? Field { get; set; }
            public string? Operator { get; set; }
            public decimal Value { get; set; }
        }

        #endregion
    }
}
