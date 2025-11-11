# Section 10: Reporting & Analytics - Overview & Workflows

**Database Tables:** 12 tables
**Purpose:** Comprehensive reporting and analytics system that consolidates data from all modules to provide actionable insights for 67 factories.

---

## System Overview

### What This Module Does

The Reporting & Analytics module is KTDA's **data intelligence layer** that transforms raw operational data into actionable insights. It consolidates data from 6 upstream modules:

```
FormTemplateSubmissions (Section 4) ─┐
MetricDefinitions (Section 3) ───────┤
HardwareInventory (Section 6) ───────┤
SoftwareInstallations (Section 5) ───┼──→ REPORTING ENGINE ──→ Dashboards
Tenants & Regions (Section 1) ───────┤                          Reports
Users & Roles (Section 2) ───────────┘                          Exports
                                                                 Alerts
```

**Core Capabilities:**
1. **Pre-built Dashboards** - Factory, Regional, and Executive views
2. **Custom Report Builder** - Drag-and-drop report designer
3. **Automated Snapshots** - Nightly pre-aggregation for performance
4. **Scheduled Reports** - Email monthly reports to stakeholders
5. **Multi-format Export** - Excel, PDF, CSV, PowerPoint
6. **Interactive Visualizations** - Charts, graphs, heatmaps, gauges

---

### Real-World Example: Monthly Factory Performance Report

**Without this system:**
- Regional Manager manually downloads 12 Excel files from 12 factories
- Opens each file, extracts KPIs into master spreadsheet
- Manually calculates regional averages, trends, rankings
- Creates PowerPoint slides with charts
- Takes 6-8 hours per month
- Errors common (copy-paste mistakes, outdated data)

**With this system:**
- Navigate to "Regional Dashboard" → Select "October 2025"
- System shows real-time data from all 12 factories
- Interactive charts: Hardware count, Software compliance, Network uptime
- Click "Export to Excel" → Formatted report in 5 seconds
- Click "Schedule Monthly Report" → Auto-sent to inbox on 1st of month
- **Result:** 6-8 hours → 5 minutes (99% time saving)

---

## User Roles & Permissions

### 1. Factory Viewer (Factory ICT Officer)
**Permissions:**
- `Reports.ViewOwnFactory` - See own factory data only
- `Reports.ExportOwnFactory` - Export own reports

**Workflows:**
- View factory performance dashboard
- Compare own factory vs regional average (anonymized)
- Export monthly report for factory manager
- View historical trends (last 12 months)

**Data Scope:** Single tenant (own factory)

---

### 2. Regional Manager (Regional ICT Manager)
**Permissions:**
- `Reports.ViewRegion` - See all factories in region
- `Reports.ExportRegion` - Export regional reports
- `Reports.CompareFactories` - Factory-to-factory comparison
- `Reports.CreateCustomReport` - Design custom reports (within region)

**Workflows:**
- View regional performance dashboard (12 factories)
- Identify underperforming factories (red flags)
- Export regional summary to Excel
- Schedule monthly report to email
- Create custom compliance report

**Data Scope:** Region-level (all factories in assigned region)

---

### 3. Head Office Analyst (HO ICT Manager / Data Analyst)
**Permissions:**
- `Reports.ViewAll` - See all 67 factories across all regions
- `Reports.ExportAll` - Export system-wide reports
- `Reports.CreatePublicReports` - Design reports for all users
- `Reports.AccessRawData` - Query raw data (advanced)

**Workflows:**
- View executive dashboard (67 factories aggregated)
- Analyze cross-regional trends
- Create KPI tracking reports
- Design report templates for Regional Managers
- Export data for Board presentations

**Data Scope:** Global (all tenants)

---

### 4. Report Designer (System Administrator / Senior Analyst)
**Permissions:**
- `Reports.DesignReports` - Access report builder
- `Reports.PublishReports` - Publish reports for others
- `Reports.ManageSnapshots` - Configure pre-aggregation
- `Reports.ManageSchedules` - Configure automated reports

**Workflows:**
- Design custom reports using drag-and-drop builder
- Configure data sources and joins
- Set up scheduled report generation
- Optimize report performance
- Manage report permissions

