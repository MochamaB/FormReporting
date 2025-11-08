# Form Submission: Review & Approval Workflows

**Purpose:** Review submissions before final submit and route through approval workflows  
**Users:** Submitters (review their own), Approvers (review assigned submissions)  
**Flow:** Submit → (Workflow if configured) → Approved → Metrics Populated

---

## Component Summary

| Component | Type | Purpose |
|-----------|------|---------|
| 1. Pre-Submission Review | Modal with Read-Only Form | User reviews before submitting |
| 2. Approval Dashboard | DataTable | Approvers see pending submissions |
| 3. Approval Detail View | Read-Only Form + Actions | Review submitted data |
| 4. Approval Workflow Engine | Background Service | Route submissions through workflow steps |
| 5. Rejection Handler | Modal + Comments | Send back for revision |
| 6. Metric Population Service | Post-Approval Process | Populate KPI metrics from approved data |

---

## 1. Pre-Submission Review

**Purpose:** Allow user to review all responses before final submission

### Review Modal

**Triggered by:** User clicks [Submit Form] button on the last section of the wizard

**Modal Display:**
```
┌─ Review Your Submission ──────────────────────────────────────┐
│                                                                 │
│  Template: Factory Monthly ICT Report                          │
│  Period: November 2025                                         │
│  Due: Nov 30, 2025                                            │
│                                                                 │
│  Please review your responses before submitting.               │
│  Once submitted, you cannot edit (unless rejected).            │
│                                                                 │
│  ──────────────────────────────────────────────────────────── │
│                                                                 │
│  [Section 1: General Information ▼]                            │
│    Factory Name: Kangaita Factory                              │
│    Reporting Period: November 2025                             │
│    Region: Central Region                                      │
│                                                                 │
│  [Section 2: Hardware Inventory ▼]                             │
│    Number of Computers: 25                                     │
│    Is LAN working?: Yes                                        │
│    Number of Printers: 5                                       │
│    ...                                                          │
│                                                                 │
│  [Section 3: Network Status ▼]                                 │
│    Network Uptime: 98.5%                                       │
│    Backup Status: Completed                                    │
│    ...                                                          │
│                                                                 │
│  ──────────────────────────────────────────────────────────── │
│                                                                 │
│  Completion: 100% (33/33 fields)                               │
│                                                                 │
│  ⚠️ Missing Required Fields: 0                                 │
│  ⚠️ Validation Warnings: 2                                     │
│    • Field "Network Uptime" exceeds expected range (warning)   │
│    • Field "Backup Status" - Manual review recommended         │
│                                                                 │
│  ──────────────────────────────────────────────────────────── │
│                                                                 │
│  [✕ Cancel] [← Edit Form] [✓ Confirm & Submit]                │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Pre-Submission Validation

**Validation Check Before Allowing Submission:**

```csharp
public class SubmissionValidationResult
{
    public bool CanSubmit { get; set; }
    public List<ValidationError> Errors { get; set; }
    public List<ValidationWarning> Warnings { get; set; }
    public int TotalFields { get; set; }
    public int CompletedFields { get; set; }
    public int MissingRequiredFields { get; set; }
}

[HttpPost("api/submissions/validate")]
public async Task<IActionResult> ValidateSubmission(int submissionId)
{
    var submission = await GetSubmission(submissionId);
    var template = await GetTemplate(submission.TemplateId);
    
    var result = new SubmissionValidationResult
    {
        Errors = new List<ValidationError>(),
        Warnings = new List<ValidationWarning>()
    };
    
    // Check all required fields have responses
    var requiredFields = template.Items.Where(i => i.IsRequired);
    foreach (var field in requiredFields)
    {
        var response = submission.Responses
            .FirstOrDefault(r => r.ItemId == field.ItemId);
        
        if (response == null || string.IsNullOrEmpty(response.ResponseValue))
        {
            result.Errors.Add(new ValidationError
            {
                ItemId = field.ItemId,
                ItemName = field.ItemName,
                Message = "This field is required"
            });
        }
    }
    
    // Run all validation rules
    foreach (var field in template.Items)
    {
        var response = submission.Responses
            .FirstOrDefault(r => r.ItemId == field.ItemId);
        
        if (response == null) continue;
        
        foreach (var validation in field.Validations)
        {
            var isValid = ExecuteValidation(validation, response.ResponseValue);
            
            if (!isValid)
            {
                if (validation.Severity == "Error")
                {
                    result.Errors.Add(new ValidationError
                    {
                        ItemId = field.ItemId,
                        ItemName = field.ItemName,
                        Message = validation.ErrorMessage
                    });
                }
                else if (validation.Severity == "Warning")
                {
                    result.Warnings.Add(new ValidationWarning
                    {
                        ItemId = field.ItemId,
                        ItemName = field.ItemName,
                        Message = validation.ErrorMessage
                    });
                }
            }
        }
    }
    
    result.TotalFields = template.Items.Count;
    result.CompletedFields = submission.Responses.Count;
    result.MissingRequiredFields = result.Errors.Count;
    result.CanSubmit = result.Errors.Count == 0;
    
    return Ok(result);
}
```

### Submit Action Logic

**On [Confirm & Submit]:**

```csharp
[HttpPost("submissions/submit")]
public async Task<IActionResult> SubmitForm(int submissionId)
{
    var submission = await _context.FormTemplateSubmissions
        .Include(s => s.Assignment)
            .ThenInclude(a => a.Template)
        .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
    
    if (submission == null)
        return NotFound();
    
    // Check ownership
    if (submission.SubmittedBy != User.GetUserId())
        return Forbid();
    
    // Validate before allowing submission
    var validationResult = await ValidateSubmission(submissionId);
    if (!validationResult.CanSubmit)
    {
        return BadRequest(new { 
            error = "Submission has validation errors", 
            validationResult 
        });
    }
    
    // Update submission status
    submission.Status = "Submitted";
    submission.SubmittedDate = DateTime.UtcNow;
    
    // Check if template requires approval
    if (submission.Assignment.Template.RequiresApproval)
    {
        // Start approval workflow (covered in Section 2)
        await _workflowService.StartApprovalWorkflow(submission);
        submission.Status = "InApproval";
    }
    else
    {
        // No approval needed, auto-approve
        submission.Status = "Approved";
        submission.ApprovedDate = DateTime.UtcNow;
        
        // Populate metrics immediately (covered in Section 6)
        await _metricPopulationService.PopulateMetrics(submissionId);
    }
    
    await _context.SaveChangesAsync();
    
    // Send notifications
    await SendSubmissionNotifications(submission);
    
    return Ok(new { 
        success = true, 
        status = submission.Status,
        message = submission.Status == "InApproval" 
            ? "Submission sent for approval"
            : "Submission completed successfully"
    });
}
```

### Success Response

**After successful submission:**
```
Redirect to: My Assignments Dashboard

