# Using NotificationTriggerService - Developer Guide

## Overview

The `NotificationTriggerService` centralizes all notification triggers across the application. Instead of building notification DTOs in your controllers/services, simply call the appropriate trigger method.

**Benefits:**
- ✅ All notifications in one place (easy to track)
- ✅ Consistent data gathering
- ✅ Simple one-line calls
- ✅ Automatic error handling
- ✅ Easy to test (mock the trigger service)

---

## Setup

### 1. Inject the Service

```csharp
public class FormAssignmentService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationTriggerService _notificationTriggers;

    public FormAssignmentService(
        ApplicationDbContext context,
        INotificationTriggerService notificationTriggers)
    {
        _context = context;
        _notificationTriggers = notificationTriggers;
    }
}
```

### 2. Call Trigger Methods

After creating/updating entities, call the appropriate trigger method:

```csharp
// Create assignment
var assignment = new FormAssignment { ... };
await _context.FormAssignments.AddAsync(assignment);
await _context.SaveChangesAsync();

// Trigger notification - ONE LINE!
await _notificationTriggers.TriggerFormAssignmentCreatedAsync(
    assignment.AssignmentId,
    assignment.AssignedToUserId.Value,
    currentUserId
);
```

---

## Available Trigger Methods (Phase 1 - MVP)

### Form Assignments

#### 1. Assignment Created
```csharp
await _notificationTriggers.TriggerFormAssignmentCreatedAsync(
    assignmentId: assignment.AssignmentId,
    assignedToUserId: userId,
    assignedByUserId: currentUserId
);
```

**When to call:** Immediately after creating a FormAssignment record

**Template:** `ASSIGNMENT_CREATED`

**Example usage:**
```csharp
// In FormAssignmentService.CreateAssignmentAsync()
public async Task<FormAssignment> CreateAssignmentAsync(CreateAssignmentDto dto)
{
    var assignment = new FormAssignment
    {
        TemplateId = dto.TemplateId,
        AssignedToUserId = dto.AssignedToUserId,
        DueDate = dto.DueDate,
        Status = "Pending"
    };

    await _context.FormAssignments.AddAsync(assignment);
    await _context.SaveChangesAsync();

    // Trigger notification
    await _notificationTriggers.TriggerFormAssignmentCreatedAsync(
        assignment.AssignmentId,
        assignment.AssignedToUserId.Value,
        currentUserId
    );

    return assignment;
}
```

---

#### 2. Deadline Reminder (Scheduled Job)
```csharp
await _notificationTriggers.TriggerAssignmentDeadlineReminderAsync(
    assignmentId: assignment.AssignmentId
);
```

**When to call:** 24 hours before due date (via Hangfire scheduled job)

**Template:** `DEADLINE_REMINDER`

**Note:** Usually called via batch method (see Batch Triggers section below)

---

#### 3. Assignment Overdue (Scheduled Job)
```csharp
await _notificationTriggers.TriggerAssignmentOverdueAsync(
    assignmentId: assignment.AssignmentId
);
```

**When to call:** When assignment is past due date (via Hangfire scheduled job)

**Template:** `OVERDUE_ALERT`

**Note:** Usually called via batch method (see Batch Triggers section below)

---

### Form Submissions

#### 4. Form Submitted
```csharp
await _notificationTriggers.TriggerFormSubmittedAsync(
    responseId: response.ResponseId
);
```

**When to call:** Immediately after form submission

**Template:** `FORM_SUBMITTED`

**Example usage:**
```csharp
// In FormResponseService.SubmitFormAsync()
public async Task<FormResponse> SubmitFormAsync(SubmitFormDto dto)
{
    var response = new FormResponse { ... };
    await _context.FormResponses.AddAsync(response);
    await _context.SaveChangesAsync();

    // Create workflow instances
    await CreateWorkflowInstancesAsync(response.ResponseId);

    // Trigger notification to first reviewer
    await _notificationTriggers.TriggerFormSubmittedAsync(response.ResponseId);

    return response;
}
```

