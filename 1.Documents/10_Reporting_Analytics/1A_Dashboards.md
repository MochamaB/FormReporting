# Reports Landing Page - My Dashboards Section

**Route:** `/Reports/Index` - Section 1
**Purpose:** Display available dashboards based on user role and provide quick navigation
**Component Type:** Card with icon + title + dashboard list
**Users:** All authenticated users (dashboards vary by role)

---

## Visual Representation

### Velzon Component Structure

```html
<!-- My Dashboards Section -->
<div class="card">
    <div class="card-header">
        <div class="d-flex align-items-center justify-content-between">
            <div class="d-flex align-items-center">
                <div class="avatar-xs me-3">
                    <div class="avatar-title bg-primary-subtle text-primary rounded fs-18">
                        <i class="ri-dashboard-line"></i>
                    </div>
                </div>
                <h5 class="card-title mb-0">My Dashboards</h5>
            </div>
            <a href="/Reports/AllDashboards" class="btn btn-sm btn-soft-primary">
                View All <i class="ri-arrow-right-line align-middle"></i>
            </a>
        </div>
    </div>
    <div class="card-body">
        <div class="list-group list-group-flush">
            <!-- Dashboard Item 1 -->
            <a href="/Reports/RegionalDashboard" class="list-group-item list-group-item-action">
                <div class="d-flex align-items-start">
                    <div class="flex-shrink-0">
                        <div class="avatar-xs">
                            <div class="avatar-title bg-success-subtle text-success rounded">
                                <i class="ri-building-line"></i>
                            </div>
                        </div>
                    </div>
                    <div class="flex-grow-1 ms-3">
                        <h6 class="mb-1">Regional Dashboard</h6>
                        <p class="text-muted mb-1 fs-12">
                            Performance overview for 12 factories in Mt. Kenya region
                        </p>
                        <div class="d-flex gap-2">
                            <span class="badge bg-success-subtle text-success">11 OK</span>
                            <span class="badge bg-danger-subtle text-danger">1 Alert</span>
                        </div>
                    </div>
                    <div class="flex-shrink-0 align-self-center">
                        <i class="ri-arrow-right-s-line fs-18 text-muted"></i>
                    </div>
                </div>
            </a>

            <!-- Dashboard Item 2 -->
            <a href="/Reports/PerformanceTrends" class="list-group-item list-group-item-action">
                <div class="d-flex align-items-start">
                    <div class="flex-shrink-0">
                        <div class="avatar-xs">
                            <div class="avatar-title bg-info-subtle text-info rounded">
                                <i class="ri-line-chart-line"></i>
                            </div>
                        </div>
                    </div>
                    <div class="flex-grow-1 ms-3">
                        <h6 class="mb-1">Performance Trends</h6>
                        <p class="text-muted mb-1 fs-12">
                            6-month trend analysis for regional metrics
                        </p>
                        <div>
                            <span class="badge bg-success-subtle text-success">
                                <i class="ri-arrow-up-line"></i> Improving
                            </span>
                        </div>
                    </div>
                    <div class="flex-shrink-0 align-self-center">
                        <i class="ri-arrow-right-s-line fs-18 text-muted"></i>
                    </div>
                </div>
            </a>

            <!-- More dashboard items... -->
        </div>
    </div>
</div>
```

---

## Complete Dashboard Catalog

### All Dashboards in System (9 Total)

| # | Dashboard Name | Dashboard Code | Route | Icon |
|---|----------------|----------------|-------|------|
| 1 | Factory Dashboard | `FACTORY_DASH` | `/Reports/FactoryDashboard` | `ri-building-4-line` |
| 2 | Regional Dashboard | `REGIONAL_DASH` | `/Reports/RegionalDashboard` | `ri-building-line` |
| 3 | Executive Dashboard | `EXECUTIVE_DASH` | `/Reports/ExecutiveDashboard` | `ri-pie-chart-line` |
| 4 | Performance Trends | `PERFORMANCE_TRENDS` | `/Reports/PerformanceTrends` | `ri-line-chart-line` |
| 5 | Compliance Scorecard | `COMPLIANCE_SCORE` | `/Reports/ComplianceScorecard` | `ri-shield-check-line` |
| 6 | Hardware Inventory Summary | `HARDWARE_SUMMARY` | `/Reports/HardwareInventory` | `ri-computer-line` |
| 7 | Software Compliance | `SOFTWARE_COMPLIANCE` | `/Reports/SoftwareCompliance` | `ri-apps-line` |
| 8 | Form Submission Tracker | `FORM_TRACKER` | `/Reports/FormSubmissionTracker` | `ri-file-list-3-line` |
| 9 | KPI Monitoring | `KPI_MONITOR` | `/Reports/KPIMonitoring` | `ri-speed-line` |