Show success message:
"✓ Submission successful! 
 Your form has been submitted for approval.
 You will be notified when it's reviewed."

OR (if no approval required):
"✓ Submission complete! 
 Your form has been processed and metrics updated."
```

---

## 2. Approval Workflow Engine

**Purpose:** Route submissions through multi-step approval process based on workflow configuration

### Workflow Configuration

**From Template Setup (Component 2 - Form Builder):**
```
Template.RequiresApproval = true
Template.WorkflowId = 5  // Links to WorkflowDefinitions table
```

**Workflow Definition Structure:**
```
Table: WorkflowDefinitions
- WorkflowId (PK)
- WorkflowName (e.g., "2-Step Approval", "Regional Manager Approval")
- Description
- Steps (JSON): [
    { "stepOrder": 1, "roleId": 12, "stepName": "Supervisor Review" },
    { "stepOrder": 2, "roleId": 8, "stepName": "Manager Approval" }
  ]
- IsActive
- CreatedDate, CreatedBy
```

**Example Workflows:**
- **1-Step:** Supervisor Review only
- **2-Step:** Supervisor Review → Manager Approval
- **3-Step:** Supervisor → Regional Manager → Head Office Manager
- **Custom:** Any number of steps with specific roles

### Start Workflow Service

**Service:** `Services/ApprovalWorkflowService.cs`

```csharp
public class ApprovalWorkflowService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<ApprovalWorkflowService> _logger;
    
    public async Task StartApprovalWorkflow(FormTemplateSubmission submission)
    {
        var workflow = await _context.WorkflowDefinitions
            .FirstOrDefaultAsync(w => w.WorkflowId == submission.Assignment.Template.WorkflowId);
        
        if (workflow == null)
        {
            throw new Exception($"Workflow {submission.Assignment.Template.WorkflowId} not found");
        }
        
        var steps = JsonConvert.DeserializeObject<List<WorkflowStep>>(workflow.Steps);
        
        // Create approval records for each step
        foreach (var step in steps.OrderBy(s => s.StepOrder))
        {
            var approvalStep = new FormSubmissionApproval
            {
                SubmissionId = submission.SubmissionId,
                StepOrder = step.StepOrder,
                StepName = step.StepName,
                RequiredRoleId = step.RoleId,
                Status = step.StepOrder == 1 ? "Pending" : "Waiting",
                CreatedDate = DateTime.UtcNow
            };
            
            _context.FormSubmissionApprovals.Add(approvalStep);
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Workflow started for submission {SubmissionId}", submission.SubmissionId);
        
        // Notify approvers for first step
        await NotifyApprovers(submission.SubmissionId, 1);
    }
    
    public async Task NotifyApprovers(int submissionId, int stepOrder)
    {
        var step = await _context.FormSubmissionApprovals
            .FirstOrDefaultAsync(a => a.SubmissionId == submissionId && a.StepOrder == stepOrder);
        
        if (step == null) return;
        
        // Get users with required role
        var approvers = await _context.UserRoles
            .Where(ur => ur.RoleId == step.RequiredRoleId)
            .Select(ur => ur.User)
            .ToListAsync();
        
        // Create notifications
        foreach (var approver in approvers)
        {
            var notification = new Notification
            {
                UserId = approver.UserId,
                NotificationType = "ApprovalRequired",
                Title = "New Submission Requires Your Approval",
                Message = $"A form submission is waiting for your review (Step: {step.StepName})",
                RelatedEntityType = "FormSubmission",
                RelatedEntityId = submissionId,
                CreatedDate = DateTime.UtcNow,
                IsRead = false
            };
            
            _context.Notifications.Add(notification);
        }
        
        await _context.SaveChangesAsync();
        
        // Send emails
        await _emailService.SendApprovalNotifications(approvers, submissionId, step.StepName);
        
        _logger.LogInformation("Notified {Count} approvers for step {StepOrder}", approvers.Count, stepOrder);
    }
}
```

### Approval Action

**When approver approves a step:**

```csharp
[HttpPost("api/submissions/{submissionId}/approve")]
public async Task<IActionResult> ApproveSubmission(
    int submissionId,
    [FromBody] ApprovalRequest request)
{
    var submission = await _context.FormTemplateSubmissions
        .Include(s => s.Approvals)
        .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
    
    if (submission == null)
        return NotFound();
    
    // Get current pending step
    var currentStep = submission.Approvals
        .FirstOrDefault(a => a.Status == "Pending");
    
    if (currentStep == null)
        return BadRequest("No pending approval step");
    
    // Check if user has permission (has required role)
    var userHasRole = await _context.UserRoles
        .AnyAsync(ur => ur.UserId == User.GetUserId() && ur.RoleId == currentStep.RequiredRoleId);
    
    if (!userHasRole)
        return Forbid("You do not have permission to approve this step");
    
    // Update approval step
    currentStep.Status = "Approved";
    currentStep.ApprovedBy = User.GetUserId();
    currentStep.ApprovedDate = DateTime.UtcNow;
    currentStep.Comments = request.Comments;
    
    // Check if there are more steps
    var nextStep = submission.Approvals
        .Where(a => a.StepOrder > currentStep.StepOrder)
        .OrderBy(a => a.StepOrder)
        .FirstOrDefault();
    
    if (nextStep != null)
    {
        // Move to next step
        nextStep.Status = "Pending";
        submission.Status = "InApproval";
        
        await _context.SaveChangesAsync();
        
        // Notify next approvers
        await _workflowService.NotifyApprovers(submissionId, nextStep.StepOrder);
        
        // Audit log
        await LogApprovalAction(submissionId, currentStep.StepOrder, "Approved", request.Comments);
        
        return Ok(new { 
            success = true, 
            message = $"Approved. Moving to next step: {nextStep.StepName}",
            nextStep = nextStep.StepName,
            status = "InApproval"
        });
    }
    else
    {
        // All steps approved - finalize
        submission.Status = "Approved";
        submission.ApprovedDate = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        // Populate metrics (covered in Section 6)
        await _metricPopulationService.PopulateMetrics(submissionId);
        
        // Notify submitter of approval
        await NotifySubmitter(submission, "Approved");
        
        // Audit log
        await LogApprovalAction(submissionId, currentStep.StepOrder, "Approved - Final", request.Comments);
        
        return Ok(new { 
            success = true, 
            message = "Submission fully approved",
            status = "Approved"
        });
    }
}
```

### Workflow Progression Diagram

```
Submission Status: InApproval

