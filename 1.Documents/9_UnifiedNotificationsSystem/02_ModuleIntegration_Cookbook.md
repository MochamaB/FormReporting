# Module Integration Cookbook - MVP

## Integration Overview

This document provides step-by-step integration instructions for adding notifications to existing Forms, Workflows, and Assignments modules.

---

## Forms Module Integration

### 1. Form Submission Created

**When**: User successfully submits a form (Individual mode)

**Where**: `FormSubmissionService.CreateAsync()`

**After**: FormSubmission record created and saved

**Code Pattern**:
```csharp
// After successful form submission creation
var submission = await _context.FormSubmissions.Add(submissionEntity);
await _context.SaveChangesAsync();

// Notify first workflow assignee
await _notificationService.CreateNotificationAsync(new CreateNotificationDto
{
    TemplateCode = "FORM_SUBMITTED",
    SourceEntityType = "FormSubmission",
    SourceEntityId = submission.SubmissionId,
    RecipientUserIds = new List<int> { firstAssigneeUserId },
    PlaceholderData = new Dictionary<string, string>
    {
        { "RecipientName", firstAssignee.FullName },
        { "SubmitterName", currentUser.FullName },
        { "FormName", template.TemplateName },
        { "SubmissionDate", DateTime.Now.ToString("MMM dd, yyyy h:mm tt") },
        { "ActionUrl", $"/Submissions/{submission.SubmissionId}/Review" }
    }
});
```

**Recipient Logic**:
- Get the first workflow step assignee
- Resolve based on AssigneeType (Role/User/Department)
- Filter by scope (only users with access to submission)

**Data Needed**:
- FormTemplate.TemplateName
- Current user (submitter) name
- First assignee user ID and name
- Submission ID for ActionUrl

---

## Workflows Module Integration

### 2. Workflow Step Assigned

**When**: WorkflowProgress step assigned to a user

**Where**: `WorkflowService.AssignStepAsync()` or when WorkflowProgress created

**After**: WorkflowProgress record created with AssignedToId populated

**Code Pattern**:
```csharp
// After workflow step assignment
var progress = new WorkflowProgress
{
    SubmissionId = submissionId,
    StepId = stepId,
    AssignedToId = assigneeUserId,
    Status = "Pending",
    AssignedDate = DateTime.UtcNow,
    DueDate = dueDateCalculated
};
await _context.WorkflowProgress.Add(progress);
await _context.SaveChangesAsync();

// Notify assignee
await _notificationService.CreateNotificationAsync(new CreateNotificationDto
{
    TemplateCode = "WORKFLOW_ASSIGNED",
    SourceEntityType = "WorkflowProgress",
    SourceEntityId = progress.ProgressId,
    RecipientUserIds = new List<int> { assigneeUserId },
    PlaceholderData = new Dictionary<string, string>
    {
        { "RecipientName", assignee.FullName },
        { "StepName", step.StepName },
        { "FormName", submission.FormTemplate.TemplateName },
        { "SubmitterName", submission.SubmittedBy.FullName },
        { "DueDate", progress.DueDate.ToString("MMM dd, yyyy h:mm tt") },
        { "ActionUrl", $"/Submissions/{submissionId}/Review" }
    }
});
```

**Recipient Logic**:
- Single assignee (AssignedToId from WorkflowProgress)
- Must have access to submission based on scope

**Data Needed**:
- WorkflowStep.StepName
- FormTemplate.TemplateName
- Submitter user name
- Assignee user ID and name
- Due date (calculated from step.DueDays)
- Submission ID for ActionUrl

---

### 3. Workflow Step Completed

**When**: User completes a workflow step (Approve, Verify, Review actions)

**Where**: `WorkflowService.CompleteStepAsync()`

**After**: WorkflowProgress.Status updated to Completed/Approved

**Code Pattern**:
```csharp
// After step completion
progress.Status = "Completed";
progress.ReviewedById = currentUserId;
progress.ReviewedDate = DateTime.UtcNow;
progress.Comments = comments;
await _context.SaveChangesAsync();

// Notify submitter
await _notificationService.CreateNotificationAsync(new CreateNotificationDto
{
    TemplateCode = "STEP_COMPLETED",
    SourceEntityType = "WorkflowProgress",
    SourceEntityId = progress.ProgressId,
    RecipientUserIds = new List<int> { submission.SubmittedBy.UserId },
    PlaceholderData = new Dictionary<string, string>
    {
        { "RecipientName", submission.SubmittedBy.FullName },
        { "ReviewerName", currentUser.FullName },
        { "StepName", step.StepName },
        { "FormName", submission.FormTemplate.TemplateName },
        { "ActionName", action.ActionName }, // e.g., "Approved"
        { "ActionUrl", $"/Submissions/{submission.SubmissionId}" }
    }
});

// If not final step, notify next assignee
if (nextStep != null)
{
    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
    {
        TemplateCode = "WORKFLOW_ASSIGNED",
        // ... (same as step assignment notification)
    });
}
```

