# Reports Landing Page - Main Dashboard

**Route:** `/Reports/Index` or `/Reports/Dashboard`
**Purpose:** Central hub for all reporting activities - view dashboards, access reports, create custom reports, manage schedules
**Users:** All authenticated users (content varies by role)
**Prerequisites:** Read `0_Section10_Overview_Workflows.md`

---

## Page Overview

The Reports Landing Page is the **main entry point** for all reporting features. It adapts based on user role and provides quick access to:

1. **My Dashboards** - Role-specific performance dashboards
2. **Report Catalog** - Browse and access all available reports
3. **Quick Actions** - Common tasks (export, schedule, create)
4. **Recent Activity** - Recently viewed reports and exports
5. **Scheduled Reports** - Manage automated report delivery
6. **Favorites** - Bookmarked reports for quick access
7. **Alerts & Notifications** - Report-related alerts

---

## UI Layout (Role: Regional Manager)

```
┌────────────────────────────────────────────────────────────────────────────┐
│ KTDA ICT Reporting System              👤 John Kamau (Regional Manager)   │
├────────────────────────────────────────────────────────────────────────────┤
│ 🏠 Home  📋 Forms  📊 Reports  💾 Inventory  ⚙️ Settings  🔔 Notifications │
└────────────────────────────────────────────────────────────────────────────┘

╔════════════════════════════════════════════════════════════════════════════╗
║ REPORTS & ANALYTICS                                          Last Updated: ║
║                                                              30 Oct 2025    ║
╠════════════════════════════════════════════════════════════════════════════╣
║                                                                            ║
║ ┌──────────────────────────────────────────────────────────────────────┐   ║
║ │ 📊 MY DASHBOARDS                                    [View All →]     │   ║
║ ├──────────────────────────────────────────────────────────────────────┤   ║
║ │                                                                      │   ║
║ │ ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐      │   ║
║ │ │ 🏭 Regional     │  │ 📈 Performance  │  │ ✅ Compliance   │      │   ║
║ │ │    Dashboard    │  │    Trends       │  │    Scorecard    │      │   ║
║ │ │                 │  │                 │  │                 │      │   ║
║ │ │ 12 Factories    │  │ 6-Month View    │  │ 96% Compliant   │      │   ║
║ │ │ 🟢 11 OK        │  │ ↗ Improving     │  │ 🟢 On Target    │      │   ║
║ │ │ 🔴 1 Alert      │  │                 │  │                 │      │   ║
║ │ │                 │  │                 │  │                 │      │   ║
║ │ │ [Open →]        │  │ [Open →]        │  │ [Open →]        │      │   ║
║ │ └─────────────────┘  └─────────────────┘  └─────────────────┘      │   ║
║ │                                                                      │   ║
║ └──────────────────────────────────────────────────────────────────────┘   ║
║                                                                            ║
║ ┌─────────────────────────────┐  ┌─────────────────────────────────────┐  ║
║ │ 📑 REPORT CATALOG           │  │ ⚡ QUICK ACTIONS                     │  ║
║ ├─────────────────────────────┤  ├─────────────────────────────────────┤  ║
║ │                             │  │                                     │  ║
║ │ Browse Reports:             │  │ 🔍 [Create Custom Report]           │  ║
║ │ • All Reports (42)          │  │                                     │  ║
║ │ • Operational (15)          │  │ 📅 [Schedule a Report]              │  ║
║ │ • Executive (8)             │  │                                     │  ║
║ │ • Compliance (12)           │  │ 📤 [Export Dashboard to Excel]      │  ║
║ │ • Custom (7)                │  │                                     │  ║
║ │                             │  │ 📧 [Email Report to Team]           │  ║
║ │ [Browse All →]              │  │                                     │  ║
║ │                             │  │ 🔄 [Refresh All Data]               │  ║
║ │ ⭐ Favorites (3):           │  │                                     │  ║
║ │ • Regional Performance      │  └─────────────────────────────────────┘  ║
║ │ • Hardware Summary          │                                          ║
║ │ • Software Compliance       │  ┌─────────────────────────────────────┐  ║
║ │                             │  │ ⏰ SCHEDULED REPORTS                 │  ║
║ └─────────────────────────────┘  ├─────────────────────────────────────┤  ║
║                                  │                                     │  ║
║ ┌─────────────────────────────┐  │ Active Schedules: 2                 │  ║
║ │ 🕐 RECENT ACTIVITY          │  │                                     │  ║
║ ├─────────────────────────────┤  │ • Regional Performance              │  ║
║ │                             │  │   📅 Monthly (1st, 07:00 AM)        │  ║
║ │ 📄 Regional Performance     │  │   📧 3 recipients                   │  ║
║ │    Viewed 2 hours ago       │  │   ✅ Last run: Success (1 Nov)      │  ║
║ │                             │  │                                     │  ║
║ │ 📥 Hardware Summary.xlsx    │  │ • Software Compliance               │  ║
║ │    Exported yesterday       │  │   📅 Quarterly (1st, 08:00 AM)      │  ║
║ │                             │  │   📧 5 recipients                   │  ║
║ │ 📄 Compliance Scorecard     │  │   🟡 Next run: 1 Jan 2026           │  ║
║ │    Viewed 3 days ago        │  │                                     │  ║
║ │                             │  │ [Manage Schedules →]                │  ║
║ │ [View All →]                │  │                                     │  ║
║ │                             │  └─────────────────────────────────────┘  ║
║ └─────────────────────────────┘                                          ║
║                                                                            ║
║ ┌──────────────────────────────────────────────────────────────────────┐   ║
║ │ 🔔 ALERTS & NOTIFICATIONS                          [View All →]      │   ║
║ ├──────────────────────────────────────────────────────────────────────┤   ║
║ │                                                                      │   ║
║ │ 🔴 HIGH: Kariara Factory - 85% software compliance (below target)   │   ║
║ │    Action needed: Review unlicensed software installations          │   ║
║ │    [View Report →] [Dismiss]                                        │   ║
║ │                                                                      │   ║
║ │ 🟡 MEDIUM: 1 factory overdue on monthly submission                  │   ║
║ │    Tetu Factory - Due 3 days ago                                    │   ║
║ │    [Send Reminder] [View Details]                                   │   ║
║ │                                                                      │   ║
║ └──────────────────────────────────────────────────────────────────────┘   ║
║                                                                            ║
╚════════════════════════════════════════════════════════════════════════════╝
```