Step 1: Supervisor Review
  Status: Pending
  Approvers: Users with Role "Supervisor" (RoleId=12)
    ↓
  [Supervisor Approves]
    ↓
Step 1: Status = Approved
Step 2: Status = Pending (was "Waiting")

Step 2: Manager Approval
  Status: Pending
  Approvers: Users with Role "Manager" (RoleId=8)
    ↓
  [Manager Approves]
    ↓
Step 2: Status = Approved
Submission Status: Approved
    ↓
Trigger Metric Population
```

### Database Schema for Approvals

**Table:** `FormSubmissionApprovals`
```sql
CREATE TABLE FormSubmissionApprovals (
    ApprovalId INT PRIMARY KEY IDENTITY,
    SubmissionId INT NOT NULL,
    StepOrder INT NOT NULL,
    StepName NVARCHAR(100) NOT NULL,
    RequiredRoleId INT NOT NULL,
    Status NVARCHAR(20) NOT NULL,  -- Waiting, Pending, Approved, Rejected, Reset
    ApprovedBy INT NULL,
    ApprovedDate DATETIME2 NULL,
    RejectedBy INT NULL,
    RejectedDate DATETIME2 NULL,
    Comments NVARCHAR(1000) NULL,
    CreatedDate DATETIME2 NOT NULL,
    
    FOREIGN KEY (SubmissionId) REFERENCES FormTemplateSubmissions(SubmissionId),
    FOREIGN KEY (RequiredRoleId) REFERENCES Roles(RoleId),
    FOREIGN KEY (ApprovedBy) REFERENCES Users(UserId),
    FOREIGN KEY (RejectedBy) REFERENCES Users(UserId)
)
```

---

## 3. Approval Dashboard

**Purpose:** Display pending approvals to users who have approval permissions

**Component Type:** DataTable with filters

**Access Control:** Users see submissions requiring approval for roles they belong to

### Navigation

**Menu Location:**
```
Form Management
  ├─ My Assignments
  ├─ Pending Approvals ← Approvers land here
  │   └─ Shows forms waiting for my approval
  ├─ Form Templates (if has permission)
  └─ Form Assignments (if has permission)
