# My Dashboard - Personalized User Dashboard (Information Requirements)

**Route:** `/Dashboard/Index` or `/Reports/MyDashboard`
**Purpose:** Personalized landing dashboard showing user context, pending tasks, alerts, and quick stats
**Users:** All authenticated users (content adapts to user role and assignments)
**Type:** Home screen / Command center after login

---

## Overview

**"My Dashboard"** is the user's **personalized command center** that aggregates information from across the entire system relevant to their role, tenant, and assignments.

**It answers:** *"What do I need to know and do today?"*

**Unlike** role-specific dashboards (Factory Dashboard, Regional Dashboard, Executive Dashboard), this is **user-centric** and shows:
- Who you are (context)
- What you need to do (pending tasks)
- What needs attention (alerts)
- How you're performing (quick stats)

---

## Information Categories (10 Categories)

This document defines ALL information that needs to be displayed. UI structure will be designed separately.

---

### Category 1: User Context & Profile

**Purpose:** Show user who they are in the system and their scope of access

**Information to Display:**

1. **User Identity**
   - Full Name (FirstName + LastName)
   - Job Title / Position
   - Profile Photo (if available)
   - User Code / Employee ID
   - Email Address

2. **Primary Tenant Assignment**
   - Tenant Name (e.g., "Kangaita Tea Factory")
   - Tenant Type (Factory / HeadOffice / Subsidiary)
   - Tenant Code

3. **Regional Assignment (if applicable)**
   - Region Name (e.g., "Mt. Kenya Region")
   - Region Code
   - Regional Manager Name

4. **Primary Role**
   - Role Name (e.g., "Regional Manager")
   - Role Code (e.g., "REGIONAL_MGR")
   - Role Description

5. **Additional Roles (if multi-role user)**
   - List of secondary roles
   - Example: User is both "ICT Officer" and "Form Approver"

6. **Department Assignment**
   - Department Name (e.g., "ICT Department")
   - Department Code
   - Department Head Name

**Data Source:**
```sql
SELECT
    u.UserId,
    u.FirstName,
    u.LastName,
    u.Email,
    u.UserCode,
    u.JobTitle,
    u.ProfilePhotoUrl,
    t.TenantId,
    t.TenantName,
    t.TenantType,
    t.TenantCode,
    r.RegionId,
    r.RegionName,
    r.RegionCode,
    manager.FirstName + ' ' + manager.LastName AS RegionalManagerName,
    d.DepartmentName,
    d.DepartmentCode,
    role.RoleName,
    role.RoleCode,
    role.Description AS RoleDescription
FROM Users u
LEFT JOIN Tenants t ON u.TenantId = t.TenantId
LEFT JOIN Regions r ON t.RegionId = r.RegionId
LEFT JOIN Users manager ON r.RegionalManagerUserId = manager.UserId
LEFT JOIN Departments d ON u.DepartmentId = d.DepartmentId
LEFT JOIN UserRoles ur ON u.UserId = ur.UserId AND ur.IsPrimaryRole = 1
LEFT JOIN Roles role ON ur.RoleId = role.RoleId
WHERE u.UserId = @CurrentUserId
```

**Refresh:** On page load (cache for session)

---

### Category 2: Multi-Tenant Access (if applicable)

**Purpose:** Show all tenants user has access to (via UserTenantAccess table)

**Information to Display:**

1. **Accessible Tenants Count**
   - Total number of tenants user can access
   - Example: "You have access to 12 factories"

2. **Tenant List (Top 5 + View All)**
   - Tenant Name
   - Access Level (View / Edit / Admin)
   - Assigned Date
   - Purpose (why they have access)

3. **Quick Tenant Switcher**
   - Dropdown to switch context to different tenant
   - Useful for Regional Managers viewing individual factories