**Data Scope:** Global with admin privileges

---

## Report Types Taxonomy

### 1. Operational Reports (Daily/Weekly)
**Purpose:** Monitor day-to-day operations
**Frequency:** Real-time to weekly
**Examples:**
- Daily Hardware Status Report
- Weekly Form Submission Compliance
- Current Software License Usage

**Characteristics:**
- Real-time or near real-time data
- Detailed drill-down capability
- Action-oriented (identify issues)
- Multiple export formats

---

### 2. Executive Dashboards (Monthly/Quarterly)
**Purpose:** High-level KPI tracking for management
**Frequency:** Monthly, Quarterly
**Examples:**
- Regional Performance Summary (6 regions)
- Top 10 / Bottom 10 Factory Rankings
- Quarterly Compliance Scorecard

**Characteristics:**
- Pre-aggregated data (snapshots)
- Visual-heavy (charts, gauges, heatmaps)
- Trend indicators (↑↓)
- Traffic light status (🟢🟡🔴)

---

### 3. Compliance Reports (Regulatory)
**Purpose:** Meet regulatory and audit requirements
**Frequency:** As needed, typically quarterly/annually
**Examples:**
- Software License Compliance Report
- Hardware Asset Audit Report
- Data Submission Compliance (factories that didn't submit)

**Characteristics:**
- Detailed audit trail
- Exception highlighting
- PDF export with watermark
- Signed and dated

---

### 4. Trend Analysis Reports (Historical)
**Purpose:** Identify patterns and long-term trends
**Frequency:** On-demand
**Examples:**
- 12-Month Hardware Acquisition Trend
- Network Uptime Trend (6 months)
- Regional Performance Over Time

**Characteristics:**
- Time-series data (line charts)
- Comparison periods (YoY, MoM)
- Forecasting (if enabled)
- Large datasets (12+ months)

---

### 5. Ad-hoc Reports (Custom)
**Purpose:** Answer specific business questions
**Frequency:** On-demand
**Examples:**
- "Which factories have Windows 7 installations?"
- "Total cost of software licenses per region"
- "Factories with printer count > 10"

**Characteristics:**
- User-designed queries
- Custom filters and grouping
- Save for future use
- Share with colleagues

---

## Core Workflows

### Workflow 1: View Factory Dashboard

**Tables:** `TenantPerformanceSnapshot`, `TenantMetrics`, `Tenants`

**User Journey:**
1. **Navigate** - Click "Dashboards" → "My Factory Dashboard"
2. **Select Period** - Choose reporting period (default: Current month)
3. **View KPIs** - See key metrics:
   - Hardware: 45 computers (42 operational, 3 faulty)
   - Software: 98% license compliance
   - Network: 99.2% uptime
   - Forms: 12/12 submitted on time
4. **View Charts** - Interactive visualizations:
   - Hardware trend (last 6 months)
   - Software license usage by category
   - Monthly form submission timeline
5. **Compare** - Toggle "Show Regional Average" (anonymized)
6. **Drill-down** - Click metric to see details
7. **Export** - Click "Export to PDF" for local records

**Performance:** < 2 seconds to load (data from snapshot)

---

### Workflow 2: View Regional Summary

**Tables:** `RegionalMonthlySnapshot`, `TenantPerformanceSnapshot`, `Regions`, `Tenants`

**User Journey:**
1. **Navigate** - Click "Dashboards" → "Regional Dashboard"
2. **Select Region** - Auto-set to assigned region (if Regional Manager)
3. **Select Period** - Choose month (default: Current month)
4. **View Summary Cards**:
   ```
   ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
   │ 12 Factories    │ │ 540 Computers   │ │ 96% Compliance  │
   │ 11 Submitted ✅ │ │ 520 Operational │ │ 🟢 On Target    │
   │ 1 Pending ⚠️    │ │ 20 Faulty       │ │                 │
   └─────────────────┘ └─────────────────┘ └─────────────────┘
   ```
5. **View Factory Comparison Table**:
   ```
   Factory          | Computers | Compliance | Uptime | Status
   ─────────────────┼───────────┼────────────┼────────┼────────
   Kangaita         | 45        | 98%        | 99.2%  | 🟢 Good
   Ragati           | 52        | 100%       | 99.8%  | 🟢 Good
   Tetu             | 38        | 92%        | 97.5%  | 🟡 Warning
   Kariara          | 41        | 85%        | 95.2%  | 🔴 Action
   ```
6. **View Regional Heatmap** - Geographic visualization
7. **Identify Issues** - Click on red/yellow factories
8. **Export** - Click "Export to Excel" for detailed report

**Performance:** < 3 seconds (pre-aggregated data)

---

### Workflow 3: Export Report to Excel

**Tables:** `ReportDefinitions`, `ReportCache`, `TenantPerformanceSnapshot`

**User Journey:**
1. **Select Report** - From dashboard or report catalog
2. **Configure Export**:
   - Format: [Excel ✓] PDF, CSV
   - Options:
     - [✓] Include charts
     - [✓] Include raw data
     - [ ] Include formulas (for further analysis)
3. **Click "Export"**
4. **Processing**:
   - If < 5,000 rows → Instant download
   - If > 5,000 rows → Background job, email link when ready
5. **Download** - File: `Regional_Performance_Oct2025.xlsx`
6. **Open in Excel**:
   - Sheet 1: Executive Summary (charts)
   - Sheet 2: Factory Details (table)
   - Sheet 3: Raw Data (pivot-ready)
   - Sheet 4: Metadata (report parameters)

**Excel Features:**
- Pre-formatted (KTDA branding)
- Pivot tables ready
- Conditional formatting (traffic lights)
- Print-ready layout

---

### Workflow 4: Schedule Monthly Report

**Tables:** `ReportSchedules`, `ReportDefinitions`, `NotificationChannels`

**User Journey:**
1. **Navigate** - Click "Reports" → "Scheduled Reports" → "Add Schedule"
2. **Select Report** - Choose from published reports
   - Example: "Regional Performance Summary"
3. **Configure Schedule**:
   - Frequency: [Monthly ▼]
   - Day of month: [1st ▼]
   - Time: [07:00 AM ▼]
4. **Configure Delivery**:
   - Delivery method: [Email ✓] Dashboard, File share
   - Recipients:
     - [✓] Myself (john.kamau@ktda.co.ke)
     - [✓] All Regional Managers (distribution list)
     - [ ] Head Office Team
   - Attachment format: [Excel ▼] PDF, Both
5. **Email Template**:
   ```
   Subject: [Auto] Regional Performance Report - {{Month}} {{Year}}

   Body:
   Hi {{RecipientName}},

   Attached is the monthly Regional Performance Report for {{MonthName}} {{Year}}.

   Quick Summary:
   - Factories Reporting: {{FactoriesReported}}/{{TotalFactories}}
   - Overall Compliance: {{CompliancePercent}}%
   - Top Performer: {{TopFactory}} ({{TopScore}}%)

   View full report online: {{ReportLink}}

   Best regards,
   KTDA ICT Reporting System
   ```
6. **Set Active Date**:
   - Start: [2025-11-01 ▼]
   - End: [Never ▼] (or specific date)
7. **Preview** - Send test email to myself
8. **Save Schedule**
9. **Confirmation**: "Report scheduled. Next run: 1 Nov 2025, 07:00 AM"

**Execution:**
- Hangfire job runs at scheduled time
- Generates report with current month data
- Sends emails to all recipients
- Logs execution in `ReportExecutionLog`
- Sends failure alert if job fails

---

### Workflow 5: Create Custom Report (Advanced)

**Tables:** `ReportDefinitions`, `ReportFields`, `ReportFilters`, `ReportGroupings`

**User Journey:**
1. **Navigate** - Click "Reports" → "Report Builder" → "Create New Report"
2. **Step 1: Select Data Source**
   - Primary table: [TenantPerformanceSnapshot ▼]
   - Join with:
     - [✓] Tenants (for TenantName, Region)
     - [✓] Regions (for RegionName)
     - [ ] TenantMetrics (for raw metric data)
3. **Step 2: Select Fields**
   - Drag fields from left panel to right:
     ```
     Selected Fields:
     1. TenantName (from Tenants)
     2. RegionName (from Regions)
     3. ReportingPeriod (from Snapshot)
     4. TotalComputers (from Snapshot)
     5. OperationalComputers (from Snapshot)
     6. [Calculated] UptimePercent = (Operational / Total) * 100
     ```
4. **Step 3: Add Filters**
   - Filter 1: ReportingPeriod = [Parameter: @Month]
   - Filter 2: TotalComputers >= 10
   - Filter 3: RegionId = @CurrentUserRegionId (auto-set)
5. **Step 4: Group & Sort**
   - Group by: RegionName
   - Aggregate: SUM(TotalComputers), AVG(UptimePercent)
   - Sort: AVG(UptimePercent) DESC
6. **Step 5: Preview**
   - Run with sample parameters
   - View results (10 rows shown)
   - Check query performance: 1.2 seconds ✅
7. **Configure Visualization**:
   - Chart type: [Bar Chart ▼]
   - X-axis: RegionName
   - Y-axis: AVG(UptimePercent)
   - Color: Traffic light thresholds
8. **Save Report**:
   - Name: "Regional Computer Uptime Comparison"
   - Description: "Average computer uptime by region"
   - Category: [Performance ▼]
   - Visibility: [My Region ▼] (Private, My Region, All Users)
9. **Publish** - Report available in catalog

---

## Integration with Other Sections

### Section 3: Metrics & KPI Tracking

**Data Flow:**
```
MetricDefinitions (20+ metrics defined)
    ↓
TenantMetrics (populated from forms/inventory)
    ↓
TenantPerformanceSnapshot (nightly aggregation)
    ↓
Reports & Dashboards
```

**Key Metrics Used:**
- TOTAL_COMPUTERS (from hardware inventory)
- COMPUTER_UPTIME_PCT (calculated from form submission)
- SOFTWARE_COMPLIANCE_PCT (from license tracking)
- NETWORK_UPTIME_PCT (from monthly checklist)
- FORM_SUBMISSION_RATE (from submission tracking)

**Query Example:**
```sql
SELECT
    t.TenantName,
    tm.NumericValue AS ComputerCount,
    tm.CapturedDate
FROM TenantMetrics tm
INNER JOIN Tenants t ON tm.TenantId = t.TenantId
INNER JOIN MetricDefinitions md ON tm.MetricId = md.MetricId
WHERE md.MetricCode = 'TOTAL_COMPUTERS'
  AND tm.ReportingPeriod = '2025-10-01'
```

---

### Section 4: Form Templates & Submissions

**Data Flow:**
```
FormTemplateSubmissions (forms submitted)
    ↓
FormTemplateResponses (EAV data)
    ↓
MetricPopulationService (extract metrics)
    ↓
TenantMetrics (store calculated values)
    ↓
Reports
```

**Reports Dependent on Forms:**
1. **Form Submission Compliance Report**
   - Which factories submitted on time?
   - Overdue submissions (past deadline)
   - Submission rate trend

2. **Form Response Analysis**
   - Common answers (e.g., "Is LAN working?" → 95% Yes)
   - Outlier detection (factory reported 0 computers)

**Query Example:**
```sql
-- Find factories that didn't submit this month
SELECT
    t.TenantName,
    a.DueDate,
    DATEDIFF(DAY, a.DueDate, GETUTCDATE()) AS DaysOverdue
FROM FormTemplateAssignments a
INNER JOIN Tenants t ON a.TenantId = t.TenantId
LEFT JOIN FormTemplateSubmissions s
    ON s.TemplateId = a.TemplateId
    AND s.TenantId = a.TenantId
    AND s.ReportingPeriod = a.AssignedDate
WHERE s.SubmissionId IS NULL
  AND a.DueDate < GETUTCDATE()
  AND a.IsActive = 1
```

---

### Section 5: Software Management

**Data Flow:**
```
SoftwareProducts (catalog)
    ↓
TenantSoftwareInstallations (what's installed where)
    ↓
SoftwareLicenses (license tracking)
    ↓
Software Compliance Reports
```

**Key Reports:**
1. **Software Compliance Report**
   - Licensed vs unlicensed installations
   - License expiry warnings (next 90 days)
   - Over-allocated licenses

2. **Software Inventory Report**
   - What software is installed per factory
   - Version currency (outdated versions)
   - Security vulnerabilities

**Query Example:**
```sql
-- Unlicensed software installations
SELECT
    t.TenantName,
    sp.ProductName,
    COUNT(*) AS UnlicensedInstallations
FROM TenantSoftwareInstallations tsi
INNER JOIN SoftwareProducts sp ON tsi.ProductId = sp.ProductId
INNER JOIN Tenants t ON tsi.TenantId = t.TenantId
WHERE tsi.LicenseId IS NULL
  AND sp.RequiresLicense = 1
  AND tsi.Status = 'Active'
GROUP BY t.TenantName, sp.ProductName
HAVING COUNT(*) > 0
```

---

### Section 6: Hardware Inventory

**Data Flow:**
```
HardwareItems (master catalog)
    ↓
TenantHardware (actual inventory per factory)
    ↓
Hardware Status Reports
```

**Key Reports:**
1. **Hardware Inventory Summary**
   - Total hardware count per factory
   - Operational vs faulty count
   - Hardware aging (purchase date analysis)

2. **Hardware Maintenance Report**
   - Maintenance due/overdue
   - Disposal tracking

**Query Example:**
```sql
-- Hardware summary per factory
SELECT
    t.TenantName,
    hc.CategoryName,
    COUNT(*) AS TotalItems,
    SUM(CASE WHEN th.Status = 'Operational' THEN 1 ELSE 0 END) AS Operational,
    SUM(CASE WHEN th.Status = 'Faulty' THEN 1 ELSE 0 END) AS Faulty
FROM TenantHardware th
INNER JOIN HardwareItems hi ON th.HardwareItemId = hi.HardwareItemId
INNER JOIN HardwareCategories hc ON hi.CategoryId = hc.CategoryId
INNER JOIN Tenants t ON th.TenantId = t.TenantId
WHERE th.IsActive = 1
GROUP BY t.TenantName, hc.CategoryName
ORDER BY t.TenantName, hc.CategoryName
```

---

## Pre-Built Report Templates

### Template 1: Factory Performance Dashboard
**Purpose:** Comprehensive factory view
**Audience:** Factory ICT Officer
**Frequency:** Real-time
**Data Sources:** TenantPerformanceSnapshot, TenantMetrics
**Sections:**
1. KPI Summary Cards (Hardware, Software, Network, Forms)
2. Hardware Status Chart (Operational vs Faulty)
3. Software License Usage (Pie chart)
4. 6-Month Trend Line (Computer count over time)
5. Form Submission Timeline
6. Recent Alerts/Issues

---

### Template 2: Regional Comparison Report
**Purpose:** Compare all factories in a region
**Audience:** Regional Manager
**Frequency:** Monthly
**Data Sources:** RegionalMonthlySnapshot, TenantPerformanceSnapshot
**Sections:**
1. Regional Summary (Total factories, aggregated metrics)
2. Factory Ranking Table (sorted by performance score)
3. Heatmap Visualization (geographic view)
4. Top 3 / Bottom 3 Performers
5. Month-over-Month Trends
6. Compliance Exceptions (red flags)

---

### Template 3: Executive Dashboard
**Purpose:** System-wide view for management
**Audience:** Head Office ICT Manager, CIO
**Frequency:** Monthly/Quarterly
**Data Sources:** All snapshots, aggregated across 67 factories
**Sections:**
1. National Summary (67 factories, 6 regions)
2. Regional Performance Comparison (bar chart)
3. Key Metrics Trend (12 months)
4. Compliance Scorecard (traffic lights)
5. Critical Alerts (urgent issues)
6. Budget vs Actual (if financial data available)

---

### Template 4: Software Compliance Report
**Purpose:** License audit and compliance tracking
**Audience:** Head Office ICT Manager, Procurement
**Frequency:** Quarterly
**Data Sources:** SoftwareProducts, TenantSoftwareInstallations, SoftwareLicenses
**Sections:**
1. License Compliance Summary (compliant, over-allocated, unlicensed)
2. Unlicensed Installations Detail Table
3. License Expiry Warnings (next 90 days)
4. Cost Analysis (license spend by product)
5. Recommendations (purchase, reclaim, remove)

---

### Template 5: Hardware Asset Audit
**Purpose:** Physical asset tracking and verification
**Audience:** Regional Manager, Auditors
**Frequency:** Annually
**Data Sources:** TenantHardware, HardwareItems
**Sections:**
1. Total Asset Count (by category)
2. Asset Status Distribution (operational, faulty, disposal)
3. Asset Aging Analysis (0-2 years, 3-5 years, 5+ years)
4. Maintenance Due/Overdue
5. Disposal Recommendations
6. Asset Verification Status (last verified date)

---

## Report Lifecycle

```
┌─────────┐      ┌──────────┐      ┌───────────┐      ┌──────────┐
│ Design  │ ───> │   Test   │ ───> │  Publish  │ ───> │ Schedule │
└─────────┘      └──────────┘      └───────────┘      └──────────┘
     │                                    │                  │
     │                                    │                  │
     v                                    v                  v
   Draft                              Published          Active
(Editable)                          (Read-only)       (Auto-run)
     │                                    │                  │
     │                                    │                  │
     v                                    v                  v
 [Delete]                             [Archive]        [Deactivate]
```

**State Transitions:**
- **Draft → Test:** Report designer runs preview with sample data
- **Test → Publish:** Validation passes, report becomes available
- **Published → Schedule:** Report can be scheduled for automation
- **Published → Archive:** Report no longer needed but kept for history
- **Schedule → Active:** Background jobs execute report at scheduled times

---

## Performance Considerations

### Data Volumes
- **Current:** 67 factories × 12 months × 20 metrics = 16,080 data points
- **5-Year Retention:** 67 × 60 months × 20 = 80,400 data points
- **With 200 factories (future):** 200 × 60 × 20 = 240,000 data points

### Query Performance Targets
- **Dashboard load:** < 2 seconds
- **Report generation:** < 5 seconds (for < 5,000 rows)
- **Export to Excel:** < 10 seconds (for < 10,000 rows)
- **Snapshot generation:** < 10 minutes (nightly job for all 67 factories)

### Optimization Strategies
1. **Pre-aggregation:** Nightly snapshot generation
2. **Caching:** Redis cache for frequently accessed reports (30-minute TTL)
3. **Indexing:** Covering indexes on TenantPerformanceSnapshot
4. **Partitioning:** TenantMetrics table partitioned by ReportingPeriod
5. **Async Export:** Background jobs for large exports

---

## Security & Access Control

### Row-Level Security
```sql
-- Regional Manager sees only their region
WHERE RegionId = @CurrentUserRegionId

-- Factory ICT Officer sees only their factory
WHERE TenantId = @CurrentUserTenantId

-- Head Office sees all
-- No WHERE clause restriction
```

### Column-Level Security
- **Sensitive Metrics:** Cost data, salary info (if tracked)
- **Restricted Access:** Only `Reports.ViewFinancialData` permission
- **Masking:** Show only to authorized users, others see "***"

### Report Access Control Matrix
```
Report Type          | Factory | Regional | Head Office | External
─────────────────────┼─────────┼──────────┼─────────────┼──────────
Factory Dashboard    | Own     | All      | All         | None
Regional Summary     | None    | Own      | All         | None
Executive Dashboard  | None    | None     | All         | View-only
Compliance Report    | None    | Own      | All         | View-only
Custom Reports       | Own     | Create   | Create/Pub  | None
```

---

## Next Steps

**Document to Read Next:** `1_Section10_DataFlow_Architecture.md`

This document covers:
- Data aggregation pipeline architecture
- ETL processes (Extract, Transform, Load)
- State machines for report execution
- Detailed SQL query patterns
- API endpoint structure
- Caching strategy diagrams

---

**Status:** ✅ Overview Complete
**Use Case:** Stakeholder alignment, developer orientation, business requirements
