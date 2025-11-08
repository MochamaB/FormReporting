# Form Assignment - Component Implementation

**Purpose:** Distribute published templates to users/tenants for completion  
**Users:** Assignment Managers (Head Office, Regional Managers)  
**Navigation:** Standalone module with own menu item + link from Template Dashboard

---

## Component Summary

| Component | Type | Purpose |
|-----------|------|---------||
| 1. Assignment Dashboard | StatCards + DataTable | Manage all assignments, view compliance |
| 2. Assignment Creation Wizard | 4-Step Wizard | Create new assignment |
| 3. Target Selection Component | Multi-select with filters | Choose who gets the form |
| 4. Recurrence Configuration | Form with schedule builder | Set up recurring assignments |
| 5. Assignment Compliance Tracker | Dashboard with charts | Monitor completion rates |
| 6. Bulk Assignment Actions | Batch operations | Cancel, extend, remind multiple |

---

## Navigation Integration

### From Template Dashboard

**Update Template Dashboard DataTable (Component 1 from 2A):**

Add new action button in Actions column:

```
Actions Column:
- [👁️ Preview] (all templates)
- [✏️ Edit] (draft only)
- [📋 Clone] (all templates)
- [🗄️ Archive] (published only)
- [📤 Assign] (published only) ← NEW BUTTON
  → Navigates to: /assignments/create?templateId={id}
  → Opens Assignment Creation Wizard with template pre-selected
```

**Button Logic:**
```csharp
// Only show for Published templates
if (template.PublishStatus == "Published" && user.HasPermission("Assignments.Create"))
{
    <a href="@Url.Action("Create", "Assignments", new { templateId = template.TemplateId })" 
       class="btn btn-sm btn-success"
       title="Assign this template">
        <i class="ri-send-plane-line"></i> Assign
    </a>
}
```

### Main Menu Item

**Add to sidebar navigation:**
```
Form Management
  ├─ Form Templates (existing)
  ├─ Form Assignments ← NEW
  │   ├─ Assignment Dashboard
  │   ├─ Create Assignment
  │   └─ Compliance Report
  └─ Form Submissions (comes later)
```

---

## 1. Assignment Dashboard

**Component Type:** Page with StatCards + DataTable + Charts

**Reusable Components:**
- StatCards (4 cards)
- DataTable
- Pie Chart (compliance visualization)
- Modal (cancel confirmation)

### Statistics Cards

**Card 1: Total Assignments**
```
Calculation: COUNT(FormTemplateAssignments WHERE IsActive=1)
Color: Primary
Icon: ri-file-list-3-line
Trend: Growth from last month
```

**Card 2: Active Assignments**
```
Calculation: COUNT(Status IN ('Pending', 'InProgress') AND DueDate >= TODAY)
Color: Info
Icon: ri-file-edit-line
Link: "View active"
```

**Card 3: Overdue**
```
Calculation: COUNT(DueDate < TODAY AND Status != 'Completed')
Color: Danger
Icon: ri-alarm-warning-line
Trend: Increase/decrease indicator
```

**Card 4: Completion Rate**
```
Calculation: (COUNT(Status='Completed') / COUNT(All)) * 100
Color: Success
Icon: ri-checkbox-circle-line
Badge: "vs. last month"
```

### DataTable Configuration

**Columns:**

1. **Template Name** (link to template preview)
2. **Assignment Type** (badge: Tenant, User, Role, Region)
3. **Assigned To** (summary: "47 Factories", "5 Users", "All in Central Region")
4. **Reporting Period** (Month/Quarter/Year)
5. **Due Date** (highlight if < 7 days, red if overdue)
6. **Status** (badge with color)
   - Pending (gray) - No submissions started
   - In Progress (blue) - Some submissions started
   - Completed (green) - All submissions approved
   - Overdue (red) - Past due date
7. **Completion** (progress bar: "45/100 (45%)")
8. **Actions** (View Details, Cancel, Extend, Send Reminder)

### Status Calculation Logic