---

## Page Sections Breakdown

### 1. MY DASHBOARDS (Top Section)

**Purpose:** Quick access to role-specific dashboards

**Content (varies by role):**

**Factory ICT Officer sees:**
- Factory Dashboard (single factory view)
- My Submissions History
- Hardware/Software Status

**Regional Manager sees:**
- Regional Dashboard (12 factories)
- Performance Trends (regional)
- Compliance Scorecard (regional)

**Head Office Analyst sees:**
- Executive Dashboard (67 factories, 6 regions)
- National Trends
- Cross-Regional Comparison
- KPI Tracking

**UI Elements:**
- Dashboard cards (3-4 visible, scroll for more)
- Each card shows: Icon, Title, Summary stats, Status indicator, Open button

**Interactions:**
- Click card → Navigate to full dashboard
- Hover → Show tooltip with last updated time
- Right-click → Options (Favorite, Share, Export)

---

### 2. REPORT CATALOG (Left Middle)

**Purpose:** Browse and access all available reports

**Categories:**
```
📊 All Reports (42 total)
  ├─ 🔧 Operational (15)
  │   ├─ Daily Hardware Status
  │   ├─ Weekly Form Submission Summary
  │   └─ Current Software License Usage
  │
  ├─ 📈 Executive (8)
  │   ├─ Regional Performance Summary
  │   ├─ Top 10 / Bottom 10 Rankings
  │   └─ Quarterly Compliance Scorecard
  │
  ├─ ✅ Compliance (12)
  │   ├─ Software License Compliance
  │   ├─ Hardware Asset Audit
  │   └─ Form Submission Compliance
  │
  └─ 🔍 Custom (7)
      └─ User-created reports
```

**Favorites Section:**
- Star icon to favorite reports
- Quick access to frequently used reports
- Synced across devices (stored in UserPreferences)

**Interactions:**
- Click category → Expand/collapse list
- Click report name → Open report viewer
- Star icon → Add/remove from favorites
- Right-click → Options (Run, Schedule, Edit, Delete)