**Recipient Logic**:
- Submitter (SubmittedBy from FormSubmission)
- Next step assignee (if not final step)

**Data Needed**:
- Current user (reviewer) name
- WorkflowStep.StepName
- WorkflowAction.ActionName
- FormTemplate.TemplateName
- Submitter details
- Next assignee (if exists)

---

### 4. Form Approved (Final Step)

**When**: Last workflow step completed with Approve action

**Where**: `WorkflowService.CompleteStepAsync()` when isLastStep == true

**After**: FormSubmission.WorkflowStatus updated to Completed

**Code Pattern**:
```csharp
// After final approval
submission.WorkflowStatus = "Completed";
submission.CompletedDate = DateTime.UtcNow;
await _context.SaveChangesAsync();

// Notify submitter of full approval
await _notificationService.CreateNotificationAsync(new CreateNotificationDto
{
    TemplateCode = "FORM_APPROVED",
    SourceEntityType = "FormSubmission",
    SourceEntityId = submission.SubmissionId,
    RecipientUserIds = new List<int> { submission.SubmittedBy.UserId },
    PlaceholderData = new Dictionary<string, string>
    {
        { "RecipientName", submission.SubmittedBy.FullName },
        { "FormName", submission.FormTemplate.TemplateName },
        { "ApproverName", currentUser.FullName },
        { "ApprovalDate", DateTime.Now.ToString("MMM dd, yyyy h:mm tt") },
        { "ActionUrl", $"/Submissions/{submission.SubmissionId}" }
    }
});
```

**Recipient Logic**:
- Submitter only

**Data Needed**:
- Current user (final approver) name
- FormTemplate.TemplateName
- Approval date
- Submitter details

---

### 5. Form Rejected

**When**: Any workflow step completed with Reject action

**Where**: `WorkflowService.RejectStepAsync()`

**After**: WorkflowProgress.Status updated to Rejected, FormSubmission.WorkflowStatus updated

**Code Pattern**:
```csharp
// After rejection
progress.Status = "Rejected";
progress.ReviewedById = currentUserId;
progress.ReviewedDate = DateTime.UtcNow;
progress.Comments = rejectionReason;
submission.WorkflowStatus = "Rejected";
await _context.SaveChangesAsync();

// Notify submitter
await _notificationService.CreateNotificationAsync(new CreateNotificationDto
{
    TemplateCode = "FORM_REJECTED",
    SourceEntityType = "WorkflowProgress",
    SourceEntityId = progress.ProgressId,
    RecipientUserIds = new List<int> { submission.SubmittedBy.UserId },
    PlaceholderData = new Dictionary<string, string>
    {
        { "RecipientName", submission.SubmittedBy.FullName },
        { "FormName", submission.FormTemplate.TemplateName },
        { "RejectorName", currentUser.FullName },
        { "StepName", step.StepName },
        { "RejectionReason", rejectionReason ?? "No reason provided" },
        { "ActionUrl", $"/Submissions/{submission.SubmissionId}" }
    }
});
```

**Recipient Logic**:
- Submitter only

**Data Needed**:
- Current user (rejector) name
- WorkflowStep.StepName
- FormTemplate.TemplateName
- Rejection reason/comments
- Submitter details

---

## Assignments Module Integration

### 6. Assignment Created

**When**: FormAssignment record created (assign forms to users/departments)

**Where**: `FormAssignmentService.CreateAsync()`

**After**: FormAssignment record created and saved

**Code Pattern**:
```csharp
// After assignment creation
var assignment = new FormAssignment
{
    TemplateId = templateId,
    AssignmentTargetType = targetType, // User or Department
    TargetUserId = userId,
    TargetDepartmentId = departmentId,
    DueDate = dueDate,
    CreatedBy = currentUserId
};
await _context.FormAssignments.Add(assignment);
await _context.SaveChangesAsync();

// Determine recipients
var recipientUserIds = await _assignmentService.ResolveAssignmentRecipientsAsync(assignment);

// Notify all assignees
await _notificationService.CreateNotificationAsync(new CreateNotificationDto
{
    TemplateCode = "ASSIGNMENT_CREATED",
    SourceEntityType = "FormAssignment",
    SourceEntityId = assignment.AssignmentId,
    RecipientUserIds = recipientUserIds,
    PlaceholderData = new Dictionary<string, string>
    {
        { "RecipientName", "{{DYNAMIC}}" }, // Replaced per recipient
        { "FormName", template.TemplateName },
        { "DepartmentName", department?.DepartmentName ?? "Individual" },
        { "DueDate", dueDate.ToString("MMM dd, yyyy h:mm tt") },
        { "AssignedBy", creator.FullName },
        { "ActionUrl", $"/Assignments/{assignment.AssignmentId}/Complete" }
    }
});
```