---

## Dashboard Details & Visibility

### Dashboard 1: Factory Dashboard

**Purpose:** Single factory performance view
**Data Scope:** One tenant (own factory)
**Refresh Frequency:** Real-time (on-demand)
**Data Source:** TenantPerformanceSnapshot, TenantMetrics

**Visible to:**
- ✅ Factory ICT Officer (own factory only)
- ✅ Regional Manager (can select any factory in region)
- ✅ Head Office ICT Manager (can select any factory)
- ❌ External users

**Display Rules:**
```sql
-- Factory ICT Officer
WHERE TenantId = @CurrentUserTenantId

-- Regional Manager
WHERE TenantId IN (
    SELECT TenantId FROM Tenants WHERE RegionId = @CurrentUserRegionId
)

-- Head Office
-- No WHERE clause - can see all factories
```

**Summary Description:**
"Comprehensive performance overview for {FactoryName} - Hardware status, software compliance, network uptime, form submissions"

**Key Metrics Displayed:**
- Total computers (operational vs faulty)
- Software license compliance %
- Network uptime %
- Monthly form submission status
- Recent alerts/issues

**Badge Indicators:**
```csharp
// Dynamic badges based on data
if (compliancePercent >= 95)
    badge = "badge bg-success-subtle text-success" + " ✓ Compliant"
else if (compliancePercent >= 85)
    badge = "badge bg-warning-subtle text-warning" + " ⚠ Warning"
else
    badge = "badge bg-danger-subtle text-danger" + " ✗ Non-Compliant"
```

**Link to:** Document `2B_Factory_Dashboard.md`

---

### Dashboard 2: Regional Dashboard

**Purpose:** Multi-factory comparison within a region
**Data Scope:** 12 factories in one region
**Refresh Frequency:** Every 30 minutes (cached)
**Data Source:** RegionalMonthlySnapshot, TenantPerformanceSnapshot

**Visible to:**
- ❌ Factory ICT Officer
- ✅ Regional Manager (own region only)
- ✅ Head Office ICT Manager (can select any region)
- ❌ External users

**Display Rules:**
```sql
-- Regional Manager
WHERE RegionId = @CurrentUserRegionId

-- Head Office
-- No WHERE clause - can select any region via dropdown
```

**Summary Description:**
"Performance overview for {RegionName} region - {FactoryCount} factories, aggregate metrics, factory rankings"

**Key Metrics Displayed:**
- Total factories in region
- Factories submitted forms on time
- Average compliance across region
- Regional hardware count
- Top 3 / Bottom 3 performers

**Badge Indicators:**
```csharp
// Based on factories reporting
var submitted = factoriesSubmitted;
var total = totalFactories;

if (submitted == total)
    badge = "badge bg-success-subtle text-success" + $" {submitted}/{total} Submitted"
else if (submitted >= total * 0.8)
    badge = "badge bg-warning-subtle text-warning" + $" {submitted}/{total} Pending"
else
    badge = "badge bg-danger-subtle text-danger" + $" {submitted}/{total} Overdue"
```

**Link to:** Document `2C_Regional_Dashboard.md`

---

### Dashboard 3: Executive Dashboard

**Purpose:** System-wide overview for senior management
**Data Scope:** All 67 factories across 6 regions
**Refresh Frequency:** Daily (pre-aggregated)
**Data Source:** RegionalMonthlySnapshot, System aggregations

**Visible to:**
- ❌ Factory ICT Officer
- ❌ Regional Manager
- ✅ Head Office ICT Manager
- ✅ CIO / Executive team
- ✅ External auditors (read-only)