---

#### 5. Form Approved
```csharp
await _notificationTriggers.TriggerFormApprovedAsync(
    responseId: response.ResponseId,
    approvedByUserId: currentUserId,
    comments: "Looks good!"  // Optional
);
```

**When to call:** When final workflow step is approved

**Template:** `FORM_APPROVED`

**Example usage:**
```csharp
// In WorkflowService.ApproveStepAsync()
public async Task ApproveStepAsync(int stepInstanceId, string comments)
{
    var step = await _context.WorkflowStepInstances.FindAsync(stepInstanceId);
    step.Status = "Approved";
    await _context.SaveChangesAsync();

    // Check if this was the final step
    var isFinalStep = await IsFinalStepAsync(stepInstanceId);

    if (isFinalStep)
    {
        // Mark response as approved
        var response = await _context.FormResponses.FindAsync(step.ResponseId);
        response.Status = "Approved";
        await _context.SaveChangesAsync();

        // Trigger approval notification
        await _notificationTriggers.TriggerFormApprovedAsync(
            step.ResponseId,
            currentUserId,
            comments
        );
    }
}
```

---

#### 6. Form Rejected
```csharp
await _notificationTriggers.TriggerFormRejectedAsync(
    responseId: response.ResponseId,
    rejectedByUserId: currentUserId,
    reason: "Missing required documents"  // Optional
);
```

**When to call:** When any workflow step is rejected

**Template:** `FORM_REJECTED`

**Example usage:**
```csharp
// In WorkflowService.RejectStepAsync()
public async Task RejectStepAsync(int stepInstanceId, string reason)
{
    var step = await _context.WorkflowStepInstances.FindAsync(stepInstanceId);
    step.Status = "Rejected";
    step.Comments = reason;
    await _context.SaveChangesAsync();

    // Mark response as rejected
    var response = await _context.FormResponses.FindAsync(step.ResponseId);
    response.Status = "Rejected";
    await _context.SaveChangesAsync();

    // Trigger rejection notification
    await _notificationTriggers.TriggerFormRejectedAsync(
        step.ResponseId,
        currentUserId,
        reason
    );
}
```

---

### Workflows

#### 7. Workflow Step Assigned
```csharp
await _notificationTriggers.TriggerWorkflowStepAssignedAsync(
    stepInstanceId: stepInstance.StepInstanceId,
    assignedToUserId: userId
);
```

**When to call:** When workflow step is assigned to a user

**Template:** `WORKFLOW_ASSIGNED`

**Example usage:**
```csharp
// In WorkflowService.CreateWorkflowInstancesAsync()
private async Task CreateWorkflowInstancesAsync(int responseId)
{
    var workflowSteps = await GetWorkflowStepsAsync(responseId);

    foreach (var step in workflowSteps)
    {
        var stepInstance = new WorkflowStepInstance
        {
            ResponseId = responseId,
            WorkflowStepId = step.StepId,
            AssignedToUserId = step.DefaultAssigneeId,
            Status = step.StepOrder == 1 ? "Pending" : "NotStarted"
        };

        await _context.WorkflowStepInstances.AddAsync(stepInstance);
        await _context.SaveChangesAsync();

        // Trigger notification for first step only
        if (step.StepOrder == 1 && step.DefaultAssigneeId.HasValue)
        {
            await _notificationTriggers.TriggerWorkflowStepAssignedAsync(
                stepInstance.StepInstanceId,
                step.DefaultAssigneeId.Value
            );
        }
    }
}
```

---

#### 8. Workflow Step Completed
```csharp
await _notificationTriggers.TriggerWorkflowStepCompletedAsync(
    stepInstanceId: stepInstance.StepInstanceId,
    completedByUserId: currentUserId
);
```

**When to call:** After completing a workflow step (approved, verified, etc.)

**Template:** `STEP_COMPLETED`