```csharp
public string GetAssignmentStatus(FormTemplateAssignment assignment)
{
    var submissions = GetSubmissions(assignment.AssignmentId);
    var totalTargets = GetTargetCount(assignment);
    
    if (submissions.Count == 0)
        return "Pending";
    
    var completedCount = submissions.Count(s => s.Status == "Approved");
    
    if (completedCount == totalTargets)
        return "Completed";
    
    if (assignment.DueDate < DateTime.Now && completedCount < totalTargets)
        return "Overdue";
    
    return "In Progress";
}

int GetTargetCount(FormTemplateAssignment assignment)
{
    return assignment.AssignmentType switch
    {
        "Tenant" => 1,
        "User" => 1,
        "Role" => GetUsersInRole(assignment.RoleId).Count,
        "Region" => GetTenantsInRegion(assignment.RegionId).Count,
        "TenantGroup" => GetTenantsInGroup(assignment.TenantGroupId).Count,
        _ => 0
    };
}
```

### Filters

```
Status: All, Pending, In Progress, Completed, Overdue
Template: Dropdown of all published templates
Assignment Type: All, Tenant, User, Role, Region, TenantGroup
Reporting Period: Month/Year picker
Due Date Range: From [date] To [date]
Created By: User selector
```

### Compliance Chart

**Pie Chart:**
```
Segments:
- Completed (green): X%
- In Progress (blue): Y%
- Pending (gray): Z%
- Overdue (red): W%

Total: 100%
Center Label: "Overall Compliance"
```

### Quick Actions

**Bulk Actions:**
```
[Select All] checkbox

With Selected:
- [Send Reminder] → Notify assigned users
- [Extend Deadline] → Add X days to due date
- [Cancel Assignments] → Deactivate selected
- [Export to Excel] → Download compliance report
```

---

## 2. Assignment Creation Wizard

**Component Type:** 4-Step Horizontal Wizard

**Reusable Components:**
- Horizontal Wizard
- Standard Forms
- Multi-select components
- Date pickers

### Step 1: Select Template & Period

**Template Selection:**
```
Template*: [Dropdown: Published templates only]
  → Filtered by: PublishStatus='Published' AND IsActive=1
  → Display: Template Name (Type) - Category
  → Example: "Factory Monthly Report (Monthly) - IT"

  If templateId in URL (from Template Dashboard [Assign] button):
    → Pre-select template
    → Dropdown disabled (read-only)
    → Show: "Template pre-selected from previous page"

Template Preview: [Preview Template] button
  → Opens template preview modal

Template Details (read-only display):
- Description
- Type: Monthly/Quarterly/Annual/OnDemand
- Sections: 5
- Fields: 33
- Estimated Time: 20 minutes
- Requires Approval: Yes
- Workflow: 2-Step Approval
```

**Reporting Period:**
```
IF Template Type = Monthly:
  Period*: [Month/Year picker]
  → Default: Current month
  → Format: "November 2025"

IF Template Type = Quarterly:
  Period*: [Quarter/Year picker]
  → Options: Q1 2025, Q2 2025, Q3 2025, Q4 2025

IF Template Type = Annual:
  Period*: [Year picker]
  → Options: 2024, 2025, 2026

IF Template Type = OnDemand:
  Period*: [Date picker]
  → Any date
  → Label: "Assignment Date"
```

**[Next: Select Targets →]**

---

### Step 2: Target Selection (Component 3)

**Assignment Type Selection:**
```
Assign to*: [Radio buttons]

○ Specific Tenant(s)
  → Multi-select dropdown of tenants
  → Filtered by user's access level
  
○ Specific User(s)
  → Multi-select dropdown of active users
  → Filtered by role access
  
○ All Users in Role
  → Dropdown: Select role
  → Shows count: "23 users will receive this assignment"
  
○ All Tenants in Region
  → Dropdown: Select region
  → Shows count: "47 tenants will receive this assignment"
  
○ All Tenants in Group
  → Dropdown: Select tenant group
  → Shows count: "15 tenants will receive this assignment"
```