---

### 3. QUICK ACTIONS (Right Middle)

**Purpose:** Common tasks without navigating away

**Actions:**

**🔍 Create Custom Report**
- Opens Report Builder (document 3A)
- Permissions: `Reports.CreateCustomReport` or higher

**📅 Schedule a Report**
- Opens Schedule Manager (document 4A)
- Permissions: `Reports.ScheduleReports`

**📤 Export Dashboard to Excel**
- Exports current dashboard view
- Downloads immediately (< 5 seconds)

**📧 Email Report to Team**
- Opens email dialog
- Pre-fills current dashboard as attachment

**🔄 Refresh All Data**
- Invalidates cache
- Regenerates current snapshot
- Shows progress indicator

**Button States:**
- Enabled (blue) - Action available
- Disabled (gray) - Missing permission or no data
- Loading (spinner) - Action in progress

---

### 4. RECENT ACTIVITY (Left Bottom)

**Purpose:** Quick access to recently used reports

**Displays:**
- Last 5 viewed reports (with timestamp)
- Last 3 exported files (with download link)
- Click to re-open report with same filters

**Data Stored:**
```sql
UserActivityLog table:
- UserId
- ActivityType ('ReportViewed', 'ReportExported')
- EntityId (ReportId)
- ActivityDate
- Details (JSON: filters, parameters)
```

**Interactions:**
- Click report name → Open with previous filters
- Click export → Re-download file (if < 7 days old)
- Click "View All" → Full activity history page

---

### 5. SCHEDULED REPORTS (Right Bottom)

**Purpose:** Manage automated report delivery

**Displays:**
- Active schedules count
- Next 2 upcoming scheduled reports
- Last run status (Success/Failed)
- Next run date/time

**Status Indicators:**
- ✅ Last run: Success (green)
- ❌ Last run: Failed (red, click to see error)
- 🟡 Next run: Upcoming (yellow if < 24 hours)

**Interactions:**
- Click schedule → Edit schedule details
- Click "Manage Schedules" → Full schedule management page (document 4A)
- Hover → Show full schedule details (frequency, recipients)

---

### 6. ALERTS & NOTIFICATIONS (Bottom Section)

**Purpose:** Highlight issues requiring attention

**Alert Types:**

**🔴 HIGH Priority:**
- Compliance below threshold (< 90%)
- Critical system failures
- Overdue submissions (> 7 days)

**🟡 MEDIUM Priority:**
- Approaching deadlines (< 3 days)
- Non-critical failures
- Data quality issues

**🟢 INFO:**
- Report generation completed
- Schedule executed successfully
- Data refresh completed

**Display:**
- Top 3 alerts on landing page
- Click "View All" → Notifications page
- Auto-refresh every 30 seconds (SignalR)

**Interactions:**
- Click alert → Navigate to related report/entity
- Click "Dismiss" → Mark as read
- Click "Send Reminder" → Email notification to assignee

---

## Workflows from Landing Page

### Workflow 1: View Regional Dashboard

**User Action:** Click "Regional Dashboard" card

**Steps:**
1. Click card in "My Dashboards" section
2. System checks permission: `Reports.ViewRegion`
3. Navigate to `/Reports/RegionalDashboard?period=current`
4. Load data from `RegionalMonthlySnapshot` table
5. Render dashboard (document 2B covers this)

**Result:** Full-screen regional dashboard with 12 factories

---

### Workflow 2: Browse Report Catalog

**User Action:** Click "Browse All →" in Report Catalog

**Steps:**
1. Click "Browse All" link
2. Navigate to `/Reports/Catalog`
3. Display all reports grouped by category
4. Show filters: Category, Date Range, Created By
5. Show search box (search by name/description)

**Query:**
```sql
SELECT
    r.ReportId,
    r.ReportName,
    r.Description,
    r.Category,
    r.CreatedBy,
    r.CreatedDate,
    r.LastRunDate,
    COUNT(ra.AccessId) AS ViewCount
FROM ReportDefinitions r
LEFT JOIN ReportAccessControl rac ON r.ReportId = rac.ReportId
LEFT JOIN ReportExecutionLog ra ON r.ReportId = ra.ReportId
WHERE rac.UserId = @CurrentUserId
   OR rac.RoleId IN (SELECT RoleId FROM UserRoles WHERE UserId = @CurrentUserId)
   OR r.IsPublic = 1
GROUP BY r.ReportId, r.ReportName, r.Description, r.Category, r.CreatedBy, r.CreatedDate, r.LastRunDate
ORDER BY r.Category, r.ReportName
```

