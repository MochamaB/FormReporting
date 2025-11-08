# Form Submission: Assigned Forms Dashboard

**Purpose:** Display forms assigned to current user/tenant and track submission status  
**Users:** All users (Factory ICT Officers, Regional Staff, Head Office Staff)  
**Access:** Based on user's assignments

---

## Component Overview

**Component Type:** Dashboard with StatCards + DataTable + Filters

**Reusable Components:**
- StatCards (4 cards)
- DataTable with status badges
- Filters
- Modal (preview template)

---

## 1. My Assignments Dashboard

### Navigation

**Main Menu Location:**
```
Form Management
  ├─ My Assignments ← User's landing page
  │   └─ Shows forms assigned to me
  ├─ Form Templates (if has permission)
  └─ Form Assignments (if has permission)
```

**Role-Based Access:**
```
Factory User: See only assignments for their TenantId
Regional User: See assignments for their region's tenants
Head Office User: See all assignments (if they have assignments)

Query Filter:
WHERE (AssignmentType = 'User' AND UserId = @CurrentUserId)
   OR (AssignmentType = 'Tenant' AND TenantId = @CurrentUserTenantId)
   OR (AssignmentType = 'Role' AND RoleId IN @CurrentUserRoleIds)
```

---

### Statistics Cards

**Card 1: Pending Forms**
```
Calculation: COUNT(Status IN ('Pending', 'Draft') AND DueDate >= TODAY)
Color: Info
Icon: ri-file-list-line
Action: Click → Filter table to Pending
Description: "Forms not yet started"
```

**Card 2: In Progress**
```
Calculation: COUNT(Status = 'Draft' AND ResponseCount > 0)
Color: Warning
Icon: ri-edit-line
Action: Click → Filter table to In Progress
Description: "Forms with saved drafts"
```

**Card 3: Due Soon**
```
Calculation: COUNT(DueDate BETWEEN TODAY AND (TODAY + 7 days) AND Status != 'Submitted')
Color: Danger
Icon: ri-time-line
Action: Click → Filter table to Due Soon
Description: "Due in next 7 days"
```

**Card 4: Completed**
```
Calculation: COUNT(Status IN ('Submitted', 'InApproval', 'Approved'))
Color: Success
Icon: ri-checkbox-circle-line
Action: Click → Filter table to Completed
Description: "Submitted this month"
```

---

### DataTable Configuration

**Columns:**

1. **Template Name**
   - Display: Template name with template type badge
   - Example: "Factory Monthly ICT Report" [Monthly]
   - Sortable: Yes
   - Searchable: Yes

2. **Reporting Period**
   - Format: "November 2025" (Monthly), "Q4 2025" (Quarterly), "2025" (Annual)
   - Sortable: Yes
   - Default Sort: DESC (most recent first)

3. **Assigned Date**
   - Format: "Nov 1, 2025"
   - Sortable: Yes

4. **Due Date**
   - Format: "Nov 30, 2025"
   - Color Coding:
     - Red: Overdue (< TODAY and not submitted)
     - Orange: Due soon (< 7 days)
     - Green: On track (> 7 days)
   - Sortable: Yes

5. **Status** (Badge with color)
   - **Pending** (gray) - Not started
   - **Draft** (blue) - Saved progress
   - **Submitted** (green) - Submitted for approval
   - **InApproval** (yellow) - Being reviewed
   - **Approved** (success) - Approved by all
   - **Rejected** (red) - Sent back for revision
   - **Overdue** (danger) - Past due date

6. **Progress**
   - Display: Progress bar + percentage
   - Calculation: (Fields with responses / Total required fields) * 100
   - Example: "15/33 (45%)" with progress bar
   - Only shown for Draft status

7. **Actions**
   - **Start** (Pending only)
     - Button: [Start Form]
     - Action: Navigate to Form Renderer
   
   - **Continue** (Draft only)
     - Button: [Continue]
     - Action: Resume from saved progress
   
   - **View** (Submitted/InApproval/Approved)
     - Button: [View Submission]
     - Action: View read-only submitted data
   
   - **Edit** (Rejected only)
     - Button: [Revise & Resubmit]
     - Action: Open form with previous responses, show rejection notes
   
   - **Preview Template**
     - Icon: [👁️]
     - Action: Open template preview modal