**Target Preview:**
```
┌─ Selected Targets ─────────────────────────────┐
│ Assignment Type: All Tenants in Region        │
│ Region: Central Region                         │
│                                                │
│ Total Recipients: 47 tenants                  │
│                                                │
│ [View Full List]                              │
│   → Opens modal with complete list            │
└────────────────────────────────────────────────┘
```

**[← Back] [Next: Configure Deadline →]**

---

### Step 3: Due Date & Recurrence (Component 4)

**Due Date:**
```
Due Date*: [Date picker]
  → Must be > Assignment Date
  → Default: End of reporting period
    (e.g., if Period = November 2025, default = Nov 30, 2025)
  
  Validation:
  - Cannot be in the past
  - Warning if < 3 days from now: "Very short deadline"
  
Days Until Due: Auto-calculated display
  → Example: "15 days from now"
```

**Recurrence Configuration:**
```
Recurring Assignment: [Checkbox]
  → Default: Checked for Monthly/Quarterly/Annual templates
  → Disabled for OnDemand templates

IF Checked:
  
  Recurrence Pattern*: [Dropdown]
    → Monthly (1st of every month)
    → Quarterly (1st of quarter)
    → Annually (1st of year)
    → Custom
  
  IF Custom selected:
    Repeat every: [__] [Dropdown: Days/Weeks/Months]
    
  Start Date: [Date picker]
    → Default: Next occurrence after current period
  
  End Recurrence: [Radio buttons]
    ○ Never
    ○ After [__] occurrences
    ○ On [date picker]
  
  Auto-Due Date: [Checkbox]
    → If checked: Due date auto-set to end of each period
    → If unchecked: Due date = [__] days after assignment
```

**Recurrence Preview:**
```
┌─ Upcoming Assignments ─────────────────────────┐
│ Next 5 occurrences:                            │
│ 1. December 2025 (Due: Dec 31, 2025)          │
│ 2. January 2026 (Due: Jan 31, 2026)           │
│ 3. February 2026 (Due: Feb 28, 2026)          │
│ 4. March 2026 (Due: Mar 31, 2026)             │
│ 5. April 2026 (Due: Apr 30, 2026)             │
└────────────────────────────────────────────────┘
```

**[← Back] [Next: Review & Confirm →]**

---

### Step 4: Review & Confirm

**Summary Display:**
```
┌─ Assignment Summary ───────────────────────────────────────────┐
│                                                                 │
│ Template:                                                       │
│   Factory Monthly ICT Report (Monthly - IT)                    │
│                                                                 │
│ Reporting Period:                                               │
│   November 2025                                                 │
│                                                                 │
│ Assigned To:                                                    │
│   All Tenants in Central Region (47 tenants)                  │
│   [View List]                                                  │
│                                                                 │
│ Due Date:                                                       │
│   November 30, 2025 (15 days from now)                        │
│                                                                 │
│ Recurrence:                                                     │
│   Monthly, on 1st of month                                     │
│   Continues indefinitely                                        │
│   Auto-due date: End of each month                            │
│                                                                 │
│ Notifications:                                                  │
│   ✓ Notify assigned users immediately                          │
│   ✓ Send reminder 3 days before due date                      │
│   ✓ Send overdue notification 1 day after due date            │
│                                                                 │
│ Total Assignments to Create: 47                                │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘

[← Back] [Create Assignment]
```

### Create Assignment Logic

**On Click [Create Assignment]:**