**Data Source:**
```sql
SELECT
    uta.AccessId,
    t.TenantId,
    t.TenantName,
    t.TenantCode,
    t.TenantType,
    uta.AccessLevel,
    uta.AssignedDate,
    uta.Purpose,
    u_assigner.FirstName + ' ' + u_assigner.LastName AS AssignedByName
FROM UserTenantAccess uta
INNER JOIN Tenants t ON uta.TenantId = t.TenantId
LEFT JOIN Users u_assigner ON uta.AssignedBy = u_assigner.UserId
WHERE uta.UserId = @CurrentUserId
  AND uta.IsActive = 1
ORDER BY t.TenantName
```

**Conditional Display:** Only show if user has access to > 1 tenant

**Refresh:** On page load

---

### Category 3: Tenant Group Memberships

**Purpose:** Show which tenant groups the user's factory belongs to

**Information to Display:**

1. **Group Memberships (if any)**
   - Group Name (e.g., "High Performers Q4 2025")
   - Group Type (Region / Project / Performance / Custom)
   - Member Count (how many tenants in group)
   - Group Description

**Data Source:**
```sql
SELECT
    tg.TenantGroupId,
    tg.GroupName,
    tg.GroupCode,
    tg.GroupType,
    tg.Description,
    COUNT(tgm_all.GroupMemberId) AS MemberCount
FROM TenantGroupMembers tgm
INNER JOIN TenantGroups tg ON tgm.TenantGroupId = tg.TenantGroupId
LEFT JOIN TenantGroupMembers tgm_all ON tg.TenantGroupId = tgm_all.TenantGroupId
WHERE tgm.TenantId = @CurrentUserTenantId
  AND tg.IsActive = 1
GROUP BY tg.TenantGroupId, tg.GroupName, tg.GroupCode, tg.GroupType, tg.Description
```

**Conditional Display:** Only show if tenant belongs to any groups

**Refresh:** On page load

---

### Category 4: Pending Form Submissions

**Purpose:** Show forms assigned to user that need completion

**Information to Display:**

1. **Summary Counts**
   - Total pending forms
   - Overdue count (past deadline)
   - In progress (draft) count

2. **Form List (Top 5, ordered by due date)**
   - Template Name
   - Assigned Date
   - Due Date
   - Days until due / Days overdue
   - Status (Not Started / Draft / Overdue)
   - Progress percentage (if draft)
   - Quick Action button: Start / Continue

**Data Source:**
```sql
SELECT
    a.AssignmentId,
    t.TemplateId,
    t.TemplateName,
    t.Description,
    a.AssignedDate,
    a.DueDate,
    DATEDIFF(DAY, GETUTCDATE(), a.DueDate) AS DaysUntilDue,
    s.SubmissionId,
    s.Status AS SubmissionStatus,
    s.ModifiedDate AS LastWorkedOn,
    -- Calculate progress (responses filled / total required fields)
    (SELECT COUNT(*) FROM FormTemplateResponses r WHERE r.SubmissionId = s.SubmissionId) AS ResponsesCount,
    (SELECT COUNT(*) FROM FormTemplateItems i
     INNER JOIN FormTemplateSections sec ON i.SectionId = sec.SectionId
     WHERE sec.TemplateId = t.TemplateId AND i.IsRequired = 1) AS RequiredFieldsCount,
    CASE
        WHEN s.Status IS NULL THEN 'Not Started'
        WHEN s.Status = 'Draft' THEN 'In Progress'
        WHEN s.Status IN ('Submitted', 'InApproval') THEN 'Submitted'
        WHEN s.Status = 'Approved' THEN 'Approved'
        WHEN a.DueDate < GETUTCDATE() AND s.Status IS NULL THEN 'Overdue'
        ELSE s.Status
    END AS DisplayStatus
FROM FormTemplateAssignments a
INNER JOIN FormTemplates t ON a.TemplateId = t.TemplateId
LEFT JOIN FormTemplateSubmissions s
    ON s.TemplateId = t.TemplateId
    AND s.TenantId = a.TenantId
    AND s.ReportingPeriod = a.AssignedDate
WHERE a.IsActive = 1
  AND t.PublishStatus = 'Published'
  AND (
    (a.AssignmentType = 'Tenant' AND a.TenantId = @CurrentUserTenantId)
    OR
    (a.AssignmentType = 'User' AND a.UserId = @CurrentUserId)
    OR
    (a.AssignmentType = 'Role' AND EXISTS (
        SELECT 1 FROM UserRoles WHERE UserId = @CurrentUserId AND RoleId = a.RoleId
    ))
  )
  AND (s.Status IS NULL OR s.Status = 'Draft')  -- Only pending or draft
ORDER BY
    CASE WHEN a.DueDate < GETUTCDATE() THEN 0 ELSE 1 END,  -- Overdue first
    a.DueDate ASC
```