```

**Badge Indicator:**
```
Pending Approvals (5) ← Red badge with count
```

### Statistics Cards

**Card 1: Pending Approvals**
```
Count: Submissions where Status = "Pending" for current user's roles
Color: Warning
Icon: ri-time-line
Description: "Awaiting your review"
```

**Card 2: Approved Today**
```
Count: Approvals approved by current user today
Color: Success
Icon: ri-checkbox-circle-line
Description: "Forms approved today"
```

**Card 3: Urgent (Overdue)**
```
Count: Submissions past due date still pending approval
Color: Danger
Icon: ri-alert-line
Description: "Past due date"
```

**Card 4: This Week**
```
Count: Approvals pending this week
Color: Info
Icon: ri-calendar-line
Description: "Due this week"
```

### DataTable Configuration

**Columns:**

1. **Template Name**
   - Display: Template name with type badge
   - Example: "Factory Monthly ICT Report" [Monthly]
   - Sortable: Yes

2. **Submitted By**
   - Display: User name + Tenant name
   - Example: "John Doe (Kangaita Factory)"
   - Sortable: Yes
   - Searchable: Yes

3. **Submitted Date**
   - Format: "Nov 28, 2025 3:45 PM"
   - Sortable: Yes
   - Default Sort: DESC

4. **Current Step**
   - Display: Step name + order
   - Example: "Step 1 of 2: Supervisor Review"
   - Color: Blue badge

5. **Days Pending**
   - Calculation: Days since submission
   - Display: "5 days" with color coding:
     - Green: 0-2 days
     - Yellow: 3-5 days
     - Orange: 6-9 days
     - Red: 10+ days

6. **Priority**
   - Based on due date proximity
   - **High** (Red): Overdue or due within 1 day
   - **Medium** (Orange): Due within 3 days
   - **Normal** (Green): More than 3 days
   - Display: Badge with priority level

7. **Actions**
   - [Review] button → Opens approval detail view

### Query Logic

```sql
SELECT 
    s.SubmissionId,
    t.TemplateName,
    t.TemplateType,
    CONCAT(u.FirstName, ' ', u.LastName) as SubmitterName,
    COALESCE(tn.TenantName, 'N/A') as TenantName,
    s.SubmittedDate,
    a.StepName as CurrentStep,
    a.StepOrder,
    (SELECT COUNT(*) FROM FormSubmissionApprovals WHERE SubmissionId = s.SubmissionId) as TotalSteps,
    DATEDIFF(DAY, s.SubmittedDate, GETUTCDATE()) as DaysPending,
    asg.DueDate,
    CASE 
        WHEN asg.DueDate < GETUTCDATE() THEN 'High'
        WHEN DATEDIFF(DAY, GETUTCDATE(), asg.DueDate) <= 1 THEN 'High'
        WHEN DATEDIFF(DAY, GETUTCDATE(), asg.DueDate) <= 3 THEN 'Medium'
        ELSE 'Normal'
    END as Priority
FROM FormTemplateSubmissions s
INNER JOIN FormTemplateAssignments asg ON s.AssignmentId = asg.AssignmentId
INNER JOIN FormTemplates t ON asg.TemplateId = t.TemplateId
INNER JOIN Users u ON s.SubmittedBy = u.UserId
LEFT JOIN Tenants tn ON asg.TenantId = tn.TenantId
INNER JOIN FormSubmissionApprovals a ON s.SubmissionId = a.SubmissionId
WHERE s.Status = 'InApproval'
  AND a.Status = 'Pending'
  AND a.RequiredRoleId IN (
      SELECT RoleId FROM UserRoles WHERE UserId = @CurrentUserId
  )
ORDER BY 
    CASE 
        WHEN asg.DueDate < GETUTCDATE() THEN 0  -- Overdue first
        WHEN DATEDIFF(DAY, GETUTCDATE(), asg.DueDate) <= 1 THEN 1  -- Due today/tomorrow
        ELSE 2
    END,
    DaysPending DESC,
    s.SubmittedDate ASC
```

### Filters

**Status Filter:**
```
Options:
- All Pending
- Urgent (Overdue)
- Due This Week
- Due This Month
```

**Template Type Filter:**
```
Multi-select:
- Monthly
- Quarterly
- Annual
- Ad-hoc
```

**Date Range Filter:**
```
Submitted Date:
- From: [Date Picker]
- To: [Date Picker]
```

**Search:**
```
Full-text search on:
- Template Name
- Submitter Name
- Tenant Name
```

### Priority Alerts

**Overdue Alert (if any exist):**
```
┌─ 🔴 Urgent: Overdue Approvals ─────────────────────────────────┐
│                                                                 │
│ You have 3 overdue approvals requiring immediate attention:    │
│                                                                 │
│ • Factory Monthly Report (Kangaita) - 5 days overdue           │
│ • Safety Checklist (Mataara) - 3 days overdue                  │
│ • Network Audit (Njunu) - 2 days overdue                       │
│                                                                 │
│ [Review All] [Dismiss]                                         │
└─────────────────────────────────────────────────────────────────┘
```

---

## 4. Approval Detail View

**Purpose:** Display submitted form data to approver for review with approve/reject actions

**URL:** `/approvals/review/{submissionId}`

**Access Control:** Only users with required role for current approval step

### View Layout

```
┌─ Approval Required ────────────────────────────────────────────┐
│                                                                 │
│  ← Back to Approvals                                           │
│                                                                 │
│  ──────────────────────────────────────────────────────────── │
│                                                                 │
│  Template: Factory Monthly ICT Report                          │
│  Type: [Monthly]                                               │
│                                                                 │
│  Submitted By: John Doe                                        │
│  Tenant: Kangaita Factory                                      │
│  Region: Central Region                                        │
│                                                                 │
│  Submitted: Nov 28, 2025 3:45 PM (2 days ago)                 │
│  Due Date: Nov 30, 2025 (Due in 1 day) ⚠️                     │
│                                                                 │
│  ──────────────────────────────────────────────────────────── │
│                                                                 │
│  Approval Workflow: 2-Step Approval                            │
│                                                                 │
│  ● Step 1 of 2: Supervisor Review (Current)                   │
│    Status: Pending your approval                              │
│    Assigned to: Supervisors (5 users)                         │
│                                                                 │
│  ○ Step 2 of 2: Manager Approval                              │
│    Status: Waiting                                             │
│    Assigned to: Managers (3 users)                            │
│                                                                 │
│  ──────────────────────────────────────────────────────────── │
│                                                                 │
│  Form Responses (Read-Only)                                    │
│                                                                 │
│  [Section 1: General Information ▼]                            │
│    Factory Name: Kangaita Factory                              │
│    Reporting Period: November 2025                             │
│    Region: Central Region                                      │
│    Contact Person: John Doe                                    │
│    Contact Email: john.doe@ktda.co.ke                         │
│                                                                 │
│  [Section 2: Hardware Inventory ▼]                             │
│    Number of Computers: 25                                     │
│    Is LAN working?: Yes                                        │
│    Number of Printers: 5                                       │
│    Number of Servers: 2                                        │
│    Attached Files:                                             │
│      📎 network_diagram.pdf (2.3 MB) [Download] [View]        │
│      📎 hardware_list.xlsx (45 KB) [Download] [View]          │
│                                                                 │
│  [Section 3: Network Status ▼]                                 │
│    Network Uptime: 98.5%                                       │
│    Backup Status: Completed                                    │
│    Last Backup Date: Nov 27, 2025                             │
│    Internet Connectivity: Stable                               │
│    Bandwidth: 50 Mbps                                          │
│                                                                 │
│  ──────────────────────────────────────────────────────────── │
│                                                                 │
│  Previous Approvals:                                           │
│  (None - this is the first approval step)                      │
│                                                                 │
│  ──────────────────────────────────────────────────────────── │
│                                                                 │
│  Your Review                                                    │
│                                                                 │
│  Comments (Optional):                                          │
│  [________________________________________________]             │
│  [________________________________________________]             │
│  [________________________________________________]             │
│                                                                 │
│  Examples:                                                      │
│  • "All information verified and accurate"                     │
│  • "Hardware count matches physical inventory"                 │
│  • "Network metrics within expected range"                     │
│                                                                 │
│  ──────────────────────────────────────────────────────────── │
│                                                                 │
│  [✕ Reject] [✓ Approve]                                        │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Approval History Display (Multi-Step Workflows)

