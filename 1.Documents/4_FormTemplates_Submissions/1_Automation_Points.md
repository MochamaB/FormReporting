# Checklist System - Automation & Pre-fill Points

**Version:** 1.0
**Date:** October 30, 2025
**Purpose:** Document automation opportunities for pre-filling checklist responses from existing system data
**Priority:** CRITICAL - Reduces manual data entry and ensures data consistency

---

## Table of Contents

1. [Overview](#overview)
2. [Section Management & Data Models](#section-management--data-models)
3. [Hardware Inventory Pre-fill Points](#hardware-inventory-pre-fill-points)
4. [Software Installation Pre-fill Points](#software-installation-pre-fill-points)
5. [Network & Infrastructure Pre-fill Points](#network--infrastructure-pre-fill-points)
6. [Support Tickets Pre-fill Points](#support-tickets-pre-fill-points)
7. [Financial Data Pre-fill Points](#financial-data-pre-fill-points)
8. [Technical Implementation](#technical-implementation)
9. [Pre-fill Algorithm](#pre-fill-algorithm)
10. [Data Validation Rules](#data-validation-rules)

---

## Overview

### The Problem

Currently, when factory ICT staff fill out monthly checklists, they manually enter data that already exists in various parts of the system:

❌ **Manual Entry Pain Points:**
- Counting hardware from inventory → typing into checklist
- Checking software versions → manually entering version numbers
- Calculating ticket statistics → adding up numbers manually
- Retrieving backup dates → looking up and entering dates
- Summing expenses → manual calculation from receipts

### The Solution

**Smart Pre-filling:** Automatically populate checklist responses from existing database tables.

✅ **Benefits:**
- Reduces data entry time by 60-70%
- Eliminates human counting/typing errors
- Ensures consistency between inventory and reports
- Allows users to verify and override if needed
- Creates audit trail of automatic vs manual entries

### Automation Strategy

```
┌────────────────────────────────────────────────┐
│ User opens checklist form                     │
└────────────────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────┐
│ System identifies tenant (e.g., Kambaa Factory)│
└────────────────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────┐
│ System analyzes each checklist item:          │
│ - Can this be pre-filled from inventory?      │
│ - Is there a calculation available?           │
│ - Is data already in database?                │
└────────────────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────┐
│ System executes pre-fill queries               │
└────────────────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────┐
│ Form displays with pre-filled values           │
│ (user can review and modify)                   │
└────────────────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────┐
│ User submits (tracks which fields were edited) │
└────────────────────────────────────────────────┘
```

---

## Section Management & Data Models

### How ChecklistSections Relate to Domain Models

The `ChecklistSections` table provides logical grouping that maps directly to database domain areas:

| ChecklistSection Name | Related Database Tables | Domain Model |
|----------------------|------------------------|--------------|
| Hardware Status | `TenantHardware`, `HardwareItems`, `HardwareCategories` | Hardware Inventory Module |
| Software Licenses | `TenantSoftwareInstallations`, `SoftwareProducts`, `SoftwareVersions` | Software Management Module |
| Network Infrastructure | `TenantMetrics` (network-related metrics) | Metrics & KPI Module |
| Support Tickets | `Tickets`, `TicketCategories`, `TicketComments` | Support Tickets Module |
| Backup & Security | `TenantMetrics` (backup metrics), `TenantSoftwareInstallations` (firewall) | Metrics & Software Modules |
| Financial Summary | `TenantExpenses`, `TenantBudgets` | Financial Tracking Module |

### Example: Hardware Status Section Structure

```
ChecklistTemplate: "Factory Monthly Report" (TemplateId = 1)
  ↓
ChecklistSection: "Hardware Status" (SectionId = 1, TemplateId = 1)
  ↓
ChecklistItems within section:
  ├─ ItemId: 1 - "Total number of computers" (SectionId = 1)
  │   → PRE-FILL FROM: SELECT COUNT(*) FROM TenantHardware WHERE TenantId = @TenantId AND HardwareItemId IN (Laptop, Desktop)
  │
  ├─ ItemId: 2 - "Operational computers" (SectionId = 1)
  │   → PRE-FILL FROM: SELECT COUNT(*) FROM TenantHardware WHERE TenantId = @TenantId AND Status = 'Operational' AND HardwareItemId IN (Laptop, Desktop)
  │
  ├─ ItemId: 3 - "Computers under repair" (SectionId = 1)
  │   → PRE-FILL FROM: SELECT COUNT(*) FROM TenantHardware WHERE TenantId = @TenantId AND Status = 'Under Repair' AND HardwareItemId IN (Laptop, Desktop)
  │
  └─ ItemId: 4 - "Any hardware failures this month?" (SectionId = 1)
      → PRE-FILL FROM: SELECT CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END FROM HardwareMaintenanceLog WHERE TenantHardwareId IN (SELECT TenantHardwareId FROM TenantHardware WHERE TenantId = @TenantId) AND MaintenanceType = 'Corrective' AND MaintenanceDate >= @MonthStart
```

This demonstrates how a **ChecklistSection** acts as a logical container that groups related **ChecklistItems**, which in turn map to queries against specific **domain model tables**.

---

## Hardware Inventory Pre-fill Points

### Pre-fill Opportunity 1: Computer Counts

**Checklist Question:** "Total number of computers"

**Data Source:** `TenantHardware` table

**Pre-fill Query:**
```sql
SELECT COUNT(*) AS TotalComputers
FROM TenantHardware th
JOIN HardwareItems hi ON th.HardwareItemId = hi.HardwareItemId
WHERE th.TenantId = @TenantId
  AND hi.ItemName IN ('Desktop PC', 'Laptop')
  AND th.Status != 'Retired';
```

**Business Logic:**
- Counts all non-retired desktops and laptops
- Excludes servers, printers, and other devices
- User can override if physical count differs

---

### Pre-fill Opportunity 2: Hardware Status Breakdown

**Checklist Questions:**
- "Operational computers"
- "Computers under repair"
- "Faulty computers"

**Data Source:** `TenantHardware.Status` column

**Pre-fill Query:**
```sql
SELECT
    SUM(CASE WHEN Status = 'Operational' THEN 1 ELSE 0 END) AS Operational,
    SUM(CASE WHEN Status = 'Under Repair' THEN 1 ELSE 0 END) AS UnderRepair,
    SUM(CASE WHEN Status = 'Faulty' THEN 1 ELSE 0 END) AS Faulty
FROM TenantHardware th
JOIN HardwareItems hi ON th.HardwareItemId = hi.HardwareItemId
WHERE th.TenantId = @TenantId
  AND hi.ItemName IN ('Desktop PC', 'Laptop');
```

**Validation Rule:**
```
Operational + UnderRepair + Faulty = Total Computers
```

If user edits one value, system can auto-adjust others or flag inconsistency.

---

### Pre-fill Opportunity 3: Hardware Failures This Month

**Checklist Question:** "Any hardware failures this month?" (Boolean)

**Data Source:** `HardwareMaintenanceLog` table

**Pre-fill Query:**
```sql
SELECT CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS HadFailures
FROM HardwareMaintenanceLog hml
JOIN TenantHardware th ON hml.TenantHardwareId = th.TenantHardwareId
WHERE th.TenantId = @TenantId
  AND hml.MaintenanceType = 'Corrective'
  AND hml.MaintenanceDate >= @ReportingMonthStart
  AND hml.MaintenanceDate < @ReportingMonthEnd;
```

**Conditional Follow-up:**
If answer = Yes, show additional question: "Describe hardware failures" (TextArea)
→ Pre-fill with: List of failure descriptions from `HardwareMaintenanceLog.Description`

---

### Pre-fill Opportunity 4: Printer Count by Type

**Checklist Question:** "Number of printers"

**Data Source:** `TenantHardware` + `HardwareItems`

**Pre-fill Query:**
```sql
SELECT COUNT(*) AS TotalPrinters
FROM TenantHardware th
JOIN HardwareItems hi ON th.HardwareItemId = hi.HardwareItemId
JOIN HardwareCategories hc ON hi.CategoryId = hc.CategoryId
WHERE th.TenantId = @TenantId
  AND hc.CategoryCode = 'PRINTER'
  AND th.Status IN ('Operational', 'Under Repair');
```

---

## Software Installation Pre-fill Points

### Pre-fill Opportunity 5: Software Version Numbers

**Checklist Questions:**
- "EWS Version"
- "Chaipro Financials Version"
- "Chaipro Factory Version"

**Data Source:** `TenantSoftwareInstallations` + `SoftwareVersions`

**Pre-fill Query:**
```sql
SELECT
    sp.ProductName,
    sv.VersionNumber,
    tsi.Status
FROM TenantSoftwareInstallations tsi
JOIN SoftwareProducts sp ON tsi.ProductId = sp.ProductId
JOIN SoftwareVersions sv ON tsi.VersionId = sv.VersionId
WHERE tsi.TenantId = @TenantId
  AND sp.ProductCode IN ('EWS', 'CHAIPRO_FIN', 'CHAIPRO_FACTORY');
```

**Example Result:**
- EWS Version: `v3.2.1`
- Chaipro Financials: `v2.5.0`
- Chaipro Factory: `v2.5.0`

**User Override:** If factory manually upgraded recently, user can update version number.

---

### Pre-fill Opportunity 6: License Counts

**Checklist Question:** "Total Windows licenses active"

**Data Source:** `TenantSoftwareInstallations.LicenseCount`

**Pre-fill Query:**
```sql
SELECT tsi.LicenseCount
FROM TenantSoftwareInstallations tsi
JOIN SoftwareProducts sp ON tsi.ProductId = sp.ProductId
WHERE tsi.TenantId = @TenantId
  AND sp.ProductName LIKE '%Windows%'
  AND tsi.Status = 'Active';
```

---

### Pre-fill Opportunity 7: License Expiry Warnings

**Checklist Question:** "Any software licensing issues?" (Boolean)

**Data Source:** `TenantSoftwareInstallations.LicenseExpiryDate`

**Pre-fill Query:**
```sql
SELECT CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS HasLicenseIssues
FROM TenantSoftwareInstallations
WHERE TenantId = @TenantId
  AND (
    LicenseExpiryDate < GETDATE()  -- Expired
    OR LicenseExpiryDate < DATEADD(DAY, 30, GETDATE())  -- Expiring soon
  );
```

**Conditional Follow-up:**
If Yes, show: "Describe licensing issues" (TextArea)
→ Pre-fill with:
```sql
SELECT CONCAT(
    sp.ProductName,
    ' - License ',
    CASE
        WHEN LicenseExpiryDate < GETDATE() THEN 'EXPIRED on '
        ELSE 'expiring on '
    END,
    FORMAT(LicenseExpiryDate, 'yyyy-MM-dd')
)
FROM TenantSoftwareInstallations tsi
JOIN SoftwareProducts sp ON tsi.ProductId = sp.ProductId
WHERE tsi.TenantId = @TenantId
  AND LicenseExpiryDate < DATEADD(DAY, 30, GETDATE());
```

**Example Pre-fill:**
```
Sophos Firewall - License EXPIRED on 2025-09-15
Office 365 - License expiring on 2025-11-20
```

---

### Pre-fill Opportunity 8: Antivirus Update Status

**Checklist Questions:**
- "Antivirus definitions up to date?" (Boolean)
- "Last antivirus update date" (Date)

**Data Source:** `TenantMetrics` (if tracked as metric) OR `TenantSoftwareInstallations.LastVerifiedDate`

**Pre-fill Query:**
```sql
SELECT
    CASE WHEN LastVerifiedDate >= DATEADD(DAY, -7, GETDATE()) THEN 1 ELSE 0 END AS IsUpToDate,
    LastVerifiedDate
FROM TenantSoftwareInstallations tsi
JOIN SoftwareProducts sp ON tsi.ProductId = sp.ProductId
WHERE tsi.TenantId = @TenantId
  AND sp.ProductCategory = 'Security'
  AND sp.ProductName LIKE '%Antivirus%';
```

---

## Network & Infrastructure Pre-fill Points

### Pre-fill Opportunity 9: Network Uptime Percentage

**Checklist Question:** "Network uptime percentage"

**Data Source:** `TenantMetrics` table

**Pre-fill Query:**
```sql
SELECT NumericValue AS UptimePercentage
FROM TenantMetrics tm
JOIN MetricDefinitions md ON tm.MetricId = md.MetricId
WHERE tm.TenantId = @TenantId
  AND md.MetricCode = 'SYSTEM_UPTIME_PCT'
  AND tm.ReportingPeriod = @LastReportingPeriod;
```

**Alternative (if tracked via system monitoring):**
```sql
-- Calculate from downtime logs
SELECT
    100.0 - (
        SUM(DATEDIFF(MINUTE, DowntimeStart, ISNULL(DowntimeEnd, GETDATE()))) * 100.0
        / (DATEDIFF(MINUTE, @MonthStart, @MonthEnd))
    ) AS UptimePercentage
FROM NetworkDowntimeLog
WHERE TenantId = @TenantId
  AND DowntimeStart >= @MonthStart
  AND DowntimeStart < @MonthEnd;
```

---

### Pre-fill Opportunity 10: Backup Status

**Checklist Questions:**
- "Backup performed this month?" (Boolean)
- "Date of last backup" (Date)
- "Backup successful?" (Boolean)

**Data Source:** `TenantMetrics` OR dedicated `BackupLog` table (if exists)

**Pre-fill Query:**
```sql
SELECT TOP 1
    CASE WHEN BackupDate IS NOT NULL THEN 1 ELSE 0 END AS BackupPerformed,
    BackupDate AS LastBackupDate,
    CASE WHEN BackupStatus = 'Success' THEN 1 ELSE 0 END AS BackupSuccessful
FROM BackupLog
WHERE TenantId = @TenantId
  AND BackupDate >= @MonthStart
ORDER BY BackupDate DESC;
```

---

### Pre-fill Opportunity 11: Internet Provider

**Checklist Question:** "Internet provider" (Dropdown)

**Data Source:** `TenantMetrics` (last recorded value)

**Pre-fill Query:**
```sql
SELECT TOP 1 TextValue AS InternetProvider
FROM TenantMetrics tm
JOIN MetricDefinitions md ON tm.MetricId = md.MetricId
WHERE tm.TenantId = @TenantId
  AND md.MetricCode = 'INTERNET_PROVIDER'
ORDER BY tm.ReportingPeriod DESC;
```

**Business Logic:**
- Provider rarely changes, so last recorded value is usually correct
- User can select different value if provider changed this month

---

## Support Tickets Pre-fill Points

### Pre-fill Opportunity 12: Ticket Statistics

**Checklist Questions:**
- "Total support tickets received"
- "Tickets resolved"
- "Tickets pending"
- "Average resolution time (hours)"

**Data Source:** `Tickets` table

**Pre-fill Query:**
```sql
SELECT
    COUNT(*) AS TotalTickets,
    SUM(CASE WHEN Status IN ('Resolved', 'Closed') THEN 1 ELSE 0 END) AS ResolvedTickets,
    SUM(CASE WHEN Status IN ('Open', 'InProgress', 'Escalated') THEN 1 ELSE 0 END) AS PendingTickets,
    AVG(
        CASE WHEN Status = 'Resolved' AND ResolvedDate IS NOT NULL
        THEN DATEDIFF(HOUR, ReportedDate, ResolvedDate)
        ELSE NULL END
    ) AS AvgResolutionHours
FROM Tickets
WHERE TenantId = @TenantId
  AND ReportedDate >= @MonthStart
  AND ReportedDate < @MonthEnd;
```

**Validation Rule:**
```
Resolved + Pending = Total Tickets
```

---

### Pre-fill Opportunity 13: Most Common Issue Type

**Checklist Question:** "Most common issue type" (Dropdown)

**Data Source:** `Tickets` + `TicketCategories`

**Pre-fill Query:**
```sql
SELECT TOP 1 tc.CategoryName AS MostCommonIssue
FROM Tickets t
JOIN TicketCategories tc ON t.CategoryId = tc.CategoryId
WHERE t.TenantId = @TenantId
  AND t.ReportedDate >= @MonthStart
  AND t.ReportedDate < @MonthEnd
GROUP BY tc.CategoryName
ORDER BY COUNT(*) DESC;
```

---

### Pre-fill Opportunity 14: Critical Unresolved Issues

**Checklist Question:** "Any critical unresolved issues?" (Boolean)

**Data Source:** `Tickets` table

**Pre-fill Query:**
```sql
SELECT CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS HasCriticalIssues
FROM Tickets
WHERE TenantId = @TenantId
  AND Priority IN ('Critical', 'High')
  AND Status NOT IN ('Resolved', 'Closed');
```

**Conditional Follow-up:**
If Yes, show: "Describe critical issues" (TextArea)
→ Pre-fill with ticket titles:
```sql
SELECT STRING_AGG(
    CONCAT('#', TicketNumber, ' - ', Title),
    CHAR(13) + CHAR(10)  -- Newline
) AS CriticalIssuesList
FROM Tickets
WHERE TenantId = @TenantId
  AND Priority IN ('Critical', 'High')
  AND Status NOT IN ('Resolved', 'Closed');
```

**Example Pre-fill:**
```
#TKT-2025-00145 - ERP database connection timeout
#TKT-2025-00152 - Email server not responding
```

---

## Financial Data Pre-fill Points

### Pre-fill Opportunity 15: Monthly ICT Expenses

**Checklist Question:** "Total ICT expenses this month"

**Data Source:** `TenantExpenses` table

**Pre-fill Query:**
```sql
SELECT SUM(Amount) AS TotalExpenses
FROM TenantExpenses
WHERE TenantId = @TenantId
  AND ExpenseDate >= @MonthStart
  AND ExpenseDate < @MonthEnd
  AND CategoryId IN (
      SELECT CategoryId FROM BudgetCategories
      WHERE CategoryCode IN ('HARDWARE_PURCHASE', 'SOFTWARE_LICENSE', 'MAINTENANCE', 'NETWORK_COSTS')
  );
```

---

### Pre-fill Opportunity 16: Budget Utilization

**Checklist Question:** "Budget utilization percentage"

**Data Source:** `TenantBudgets` + `TenantExpenses`

**Pre-fill Query:**
```sql
SELECT
    CAST(
        (SUM(te.Amount) * 100.0) / NULLIF(tb.BudgetedAmount, 0)
    AS DECIMAL(5,2)) AS BudgetUtilizationPct
FROM TenantBudgets tb
LEFT JOIN TenantExpenses te ON tb.TenantId = te.TenantId
    AND tb.CategoryId = te.CategoryId
    AND YEAR(te.ExpenseDate) = tb.FiscalYear
WHERE tb.TenantId = @TenantId
  AND tb.FiscalYear = YEAR(@MonthStart);
```

---

## Technical Implementation

### Architecture: Pre-fill Service Layer

```
┌────────────────────────────────────────────────────────────┐
│ Presentation Layer (ChecklistController)                  │
└────────────────────────────────────────────────────────────┘
                        ↓
┌────────────────────────────────────────────────────────────┐
│ Application Layer (ChecklistService)                      │
│ - CreateDraftSubmission(templateId, tenantId)             │
└────────────────────────────────────────────────────────────┘
                        ↓
┌────────────────────────────────────────────────────────────┐
│ Domain Layer (ChecklistPreFillService)                    │
│ - PreFillResponses(submissionId)                          │
│ - GetPreFillValue(checklistItemId, tenantId, period)      │
└────────────────────────────────────────────────────────────┘
                        ↓
┌────────────────────────────────────────────────────────────┐
│ Infrastructure Layer (Repositories)                       │
│ - HardwareRepository.CountByTenant(tenantId)              │
│ - SoftwareRepository.GetVersions(tenantId)                │
│ - TicketRepository.GetMonthlyStats(tenantId, month)       │
└────────────────────────────────────────────────────────────┘
```

### Code Example: Pre-fill Service

```csharp
// Application/Services/ChecklistPreFillService.cs
public class ChecklistPreFillService
{
    private readonly IHardwareRepository _hardwareRepo;
    private readonly ISoftwareRepository _softwareRepo;
    private readonly ITicketRepository _ticketRepo;
    private readonly IMetricsRepository _metricsRepo;
    private readonly IChecklistRepository _checklistRepo;

    public async Task<Dictionary<int, object>> PreFillResponses(
        int templateId,
        int tenantId,
        DateTime reportingPeriod)
    {
        var preFillValues = new Dictionary<int, object>();
        var items = await _checklistRepo.GetItemsByTemplate(templateId);

        foreach (var item in items)
        {
            var value = await GetPreFillValue(item, tenantId, reportingPeriod);
            if (value != null)
            {
                preFillValues[item.ItemId] = value;
            }
        }

        return preFillValues;
    }

    private async Task<object> GetPreFillValue(
        ChecklistItem item,
        int tenantId,
        DateTime period)
    {
        // Match by question pattern or item code
        return item.ItemCode switch
        {
            "TOTAL_COMPUTERS" => await _hardwareRepo.CountComputers(tenantId),

            "OPERATIONAL_COMPUTERS" => await _hardwareRepo.CountByStatus(
                tenantId, HardwareStatus.Operational),

            "COMPUTERS_UNDER_REPAIR" => await _hardwareRepo.CountByStatus(
                tenantId, HardwareStatus.UnderRepair),

            "EWS_VERSION" => await _softwareRepo.GetVersionNumber(
                tenantId, "EWS"),

            "WINDOWS_LICENSES" => await _softwareRepo.GetLicenseCount(
                tenantId, "Windows"),

            "NETWORK_UPTIME_PCT" => await _metricsRepo.GetLatestMetric(
                tenantId, "SYSTEM_UPTIME_PCT"),

            "TOTAL_TICKETS" => await _ticketRepo.CountTicketsInPeriod(
                tenantId, period),

            "RESOLVED_TICKETS" => await _ticketRepo.CountResolvedInPeriod(
                tenantId, period),

            "PENDING_TICKETS" => await _ticketRepo.CountPendingTickets(
                tenantId),

            "AVG_RESOLUTION_TIME" => await _ticketRepo.GetAvgResolutionHours(
                tenantId, period),

            _ => await TryInferFromQuestionText(item, tenantId, period)
        };
    }

    private async Task<object> TryInferFromQuestionText(
        ChecklistItem item,
        int tenantId,
        DateTime period)
    {
        var question = item.ItemName.ToLower();

        // Pattern matching on question text
        if (question.Contains("total") && question.Contains("computer"))
            return await _hardwareRepo.CountComputers(tenantId);

        if (question.Contains("backup") && question.Contains("date"))
            return await _metricsRepo.GetLastBackupDate(tenantId);

        if (question.Contains("license") && question.Contains("expir"))
            return await _softwareRepo.CheckLicenseExpiry(tenantId);

        // No pre-fill available
        return null;
    }
}
```

---

### Code Example: Controller Usage

```csharp
// Web/Controllers/ChecklistController.cs
public async Task<IActionResult> FillChecklist(int templateId)
{
    var tenantId = User.GetTenantId();
    var reportingPeriod = DateTime.Now.Date;

    // Create draft submission
    var submission = await _checklistService.CreateDraftSubmission(
        templateId, tenantId, reportingPeriod);

    // Pre-fill responses
    var preFillValues = await _preFillService.PreFillResponses(
        templateId, tenantId, reportingPeriod);

    // Save pre-filled responses to database
    foreach (var (itemId, value) in preFillValues)
    {
        await _responseRepo.AddAsync(new ChecklistResponse
        {
            SubmissionId = submission.SubmissionId,
            ItemId = itemId,
            NumericValue = value is decimal d ? d : null,
            TextValue = value is string s ? s : null,
            DateValue = value is DateTime dt ? dt : null,
            BooleanValue = value is bool b ? b : null,
            IsPreFilled = true  // Track that this was auto-generated
        });
    }

    await _unitOfWork.SaveChangesAsync();

    // Return view with pre-filled form
    return View("FillForm", new ChecklistViewModel
    {
        Template = await _checklistRepo.GetTemplateById(templateId),
        Submission = submission,
        PreFilledResponses = preFillValues
    });
}
```

---

## Pre-fill Algorithm

### Decision Tree for Pre-filling

```
For each ChecklistItem in Template:
    ↓
    1. Check ItemCode
       ├─ Match known code? → Use specific query
       └─ No match? → Go to step 2
    ↓
    2. Check SectionId
       ├─ Hardware section? → Try hardware queries
       ├─ Software section? → Try software queries
       ├─ Tickets section? → Try ticket queries
       └─ Other? → Go to step 3
    ↓
    3. Analyze QuestionText (NLP-lite)
       ├─ Contains "total" + "computer"? → Count hardware
       ├─ Contains "version" + software name? → Get version
       ├─ Contains "ticket" + "resolved"? → Count resolved
       └─ No pattern match? → Go to step 4
    ↓
    4. Check DataType
       ├─ Number + "count"? → Try counting query
       ├─ Date + "last"? → Try getting max date
       ├─ Boolean + "any"? → Try existence check
       └─ No inference possible? → Skip pre-fill
    ↓
    5. Return pre-fill value OR null
```

---

## Data Validation Rules

### Validation After Pre-fill

Even with pre-filled data, validation ensures data integrity:

#### Rule 1: Sum Validation (Hardware)
```
Total Computers = Operational + UnderRepair + Faulty
```
If user edits one field, prompt to auto-adjust others.

#### Rule 2: Sum Validation (Tickets)
```
Total Tickets = Resolved + Pending
```

#### Rule 3: Percentage Range
```
0 ≤ Network Uptime % ≤ 100
0 ≤ Budget Utilization % ≤ 200 (allow over-budget)
```

#### Rule 4: Date Logic
```
Last Backup Date ≤ Today
Last Backup Date ≥ (Today - 31 days) (warn if older)
```

#### Rule 5: License Count vs Hardware Count
```
Windows Licenses ≥ Total Computers (warn if less)
```

#### Rule 6: Conditional Required Fields

If pre-fill sets "Hardware failures" = Yes, then "Describe failures" becomes REQUIRED.

---

## Summary

### Pre-fill Coverage by Section

| Section | Pre-fillable Items | Manual Items | Coverage % |
|---------|-------------------|--------------|------------|
| Hardware Status | 5/5 | 0/5 | 100% |
| Software Licenses | 6/8 | 2/8 | 75% |
| Network Infrastructure | 4/6 | 2/6 | 67% |
| Support Tickets | 5/5 | 0/5 | 100% |
| Backup & Security | 3/4 | 1/4 | 75% |
| Financial Summary | 2/3 | 1/3 | 67% |
| General Comments | 0/3 | 3/3 | 0% (text area) |
| **TOTAL** | **25/34** | **9/34** | **74%** |

**Result:** **74% of checklist items can be pre-filled automatically**, reducing manual data entry significantly.

### Items That Cannot Be Pre-filled

Some items require human observation and cannot be automated:

1. **Key achievements this month** (Text Area) - Subjective assessment
2. **Challenges faced** (Text Area) - Narrative description
3. **Support needed from Head Office** (Text Area) - Strategic requests
4. **Any network outages** (Boolean) - If not tracked in system
5. **Power outage incidents** (Text Area) - If not logged in system
6. **User training conducted** (Boolean) - Manual tracking
7. **New projects initiated** (Text Area) - Strategic information
8. **Security incidents** (Text Area) - Sensitive information
9. **Staff changes** (Text Area) - HR information

These fields add qualitative value that complements quantitative pre-filled data.

---

## Next Steps

### Implementation Priority

**Phase 1 (Week 7-8):** Basic Pre-fill
- Hardware counts (5 items)
- Software versions (3 items)
- Ticket statistics (5 items)

**Phase 2 (Week 9):** Advanced Pre-fill
- Financial data (2 items)
- Network metrics (4 items)
- Conditional pre-fill (6 items)

**Phase 3 (Week 10+):** Machine Learning
- Predict text responses based on historical patterns
- Suggest "typical" answers for recurring questions
- Anomaly detection (flag unusual values)

---

**Document Version:** 1.0
**Last Updated:** October 30, 2025
**Maintained By:** KTDA ICT Development Team
**Questions?** Contact system administrator or refer to [ChecklistSystem_Overview.md](ChecklistSystem_Overview.md)