**Display Rules:**
```sql
-- No tenant/region filter - system-wide view
-- Permission check only
WHERE User.HasPermission('Reports.ViewExecutiveDashboard')
```

**Summary Description:**
"National ICT performance overview - 67 factories, 6 regions, cross-regional trends, executive KPIs"

**Key Metrics Displayed:**
- National compliance average
- Regional comparison (bar chart)
- Top 5 / Bottom 5 factories nationwide
- Budget vs actual (if enabled)
- Critical alerts requiring attention

**Badge Indicators:**
```csharp
// System health indicator
var criticalAlerts = alertCount;

if (criticalAlerts == 0)
    badge = "badge bg-success-subtle text-success" + " All Systems Normal"
else if (criticalAlerts <= 3)
    badge = "badge bg-warning-subtle text-warning" + $" {criticalAlerts} Alerts"
else
    badge = "badge bg-danger-subtle text-danger" + $" {criticalAlerts} Critical Issues"
```

**Link to:** Document `2D_Executive_Dashboard.md`

---

### Dashboard 4: Performance Trends

**Purpose:** Historical trend analysis (time-series data)
**Data Scope:** Varies by user role (factory/region/national)
**Refresh Frequency:** Daily (historical data)
**Data Source:** TenantMetrics (aggregated by month)

**Visible to:**
- ✅ Factory ICT Officer (own factory trends)
- ✅ Regional Manager (regional trends)
- ✅ Head Office ICT Manager (national trends)
- ❌ External users

**Display Rules:**
```sql
-- Data scope based on role (same as other dashboards)
-- Time range: Last 6 months default, configurable to 12/24 months
```

**Summary Description:**
"Historical performance trends - 6-month view of key metrics with month-over-month comparisons"

**Key Metrics Displayed:**
- Hardware count trend (line chart)
- Compliance trend (line chart)
- Form submission rate trend
- Network uptime trend
- Calculated: Improving / Declining / Stable

**Badge Indicators:**
```csharp
// Trend direction
var currentMonth = metrics[0].Value;
var previousMonth = metrics[1].Value;
var change = ((currentMonth - previousMonth) / previousMonth) * 100;

if (change > 5)
    badge = "badge bg-success-subtle text-success" + $" ↑ {change:F1}% Improving"
else if (change < -5)
    badge = "badge bg-danger-subtle text-danger" + $" ↓ {Math.Abs(change):F1}% Declining"
else
    badge = "badge bg-secondary-subtle text-secondary" + " → Stable"
```

**Link to:** Document `2E_Performance_Trends_Dashboard.md`

---

### Dashboard 5: Compliance Scorecard

**Purpose:** Detailed compliance tracking across multiple dimensions
**Data Scope:** Varies by user role
**Refresh Frequency:** Real-time
**Data Source:** TenantMetrics (compliance-related metrics)

**Visible to:**
- ✅ Factory ICT Officer (own factory)
- ✅ Regional Manager (region)
- ✅ Head Office ICT Manager (all)
- ✅ External auditors (read-only)

**Display Rules:**
```sql
-- Role-scoped
-- Compliance categories:
--   1. Software License Compliance
--   2. Hardware Asset Compliance
--   3. Form Submission Compliance
--   4. Network Security Compliance (if tracked)
```

**Summary Description:**
"Multi-dimensional compliance tracking - Software, Hardware, Forms, Security with traffic light indicators"

**Key Metrics Displayed:**
- Overall compliance score (average)
- Software compliance % (licensed vs unlicensed)
- Hardware compliance % (verified vs unverified)
- Form submission compliance % (on-time vs late)
- Breakdown by category

**Badge Indicators:**
```csharp
// Traffic light system
var overallCompliance = (softwareComp + hardwareComp + formComp) / 3;

if (overallCompliance >= 95)
    badge = "badge bg-success-subtle text-success" + " 🟢 Excellent"
else if (overallCompliance >= 85)
    badge = "badge bg-warning-subtle text-warning" + " 🟡 Good"
else if (overallCompliance >= 75)
    badge = "badge bg-danger-subtle text-danger" + " 🟠 Fair"
else
    badge = "badge bg-danger text-white" + " 🔴 Poor"
```