**For Step 2+ approvals, show previous approvals:**
```
Previous Approvals:

  ✓ Step 1: Supervisor Review
    Approved by: Jane Smith
    Date: Nov 28, 2025 4:15 PM
    Comments: "All information verified and correct. Hardware inventory 
               matches our records from last month."
    
  ● Step 2: Manager Approval (Current)
    Status: Pending your approval
```

### Read-Only Form Display

**All field values are displayed in read-only format:**
- Text fields: Plain text display
- Number fields: Value with unit (e.g., "25 units", "98.5%")
- Boolean fields: "Yes" or "No"
- Date fields: Formatted date
- Dropdown fields: Selected option text
- File uploads: Download links with file size

**No editing allowed** - Approvers can only view the data

### Controller Logic

```csharp
[HttpGet("approvals/review/{submissionId}")]
public async Task<IActionResult> ReviewSubmission(int submissionId)
{
    var submission = await _context.FormTemplateSubmissions
        .Include(s => s.Assignment)
            .ThenInclude(a => a.Template)
        .Include(s => s.Assignment.Tenant)
        .Include(s => s.Responses)
        .Include(s => s.Approvals)
            .ThenInclude(a => a.ApprovedByUser)
        .Include(s => s.Approvals)
            .ThenInclude(a => a.RejectedByUser)
        .Include(s => s.Approvals)
            .ThenInclude(a => a.RequiredRole)
        .Include(s => s.SubmittedByUser)
        .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
    
    if (submission == null)
        return NotFound();
    
    // Check if user has permission to review
    var currentStep = submission.Approvals.FirstOrDefault(a => a.Status == "Pending");
    if (currentStep == null)
        return BadRequest("No pending approval step");
    
    var userHasRole = await _context.UserRoles
        .AnyAsync(ur => ur.UserId == User.GetUserId() && ur.RoleId == currentStep.RequiredRoleId);
    
    if (!userHasRole)
        return Forbid("You do not have permission to review this submission");
    
    // Build view model
    var viewModel = new ApprovalReviewViewModel
    {
        SubmissionId = submission.SubmissionId,
        Template = submission.Assignment.Template,
        SubmittedBy = submission.SubmittedByUser,
        Tenant = submission.Assignment.Tenant,
        SubmittedDate = submission.SubmittedDate,
        DueDate = submission.Assignment.DueDate,
        Sections = LoadSectionsWithResponses(submission),
        CurrentStep = currentStep,
        ApprovalHistory = submission.Approvals
            .Where(a => a.Status == "Approved" || a.Status == "Rejected")
            .OrderBy(a => a.StepOrder)
            .ToList()
    };
    
    return View(viewModel);
}
```

---

## 5. Rejection Handler

**Purpose:** Allow approvers to reject submissions and send back to submitter for revision

### Rejection Modal

**Triggered by:** Approver clicks [Reject] button on approval detail view