```csharp
POST /api/assignments/create
Body: {
    templateId,
    reportingPeriod,
    assignmentType,  // "Tenant", "User", "Role", "Region", "TenantGroup"
    targetId,        // RoleId, RegionId, TenantGroupId (if applicable)
    targetIds,       // Array of TenantIds or UserIds (if Specific)
    dueDate,
    isRecurring,
    recurrencePattern,
    recurrenceEndDate,
    recurrenceOccurrences
}

Database Operations:

1. Create Assignment Records (one per target):
   
   IF AssignmentType = "Role":
     var users = GetUsersInRole(targetId);
     foreach (var user in users)
     {
         INSERT FormTemplateAssignments (
             TemplateId, AssignmentType, UserId,
             ReportingPeriod, DueDate, AssignedDate, AssignedBy
         );
     }
   
   IF AssignmentType = "Region":
     var tenants = GetTenantsInRegion(targetId);
     foreach (var tenant in tenants)
     {
         INSERT FormTemplateAssignments (
             TemplateId, AssignmentType, TenantId,
             ReportingPeriod, DueDate, AssignedDate, AssignedBy
         );
     }
   
   // Similar logic for TenantGroup, Specific Users, Specific Tenants

2. Create Notifications:
   
   INSERT Notifications (
       NotificationType = 'FormAssigned',
       Title = 'New Form Assigned: {TemplateName}',
       Message = 'Please complete by {DueDate}',
       CreatedDate = NOW()
   );
   
   INSERT NotificationRecipients (
       NotificationId, UserId, IsRead = 0
   );

3. Schedule Recurrence (if applicable):
   
   IF IsRecurring:
     var jobId = BackgroundJob.Schedule<RecurringAssignmentJob>(
         x => x.CreateNextAssignment(assignmentId),
         nextOccurrenceDate
     );
     
     UPDATE FormTemplateAssignments
     SET RecurrenceJobId = jobId;

4. Audit Log:
   
   INSERT AuditLogs (
       EntityType = 'FormTemplateAssignment',
       Action = 'Created',
       Details = '{count} assignments created for {templateName}',
       UserId = CurrentUserId
   );

Success Response:
{
    "success": true,
    "assignmentCount": 47,
    "message": "47 assignments created successfully",
    "assignmentIds": [1, 2, 3, ...]
}

Redirect to: Assignment Dashboard with success message
```

---

## 3. Target Selection Component

**Reusable Component:** Multi-select with dynamic filtering

### Target Type: Specific Tenants

**Interface:**
```
Multi-select Dropdown:
  Data Source: SELECT * FROM Tenants WHERE IsActive=1
  Filtered by: User's RoleLevel
    - HeadOffice: See all tenants
    - Regional: See tenants in their region
    - Factory: See only their tenant (single-select)
  
  Display Format: TenantName (TenantType - Region)
  Example: "Kangaita Factory (Factory - Central)"
  
  Search: Full-text on TenantName, TenantCode
  
  Grouping: Group by TenantType or Region
```

**Query Logic:**
```sql
-- HeadOffice user
SELECT TenantId, TenantName, TenantType, RegionName
FROM Tenants t
INNER JOIN Regions r ON t.RegionId = r.RegionId
WHERE t.IsActive = 1
ORDER BY r.RegionName, t.TenantName

-- Regional user
SELECT TenantId, TenantName, TenantType, RegionName
FROM Tenants t
INNER JOIN Regions r ON t.RegionId = r.RegionId
WHERE t.IsActive = 1
  AND t.RegionId = @UserRegionId
ORDER BY t.TenantType, t.TenantName
```

### Target Type: Specific Users

**Interface:**
```
Multi-select Dropdown:
  Data Source: SELECT * FROM Users WHERE IsActive=1
  Filtered by: User's permissions
  
  Display Format: FirstName LastName (Email - Role)
  Example: "John Doe (john.doe@ktda.co.ke - ICT Officer)"
  
  Search: On name, email, role
  
  Grouping: Group by Role or Tenant
```

### Target Type: All Users in Role

**Interface:**
```
Single-select Dropdown:
  Data Source: SELECT * FROM Roles WHERE IsActive=1
  
  Display: RoleName (User Count)
  Example: "ICT Officer (23 users)"
  
  On Select: Show preview of affected users
  [View Users] button → Modal with list
```

**User Count Query:**
```sql
SELECT 
    r.RoleId,
    r.RoleName,
    COUNT(ur.UserId) as UserCount
FROM Roles r
LEFT JOIN UserRoles ur ON r.RoleId = ur.RoleId
LEFT JOIN Users u ON ur.UserId = u.UserId AND u.IsActive = 1
WHERE r.IsActive = 1
GROUP BY r.RoleId, r.RoleName
ORDER BY r.RoleName
```