**Priority Indicators:**
- 🔴 Overdue (DueDate < today)
- 🟡 Due soon (DueDate within 3 days)
- 🟢 On track (DueDate > 3 days away)

**Refresh:** Every 5 minutes (or real-time via SignalR)

---

### Category 5: Pending Approvals

**Purpose:** Show form submissions awaiting user's approval (if user is an approver)

**Information to Display:**

1. **Summary Counts**
   - Total pending approvals
   - Overdue approvals (past SLA deadline)

2. **Approval List (Top 5, ordered by age)**
   - Submission ID
   - Template Name
   - Submitted By (Factory + User Name)
   - Submitted Date
   - Days pending
   - Current workflow step
   - SLA deadline (if configured)
   - Quick Action button: Review

**Data Source:**
```sql
SELECT
    s.SubmissionId,
    t.TemplateName,
    tenant.TenantName,
    submitter.FirstName + ' ' + submitter.LastName AS SubmittedBy,
    submitter.ProfilePhotoUrl AS SubmitterPhoto,
    s.SubmittedDate,
    DATEDIFF(DAY, s.SubmittedDate, GETUTCDATE()) AS DaysPending,
    wp.StepId,
    ws.StepName,
    ws.StepOrder,
    wp.DueDate,
    CASE
        WHEN wp.DueDate < GETUTCDATE() THEN 1
        ELSE 0
    END AS IsOverdue,
    DATEDIFF(DAY, GETUTCDATE(), wp.DueDate) AS DaysUntilDue
FROM FormTemplateSubmissions s
INNER JOIN FormTemplates t ON s.TemplateId = t.TemplateId
LEFT JOIN Tenants tenant ON s.TenantId = tenant.TenantId
INNER JOIN Users submitter ON s.SubmittedBy = submitter.UserId
INNER JOIN SubmissionWorkflowProgress wp ON s.SubmissionId = wp.SubmissionId
INNER JOIN WorkflowSteps ws ON wp.StepId = ws.StepId
WHERE s.Status IN ('Submitted', 'InApproval')
  AND wp.Status = 'Pending'
  AND (
    -- Role-based approver
    (ws.ApproverRoleId IS NOT NULL AND EXISTS (
        SELECT 1 FROM UserRoles WHERE UserId = @CurrentUserId AND RoleId = ws.ApproverRoleId
    ))
    OR
    -- User-specific approver
    (ws.ApproverUserId = @CurrentUserId)
    OR
    -- Delegated to current user
    (wp.DelegatedTo = @CurrentUserId)
  )
ORDER BY
    CASE WHEN wp.DueDate < GETUTCDATE() THEN 0 ELSE 1 END,  -- Overdue first
    s.SubmittedDate ASC  -- Oldest first
```

**Conditional Display:** Only show if user has approver role(s)

**Refresh:** Every 5 minutes (or real-time via SignalR)

---

### Category 6: Alerts & Notifications

**Purpose:** Show active alerts and unread notifications for the user

**Information to Display:**