**Result:** Full catalog page with search/filter (document 2C covers this)

---

### Workflow 3: Create Custom Report

**User Action:** Click "🔍 Create Custom Report" in Quick Actions

**Permission Check:**
```csharp
if (!User.HasPermission("Reports.CreateCustomReport"))
{
    return Unauthorized("You don't have permission to create reports.");
}
```

**Steps:**
1. Click "Create Custom Report" button
2. System checks permission
3. Navigate to `/Reports/Builder/New`
4. Load Report Builder wizard (document 3A)

**Result:** Report Builder interface with 5-step wizard

---

### Workflow 4: Schedule a Report

**User Action:** Click "📅 Schedule a Report" in Quick Actions

**Steps:**
1. Click "Schedule a Report" button
2. System shows dialog: "Select report to schedule"
3. Display dropdown of available reports
4. User selects report (e.g., "Regional Performance Summary")
5. Navigate to `/Reports/Schedules/Create?reportId=12`
6. Load Schedule Configuration form (document 4A)

**Result:** Schedule creation form with frequency/delivery options

---

### Workflow 5: Export Dashboard to Excel

**User Action:** Click "📤 Export Dashboard to Excel" in Quick Actions

**Steps:**
1. Click "Export Dashboard to Excel" button
2. System shows dialog:
   ```
   ┌─────────────────────────────────────────┐
   │ Export Dashboard                        │
   ├─────────────────────────────────────────┤
   │                                         │
   │ Dashboard: Regional Performance         │
   │ Period: October 2025                    │
   │                                         │
   │ Include:                                │
   │ [✓] Summary Cards                       │
   │ [✓] Charts (as images)                  │
   │ [✓] Data Tables                         │
   │ [ ] Raw Data (pivot-ready)              │
   │                                         │
   │ Format: [Excel ▼]  [Cancel] [Export]   │
   └─────────────────────────────────────────┘
   ```
3. User clicks "Export"
4. Backend generates Excel file using EPPlus
5. Download starts: `Regional_Performance_Oct2025.xlsx`

**Processing:**
```csharp
// Controller action
[HttpPost]
public async Task<IActionResult> ExportDashboard(int dashboardId, ExportOptions options)
{
    // Load dashboard data
    var data = await _reportService.GetDashboardData(dashboardId);

    // Generate Excel file
    var excelFile = await _excelExportService.GenerateDashboardExcel(data, options);

    // Return file for download
    return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        $"Dashboard_{DateTime.Now:yyyyMMdd}.xlsx");
}
```

**Result:** Excel file downloaded with dashboard data

---

### Workflow 6: Email Report to Team

**User Action:** Click "📧 Email Report to Team" in Quick Actions

**Steps:**
1. Click "Email Report to Team" button
2. System shows email dialog:
   ```
   ┌─────────────────────────────────────────────────┐
   │ Email Report                                    │
   ├─────────────────────────────────────────────────┤
   │                                                 │
   │ Report: [Regional Performance Summary ▼]       │
   │                                                 │
   │ Recipients:                                     │
   │ [✓] All Regional Managers (distribution list)  │
   │ [ ] Head Office Team                           │
   │ [ ] Custom (enter emails)                      │
   │                                                 │
   │ Additional Recipients:                          │
   │ ┌─────────────────────────────────────────┐     │
   │ │ manager@ktda.co.ke; analyst@ktda.co.ke  │     │
   │ └─────────────────────────────────────────┘     │
   │                                                 │
   │ Format: [✓] PDF  [✓] Excel  [ ] Both           │
   │                                                 │
   │ Message (optional):                             │
   │ ┌─────────────────────────────────────────┐     │
   │ │ Please review the regional performance  │     │
   │ │ for October 2025.                       │     │
   │ └─────────────────────────────────────────┘     │
   │                                                 │
   │               [Cancel] [Send Email]             │
   └─────────────────────────────────────────────────┘
   ```
