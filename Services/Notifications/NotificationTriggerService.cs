using FormReporting.Data;
using FormReporting.Models.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Services.Notifications
{
    /// <summary>
    /// Centralized service for triggering notifications
    /// Handles data gathering and DTO construction for all notification scenarios
    /// </summary>
    public class NotificationTriggerService : INotificationTriggerService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationTriggerService> _logger;

        public NotificationTriggerService(
            ApplicationDbContext context,
            INotificationService notificationService,
            ILogger<NotificationTriggerService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        // ========================================================================
        // FORM ASSIGNMENT TRIGGERS
        // ========================================================================
        // NOTE: FormTemplateAssignment is for access control, not task assignments with deadlines
        // These triggers are implemented as placeholders for future FormTaskAssignment entity

        public async Task TriggerFormAssignmentCreatedAsync(
            int assignmentId,
            int assignedToUserId,
            int assignedByUserId)
        {
            try
            {
                var assignment = await _context.FormTemplateAssignments
                    .Include(a => a.Template)
                    .Include(a => a.User)
                    .Include(a => a.AssignedByUser)
                    .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

                if (assignment == null)
                {
                    _logger.LogWarning("Assignment {AssignmentId} not found for notification", assignmentId);
                    return;
                }

                var assignedUser = assignment.User ?? await _context.Users.FindAsync(assignedToUserId);
                var assignedBy = assignment.AssignedByUser ?? await _context.Users.FindAsync(assignedByUserId);

                var dto = new CreateNotificationDto
                {
                    TemplateCode = "ASSIGNMENT_CREATED",
                    RecipientUserIds = new List<int> { assignedToUserId },
                    PlaceholderData = new Dictionary<string, string>
                    {
                        { "RecipientName", assignedUser?.FullName ?? "User" },
                        { "FormName", assignment.Template?.TemplateName ?? "Form" },
                        { "DueDate", assignment.EffectiveUntil?.ToString("MMM dd, yyyy h:mm tt") ?? "Not set" },
                        { "AssignedBy", assignedBy?.FullName ?? "Administrator" },
                        { "ActionUrl", $"/Forms/Submit/{assignment.TemplateId}" }
                    },
                    SourceEntityType = "FormTemplateAssignment",
                    SourceEntityId = assignmentId
                };

                await _notificationService.CreateNotificationAsync(dto);

                _logger.LogInformation(
                    "Triggered ASSIGNMENT_CREATED notification for assignment {AssignmentId}",
                    assignmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error triggering assignment created notification for {AssignmentId}",
                    assignmentId);
            }
        }

        public async Task TriggerAssignmentDeadlineReminderAsync(int assignmentId)
        {
            try
            {
                var assignment = await _context.FormTemplateAssignments
                    .Include(a => a.Template)
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

                if (assignment == null || assignment.UserId == null)
                    return;

                var hoursRemaining = assignment.EffectiveUntil.HasValue
                    ? (assignment.EffectiveUntil.Value - DateTime.UtcNow).TotalHours
                    : 0;

                var dto = new CreateNotificationDto
                {
                    TemplateCode = "DEADLINE_REMINDER",
                    RecipientUserIds = new List<int> { assignment.UserId.Value },
                    PlaceholderData = new Dictionary<string, string>
                    {
                        { "RecipientName", assignment.User?.FullName ?? "User" },
                        { "FormName", assignment.Template?.TemplateName ?? "Form" },
                        { "DueDate", assignment.EffectiveUntil?.ToString("MMM dd, yyyy h:mm tt") ?? "Soon" },
                        { "HoursRemaining", Math.Max(0, (int)hoursRemaining).ToString() },
                        { "ActionUrl", $"/Forms/Submit/{assignment.TemplateId}" }
                    },
                    SourceEntityType = "FormTemplateAssignment",
                    SourceEntityId = assignmentId,
                    CustomPriority = "High"
                };

                await _notificationService.CreateNotificationAsync(dto);

                _logger.LogInformation(
                    "Triggered DEADLINE_REMINDER notification for assignment {AssignmentId}",
                    assignmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error triggering deadline reminder for assignment {AssignmentId}",
                    assignmentId);
            }
        }

        public async Task TriggerAssignmentOverdueAsync(int assignmentId)
        {
            try
            {
                var assignment = await _context.FormTemplateAssignments
                    .Include(a => a.Template)
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

                if (assignment == null || assignment.UserId == null)
                    return;

                var daysOverdue = assignment.EffectiveUntil.HasValue
                    ? (DateTime.UtcNow - assignment.EffectiveUntil.Value).Days
                    : 0;

                var recipients = new List<int> { assignment.UserId.Value };

                var dto = new CreateNotificationDto
                {
                    TemplateCode = "OVERDUE_ALERT",
                    RecipientUserIds = recipients,
                    PlaceholderData = new Dictionary<string, string>
                    {
                        { "RecipientName", assignment.User?.FullName ?? "User" },
                        { "FormName", assignment.Template?.TemplateName ?? "Form" },
                        { "AssigneeName", assignment.User?.FullName ?? "User" },
                        { "DueDate", assignment.EffectiveUntil?.ToString("MMM dd, yyyy") ?? "Unknown" },
                        { "DaysOverdue", Math.Max(0, daysOverdue).ToString() },
                        { "ActionUrl", $"/Forms/Submit/{assignment.TemplateId}" }
                    },
                    SourceEntityType = "FormTemplateAssignment",
                    SourceEntityId = assignmentId,
                    CustomPriority = "Urgent"
                };

                await _notificationService.CreateNotificationAsync(dto);

                _logger.LogInformation(
                    "Triggered OVERDUE_ALERT notification for assignment {AssignmentId}",
                    assignmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error triggering overdue alert for assignment {AssignmentId}",
                    assignmentId);
            }
        }

        // ========================================================================
        // FORM SUBMISSION TRIGGERS
        // ========================================================================

        public async Task TriggerFormSubmittedAsync(int responseId)
        {
            try
            {
                // responseId maps to SubmissionId in FormTemplateSubmission
                var submission = await _context.FormTemplateSubmissions
                    .Include(s => s.Template)
                    .Include(s => s.Submitter)
                    .FirstOrDefaultAsync(s => s.SubmissionId == responseId);

                if (submission == null)
                {
                    _logger.LogWarning("Submission {SubmissionId} not found for notification", responseId);
                    return;
                }

                // Find first workflow step assignee
                var firstProgress = await _context.SubmissionWorkflowProgresses
                    .Include(p => p.AssignedToUser)
                    .Where(p => p.SubmissionId == responseId)
                    .OrderBy(p => p.StepOrder)
                    .FirstOrDefaultAsync();

                if (firstProgress?.AssignedTo == null)
                {
                    _logger.LogWarning(
                        "No workflow assignee found for submission {SubmissionId}",
                        responseId);
                    return;
                }

                var dto = new CreateNotificationDto
                {
                    TemplateCode = "FORM_SUBMITTED",
                    RecipientUserIds = new List<int> { firstProgress.AssignedTo.Value },
                    PlaceholderData = new Dictionary<string, string>
                    {
                        { "RecipientName", firstProgress.AssignedToUser?.FullName ?? "Reviewer" },
                        { "SubmitterName", submission.Submitter?.FullName ?? "User" },
                        { "FormName", submission.Template?.TemplateName ?? "Form" },
                        { "SubmissionDate", submission.SubmittedDate?.ToString("MMM dd, yyyy h:mm tt") ?? DateTime.UtcNow.ToString("MMM dd, yyyy h:mm tt") },
                        { "ActionUrl", $"/Workflows/Review/{firstProgress.ProgressId}" }
                    },
                    SourceEntityType = "FormTemplateSubmission",
                    SourceEntityId = responseId
                };

                await _notificationService.CreateNotificationAsync(dto);

                _logger.LogInformation(
                    "Triggered FORM_SUBMITTED notification for submission {SubmissionId}",
                    responseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error triggering form submitted notification for submission {SubmissionId}",
                    responseId);
            }
        }

        public async Task TriggerFormApprovedAsync(
            int responseId,
            int approvedByUserId,
            string? comments = null)
        {
            try
            {
                var submission = await _context.FormTemplateSubmissions
                    .Include(s => s.Template)
                    .Include(s => s.Submitter)
                    .FirstOrDefaultAsync(s => s.SubmissionId == responseId);

                if (submission == null || submission.SubmittedBy == 0)
                    return;

                var approver = await _context.Users.FindAsync(approvedByUserId);

                var dto = new CreateNotificationDto
                {
                    TemplateCode = "FORM_APPROVED",
                    RecipientUserIds = new List<int> { submission.SubmittedBy },
                    PlaceholderData = new Dictionary<string, string>
                    {
                        { "RecipientName", submission.Submitter?.FullName ?? "User" },
                        { "FormName", submission.Template?.TemplateName ?? "Form" },
                        { "ApproverName", approver?.FullName ?? "Approver" },
                        { "ApprovalDate", DateTime.UtcNow.ToString("MMM dd, yyyy h:mm tt") },
                        { "ActionUrl", $"/Submissions/Details/{responseId}" }
                    },
                    SourceEntityType = "FormTemplateSubmission",
                    SourceEntityId = responseId
                };

                await _notificationService.CreateNotificationAsync(dto);

                _logger.LogInformation(
                    "Triggered FORM_APPROVED notification for submission {SubmissionId}",
                    responseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error triggering form approved notification for submission {SubmissionId}",
                    responseId);
            }
        }

        public async Task TriggerFormRejectedAsync(
            int responseId,
            int rejectedByUserId,
            string? reason = null)
        {
            try
            {
                var submission = await _context.FormTemplateSubmissions
                    .Include(s => s.Template)
                    .Include(s => s.Submitter)
                    .FirstOrDefaultAsync(s => s.SubmissionId == responseId);

                if (submission == null || submission.SubmittedBy == 0)
                    return;

                var rejector = await _context.Users.FindAsync(rejectedByUserId);

                // Find the step where it was rejected
                var rejectedProgress = await _context.SubmissionWorkflowProgresses
                    .Include(p => p.Step)
                    .Where(p => p.SubmissionId == responseId && p.Status == "Rejected")
                    .OrderByDescending(p => p.ReviewedDate)
                    .FirstOrDefaultAsync();

                var dto = new CreateNotificationDto
                {
                    TemplateCode = "FORM_REJECTED",
                    RecipientUserIds = new List<int> { submission.SubmittedBy },
                    PlaceholderData = new Dictionary<string, string>
                    {
                        { "RecipientName", submission.Submitter?.FullName ?? "User" },
                        { "FormName", submission.Template?.TemplateName ?? "Form" },
                        { "RejectorName", rejector?.FullName ?? "Reviewer" },
                        { "StepName", rejectedProgress?.Step?.StepName ?? "Review" },
                        { "RejectionReason", reason ?? "No reason provided" },
                        { "ActionUrl", $"/Submissions/Details/{responseId}" }
                    },
                    SourceEntityType = "FormTemplateSubmission",
                    SourceEntityId = responseId,
                    CustomPriority = "High"
                };

                await _notificationService.CreateNotificationAsync(dto);

                _logger.LogInformation(
                    "Triggered FORM_REJECTED notification for submission {SubmissionId}",
                    responseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error triggering form rejected notification for submission {SubmissionId}",
                    responseId);
            }
        }

        // ========================================================================
        // WORKFLOW TRIGGERS
        // ========================================================================

        public async Task TriggerWorkflowStepAssignedAsync(
            int stepInstanceId,
            int assignedToUserId)
        {
            try
            {
                // stepInstanceId maps to ProgressId in SubmissionWorkflowProgress
                var progress = await _context.SubmissionWorkflowProgresses
                    .Include(p => p.Step)
                    .Include(p => p.Submission)
                        .ThenInclude(s => s.Template)
                    .Include(p => p.Submission)
                        .ThenInclude(s => s.Submitter)
                    .Include(p => p.AssignedToUser)
                    .FirstOrDefaultAsync(p => p.ProgressId == stepInstanceId);

                if (progress == null)
                {
                    _logger.LogWarning(
                        "WorkflowProgress {ProgressId} not found for notification",
                        stepInstanceId);
                    return;
                }

                var dto = new CreateNotificationDto
                {
                    TemplateCode = "WORKFLOW_ASSIGNED",
                    RecipientUserIds = new List<int> { assignedToUserId },
                    PlaceholderData = new Dictionary<string, string>
                    {
                        { "RecipientName", progress.AssignedToUser?.FullName ?? "Reviewer" },
                        { "StepName", progress.Step?.StepName ?? "Review Step" },
                        { "FormName", progress.Submission?.Template?.TemplateName ?? "Form" },
                        { "SubmitterName", progress.Submission?.Submitter?.FullName ?? "User" },
                        { "DueDate", progress.DueDate?.ToString("MMM dd, yyyy h:mm tt") ?? "Not set" },
                        { "ActionUrl", $"/Workflows/Review/{stepInstanceId}" }
                    },
                    SourceEntityType = "SubmissionWorkflowProgress",
                    SourceEntityId = stepInstanceId,
                    CustomPriority = "High"
                };

                await _notificationService.CreateNotificationAsync(dto);

                _logger.LogInformation(
                    "Triggered WORKFLOW_ASSIGNED notification for progress {ProgressId}",
                    stepInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error triggering workflow assigned notification for progress {ProgressId}",
                    stepInstanceId);
            }
        }

        public async Task TriggerWorkflowStepCompletedAsync(
            int stepInstanceId,
            int completedByUserId)
        {
            try
            {
                var progress = await _context.SubmissionWorkflowProgresses
                    .Include(p => p.Step)
                    .Include(p => p.Submission)
                        .ThenInclude(s => s.Template)
                    .Include(p => p.Submission)
                        .ThenInclude(s => s.Submitter)
                    .FirstOrDefaultAsync(p => p.ProgressId == stepInstanceId);

                if (progress == null)
                    return;

                var completedBy = await _context.Users.FindAsync(completedByUserId);

                // Recipients: Submitter + Next step assignee (if exists)
                var recipients = new List<int>();

                if (progress.Submission?.SubmittedBy != 0)
                    recipients.Add(progress.Submission.SubmittedBy);

                // Find next step
                var nextProgress = await _context.SubmissionWorkflowProgresses
                    .Where(p => p.SubmissionId == progress.SubmissionId)
                    .Where(p => p.StepOrder == progress.StepOrder + 1)
                    .Where(p => p.AssignedTo != null)
                    .FirstOrDefaultAsync();

                if (nextProgress?.AssignedTo != null && !recipients.Contains(nextProgress.AssignedTo.Value))
                    recipients.Add(nextProgress.AssignedTo.Value);

                if (recipients.Count == 0)
                {
                    _logger.LogWarning(
                        "No recipients found for step completed notification {ProgressId}",
                        stepInstanceId);
                    return;
                }

                var dto = new CreateNotificationDto
                {
                    TemplateCode = "STEP_COMPLETED",
                    RecipientUserIds = recipients,
                    PlaceholderData = new Dictionary<string, string>
                    {
                        { "RecipientName", progress.Submission?.Submitter?.FullName ?? "User" },
                        { "ReviewerName", completedBy?.FullName ?? "Reviewer" },
                        { "StepName", progress.Step?.StepName ?? "Step" },
                        { "FormName", progress.Submission?.Template?.TemplateName ?? "Form" },
                        { "ActionName", progress.Status ?? "Completed" },
                        { "ActionUrl", $"/Submissions/Details/{progress.SubmissionId}" }
                    },
                    SourceEntityType = "SubmissionWorkflowProgress",
                    SourceEntityId = stepInstanceId
                };

                await _notificationService.CreateNotificationAsync(dto);

                _logger.LogInformation(
                    "Triggered STEP_COMPLETED notification for progress {ProgressId}",
                    stepInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error triggering step completed notification for progress {ProgressId}",
                    stepInstanceId);
            }
        }

        public async Task TriggerPendingApprovalReminderAsync(int stepInstanceId)
        {
            try
            {
                var progress = await _context.SubmissionWorkflowProgresses
                    .Include(p => p.Step)
                    .Include(p => p.Submission)
                        .ThenInclude(s => s.Template)
                    .Include(p => p.AssignedToUser)
                    .FirstOrDefaultAsync(p => p.ProgressId == stepInstanceId);

                if (progress?.AssignedTo == null)
                    return;

                var hoursPending = progress.AssignedDate.HasValue
                    ? (DateTime.UtcNow - progress.AssignedDate.Value).TotalHours
                    : 0;

                var recipients = new List<int> { progress.AssignedTo.Value };

                var dto = new CreateNotificationDto
                {
                    TemplateCode = "PENDING_APPROVAL_ALERT",
                    RecipientUserIds = recipients,
                    PlaceholderData = new Dictionary<string, string>
                    {
                        { "RecipientName", progress.AssignedToUser?.FullName ?? "Reviewer" },
                        { "FormName", progress.Submission?.Template?.TemplateName ?? "Form" },
                        { "StepName", progress.Step?.StepName ?? "Step" },
                        { "AssigneeName", progress.AssignedToUser?.FullName ?? "Reviewer" },
                        { "HoursPending", ((int)hoursPending).ToString() },
                        { "ActionUrl", $"/Workflows/Review/{stepInstanceId}" }
                    },
                    SourceEntityType = "SubmissionWorkflowProgress",
                    SourceEntityId = stepInstanceId,
                    CustomPriority = "Urgent"
                };

                await _notificationService.CreateNotificationAsync(dto);

                _logger.LogInformation(
                    "Triggered PENDING_APPROVAL_ALERT notification for progress {ProgressId}",
                    stepInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error triggering pending approval alert for progress {ProgressId}",
                    stepInstanceId);
            }
        }

        // ========================================================================
        // BATCH TRIGGERS (for scheduled jobs - Hangfire)
        // ========================================================================

        public async Task SendBatchDeadlineRemindersAsync(int hoursBeforeDue = 24)
        {
            try
            {
                var reminderTime = DateTime.UtcNow.AddHours(hoursBeforeDue);
                var minTime = DateTime.UtcNow.AddHours(hoursBeforeDue - 1); // 1-hour window

                var assignments = await _context.FormTemplateAssignments
                    .Where(a => a.EffectiveUntil.HasValue)
                    .Where(a => a.EffectiveUntil >= minTime && a.EffectiveUntil <= reminderTime)
                    .Where(a => a.Status == "Active")
                    .Where(a => a.UserId != null)
                    .ToListAsync();

                foreach (var assignment in assignments)
                {
                    await TriggerAssignmentDeadlineReminderAsync(assignment.AssignmentId);
                }

                _logger.LogInformation(
                    "Sent {Count} deadline reminder notifications",
                    assignments.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending batch deadline reminders");
            }
        }

        public async Task SendBatchOverdueAlertsAsync()
        {
            try
            {
                var overdueAssignments = await _context.FormTemplateAssignments
                    .Where(a => a.EffectiveUntil.HasValue && a.EffectiveUntil < DateTime.UtcNow)
                    .Where(a => a.Status == "Active")
                    .Where(a => a.UserId != null)
                    .ToListAsync();

                foreach (var assignment in overdueAssignments)
                {
                    await TriggerAssignmentOverdueAsync(assignment.AssignmentId);
                }

                _logger.LogInformation(
                    "Sent {Count} overdue alert notifications",
                    overdueAssignments.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending batch overdue alerts");
            }
        }

        public async Task SendBatchPendingApprovalAlertsAsync(int hoursPending = 48)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddHours(-hoursPending);

                var pendingProgresses = await _context.SubmissionWorkflowProgresses
                    .Where(p => p.Status == "Pending")
                    .Where(p => p.AssignedDate.HasValue && p.AssignedDate <= cutoffTime)
                    .Where(p => p.AssignedTo != null)
                    .ToListAsync();

                foreach (var progress in pendingProgresses)
                {
                    await TriggerPendingApprovalReminderAsync(progress.ProgressId);
                }

                _logger.LogInformation(
                    "Sent {Count} pending approval alert notifications",
                    pendingProgresses.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending batch pending approval alerts");
            }
        }
    }
}