```
┌─ Reject Submission ────────────────────────────────────────────┐
│                                                                 │
│  ⚠️ Reject this submission?                                    │
│                                                                 │
│  The submission will be sent back to the submitter for         │
│  revision with your comments.                                   │
│                                                                 │
│  Template: Factory Monthly ICT Report                          │
│  Submitted by: John Doe (Kangaita Factory)                     │
│                                                                 │
│  ──────────────────────────────────────────────────────────── │
│                                                                 │
│  Rejection Reason* (required):                                 │
│  [_____________________________________________________]       │
│  [_____________________________________________________]       │
│  [_____________________________________________________]       │
│  [_____________________________________________________]       │
│                                                                 │
│  Please provide specific details about what needs to be        │
│  corrected so the submitter can make necessary changes.        │
│                                                                 │
│  Examples:                                                      │
│  • "Hardware inventory numbers don't match physical count"     │
│  • "Network uptime calculation appears incorrect"              │
│  • "Missing supporting documentation for backup failures"      │
│  • "Server count differs from IT department records"           │
│                                                                 │
│  ──────────────────────────────────────────────────────────── │
│                                                                 │
│  [Cancel] [Confirm Rejection]                                  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Rejection Logic

**Controller Action:**

```csharp
[HttpPost("api/submissions/{submissionId}/reject")]
public async Task<IActionResult> RejectSubmission(
    int submissionId,
    [FromBody] RejectionRequest request)
{
    // Validate rejection reason
    if (string.IsNullOrWhiteSpace(request.Reason))
        return BadRequest(new { error = "Rejection reason is required" });
    
    if (request.Reason.Length < 10)
        return BadRequest(new { error = "Rejection reason must be at least 10 characters" });
    
    var submission = await _context.FormTemplateSubmissions
        .Include(s => s.Assignment)
        .Include(s => s.Approvals)
        .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
    
    if (submission == null)
        return NotFound();
    
    // Get current pending step
    var currentStep = submission.Approvals
        .FirstOrDefault(a => a.Status == "Pending");
    
    if (currentStep == null)
        return BadRequest(new { error = "No pending approval step" });
    
    // Check permission
    var userHasRole = await _context.UserRoles
        .AnyAsync(ur => ur.UserId == User.GetUserId() && ur.RoleId == currentStep.RequiredRoleId);
    
    if (!userHasRole)
        return Forbid("You do not have permission to reject this submission");
    
    // Update approval step
    currentStep.Status = "Rejected";
    currentStep.RejectedBy = User.GetUserId();
    currentStep.RejectedDate = DateTime.UtcNow;
    currentStep.Comments = request.Reason;
    
    // Update submission status
    submission.Status = "Rejected";
    submission.RejectedDate = DateTime.UtcNow;
    submission.RejectionReason = request.Reason;
    submission.RejectedBy = User.GetUserId();
    submission.RejectedAtStep = currentStep.StepOrder;
    
    // Reset all subsequent approval steps (they need to start over when resubmitted)
    foreach (var step in submission.Approvals.Where(a => a.StepOrder > currentStep.StepOrder))
    {
        step.Status = "Reset";
    }
    
    await _context.SaveChangesAsync();
    
    // Log rejection action
    await LogApprovalAction(
        submissionId, 
        currentStep.StepOrder, 
        "Rejected", 
        request.Reason
    );
    
    // Notify submitter
    await NotifySubmitter(submission, "Rejected", request.Reason);
    
    _logger.LogInformation(
        "Submission {SubmissionId} rejected by user {UserId} at step {StepOrder}",
        submissionId, 
        User.GetUserId(), 
        currentStep.StepOrder
    );
    
    return Ok(new { 
        success = true, 
        message = "Submission rejected and sent back for revision",
        status = "Rejected"
    });
}
```

### Rejection Notification

**Email to Submitter:**
```
Subject: Form Submission Rejected - Revision Required

Dear John Doe,

Your form submission has been rejected and requires revision.

Template: Factory Monthly ICT Report
Period: November 2025
Rejected by: Jane Smith (Supervisor)
Rejected on: Nov 28, 2025 5:30 PM

Rejection Reason:
"Hardware inventory numbers don't match our physical count from last week. 
Please verify the computer count - we have 27 computers, not 25. Also, 
printer count should be 6, not 5."

What to do next:
1. Review the rejection comments carefully
2. Make necessary corrections to your responses
3. Resubmit the form for approval

[Revise & Resubmit Form]

Please complete the revision by: Nov 30, 2025

Best regards,
KTDA ICT Reporting System
```

---

## 6. Resubmission Flow

**Purpose:** Allow submitters to revise and resubmit rejected forms

### Rejected Submission Display

**On My Assignments Dashboard:**

**Status Badge:** [Rejected] (Red background)

**Actions Column:**
- [Revise & Resubmit] button (Primary action)
- [View Rejection Details] button

**Rejection Alert on Form:**
```
┌─ 🔴 This Submission Was Rejected ──────────────────────────────┐
│                                                                 │
│  Your submission was rejected at Step 1: Supervisor Review     │
│  Rejected by: Jane Smith                                       │
│  Date: Nov 28, 2025 5:30 PM                                   │
│                                                                 │
│  Rejection Reason:                                             │
│  "Hardware inventory numbers don't match our physical count    │
│   from last week. Please verify the computer count - we have   │
│   27 computers, not 25. Also, printer count should be 6, not 5."│
│                                                                 │
│  Please review the rejection comments and make necessary       │
│  corrections before resubmitting.                              │
│                                                                 │
│  [View Full Details] [Dismiss]                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Revision Form

**When user clicks [Revise & Resubmit]:**

1. **Load Form with Previous Responses**
   - All previous responses pre-filled
   - Form is editable
   - Rejection reason displayed at top as persistent alert
   - Fields mentioned in rejection highlighted (optional enhancement)

2. **Track Changes** (Optional)
   - Show which fields were modified since rejection
   - Mark: "Updated since rejection" badge on changed fields

3. **Resubmission Logic**