3. User configures and clicks "Send Email"
4. System sends emails via background job
5. Confirmation: "Email queued. 5 recipients will receive the report."

**Backend:**
```csharp
// Queue background job
BackgroundJob.Enqueue<EmailReportJob>(job =>
    job.SendReportEmail(reportId, recipients, format, message)
);
```

**Result:** Emails sent to recipients with report attached

---

### Workflow 7: Refresh All Data

**User Action:** Click "🔄 Refresh All Data" in Quick Actions

**Permission Required:** `Reports.RefreshData` (typically admin only)

**Steps:**
1. Click "Refresh All Data" button
2. System shows confirmation:
   ```
   ┌─────────────────────────────────────────┐
   │ Refresh All Data                        │
   ├─────────────────────────────────────────┤
   │                                         │
   │ This will:                              │
   │ • Invalidate all cached reports         │
   │ • Regenerate performance snapshots      │
   │ • Recalculate all metrics               │
   │                                         │
   │ This may take 5-10 minutes.             │
   │                                         │
   │ Proceed?    [Cancel] [Yes, Refresh]     │
   └─────────────────────────────────────────┘
   ```
3. User clicks "Yes, Refresh"
4. System triggers Hangfire job: `RefreshAllSnapshots()`
5. Progress modal shown:
   ```
   ┌─────────────────────────────────────────┐
   │ Refreshing Data...                      │
   ├─────────────────────────────────────────┤
   │                                         │
   │ [████████░░░░░░░░] 45% Complete         │
   │                                         │
   │ Currently processing: Region 3          │
   │ Factories processed: 30 / 67            │
   │ Estimated time remaining: 3 minutes     │
   │                                         │
   │              [Cancel Job]               │
   └─────────────────────────────────────────┘
   ```
6. Job completes
7. Notification: "Data refresh complete. All reports updated."

**Background Job:**
```csharp
public class RefreshAllSnapshotsJob
{
    public async Task Execute(IJobCancellationToken cancellationToken)
    {
        // Get all active tenants
        var tenants = await _context.Tenants.Where(t => t.IsActive).ToListAsync();

        int processed = 0;
        foreach (var tenant in tenants)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Regenerate snapshot for this tenant
            await _snapshotService.GenerateTenantSnapshot(tenant.TenantId);

            processed++;
            // Update progress (stored in job state)
            await UpdateProgress(processed, tenants.Count);
        }

        // Invalidate all cached reports
        await _cacheService.InvalidateAll("Reports:*");
    }
}
```

**Result:** All data refreshed, dashboards show latest data

---

### Workflow 8: Manage Scheduled Reports

**User Action:** Click "Manage Schedules →" in Scheduled Reports section

**Steps:**
1. Click "Manage Schedules" link
2. Navigate to `/Reports/Schedules`
3. Display all active schedules in table:
   ```
   ┌────────────────────────────────────────────────────────────────┐
   │ SCHEDULED REPORTS                           [+ New Schedule]   │
   ├────┬─────────────────┬───────────┬────────────┬──────┬────────┤
   │ ID │ Report Name     │ Frequency │ Next Run   │ Last │ Action │
   ├────┼─────────────────┼───────────┼────────────┼──────┼────────┤
   │ 1  │ Regional Perf   │ Monthly   │ 1 Nov 2025 │ ✅   │ [Edit] │
   │    │                 │ (1st, 7AM)│ 07:00 AM   │      │ [Del]  │
   ├────┼─────────────────┼───────────┼────────────┼──────┼────────┤
   │ 2  │ SW Compliance   │ Quarterly │ 1 Jan 2026 │ ✅   │ [Edit] │
   │    │                 │ (1st, 8AM)│ 08:00 AM   │      │ [Del]  │
   ├────┼─────────────────┼───────────┼────────────┼──────┼────────┤
   │ 3  │ HW Inventory    │ Weekly    │ Mon 4 Nov  │ ❌   │ [Edit] │
   │    │                 │ (Mon, 9AM)│ 09:00 AM   │ Fail │ [Del]  │
   └────┴─────────────────┴───────────┴────────────┴──────┴────────┘
   ```
4. Actions available: Edit, Delete, Pause, Resume