**Example usage:**
```csharp
// In WorkflowService.CompleteStepAsync()
public async Task CompleteStepAsync(int stepInstanceId)
{
    var step = await _context.WorkflowStepInstances.FindAsync(stepInstanceId);
    step.Status = "Completed";
    step.CompletedDate = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    // Activate next step
    await ActivateNextStepAsync(stepInstanceId);

    // Trigger completion notification
    await _notificationTriggers.TriggerWorkflowStepCompletedAsync(
        stepInstanceId,
        currentUserId
    );
}
```

---

#### 9. Pending Approval Reminder (Scheduled Job)
```csharp
await _notificationTriggers.TriggerPendingApprovalReminderAsync(
    stepInstanceId: stepInstance.StepInstanceId
);
```

**When to call:** When workflow step has been pending for > 48 hours (via Hangfire)

**Template:** `PENDING_APPROVAL_ALERT`

**Note:** Usually called via batch method (see Batch Triggers section below)

---

## Batch Trigger Methods (for Hangfire Jobs)

These methods query the database for matching records and trigger individual notifications.

### 1. Batch Deadline Reminders

```csharp
// In Hangfire recurring job
public async Task SendDeadlineReminders()
{
    await _notificationTriggers.SendBatchDeadlineRemindersAsync(
        hoursBeforeDue: 24  // Send 24 hours before due
    );
}
```

**What it does:**
- Finds all assignments due in 23-24 hours
- Calls `TriggerAssignmentDeadlineReminderAsync()` for each

**Hangfire configuration:**
```csharp
RecurringJob.AddOrUpdate(
    "send-deadline-reminders",
    () => notificationTriggers.SendBatchDeadlineRemindersAsync(24),
    Cron.Hourly // Run every hour
);
```

---

### 2. Batch Overdue Alerts

```csharp
// In Hangfire recurring job
public async Task SendOverdueAlerts()
{
    await _notificationTriggers.SendBatchOverdueAlertsAsync();
}
```

**What it does:**
- Finds all overdue assignments (past due date)
- Calls `TriggerAssignmentOverdueAsync()` for each

**Hangfire configuration:**
```csharp
RecurringJob.AddOrUpdate(
    "send-overdue-alerts",
    () => notificationTriggers.SendBatchOverdueAlertsAsync(),
    Cron.Daily(9) // Run daily at 9 AM
);
```

---

### 3. Batch Pending Approval Alerts

```csharp
// In Hangfire recurring job
public async Task SendPendingApprovalAlerts()
{
    await _notificationTriggers.SendBatchPendingApprovalAlertsAsync(
        hoursPending: 48  // Alert after 48 hours
    );
}
```

**What it does:**
- Finds workflow steps pending > 48 hours
- Calls `TriggerPendingApprovalReminderAsync()` for each

**Hangfire configuration:**
```csharp
RecurringJob.AddOrUpdate(
    "send-pending-approval-alerts",
    () => notificationTriggers.SendBatchPendingApprovalAlertsAsync(48),
    Cron.Daily(10) // Run daily at 10 AM
);
```

---

## Complete Example: Form Assignment Flow

```csharp
public class FormAssignmentService : IFormAssignmentService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationTriggerService _notificationTriggers;
    private readonly ILogger<FormAssignmentService> _logger;

    public FormAssignmentService(
        ApplicationDbContext context,
        INotificationTriggerService notificationTriggers,
        ILogger<FormAssignmentService> logger)
    {
        _context = context;
        _notificationTriggers = notificationTriggers;
        _logger = logger;
    }

    public async Task<FormAssignment> CreateAssignmentAsync(CreateAssignmentDto dto, int currentUserId)
    {
        // 1. Create assignment
        var assignment = new FormAssignment
        {
            TemplateId = dto.TemplateId,
            AssignedToUserId = dto.AssignedToUserId,
            DueDate = dto.DueDate,
            Status = "Pending",
            CreatedBy = currentUserId,
            CreatedDate = DateTime.UtcNow
        };

        await _context.FormAssignments.AddAsync(assignment);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Created assignment {AssignmentId} for user {UserId}",
            assignment.AssignmentId,
            dto.AssignedToUserId);

        // 2. Trigger notification - ONE LINE!
        await _notificationTriggers.TriggerFormAssignmentCreatedAsync(
            assignment.AssignmentId,
            dto.AssignedToUserId,
            currentUserId
        );

        return assignment;
    }
}
```