### Target Type: All Tenants in Region

**Interface:**
```
Single-select Dropdown:
  Data Source: SELECT * FROM Regions WHERE IsActive=1
  
  Display: RegionName (Tenant Count)
  Example: "Central Region (47 tenants)"
  
  On Select: Show tenant types breakdown
  Preview: "47 tenants: 40 Factories, 5 Buying Centers, 2 Collection Points"
```

**Tenant Count Query:**
```sql
SELECT 
    r.RegionId,
    r.RegionName,
    COUNT(t.TenantId) as TenantCount,
    SUM(CASE WHEN t.TenantType = 'Factory' THEN 1 ELSE 0 END) as FactoryCount,
    SUM(CASE WHEN t.TenantType = 'BuyingCenter' THEN 1 ELSE 0 END) as BuyingCenterCount,
    SUM(CASE WHEN t.TenantType = 'CollectionPoint' THEN 1 ELSE 0 END) as CollectionPointCount
FROM Regions r
LEFT JOIN Tenants t ON r.RegionId = t.RegionId AND t.IsActive = 1
WHERE r.IsActive = 1
GROUP BY r.RegionId, r.RegionName
ORDER BY r.RegionName
```

### Target Type: All Tenants in Group

**Interface:**
```
Single-select Dropdown:
  Data Source: SELECT * FROM TenantGroups WHERE IsActive=1
  
  Display: GroupName (Tenant Count)
  Example: "High Performers (15 tenants)"
  
  On Select: Load group members
  [View Members] button → Modal with tenant list
```

**Group Members Query:**
```sql
SELECT 
    tg.GroupId,
    tg.GroupName,
    COUNT(tgm.TenantId) as TenantCount
FROM TenantGroups tg
LEFT JOIN TenantGroupMembers tgm ON tg.GroupId = tgm.GroupId AND tgm.IsActive = 1
WHERE tg.IsActive = 1
GROUP BY tg.GroupId, tg.GroupName
ORDER BY tg.GroupName
```

---

## 4. Recurrence Configuration

**Component Type:** Form with schedule builder

### Recurrence Patterns

**Monthly:**
```
Options:
- 1st of every month
- Last day of every month
- Day [1-31] of every month
- First [Monday/Tuesday/...] of every month
- Last [Monday/Tuesday/...] of every month

Example: "1st of every month" for Monthly reports
```

**Quarterly:**
```
Options:
- 1st of quarter (Jan 1, Apr 1, Jul 1, Oct 1)
- Last day of quarter (Mar 31, Jun 30, Sep 30, Dec 31)

Example: "1st of quarter" for Quarterly reports
```

**Annual:**
```
Options:
- 1st of year (January 1)
- Last day of year (December 31)
- Specific date (e.g., April 15 for tax reports)
```

**Custom:**
```
Repeat every: [X] [Days/Weeks/Months/Years]
Examples:
- Every 2 weeks
- Every 3 months
- Every 6 months
```

### Recurrence Logic (Background Job)