---

### Status Logic

```csharp
public string GetSubmissionStatus(FormTemplateAssignment assignment)
{
    var submission = GetSubmission(assignment.AssignmentId);
    
    if (submission == null)
    {
        // No submission record exists
        if (assignment.DueDate < DateTime.Now)
            return "Overdue";
        return "Pending";
    }
    
    // Submission exists, check its status
    return submission.Status switch
    {
        "Draft" => assignment.DueDate < DateTime.Now ? "Overdue" : "Draft",
        "Submitted" => "Submitted",
        "InApproval" => "InApproval",
        "Approved" => "Approved",
        "Rejected" => "Rejected",
        _ => "Pending"
    };
}

decimal GetProgressPercentage(int submissionId)
{
    var submission = GetSubmission(submissionId);
    var template = GetTemplate(submission.TemplateId);
    
    var totalRequiredFields = template.Items.Count(i => i.IsRequired);
    var completedFields = submission.Responses.Count(r => !string.IsNullOrEmpty(r.ResponseValue));
    
    if (totalRequiredFields == 0) return 100;
    
    return (decimal)completedFields / totalRequiredFields * 100;
}
```

---

### Filters

**Status Filter:**
```
Dropdown: All, Pending, Draft, Submitted, InApproval, Approved, Rejected, Overdue
Default: All
```

**Template Filter:**
```
Dropdown: All templates assigned to user
Display: Template Name
Multi-select: Yes
```

**Reporting Period Filter:**
```
Date Range: From [Month/Year] To [Month/Year]
Default: Current year
```

**Due Date Filter:**
```
Options:
- All
- Due Today
- Due This Week
- Due This Month
- Overdue

Custom Range: From [Date] To [Date]
```

**Search Box:**
```
Full-text search on:
- Template Name
- Template Description
- Reporting Period
```

---

### Notifications & Alerts

**Due Soon Alert:**
```
Show at top of page if forms due within 3 days:

┌─ ⚠️ Urgent: Forms Due Soon ────────────────────────────┐
│                                                         │
│ You have 3 forms due in the next 3 days:              │
│                                                         │
│ • Factory Monthly Report - Due Nov 30 (2 days)        │
│ • Safety Checklist - Due Dec 1 (3 days)               │
│ • Network Audit - Due Nov 29 (1 day) ⚠️ URGENT         │
│                                                         │
│ [View All] [Dismiss]                                   │
└─────────────────────────────────────────────────────────┘
```

**Overdue Alert:**
```
Show prominently if overdue forms exist:

┌─ 🔴 Overdue Forms ──────────────────────────────────────┐
│                                                         │
│ You have 2 overdue forms:                              │
│                                                         │
│ • Hardware Inventory - Was due Nov 25 (5 days ago)    │
│ • Monthly Performance - Was due Nov 20 (10 days ago)  │
│                                                         │
│ [Start Now] [Contact Support]                          │
└─────────────────────────────────────────────────────────┘
```

---

### Recurring Assignment Indicator

**For recurring assignments, show next occurrence:**
```
Template row with recurring indicator:

Factory Monthly ICT Report
[Monthly] [🔄 Recurring]
Next due: December 31, 2025
```

---

### Empty State

**When no assignments:**
```
┌─────────────────────────────────────────────────────────┐
│                                                         │
│                     📋                                  │
│                                                         │
│           No Forms Assigned Yet                        │
│                                                         │
│   You currently have no form assignments.              │
│   Check back later or contact your manager.            │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

### Quick Actions Panel

**Sidebar or top panel:**
```
┌─ Quick Actions ─────────────────┐
│                                  │
│ [📝 Start New Form]              │
│   → Shows pending assignments    │
│                                  │
│ [💾 Continue Draft]              │
│   → Resume saved forms           │
│                                  │
│ [📊 View My Submissions]         │
│   → History of completed forms   │
│                                  │
│ [❓ Help & Guidelines]           │
│   → Instructions for filling     │
│                                  │
└──────────────────────────────────┘
```

---

### Mobile Responsive Considerations

**Card View on Mobile:**
```
Each assignment displayed as card:

┌─────────────────────────────────────┐
│ Factory Monthly ICT Report          │
│ [Monthly] [Draft]                   │
│                                     │
│ Period: November 2025               │
│ Due: Nov 30, 2025 (2 days left)    │
│                                     │
│ Progress: ████░░░░░░ 45% (15/33)   │
│                                     │
│ [Continue Form →]                   │
└─────────────────────────────────────┘
```

---

## API Endpoints

```
GET    /api/my-assignments                    - List all assignments for current user
GET    /api/my-assignments/statistics         - Get dashboard statistics
GET    /api/my-assignments/{id}               - Get assignment details
GET    /api/my-assignments/{id}/progress      - Get completion progress
GET    /api/submissions/{id}                  - Get submission data (if exists)
POST   /api/submissions/start                 - Start new submission
```

---

## Database Query Examples

**Get User's Assignments:**
```sql
SELECT 
    a.AssignmentId,
    a.TemplateId,
    t.TemplateName,
    t.TemplateType,
    a.ReportingPeriod,
    a.DueDate,
    a.AssignedDate,
    s.SubmissionId,
    s.Status as SubmissionStatus,
    s.LastSavedDate,
    CASE 
        WHEN s.SubmissionId IS NULL AND a.DueDate < GETUTCDATE() THEN 'Overdue'
        WHEN s.SubmissionId IS NULL THEN 'Pending'
        ELSE s.Status
    END as DisplayStatus
FROM FormTemplateAssignments a
INNER JOIN FormTemplates t ON a.TemplateId = t.TemplateId
LEFT JOIN FormTemplateSubmissions s ON a.AssignmentId = s.AssignmentId
WHERE a.IsActive = 1
  AND (
      (a.AssignmentType = 'User' AND a.UserId = @CurrentUserId)
      OR (a.AssignmentType = 'Tenant' AND a.TenantId = @CurrentUserTenantId)
      OR (a.AssignmentType = 'Role' AND a.RoleId IN (
          SELECT RoleId FROM UserRoles WHERE UserId = @CurrentUserId
      ))
  )
ORDER BY 
    CASE 
        WHEN a.DueDate < GETUTCDATE() THEN 0
        WHEN DATEDIFF(DAY, GETUTCDATE(), a.DueDate) <= 7 THEN 1
        ELSE 2
    END,
    a.DueDate ASC
```

**Get Progress Statistics:**
```sql
SELECT 
    COUNT(*) as TotalAssignments,
    SUM(CASE WHEN DisplayStatus = 'Pending' THEN 1 ELSE 0 END) as PendingCount,
    SUM(CASE WHEN DisplayStatus = 'Draft' THEN 1 ELSE 0 END) as DraftCount,
    SUM(CASE WHEN DisplayStatus IN ('Submitted', 'InApproval', 'Approved') THEN 1 ELSE 0 END) as CompletedCount,
    SUM(CASE WHEN DisplayStatus = 'Overdue' THEN 1 ELSE 0 END) as OverdueCount,
    SUM(CASE WHEN DATEDIFF(DAY, GETUTCDATE(), a.DueDate) BETWEEN 0 AND 7 THEN 1 ELSE 0 END) as DueSoonCount
FROM (
    -- Same query as above
) AS AssignmentSummary
```

---

## User Experience Enhancements

**Auto-Redirect for Single Assignment:**
```
IF user has only 1 pending assignment on login:
  → Show notification: "You have 1 form to complete"
  → Provide quick button: [Start Now]
  → Auto-redirect after 3 seconds (with cancel option)
```

**Daily Email Digest:**
```
Send daily at 8 AM (configurable):

Subject: Daily Form Assignment Summary

Body:
"Good morning [User Name],

You have:
- 2 forms due today
- 3 forms due this week
- 1 overdue form (requires immediate attention)

[View All Assignments]"
```

**Browser Notifications:**
```
Request permission on first visit:
"Enable notifications to get reminders about due forms"

Send notifications:
- 3 days before due date
- 1 day before due date
- On due date
- When form is rejected (needs revision)
```

---

**Next:** Dynamic Form Renderer + Wizard + Auto-save (File 4B)