```csharp
[HttpPost("submissions/resubmit")]
public async Task<IActionResult> ResubmitForm(int submissionId)
{
    var submission = await _context.FormTemplateSubmissions
        .Include(s => s.Assignment)
            .ThenInclude(a => a.Template)
        .Include(s => s.Approvals)
        .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
    
    if (submission == null)
        return NotFound();
    
    // Check ownership
    if (submission.SubmittedBy != User.GetUserId())
        return Forbid();
    
    // Check status
    if (submission.Status != "Rejected")
        return BadRequest(new { error = "Only rejected submissions can be resubmitted" });
    
    // Validate (same validation as initial submission)
    var validationResult = await ValidateSubmission(submissionId);
    if (!validationResult.CanSubmit)
    {
        return BadRequest(new { 
            error = "Submission has validation errors", 
            validationResult 
        });
    }
    
    // Clear rejection info
    submission.Status = "Submitted";
    submission.SubmittedDate = DateTime.UtcNow;
    submission.RejectionReason = null;
    submission.RejectedDate = null;
    submission.RejectedBy = null;
    submission.RejectedAtStep = null;
    submission.ResubmissionCount = (submission.ResubmissionCount ?? 0) + 1;
    
    // Reset all approval steps to restart workflow
    foreach (var approval in submission.Approvals)
    {
        if (approval.Status == "Rejected")
        {
            approval.Status = "Waiting";
            approval.RejectedBy = null;
            approval.RejectedDate = null;
            approval.Comments = $"[Previous rejection: {approval.Comments}]";
        }
        else if (approval.Status == "Approved")
        {
            // Reset previously approved steps (workflow restarts from beginning)
            approval.Status = "Waiting";
            // Keep approval history for audit
            approval.Comments = $"[Previously approved by {approval.ApprovedByUser?.FullName} on {approval.ApprovedDate}] {approval.Comments}";
        }
        
        // Set first step as pending
        if (approval.StepOrder == 1)
        {
            approval.Status = "Pending";
        }
    }
    
    // Restart workflow
    if (submission.Assignment.Template.RequiresApproval)
    {
        submission.Status = "InApproval";
        
        // Notify approvers for Step 1
        await _workflowService.NotifyApprovers(submissionId, 1);
    }
    else
    {
        submission.Status = "Approved";
        submission.ApprovedDate = DateTime.UtcNow;
        await _metricPopulationService.PopulateMetrics(submissionId);
    }
    
    await _context.SaveChangesAsync();
    
    // Log resubmission
    await LogResubmission(submissionId, submission.ResubmissionCount ?? 0);
    
    _logger.LogInformation(
        "Submission {SubmissionId} resubmitted (attempt #{Attempt})",
        submissionId,
        submission.ResubmissionCount
    );
    
    return Ok(new { 
        success = true, 
        message = "Form resubmitted successfully",
        status = submission.Status,
        resubmissionCount = submission.ResubmissionCount
    });
}
```

### Resubmission Tracking

**Additional Database Fields:**

```sql
ALTER TABLE FormTemplateSubmissions
ADD ResubmissionCount INT NULL DEFAULT 0,
    RejectedBy INT NULL,
    RejectedAtStep INT NULL;

ALTER TABLE FormTemplateSubmissions
ADD FOREIGN KEY (RejectedBy) REFERENCES Users(UserId);
```

**Audit Trail:**

Every rejection and resubmission is logged:
```
AuditLogs:
- EntityType: FormTemplateSubmission
- Action: Rejected / Resubmitted
- Details: {
    "rejectedBy": "Jane Smith",
    "rejectedAtStep": 1,
    "reason": "...",
    "resubmissionNumber": 1
  }
```

### Resubmission Limits (Optional)

**Prevent infinite rejection loops:**

```csharp
const int MAX_RESUBMISSIONS = 5;

if (submission.ResubmissionCount >= MAX_RESUBMISSIONS)
{
    return BadRequest(new { 
        error = $"Maximum resubmission limit ({MAX_RESUBMISSIONS}) reached. Please contact support.",
        resubmissionCount = submission.ResubmissionCount
    });
}
```

### Workflow Restart Behavior

**Option A: Restart from Beginning** (Recommended)
- All previous approvals are reset
- Workflow starts from Step 1
- Previous approval history retained for audit

**Option B: Resume from Rejection Point** (Alternative)
- Only rejected step and subsequent steps are reset
- Previously approved steps remain approved
- Faster but less thorough review

**Current Implementation:** Option A (more thorough)

---

## 7. Workflow States & Transitions

### Complete Status Flow

```
Draft (Auto-saved)
  ↓
  [User clicks Submit]
  ↓
Submitted
  ├─ [No approval required] → Approved → Metric Population
  │
  └─ [Approval required] → InApproval
      ↓
      Step 1: Pending
        ├─ [Approved] → Step 2: Pending
        │   ├─ [Approved] → ... → Last Step: Pending
        │   │   └─ [Approved] → Approved → Metric Population
        │   └─ [Rejected] → Rejected
        │       └─ [User revises] → Resubmitted → InApproval (restart)
        │
        └─ [Rejected] → Rejected
            └─ [User revises] → Resubmitted → InApproval (restart)
```

### Status Definitions

| Status | Description | User Actions | System Actions |
|--------|-------------|--------------|----------------|
| Draft | Form auto-saved, not submitted | Continue editing, Submit | Auto-save every 30s |
| Submitted | Submitted but processing | None | Route to approval or auto-approve |
| InApproval | In approval workflow | View status | Notify current step approvers |
| Approved | All approvals complete | View submission | Populate metrics, notify submitter |
| Rejected | Sent back for revision | Revise & Resubmit | Notify submitter with reason |