**Hangfire Job:**
```csharp
public class RecurringAssignmentJob
{
    public async Task CreateNextAssignment(int originalAssignmentId)
    {
        var originalAssignment = await GetAssignment(originalAssignmentId);
        
        // Calculate next occurrence
        var nextPeriod = CalculateNextPeriod(
            originalAssignment.ReportingPeriod,
            originalAssignment.RecurrencePattern
        );
        
        // Check if recurrence should continue
        if (ShouldContinueRecurrence(originalAssignment, nextPeriod))
        {
            // Create new assignment records
            var targets = GetTargets(originalAssignment);
            
            foreach (var target in targets)
            {
                await CreateAssignment(new {
                    TemplateId = originalAssignment.TemplateId,
                    AssignmentType = originalAssignment.AssignmentType,
                    TargetId = target.Id,
                    ReportingPeriod = nextPeriod,
                    DueDate = CalculateDueDate(nextPeriod, originalAssignment),
                    AssignedBy = originalAssignment.AssignedBy
                });
            }
            
            // Send notifications
            await SendAssignmentNotifications(targets);
            
            // Schedule next occurrence
            var nextOccurrence = CalculateNextOccurrence(nextPeriod, originalAssignment.RecurrencePattern);
            BackgroundJob.Schedule<RecurringAssignmentJob>(
                x => x.CreateNextAssignment(originalAssignmentId),
                nextOccurrence
            );
        }
    }
    
    DateTime CalculateNextPeriod(DateTime currentPeriod, string pattern)
    {
        return pattern switch
        {
            "Monthly" => currentPeriod.AddMonths(1),
            "Quarterly" => currentPeriod.AddMonths(3),
            "Annually" => currentPeriod.AddYears(1),
            _ => currentPeriod
        };
    }
    
    bool ShouldContinueRecurrence(Assignment assignment, DateTime nextPeriod)
    {
        if (assignment.RecurrenceEndDate.HasValue)
            return nextPeriod <= assignment.RecurrenceEndDate.Value;
        
        if (assignment.RecurrenceOccurrences.HasValue)
        {
            var completedOccurrences = CountCompletedOccurrences(assignment.AssignmentId);
            return completedOccurrences < assignment.RecurrenceOccurrences.Value;
        }
        
        return true; // Continue indefinitely
    }
}
```

---

## 5. Assignment Compliance Tracker

**Component Type:** Dashboard with charts and detailed breakdown

**Reusable Components:**
- StatCards
- Pie Chart
- Bar Chart (completion by region/tenant)
- DataTable (detailed view)

### Compliance Metrics

**Overall Compliance:**
```
Completed: 245 / 300 (81.7%)
In Progress: 30 / 300 (10%)
Pending: 15 / 300 (5%)
Overdue: 10 / 300 (3.3%)
```

**Compliance by Region:**
```
Bar Chart:
Central Region:   ████████████████████ 85% (40/47)
Eastern Region:   ███████████████░░░░░ 75% (30/40)
Western Region:   ██████████████████░░ 90% (27/30)
Northern Region:  ████████████░░░░░░░░ 60% (18/30)
```

**Compliance by Template:**
```
Monthly ICT Report:     ████████████████████ 95% (190/200)
Safety Compliance:      ███████████████░░░░░ 75% (30/40)
Quarterly Performance:  ██████████████░░░░░░ 70% (14/20)
```

**Trending:**
```
Line Chart (Last 6 months):
Completion rate trend:
June:     78%
July:     82%
August:   85%
September: 83%
October:  87%
November: 89% ↑ +2%
```

### Detailed Breakdown Table

**Columns:**
- Tenant/User Name
- Template Name
- Reporting Period
- Due Date
- Status
- Submitted Date (if submitted)
- Approved Date (if approved)
- Days to Complete (submission date - assignment date)
- Days Overdue (if overdue)

**Actions:**
- Send Reminder (for Pending/InProgress)
- View Submission (for Submitted/Approved)
- Cancel Assignment (for Pending)
- Extend Deadline (for any status)

---

## 6. Bulk Assignment Actions

**Component Type:** Batch operations toolbar

### Send Reminder

**Action:**
```
Select multiple assignments → [Send Reminder]

Modal:
┌─ Send Reminder ────────────────────────────┐
│ Send reminder to: 15 users                 │
│                                             │
│ Message (optional):                        │
│ [Please complete the assigned form...]     │
│                                             │
│ [Send Now] [Schedule for Later] [Cancel]   │
└─────────────────────────────────────────────┘

Logic:
1. Get unique list of users from selected assignments
2. Create notification for each user
3. Send email (if enabled)
4. Log action in AuditLogs
```

### Extend Deadline