---

## Error Handling

All trigger methods include try-catch blocks with logging. **Notification failures will NOT break your business logic.**

```csharp
// Even if notification fails, assignment is still created
var assignment = await CreateAssignmentAsync(dto);

// If this fails, it's logged but doesn't throw exception
await _notificationTriggers.TriggerFormAssignmentCreatedAsync(...);

// Your code continues normally
return Ok(assignment);
```

---

## Testing

### Mocking the Trigger Service

```csharp
[Fact]
public async Task CreateAssignment_ShouldTriggerNotification()
{
    // Arrange
    var mockTriggers = new Mock<INotificationTriggerService>();
    var service = new FormAssignmentService(_context, mockTriggers.Object, _logger);

    // Act
    await service.CreateAssignmentAsync(dto, currentUserId);

    // Assert
    mockTriggers.Verify(
        x => x.TriggerFormAssignmentCreatedAsync(
            It.IsAny<int>(),
            dto.AssignedToUserId,
            currentUserId
        ),
        Times.Once
    );
}
```

---

## Future Triggers (Phase 2+)

When implementing Phase 2, add new trigger methods to `INotificationTriggerService` and `NotificationTriggerService`:

**Identity Module:**
```csharp
Task TriggerUserAccountCreatedAsync(int userId, int createdByUserId);
Task TriggerPasswordResetRequestAsync(int userId);
Task TriggerPasswordChangedAsync(int userId);
// etc.
```

**Usage will be identical:**
```csharp
// In IdentityService.CreateUserAsync()
await _context.Users.AddAsync(user);
await _context.SaveChangesAsync();

await _notificationTriggers.TriggerUserAccountCreatedAsync(
    user.UserId,
    currentUserId
);
```

---

## Quick Reference

| Trigger Method | When to Call | Where to Call |
|----------------|--------------|---------------|
| `TriggerFormAssignmentCreatedAsync()` | After creating assignment | FormAssignmentService.CreateAsync() |
| `TriggerFormSubmittedAsync()` | After form submission | FormResponseService.SubmitAsync() |
| `TriggerFormApprovedAsync()` | When final step approved | WorkflowService.ApproveStepAsync() |
| `TriggerFormRejectedAsync()` | When step rejected | WorkflowService.RejectStepAsync() |
| `TriggerWorkflowStepAssignedAsync()` | When step assigned | WorkflowService.CreateInstanceAsync() |
| `TriggerWorkflowStepCompletedAsync()` | After completing step | WorkflowService.CompleteStepAsync() |
| `TriggerAssignmentDeadlineReminderAsync()` | Via Hangfire (24h before) | Batch job |
| `TriggerAssignmentOverdueAsync()` | Via Hangfire (daily) | Batch job |
| `TriggerPendingApprovalReminderAsync()` | Via Hangfire (48h pending) | Batch job |

---

## Next Steps

1. ✅ Service registered in Program.cs
2. ⏳ Inject `INotificationTriggerService` into existing services
3. ⏳ Add trigger calls after entity operations
4. ⏳ Set up Hangfire for batch triggers (Phase 3)
5. ⏳ Implement Phase 2 triggers (Identity, Comments, etc.)

---

For questions or issues, refer to:
- [Notification Registry](00_Notification_Registry.md) - List of all notifications
- [Template Matrix](01_NotificationTypes_Templates_Matrix.md) - Template specifications
- [Implementation Plan](03_MVP_Implementation_Plan.md) - Overall plan