**Recipient Logic**:
- If TargetUserId: Single user
- If TargetDepartmentId: All active users in department
- Filter by scope and IsActive
- RecipientName placeholder replaced individually per recipient

**Data Needed**:
- FormTemplate.TemplateName
- Department name (if department assignment)
- Due date
- Assignment creator name
- Recipient user IDs (resolved from assignment target)

---

## Alert Integration

### 7. Deadline Reminder Alert

**When**: AlertMonitoringJob runs, checks for assignments/submissions due in 24 hours

**Where**: `AlertService.CheckAlertConditionAsync(DEADLINE_REMINDER)`

**Condition**:
```csharp
// Check for submissions due in 24 hours
var upcomingDeadlines = await _context.FormAssignments
    .Where(a => a.DueDate <= DateTime.UtcNow.AddHours(24)
             && a.DueDate > DateTime.UtcNow
             && a.SubmissionId == null) // Not yet submitted
    .ToListAsync();

foreach (var assignment in upcomingDeadlines)
{
    // Check if alert already sent (using AlertHistory)
    var alreadySent = await _context.AlertHistory
        .AnyAsync(h => h.AlertId == alertDef.AlertId
                    && h.TriggerDetails.Contains(assignment.AssignmentId.ToString())
                    && h.TriggeredDate > DateTime.UtcNow.AddDays(-1));

    if (!alreadySent)
    {
        var hoursRemaining = (int)(assignment.DueDate - DateTime.UtcNow).TotalHours;

        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            TemplateCode = "DEADLINE_REMINDER",
            SourceEntityType = "FormAssignment",
            SourceEntityId = assignment.AssignmentId,
            RecipientUserIds = await GetAssignmentRecipients(assignment),
            PlaceholderData = new Dictionary<string, string>
            {
                { "RecipientName", "{{DYNAMIC}}" },
                { "FormName", assignment.FormTemplate.TemplateName },
                { "DueDate", assignment.DueDate.ToString("MMM dd, yyyy h:mm tt") },
                { "HoursRemaining", hoursRemaining.ToString() },
                { "ActionUrl", $"/Assignments/{assignment.AssignmentId}/Complete" }
            }
        });

        // Log alert trigger
        await _alertService.LogAlertTriggerAsync(alertDef.AlertId, assignment.AssignmentId);
    }
}
```

**Alert Configuration**:
- CheckFrequencyMinutes: 60 (every hour)
- CooldownMinutes: 1440 (24 hours - prevent duplicate reminders)
- Severity: Warning

---

### 8. Overdue Submission Alert

**When**: AlertMonitoringJob runs, checks for submissions past due date

**Where**: `AlertService.CheckAlertConditionAsync(OVERDUE_ALERT)`

**Condition**:
```csharp
// Check for overdue assignments
var overdueAssignments = await _context.FormAssignments
    .Include(a => a.TargetUser)
    .Include(a => a.FormTemplate)
    .Where(a => a.DueDate < DateTime.UtcNow
             && a.SubmissionId == null)
    .ToListAsync();

foreach (var assignment in overdueAssignments)
{
    var daysOverdue = (int)(DateTime.UtcNow - assignment.DueDate).TotalDays;

    // Get assignee and their supervisor
    var assignee = assignment.TargetUser;
    var supervisor = await _userService.GetUserSupervisorAsync(assignee.UserId);

    var recipients = new List<int> { assignee.UserId };
    if (supervisor != null) recipients.Add(supervisor.UserId);

    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
    {
        TemplateCode = "OVERDUE_ALERT",
        SourceEntityType = "FormAssignment",
        SourceEntityId = assignment.AssignmentId,
        RecipientUserIds = recipients,
        PlaceholderData = new Dictionary<string, string>
        {
            { "RecipientName", "{{DYNAMIC}}" },
            { "FormName", assignment.FormTemplate.TemplateName },
            { "AssigneeName", assignee.FullName },
            { "DueDate", assignment.DueDate.ToString("MMM dd, yyyy h:mm tt") },
            { "DaysOverdue", daysOverdue.ToString() },
            { "ActionUrl", $"/Assignments/{assignment.AssignmentId}/Complete" }
        }
    });

    await _alertService.LogAlertTriggerAsync(alertDef.AlertId, assignment.AssignmentId);
}
```