**Action:**
```
Select multiple assignments → [Extend Deadline]

Modal:
┌─ Extend Deadline ──────────────────────────┐
│ Extending deadline for: 15 assignments     │
│                                             │
│ Extend by:                                  │
│ ○ [3] days                                 │
│ ○ [7] days                                 │
│ ○ [14] days                                │
│ ○ Custom: [__] days                        │
│                                             │
│ New due dates will be:                     │
│ Current: Nov 30, 2025 → New: Dec 7, 2025  │
│                                             │
│ [Extend] [Cancel]                          │
└─────────────────────────────────────────────┘

Logic:
UPDATE FormTemplateAssignments
SET DueDate = DATEADD(DAY, @ExtensionDays, DueDate),
    ModifiedBy = @CurrentUserId,
    ModifiedDate = GETUTCDATE()
WHERE AssignmentId IN @SelectedIds

Send notification to affected users
```

### Cancel Assignments

**Action:**
```
Select multiple assignments → [Cancel Assignments]

Confirmation:
┌─ Cancel Assignments ───────────────────────┐
│ ⚠️ Are you sure?                           │
│                                             │
│ This will cancel 15 assignments.           │
│                                             │
│ Assignments with submitted/approved        │
│ forms will NOT be cancelled.               │
│                                             │
│ Reason (optional):                         │
│ [Template no longer required...]           │
│                                             │
│ [Confirm Cancel] [Back]                    │
└─────────────────────────────────────────────┘

Logic:
UPDATE FormTemplateAssignments
SET IsActive = 0,
    CancelledBy = @CurrentUserId,
    CancelledDate = GETUTCDATE(),
    CancellationReason = @Reason
WHERE AssignmentId IN @SelectedIds
  AND AssignmentId NOT IN (
      SELECT DISTINCT AssignmentId
      FROM FormTemplateSubmissions
      WHERE Status IN ('Submitted', 'InApproval', 'Approved')
  )

Send cancellation notification to affected users
```

### Export to Excel

**Action:**
```
[Export to Excel] button

Generates Excel file with columns:
- Template Name
- Assignment Type
- Assigned To
- Reporting Period
- Due Date
- Status
- Completion %
- Submitted Date
- Approved Date
- Days to Complete
- Assigned By
- Assigned Date

Filename: FormAssignments_Report_2025-11-06.xlsx
```

---

## API Endpoints Summary

```
# Assignment Dashboard
GET    /api/assignments                      - List all assignments (with filters)
GET    /api/assignments/statistics           - Get dashboard statistics
GET    /api/assignments/compliance           - Get compliance metrics

# Assignment Creation
GET    /api/templates/published              - List published templates
POST   /api/assignments/create               - Create new assignment(s)
POST   /api/assignments/preview-targets      - Preview who will be assigned

# Assignment Management
GET    /api/assignments/{id}                 - Get assignment details
PUT    /api/assignments/{id}/extend          - Extend due date
DELETE /api/assignments/{id}                 - Cancel assignment
POST   /api/assignments/bulk/remind          - Send reminders
POST   /api/assignments/bulk/extend          - Bulk extend deadlines
POST   /api/assignments/bulk/cancel          - Bulk cancel

# Recurrence
POST   /api/assignments/{id}/recurrence      - Configure recurrence
DELETE /api/assignments/{id}/recurrence      - Stop recurrence

# Reports
GET    /api/assignments/export               - Export to Excel
GET    /api/assignments/compliance-report    - Detailed compliance report
```

---

## Validation Rules

**Assignment Creation:**
```
- Template must be Published
- Due Date must be > Assignment Date
- Due Date cannot be in the past
- At least one target must be selected
- Reporting Period cannot overlap with existing assignment for same target+template
- Recurrence end date must be > start date
- User must have permission: Assignments.Create
```

**Assignment Extension:**
```
- Extension days must be > 0
- Cannot extend completed assignments
- User must have permission: Assignments.Extend
```

**Assignment Cancellation:**
```
- Cannot cancel assignments with approved submissions
- User must have permission: Assignments.Cancel
- Cancellation reason required if > 5 assignments cancelled at once
```

---

**Implementation Complete:** Form Assignment module documented with all components and logic