1. **Unread Count by Type**
   - Total unread notifications
   - Forms (assignments, deadlines)
   - Approvals (pending your review)
   - Alerts (compliance, system)
   - System announcements

2. **High Priority Alerts (Top 5)**
   - Alert Type (Compliance / Deadline / System / Approval)
   - Priority (High / Medium / Low)
   - Title
   - Message preview
   - Related Entity (Factory, Form, Report)
   - Created Date (relative: "2 hours ago")
   - Quick Actions (View / Dismiss / Snooze)

**Data Source:**
```sql
SELECT
    n.NotificationId,
    n.NotificationType,
    n.Priority,
    n.Title,
    n.Message,
    n.CreatedDate,
    n.RelatedEntityType,
    n.RelatedEntityId,
    nr.IsRead,
    nr.ReadDate,
    nr.IsDismissed,
    -- Calculate relative time
    CASE
        WHEN DATEDIFF(MINUTE, n.CreatedDate, GETUTCDATE()) < 60
            THEN CAST(DATEDIFF(MINUTE, n.CreatedDate, GETUTCDATE()) AS NVARCHAR) + ' minutes ago'
        WHEN DATEDIFF(HOUR, n.CreatedDate, GETUTCDATE()) < 24
            THEN CAST(DATEDIFF(HOUR, n.CreatedDate, GETUTCDATE()) AS NVARCHAR) + ' hours ago'
        ELSE CAST(DATEDIFF(DAY, n.CreatedDate, GETUTCDATE()) AS NVARCHAR) + ' days ago'
    END AS RelativeTime
FROM Notifications n
INNER JOIN NotificationRecipients nr ON n.NotificationId = nr.NotificationId
WHERE nr.RecipientUserId = @CurrentUserId
  AND nr.IsRead = 0
  AND nr.IsDismissed = 0
  AND n.IsActive = 1
ORDER BY
    CASE n.Priority
        WHEN 'High' THEN 1
        WHEN 'Medium' THEN 2
        WHEN 'Low' THEN 3
    END,
    n.CreatedDate DESC
```

**Notification Types:**
- `FormAssigned` - New form assigned to you
- `FormSubmissionDue` - Form deadline approaching
- `ApprovalRequired` - Submission needs your approval
- `FormApproved` - Your submission was approved
- `FormRejected` - Your submission was rejected (with comments)
- `ComplianceAlert` - Compliance threshold breached
- `LicenseExpiry` - Software license expiring soon
- `SystemAlert` - System-wide announcement
- `ReportScheduled` - Scheduled report generated

**Refresh:** Real-time (SignalR push notifications)

---

### Category 7: Recent Activity

**Purpose:** Show user's recent actions in the system

**Information to Display:**

1. **Activity Timeline (Last 10 actions)**
   - Activity Type icon
   - Activity Description
   - Entity Name (clickable link)
   - Timestamp (relative: "2 hours ago")

**Data Source:**
```sql
SELECT TOP 10
    ual.ActivityLogId,
    ual.ActivityType,
    ual.EntityType,
    ual.EntityId,
    ual.EntityName,
    ual.Description,
    ual.ActivityDate,
    -- Calculate relative time
    CASE
        WHEN DATEDIFF(MINUTE, ual.ActivityDate, GETUTCDATE()) < 60
            THEN CAST(DATEDIFF(MINUTE, ual.ActivityDate, GETUTCDATE()) AS NVARCHAR) + ' min ago'
        WHEN DATEDIFF(HOUR, ual.ActivityDate, GETUTCDATE()) < 24
            THEN CAST(DATEDIFF(HOUR, ual.ActivityDate, GETUTCDATE()) AS NVARCHAR) + ' hours ago'
        WHEN DATEDIFF(DAY, ual.ActivityDate, GETUTCDATE()) < 7
            THEN CAST(DATEDIFF(DAY, ual.ActivityDate, GETUTCDATE()) AS NVARCHAR) + ' days ago'
        ELSE FORMAT(ual.ActivityDate, 'MMM dd, yyyy')
    END AS RelativeTime
FROM UserActivityLog ual
WHERE ual.UserId = @CurrentUserId
ORDER BY ual.ActivityDate DESC
```