**Alert Configuration**:
- CheckFrequencyMinutes: 30 (every 30 minutes)
- CooldownMinutes: 1440 (daily - one alert per day per overdue item)
- Severity: Critical

---

### 9. Pending Approval Alert

**When**: AlertMonitoringJob runs, checks for workflow steps pending >48 hours

**Where**: `AlertService.CheckAlertConditionAsync(PENDING_APPROVAL_ALERT)`

**Condition**:
```csharp
// Check for pending workflow steps
var pendingSteps = await _context.WorkflowProgress
    .Include(p => p.AssignedTo)
    .Include(p => p.Submission.FormTemplate)
    .Include(p => p.Step)
    .Where(p => p.Status == "Pending"
             && p.AssignedDate <= DateTime.UtcNow.AddHours(-48))
    .ToListAsync();

foreach (var progress in pendingSteps)
{
    var hoursPending = (int)(DateTime.UtcNow - progress.AssignedDate).TotalHours;

    var assignee = progress.AssignedTo;
    var supervisor = await _userService.GetUserSupervisorAsync(assignee.UserId);

    var recipients = new List<int> { assignee.UserId };
    if (supervisor != null) recipients.Add(supervisor.UserId);

    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
    {
        TemplateCode = "PENDING_APPROVAL_ALERT",
        SourceEntityType = "WorkflowProgress",
        SourceEntityId = progress.ProgressId,
        RecipientUserIds = recipients,
        PlaceholderData = new Dictionary<string, string>
        {
            { "RecipientName", "{{DYNAMIC}}" },
            { "FormName", progress.Submission.FormTemplate.TemplateName },
            { "StepName", progress.Step.StepName },
            { "AssigneeName", assignee.FullName },
            { "HoursPending", hoursPending.ToString() },
            { "ActionUrl", $"/Submissions/{progress.SubmissionId}/Review" }
        }
    });

    await _alertService.LogAlertTriggerAsync(alertDef.AlertId, progress.ProgressId);
}
```

**Alert Configuration**:
- CheckFrequencyMinutes: 60 (every hour)
- CooldownMinutes: 1440 (daily)
- Severity: Critical

---

## Helper Methods Needed

### Resolve Assignment Recipients
```csharp
public async Task<List<int>> ResolveAssignmentRecipientsAsync(FormAssignment assignment)
{
    if (assignment.TargetUserId.HasValue)
    {
        return new List<int> { assignment.TargetUserId.Value };
    }
    else if (assignment.TargetDepartmentId.HasValue)
    {
        return await _context.Users
            .Where(u => u.DepartmentId == assignment.TargetDepartmentId.Value && u.IsActive)
            .Select(u => u.UserId)
            .ToListAsync();
    }
    return new List<int>();
}
```

### Get User Supervisor
```csharp
public async Task<User?> GetUserSupervisorAsync(int userId)
{
    var user = await _context.Users
        .Include(u => u.Department)
            .ThenInclude(d => d.Users.Where(u => u.IsSupervisor))
        .FirstOrDefaultAsync(u => u.UserId == userId);

    return user?.Department?.Users.FirstOrDefault(u => u.IsSupervisor);
}
```

### Dynamic RecipientName Replacement
```csharp
// In NotificationService when creating NotificationRecipient
foreach (var userId in recipientUserIds)
{
    var recipient = await _context.Users.FindAsync(userId);

    // Clone placeholder data and replace dynamic values
    var personalizedData = new Dictionary<string, string>(placeholderData);
    if (personalizedData.ContainsKey("RecipientName") && personalizedData["RecipientName"] == "{{DYNAMIC}}")
    {
        personalizedData["RecipientName"] = recipient.FullName;
    }

    // Create notification recipient with personalized message
    // ...
}
```

---

## Integration Checklist

For each notification type, ensure:

- [ ] TemplateCode matches seeded template
- [ ] SourceEntityType and SourceEntityId populated
- [ ] RecipientUserIds resolved correctly
- [ ] All required placeholders provided
- [ ] ActionUrl points to correct page
- [ ] Scope filtering applied to recipients
- [ ] Error handling for notification creation failures
- [ ] Logging for debugging