**Link to:** Document `2F_Compliance_Scorecard_Dashboard.md`

---

### Dashboard 6: Hardware Inventory Summary

**Purpose:** Hardware asset overview with counts and status
**Data Scope:** Varies by user role
**Refresh Frequency:** Real-time (manual refresh available)
**Data Source:** TenantHardware, HardwareItems

**Visible to:**
- ✅ Factory ICT Officer (own factory)
- ✅ Regional Manager (region)
- ✅ Head Office ICT Manager (all)
- ❌ External users

**Display Rules:**
```sql
-- Role-scoped
-- Categories: Computers, Printers, Servers, Network Equipment, etc.
```

**Summary Description:**
"Hardware inventory overview - Asset counts by category, operational status, aging analysis"

**Key Metrics Displayed:**
- Total hardware count by category
- Operational vs Faulty breakdown
- Hardware aging (0-2 years, 3-5 years, 5+ years)
- Maintenance due/overdue
- Recent additions/disposals

**Badge Indicators:**
```csharp
// Operational ratio
var operationalPercent = (operational / total) * 100;

if (operationalPercent >= 95)
    badge = "badge bg-success-subtle text-success" + $" {operational}/{total} Operational"
else if (operationalPercent >= 85)
    badge = "badge bg-warning-subtle text-warning" + $" {total - operational} Faulty"
else
    badge = "badge bg-danger-subtle text-danger" + $" {total - operational} Faulty (Critical)"
```

**Link to:** Document `2G_Hardware_Inventory_Dashboard.md`

---

### Dashboard 7: Software Compliance

**Purpose:** Software license tracking and compliance monitoring
**Data Scope:** Varies by user role
**Refresh Frequency:** Real-time
**Data Source:** TenantSoftwareInstallations, SoftwareLicenses

**Visible to:**
- ✅ Factory ICT Officer (own factory)
- ✅ Regional Manager (region)
- ✅ Head Office ICT Manager (all)
- ✅ Procurement team (view-only)
- ❌ External users

**Display Rules:**
```sql
-- Role-scoped
-- Show: Licensed, Unlicensed, Expiring Soon, Over-allocated
```

**Summary Description:**
"Software license compliance - Licensed vs unlicensed installations, expiring licenses, over-allocation alerts"

**Key Metrics Displayed:**
- Total software installations
- Licensed installations count
- Unlicensed installations count (🔴 Alert if > 0)
- Licenses expiring in 90 days
- Over-allocated products

**Badge Indicators:**
```csharp
// Unlicensed count
var unlicensed = unlicensedCount;

if (unlicensed == 0)
    badge = "badge bg-success-subtle text-success" + " ✓ Fully Compliant"
else if (unlicensed <= 3)
    badge = "badge bg-warning-subtle text-warning" + $" {unlicensed} Unlicensed"
else
    badge = "badge bg-danger-subtle text-danger" + $" {unlicensed} Unlicensed (Action Required)"
```

**Link to:** Document `2H_Software_Compliance_Dashboard.md`

---

### Dashboard 8: Form Submission Tracker

**Purpose:** Track form assignment and submission compliance
**Data Scope:** Varies by user role
**Refresh Frequency:** Real-time
**Data Source:** FormTemplateAssignments, FormTemplateSubmissions

**Visible to:**
- ✅ Factory ICT Officer (own factory)
- ✅ Regional Manager (region)
- ✅ Head Office ICT Manager (all)
- ❌ External users

**Display Rules:**
```sql
-- Role-scoped
-- Show: Pending, Submitted, Approved, Overdue
```

**Summary Description:**
"Form submission compliance tracker - Assignments, submissions, approvals, overdue tracking"

**Key Metrics Displayed:**
- Total assignments this month
- Submitted count
- Pending count
- Overdue count (past deadline)
- Approval status