**Activity Types & Icons:**
- `ReportViewed` → 📊
- `ReportExported` → 📥
- `FormSubmitted` → 📝
- `FormApproved` → ✅
- `FormRejected` → ❌
- `DashboardAccessed` → 📈
- `SettingsChanged` → ⚙️

**Refresh:** On page load

---

### Category 8: Quick Stats / User Performance KPIs

**Purpose:** Show user's performance metrics and contribution stats

**Information to Display:**

1. **Form Submission Stats (This Month)**
   - Forms assigned: Count
   - Forms submitted: Count
   - Submission rate: Percentage + Progress bar
   - Average submission time (days before deadline)

2. **Approval Stats (This Month, if approver)**
   - Approvals pending: Count
   - Approvals completed: Count
   - Average approval time: Days

3. **Factory/Regional Performance (if applicable)**
   - Overall compliance: Percentage
   - Hardware operational: Percentage
   - Software compliant: Percentage
   - Trend indicator (improving/declining)

**Data Sources:**

**Form Submission Stats:**
```sql
DECLARE @CurrentMonth DATE = DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1);

SELECT
    COUNT(DISTINCT a.AssignmentId) AS FormsAssigned,
    COUNT(DISTINCT s.SubmissionId) AS FormsSubmitted,
    CAST(COUNT(DISTINCT s.SubmissionId) AS FLOAT) / NULLIF(COUNT(DISTINCT a.AssignmentId), 0) * 100 AS SubmissionRate,
    AVG(CASE
        WHEN s.SubmittedDate IS NOT NULL
        THEN DATEDIFF(DAY, s.SubmittedDate, a.DueDate)
        ELSE NULL
    END) AS AvgDaysBeforeDeadline
FROM FormTemplateAssignments a
LEFT JOIN FormTemplateSubmissions s
    ON s.TemplateId = a.TemplateId
    AND s.TenantId = a.TenantId
    AND s.ReportingPeriod = a.AssignedDate
WHERE a.AssignedDate >= @CurrentMonth
  AND (
    (a.AssignmentType = 'Tenant' AND a.TenantId = @CurrentUserTenantId)
    OR
    (a.AssignmentType = 'User' AND a.UserId = @CurrentUserId)
  )
```

**Approval Stats:**
```sql
SELECT
    COUNT(CASE WHEN wp.Status = 'Pending' THEN 1 END) AS PendingApprovals,
    COUNT(CASE WHEN wp.Status = 'Approved' AND wp.ReviewedDate >= @CurrentMonth THEN 1 END) AS CompletedApprovals,
    AVG(CASE
        WHEN wp.Status = 'Approved' AND wp.ReviewedDate >= @CurrentMonth
        THEN DATEDIFF(DAY, s.SubmittedDate, wp.ReviewedDate)
        ELSE NULL
    END) AS AvgApprovalTimeDays
FROM SubmissionWorkflowProgress wp
INNER JOIN FormTemplateSubmissions s ON wp.SubmissionId = s.SubmissionId
INNER JOIN WorkflowSteps ws ON wp.StepId = ws.StepId
WHERE (
    (ws.ApproverUserId = @CurrentUserId)
    OR
    (ws.ApproverRoleId IN (SELECT RoleId FROM UserRoles WHERE UserId = @CurrentUserId))
  )
```