**Result:** Full schedule management interface (document 4A)

---

### Workflow 9: View Alert Details

**User Action:** Click "View Report →" on alert in Alerts section

**Steps:**
1. Click "View Report" link on alert:
   ```
   🔴 HIGH: Kariara Factory - 85% software compliance (below target)
   Action needed: Review unlicensed software installations
   [View Report →] [Dismiss]
   ```
2. System navigates to related report with pre-applied filters:
   - Report: Software Compliance Report
   - Filter: TenantId = Kariara Factory
   - Highlight: Unlicensed installations
3. Report opens in viewer (document 2D)

**Result:** Compliance report showing specific issue for Kariara Factory

---

### Workflow 10: Dismiss Alert

**User Action:** Click "Dismiss" on alert

**Steps:**
1. Click "Dismiss" button on alert
2. System updates UserActivityLog:
   ```sql
   UPDATE Notifications
   SET IsRead = 1, ReadDate = GETUTCDATE()
   WHERE NotificationId = @NotificationId
     AND RecipientUserId = @CurrentUserId
   ```
3. Alert removed from visible list
4. Alert moves to "Notifications History" (accessible via bell icon)

**Result:** Alert dismissed, landing page refreshed

---

## Role-Based Page Variations

### Factory ICT Officer View

**Differences:**
- "My Dashboards" shows only: Factory Dashboard
- "Report Catalog" filtered to own factory reports only
- "Quick Actions" limited:
  - ✅ Export Dashboard to Excel
  - ❌ Create Custom Report (no permission)
  - ❌ Schedule a Report (no permission)
  - ❌ Refresh All Data (no permission)
- "Alerts" shows only own factory alerts

**Data Scope:**
```sql
WHERE TenantId = @CurrentUserTenantId
```

---

### Regional Manager View

**Differences:**
- "My Dashboards" shows: Regional Dashboard, Performance Trends, Compliance Scorecard
- "Report Catalog" shows all regional reports
- "Quick Actions" all enabled except "Refresh All Data"
- "Alerts" shows alerts for 12 factories in region

**Data Scope:**
```sql
WHERE RegionId = @CurrentUserRegionId
```

---

### Head Office Analyst View

**Differences:**
- "My Dashboards" shows: Executive Dashboard, National Trends, Cross-Regional Comparison
- "Report Catalog" shows all 42 reports
- "Quick Actions" all enabled including "Refresh All Data"
- "Alerts" shows system-wide alerts

**Data Scope:**
```sql
-- No WHERE clause - see all data
```

---

## Page Load Performance

### Initial Load
```
Page request → Controller action (10ms)
    ↓
Load user preferences (20ms)
    ↓
Query recent activity (30ms)
    ↓
Query dashboard summaries (50ms, from cache)
    ↓
Query active schedules (20ms)
    ↓
Query alerts (40ms)
    ↓
Render page (50ms)
    ↓
Total: ~220ms target (< 300ms acceptable)
```

### Caching Strategy
```csharp
// Dashboard summaries cached for 30 minutes
_cache.GetOrCreate($"Dashboard:Summary:{userId}", entry =>
{
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
    return _reportService.GetDashboardSummaries(userId);
});

// Report catalog cached for 1 hour
_cache.GetOrCreate($"Reports:Catalog:{roleId}", entry =>
{
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
    return _reportService.GetReportCatalog(roleId);
});

// Alerts NOT cached (real-time)
var alerts = await _notificationService.GetActiveAlerts(userId);
```

---

## Next Workflows (Navigate to Other Documents)

From this landing page, users can navigate to:

**→ 2B_Dashboard_Viewer.md** - View full dashboard (click dashboard card)
**→ 2C_Report_Catalog.md** - Browse all reports (click "Browse All")
**→ 2D_Report_Viewer.md** - View specific report (click report name)
**→ 3A_Report_Builder.md** - Create custom report (click "Create Custom Report")
**→ 4A_Scheduled_Reports.md** - Manage schedules (click "Manage Schedules")
**→ 4B_Export_Functionality.md** - Export reports (click "Export" buttons)

---

**Status:** ✅ Landing Page Complete
**Next Document:** `2B_Dashboard_Viewer.md` - How users interact with full dashboards