**Badge Indicators:**
```csharp
// Submission compliance
var submittedOnTime = submittedCount;
var totalAssigned = assignedCount;
var overdue = overdueCount;

if (overdue == 0 && submittedOnTime == totalAssigned)
    badge = "badge bg-success-subtle text-success" + " All Submitted"
else if (overdue == 0)
    badge = "badge bg-warning-subtle text-warning" + $" {totalAssigned - submittedOnTime} Pending"
else
    badge = "badge bg-danger-subtle text-danger" + $" {overdue} Overdue"
```

**Link to:** Document `2I_Form_Submission_Tracker_Dashboard.md`

---

### Dashboard 9: KPI Monitoring

**Purpose:** Real-time KPI tracking with threshold alerts
**Data Scope:** Varies by user role
**Refresh Frequency:** Every 5 minutes (SignalR push updates)
**Data Source:** TenantMetrics (filtered to IsKPI = 1)

**Visible to:**
- ✅ Factory ICT Officer (own factory KPIs)
- ✅ Regional Manager (regional KPIs)
- ✅ Head Office ICT Manager (all KPIs)
- ✅ External auditors (view-only)

**Display Rules:**
```sql
-- Role-scoped
-- Only show metrics where MetricDefinitions.IsKPI = 1
-- Apply traffic light thresholds (Green/Yellow/Red)
```

**Summary Description:**
"Real-time KPI monitoring - Critical metrics with traffic light indicators and threshold alerts"

**Key Metrics Displayed:**
- All KPIs defined in MetricDefinitions (where IsKPI = 1)
- Current value vs target
- Threshold status (Green/Yellow/Red)
- Trend (improving/declining)
- Last updated timestamp

**Badge Indicators:**
```csharp
// KPI threshold status
var value = metric.NumericValue;
var green = metric.ThresholdGreen;
var yellow = metric.ThresholdYellow;

if (value >= green)
    badge = "badge bg-success text-white" + " 🟢 On Target"
else if (value >= yellow)
    badge = "badge bg-warning text-dark" + " 🟡 Warning"
else
    badge = "badge bg-danger text-white" + " 🔴 Below Target"
```

**Link to:** Document `2J_KPI_Monitoring_Dashboard.md`

---

## Role-Based Dashboard Visibility Matrix

| Dashboard | Factory ICT | Regional Mgr | HO ICT Mgr | Executive | Auditor |
|-----------|-------------|--------------|------------|-----------|---------|
| 1. Factory Dashboard | ✅ Own only | ✅ Region | ✅ All | ❌ | ❌ |
| 2. Regional Dashboard | ❌ | ✅ Own region | ✅ All | ❌ | ❌ |
| 3. Executive Dashboard | ❌ | ❌ | ✅ | ✅ | ✅ Read-only |
| 4. Performance Trends | ✅ Own only | ✅ Region | ✅ All | ✅ | ❌ |
| 5. Compliance Scorecard | ✅ Own only | ✅ Region | ✅ All | ✅ | ✅ Read-only |
| 6. Hardware Inventory | ✅ Own only | ✅ Region | ✅ All | ❌ | ❌ |
| 7. Software Compliance | ✅ Own only | ✅ Region | ✅ All | ❌ | ✅ View-only |
| 8. Form Submission Tracker | ✅ Own only | ✅ Region | ✅ All | ❌ | ❌ |
| 9. KPI Monitoring | ✅ Own only | ✅ Region | ✅ All | ✅ | ✅ View-only |

---

## Dashboard Loading Logic

### Controller Action