**Performance Stats:**
```sql
SELECT
    tm.MetricId,
    md.MetricName,
    tm.NumericValue,
    md.ThresholdGreen,
    md.ThresholdYellow,
    CASE
        WHEN tm.NumericValue >= md.ThresholdGreen THEN 'Good'
        WHEN tm.NumericValue >= md.ThresholdYellow THEN 'Warning'
        ELSE 'Critical'
    END AS Status
FROM TenantMetrics tm
INNER JOIN MetricDefinitions md ON tm.MetricId = md.MetricId
WHERE tm.TenantId = @CurrentUserTenantId
  AND tm.ReportingPeriod = @CurrentMonth
  AND md.MetricCode IN ('OVERALL_COMPLIANCE', 'HARDWARE_OPERATIONAL_PCT', 'SOFTWARE_COMPLIANCE_PCT')
```

**Refresh:** Cache for 10 minutes

---

### Category 9: Scheduled Reports (User-Created)

**Purpose:** Show reports user has scheduled for automatic delivery

**Information to Display:**

1. **Active Schedules Count**

2. **Schedule List (Top 3)**
   - Report Name
   - Frequency (Daily / Weekly / Monthly / Quarterly)
   - Next Run Date/Time
   - Delivery Method (Email / Dashboard / Both)
   - Recipients count
   - Last Run Status (Success / Failed)
   - Quick Actions: Edit / Pause / Run Now / Delete

**Data Source:**
```sql
SELECT
    rs.ScheduleId,
    rd.ReportName,
    rs.Frequency,
    rs.NextRunDate,
    rs.DeliveryMethod,
    rs.IsActive,
    rs.LastRunDate,
    rs.LastRunStatus,
    rs.LastRunErrorMessage,
    (SELECT COUNT(*) FROM STRING_SPLIT(rs.EmailRecipients, ';')) AS RecipientCount
FROM ReportSchedules rs
INNER JOIN ReportDefinitions rd ON rs.ReportId = rd.ReportId
WHERE rs.CreatedBy = @CurrentUserId
  AND rs.IsActive = 1
ORDER BY rs.NextRunDate ASC
```

**Conditional Display:** Only show if user has created schedules

**Refresh:** On page load

---

### Category 10: Quick Access Links

**Purpose:** Provide shortcuts to frequently used features (role-specific)

**Information to Display:**

1. **Based on User Role (6-8 links max)**

**Factory ICT Officer:**
- 📝 Submit Monthly Checklist
- 📊 Factory Dashboard
- 💻 Hardware Inventory
- 📀 Software Licenses
- 📋 My Form Submissions
- ⚙️ Settings

**Regional Manager:**
- 📊 Regional Dashboard
- ✅ Pending Approvals (with count badge)
- 📈 Factory Comparison
- 📅 Schedule Report
- 👥 User Management (region)
- ⚙️ Settings

**Head Office ICT Manager:**
- 📊 Executive Dashboard
- 📑 All Reports
- 👥 User Management (all)
- 🏭 Tenant Management
- ⚙️ System Settings
- 📊 Metrics Configuration

**Data Source:**
```csharp
// Hardcoded based on role OR configured in database
// Table: RoleQuickLinks
// Columns: RoleId, LinkTitle, LinkIcon, LinkRoute, DisplayOrder
```

**Refresh:** Static (loaded once per session)

---

## Complete Data Summary Table

| # | Category | Tables Involved | Query Complexity | Display Condition | Refresh |
|---|----------|-----------------|------------------|-------------------|---------|
| 1 | User Context | Users, Tenants, Regions, Departments, Roles | Medium JOIN | Always | On load, cache 1h |
| 2 | Multi-Tenant Access | UserTenantAccess, Tenants | Simple JOIN | If access > 1 tenant | On load |
| 3 | Tenant Groups | TenantGroups, TenantGroupMembers | Simple JOIN + GROUP BY | If belongs to groups | On load |
| 4 | Pending Forms | FormTemplateAssignments, FormTemplateSubmissions | Complex JOIN + CASE | Always | Every 5 min or SignalR |
| 5 | Pending Approvals | SubmissionWorkflowProgress, WorkflowSteps | Complex JOIN + EXISTS | If is approver | Every 5 min or SignalR |
| 6 | Alerts | Notifications, NotificationRecipients | Simple JOIN | Always | Real-time (SignalR) |
| 7 | Recent Activity | UserActivityLog | Simple SELECT TOP 10 | Always | On load |
| 8 | Quick Stats | FormTemplateAssignments, TenantMetrics | Medium aggregation | Always | Cache 10 min |
| 9 | Scheduled Reports | ReportSchedules, ReportDefinitions | Simple JOIN | If has schedules | On load |
| 10 | Quick Access | Static config or RoleQuickLinks | N/A | Always | Static |