---

## 8. Notifications Summary

### Notification Types & Triggers

**1. Submission Received (to Approvers)**
- **Trigger:** Submission enters approval queue
- **Recipients:** Users with required role for current approval step
- **Content:** Template name, submitter, due date
- **Action:** [Review Submission]

**2. Approval Step Completed (to Next Approvers)**
- **Trigger:** Previous approval step approved
- **Recipients:** Users with required role for next step
- **Content:** Template name, current step, previous approver
- **Action:** [Review Submission]

**3. Submission Approved (to Submitter)**
- **Trigger:** All approval steps complete
- **Recipients:** Original submitter
- **Content:** Template name, final approver, completion date
- **Action:** [View Submission]

**4. Submission Rejected (to Submitter)**
- **Trigger:** Any approval step rejected
- **Recipients:** Original submitter
- **Content:** Template name, rejector, rejection reason, rejected step
- **Action:** [Revise & Resubmit]

**5. Submission Overdue (to Approvers & Submitter)**
- **Trigger:** Due date passed without approval
- **Recipients:** Current step approvers + submitter
- **Content:** Template name, days overdue, urgency level
- **Action:** [Review Submission] / [View Status]

---

## 9. Metric Population

**Note:** The Metric Population Service is documented in detail in:

**→ See: `4D_MetricPopulation_Service.md`**

This service automatically extracts approved form data and populates KPI metrics in the `TenantMetrics` table, supporting:
- **Direct mapping** (field value → metric value)
- **Calculated mapping** (formulas using multiple fields)
- **Binary compliance mapping** (Yes/No → 100% / 0%)

The service triggers automatically after submission approval and includes comprehensive error handling, logging, and retry mechanisms.

---

## 10. Reporting & Analytics

**Note:** Form submission reporting is handled in **Section 10: Reporting & Analytics** which covers:

- Submission compliance dashboards
- Approval workflow performance metrics
- Form response analytics
- Metric trend analysis
- Export capabilities (Excel, PDF)
- Comparative analysis across tenants/regions
- Visual charts and graphs

---

## API Endpoints Summary

```
# Submission Actions
POST   /submissions/submit                        - Submit form for approval
POST   /submissions/resubmit                      - Resubmit rejected form
GET    /submissions/{id}/validation-status        - Check validation status

# Approval Actions
GET    /approvals/dashboard                       - Approver's dashboard
GET    /approvals/review/{submissionId}          - Review submission detail
POST   /api/submissions/{id}/approve             - Approve current step
POST   /api/submissions/{id}/reject              - Reject and send back

# Workflow Management
GET    /api/workflows/{id}                        - Get workflow definition
GET    /api/submissions/{id}/approval-history    - Get approval history

# Admin Actions
POST   /api/admin/metrics/reprocess/{id}         - Manually reprocess metrics
GET    /api/admin/approvals/overdue              - Get overdue approvals
```

---

## Database Schema Summary

### Key Tables

**FormTemplateSubmissions**
- SubmissionId, AssignmentId, Status, SubmittedBy
- SubmittedDate, ApprovedDate, RejectedDate
- RejectionReason, RejectedBy, RejectedAtStep
- ResubmissionCount

**FormSubmissionApprovals**
- ApprovalId, SubmissionId, StepOrder, StepName
- RequiredRoleId, Status
- ApprovedBy, ApprovedDate
- RejectedBy, RejectedDate, Comments

**FormTemplateResponses**
- ResponseId, SubmissionId, ItemId
- ResponseValue
- CreatedDate, ModifiedDate

**FormSubmissionAttachments**
- AttachmentId, SubmissionId, ItemId
- FileName, FilePath, FileSize
- UploadedBy, UploadedDate

**MetricPopulationLog** (See 4D document)
- LogId, SubmissionId, MetricId, MappingId, SourceItemId
- SourceValue, CalculatedValue, CalculationFormula
- Status, ErrorMessage, ProcessingTimeMs, PopulatedBy

---

## Implementation Checklist

### Phase 1: Core Submission
- [ ] Pre-submission review modal
- [ ] Validation service
- [ ] Submit action with approval routing
- [ ] Success notifications

### Phase 2: Approval Workflow
- [ ] Workflow definition management
- [ ] Start workflow service
- [ ] Approval dashboard for approvers
- [ ] Approval detail view (read-only)
- [ ] Approve action with step progression
- [ ] Approval notifications

### Phase 3: Rejection & Resubmission
- [ ] Rejection modal with reason
- [ ] Reject action
- [ ] Rejection notifications
- [ ] Resubmission form (pre-filled)
- [ ] Workflow restart logic
- [ ] Resubmission tracking

### Phase 4: Metric Population
- [ ] Metric mapping configuration
- [ ] Population service implementation
- [ ] Direct mapping processor
- [ ] Calculated mapping processor
- [ ] Binary compliance processor
- [ ] Logging and audit trail
- [ ] Error handling and retry
- [ ] Manual reprocessing endpoint

### Phase 5: Monitoring & Reporting
- [ ] Approval performance dashboards
- [ ] Overdue approval alerts
- [ ] Submission analytics
- [ ] Audit log viewers

---

**Document Complete** ✅

This document covers the complete submission review and approval workflow. For metric population details, see `4D_MetricPopulation_Service.md`.