```csharp
public async Task<IActionResult> Index()
{
    var currentUser = await _userService.GetCurrentUserAsync();
    var availableDashboards = new List<DashboardItem>();

    // Get user's highest role
    var role = await _userService.GetHighestRoleAsync(currentUser.UserId);

    // Load dashboards based on role
    switch (role.RoleCode)
    {
        case "FACTORY_ICT":
            availableDashboards.Add(GetFactoryDashboard(currentUser.TenantId));
            availableDashboards.Add(GetPerformanceTrends(currentUser.TenantId));
            availableDashboards.Add(GetComplianceScorecard(currentUser.TenantId));
            availableDashboards.Add(GetHardwareInventory(currentUser.TenantId));
            availableDashboards.Add(GetSoftwareCompliance(currentUser.TenantId));
            availableDashboards.Add(GetFormTracker(currentUser.TenantId));
            availableDashboards.Add(GetKPIMonitoring(currentUser.TenantId));
            break;

        case "REGIONAL_MGR":
            availableDashboards.Add(GetRegionalDashboard(currentUser.RegionId));
            availableDashboards.Add(GetPerformanceTrends(currentUser.RegionId));
            availableDashboards.Add(GetComplianceScorecard(currentUser.RegionId));
            availableDashboards.Add(GetHardwareInventory(currentUser.RegionId));
            availableDashboards.Add(GetSoftwareCompliance(currentUser.RegionId));
            availableDashboards.Add(GetFormTracker(currentUser.RegionId));
            availableDashboards.Add(GetKPIMonitoring(currentUser.RegionId));
            break;

        case "HO_ICT_MGR":
        case "SYSADMIN":
            availableDashboards.Add(GetExecutiveDashboard());
            availableDashboards.Add(GetRegionalDashboard(null)); // Can select region
            availableDashboards.Add(GetPerformanceTrends(null));
            availableDashboards.Add(GetComplianceScorecard(null));
            availableDashboards.Add(GetHardwareInventory(null));
            availableDashboards.Add(GetSoftwareCompliance(null));
            availableDashboards.Add(GetFormTracker(null));
            availableDashboards.Add(GetKPIMonitoring(null));
            break;
    }

    var viewModel = new ReportsLandingViewModel
    {
        AvailableDashboards = availableDashboards,
        UserRole = role.RoleName,
        UserTenantName = currentUser.Tenant?.TenantName
    };

    return View(viewModel);
}

private DashboardItem GetFactoryDashboard(int tenantId)
{
    var summary = _reportService.GetFactorySummary(tenantId);

    return new DashboardItem
    {
        DashboardCode = "FACTORY_DASH",
        Title = "Factory Dashboard",
        Description = $"Performance overview for {summary.TenantName}",
        Route = $"/Reports/FactoryDashboard?tenantId={tenantId}",
        Icon = "ri-building-4-line",
        IconColor = "primary",
        Badges = new List<BadgeItem>
        {
            new BadgeItem
            {
                Label = $"{summary.ComputersOperational}/{summary.ComputersTotal} Operational",
                Color = summary.ComputersOperational == summary.ComputersTotal ? "success" : "warning"
            },
            new BadgeItem
            {
                Label = $"{summary.CompliancePercent:F0}% Compliant",
                Color = summary.CompliancePercent >= 95 ? "success" : "warning"
            }
        }
    };
}
```

---

## User Interaction Workflow

### Step 1: Page Load
1. User navigates to `/Reports/Index`
2. Controller loads user profile and role
3. Controller queries available dashboards based on role
4. Controller loads summary data for each dashboard (badges)
5. View renders "My Dashboards" section with dashboard list

### Step 2: Dashboard Click
1. User clicks dashboard item (e.g., "Regional Dashboard")
2. Browser navigates to `/Reports/RegionalDashboard`
3. Full dashboard page loads (documented in separate file)

### Step 3: View All Click
1. User clicks "View All" link in header
2. Browser navigates to `/Reports/AllDashboards`
3. Shows full catalog of all dashboards (grid view instead of list)

---

## Next Documents to Create

**Immediate next:**
- `2B_Factory_Dashboard.md` - Full detail on Factory Dashboard
- `2C_Regional_Dashboard.md` - Full detail on Regional Dashboard
- `2D_Executive_Dashboard.md` - Full detail on Executive Dashboard

**Subsequent:**
- `2E_Performance_Trends_Dashboard.md`
- `2F_Compliance_Scorecard_Dashboard.md`
- `2G_Hardware_Inventory_Dashboard.md`
- `2H_Software_Compliance_Dashboard.md`
- `2I_Form_Submission_Tracker_Dashboard.md`
- `2J_KPI_Monitoring_Dashboard.md`

Each dashboard document will contain:
- Complete UI mockup
- Data sources and queries
- Chart configurations
- Filters and parameters
- Export options
- Drill-down workflows
- Performance requirements

---

**Status:** ✅ My Dashboards Section Complete
**Next:** Create `2B_Factory_Dashboard.md` with full dashboard structure