---

## Priority Levels

### MUST HAVE (Always Visible)

1. ✅ **User Context** - Who am I?
2. ✅ **Pending Forms** - What do I need to submit?
3. ✅ **Alerts** - What requires attention?
4. ✅ **Quick Stats** - How am I doing?

### SHOULD HAVE (Conditional - High Value)

5. ✅ **Pending Approvals** - (If user is approver)
6. ✅ **Multi-Tenant Access** - (If user has access to > 1 tenant)
7. ✅ **Quick Access Links** - Shortcuts

### NICE TO HAVE (Optional - Medium Value)

8. ⚪ **Recent Activity** - Historical context
9. ⚪ **Scheduled Reports** - (If user has created schedules)
10. ⚪ **Tenant Groups** - (If tenant belongs to groups)

---

## Performance Requirements

### Page Load Time Target
- **< 500ms** for initial page render
- **< 1 second** for all data loaded

### Caching Strategy
```csharp
// User context - cache for session (1 hour)
_cache.Set($"UserContext:{userId}", userContext, TimeSpan.FromHours(1));

// Pending forms/approvals - cache for 5 minutes
_cache.Set($"PendingForms:{userId}", pendingForms, TimeSpan.FromMinutes(5));

// Quick stats - cache for 10 minutes
_cache.Set($"QuickStats:{userId}:{month}", stats, TimeSpan.FromMinutes(10));

// Alerts - NO cache (real-time via SignalR)
```

### Query Optimization
```sql
-- Use single query with CTEs instead of 10 separate queries
WITH UserContext AS (
    SELECT u.UserId, u.FirstName, t.TenantName, r.RoleName ...
),
PendingForms AS (
    SELECT COUNT(*) AS PendingCount, COUNT(CASE WHEN DueDate < GETUTCDATE() THEN 1 END) AS OverdueCount ...
),
PendingApprovals AS (
    SELECT COUNT(*) AS ApprovalCount ...
),
Alerts AS (
    SELECT COUNT(*) AS UnreadCount ...
),
QuickStats AS (
    SELECT ...
)
SELECT * FROM UserContext, PendingForms, PendingApprovals, Alerts, QuickStats
```

---

## Next Steps

**After defining all information requirements, we need:**

1. ✅ **UI Layout Design** - How to arrange 10 categories on screen
   - Grid structure (responsive: desktop/tablet/mobile)
   - Card arrangement and sizing
   - Visual hierarchy (what's prominent vs secondary)

2. **Component Selection** - Which Velzon components to use
   - Cards with headers
   - List groups
   - Progress bars
   - Badges
   - Timeline components

3. **Interactive Elements** - User actions
   - Click handlers (navigate to forms, approvals, etc.)
   - Real-time updates (SignalR connection)
   - Refresh buttons
   - Filters (date range for stats)

4. **Color Coding & Icons**
   - Red = Urgent/Overdue
   - Yellow = Warning/Due Soon
   - Green = Good/On Track
   - Icons for each category

---

**Status:** ✅ Information Requirements Defined (All 10 Categories)
**Next Document:** Design UI layout structure with Velzon components
**File Name:** `1A_MyDashboard_UI_Layout.md` (to be created next)
