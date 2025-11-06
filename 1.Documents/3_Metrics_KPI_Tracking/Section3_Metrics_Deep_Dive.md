# SECTION 3: METRICS & KPI TRACKING - Deep Dive & Implementation Guide

**Module:** Performance Metrics, KPI Tracking, and Automated Monitoring
**Tables:** 3 Core + 2 Integration (MetricDefinitions, TenantMetrics, SystemMetricLogs, FormItemMetricMappings, MetricPopulationLog)
**Connected Modules:** Form Templates (Section 4), Reporting (Section 10), Alerts (Section 9)

---

## TABLE OF CONTENTS

1. [System Overview & Architecture](#1-system-overview--architecture)
2. [The Three Core Tables Explained](#2-the-three-core-tables-explained)
3. [Five Source Types Deep Dive](#3-five-source-types-deep-dive)
4. [Four Mapping Types for Form Integration](#4-four-mapping-types-for-form-integration)
5. [How Metrics Get Populated (The Complete Flow)](#5-how-metrics-get-populated-the-complete-flow)
6. [Defining Metrics - Step by Step](#6-defining-metrics---step-by-step)
7. [Connecting Forms to Metrics](#7-connecting-forms-to-metrics)
8. [Reporting & Visualization](#8-reporting--visualization)
9. [Real KTDA Examples](#9-real-ktda-examples)
10. [Workflows & Actions](#10-workflows--actions)
11. [Best Practices & Recommendations](#11-best-practices--recommendations)

---

## 1. SYSTEM OVERVIEW & ARCHITECTURE

### **What Problem Does This Solve?**

**Current State (Manual):**
- Factory ICT staff fill forms with infrastructure data
- Regional managers manually compile metrics from Excel
- No automatic calculation of KPIs (e.g., system availability %)
- No threshold alerts (e.g., "backup failed 3 days in a row")
- No time-series data for trend analysis
- Metrics scattered across different spreadsheets

**Target State (Automated):**
- User fills form → System **automatically** populates metrics
- Calculated metrics (e.g., availability = operational/total × 100) computed in real-time
- Background jobs check system health daily (e.g., backup status, license expiry)
- Threshold-based traffic lights (Green/Yellow/Red) on dashboards
- Historical trends ("LAN uptime last 6 months")
- Instant reporting ("Show me all factories with <95% system availability")

### **The Big Picture**

```
┌─────────────────────────────────────────────────────────────────────┐
│                    METRICS & KPI TRACKING SYSTEM                     │
└─────────────────────────────────────────────────────────────────────┘

INPUT SOURCES (How metrics get values):
├─ 1. User Input (from form submissions)
│  └─ User fills "Factory Monthly Report" → Auto-populates metrics
│
├─ 2. System Calculated (formulas from multiple form fields)
│  └─ Availability% = (Operational Computers / Total Computers) × 100
│
├─ 3. External System (API calls to monitoring tools)
│  └─ Pull network uptime from PRTG monitoring system
│
├─ 4. Compliance Tracking (deadline monitoring)
│  └─ "Was report submitted by 5th of month?" → Yes/No
│
└─ 5. Automated Checks (Hangfire background jobs)
   └─ Daily: Check if backup ran successfully, check license expiry

STORAGE:
├─ MetricDefinitions (What metrics exist, their rules)
├─ TenantMetrics (Actual metric values per tenant per period)
└─ SystemMetricLogs (Job execution logs, automated checks)

OUTPUT (How metrics are used):
├─ Dashboards (Real-time KPI widgets with traffic lights)
├─ Reports (Monthly summaries, trend analysis)
├─ Alerts (Threshold breaches → notifications)
└─ Analytics (Historical trends, forecasting)
```

---

## 2. THE THREE CORE TABLES EXPLAINED

### **2.1 MetricDefinitions - The "Master Catalog"**

**Purpose:** Defines WHAT metrics exist and HOW they should be tracked.

**Think of it as:** A recipe book. Each recipe (metric) has:
- Name and description
- Ingredients needed (source type)
- How to measure (data type, unit)
- What's "good" (thresholds)

**Key Fields:**

```sql
MetricDefinitions (
    MetricId,
    MetricCode          -- Unique identifier: 'LAN_STATUS', 'COMPUTER_AVAILABILITY'
    MetricName          -- Display name: "LAN Network Status", "Computer Availability %"
    Category            -- Group: Infrastructure, Software, Hardware, Performance, Compliance

    SourceType          -- HOW is this metric captured?
                        -- 'UserInput', 'SystemCalculated', 'ExternalSystem',
                        -- 'ComplianceTracking', 'AutomatedCheck'

    DataType            -- WHAT format? Integer, Decimal, Percentage, Boolean, Text
    Unit                -- HOW to display? Count, Percentage, Status, Days, GB
    AggregationType     -- HOW to summarize? SUM, AVG, MAX, MIN, LAST_VALUE

    IsKPI               -- Flag: Is this a Key Performance Indicator?
    ThresholdGreen      -- Target value (good)
    ThresholdYellow     -- Warning value
    ThresholdRed        -- Critical value

    ExpectedValue       -- For binary metrics: 'TRUE', 'Yes', 'Operational'
    ComplianceRule      -- JSON: Deadline rules, validation logic
)
```

**Example Metric Definition:**

```sql
INSERT INTO MetricDefinitions VALUES (
    MetricCode: 'COMPUTER_AVAILABILITY_PCT',
    MetricName: 'Computer Availability Percentage',
    Category: 'Hardware',
    SourceType: 'SystemCalculated',  -- Calculated from 2 form fields
    DataType: 'Percentage',
    Unit: 'Percentage',
    AggregationType: 'AVG',  -- Average across tenants for regional summary
    IsKPI: 1,  -- Yes, this is a KPI
    ThresholdGreen: 95.0,   -- ≥95% = Good (Green)
    ThresholdYellow: 85.0,  -- 85-94% = Warning (Yellow)
    ThresholdRed: 85.0,     -- <85% = Critical (Red)
    Description: 'Percentage of computers that are operational vs total inventory'
);
```

---

### **2.2 TenantMetrics - The "Data Warehouse"**

**Purpose:** Stores ACTUAL metric values for each tenant for each time period.

**Think of it as:** A massive spreadsheet where:
- Each row = One metric value for one tenant for one month
- Time-series data (track changes over time)
- Can store both numbers and text

**Key Fields:**

```sql
TenantMetrics (
    MetricValueId       -- Primary key
    TenantId            -- Which factory/office?
    MetricId            -- Which metric? (FK to MetricDefinitions)
    ReportingPeriod     -- Which month? (DATE: '2025-11-01')

    NumericValue        -- For numbers: 95.5, 12, 100
    TextValue           -- For text: 'Operational', 'Version 5.2.1'

    SourceType          -- Where did this value come from?
                        -- 'UserInput', 'SystemCalculated', 'HangfireJob'
    SourceReferenceId   -- Link back to source (SubmissionId or LogId)

    CapturedDate        -- When was this recorded?
    CapturedBy          -- Who recorded it? (NULL if automated)
)
```

**Example Data:**

```sql
-- Factory 50 (Kiambu) - November 2025 metrics
TenantId=50, MetricId=5, ReportingPeriod='2025-11-01'
  NumericValue=92.3, TextValue=NULL
  SourceType='SystemCalculated', SourceReferenceId=12345 (SubmissionId)

TenantId=50, MetricId=12, ReportingPeriod='2025-11-01'
  NumericValue=1, TextValue='Operational'
  SourceType='UserInput', SourceReferenceId=12345
```

**Important:**
- One row per (Tenant, Metric, Period) - enforced by UNIQUE constraint
- If user resubmits form, value is **UPDATED**, not duplicated
- Historical data preserved for trend analysis

---

### **2.3 SystemMetricLogs - The "Job Execution Log"**

**Purpose:** Tracks automated checks and background job results.

**Think of it as:** A logbook for automated health checks:
- Daily backup verification
- License expiry checks
- Network monitoring pulls
- Any Hangfire job that populates metrics

**Key Fields:**

```sql
SystemMetricLogs (
    LogId               -- Primary key
    TenantId            -- Which tenant was checked?
    MetricId            -- Which metric was measured?
    CheckDate           -- When did the job run?

    Status              -- 'Success', 'Failed', 'Warning'
    NumericValue        -- Measured value
    TextValue           -- Status message
    Details             -- JSON: Additional context

    JobName             -- Which Hangfire job? 'DailyBackupCheck'
    ExecutionDuration   -- How long did it take? (milliseconds)
    ErrorMessage        -- If failed, what went wrong?
)
```

**Example Log Entry:**

```sql
-- Daily backup check for Factory 50
LogId=789, TenantId=50, MetricId=15 (BACKUP_STATUS)
  CheckDate='2025-11-06 02:00:00'
  Status='Success'
  NumericValue=1, TextValue='Backup completed successfully'
  Details='{"backupSize": "150GB", "duration": "45min", "location": "\\backup\factory50"}'
  JobName='DailyBackupVerification'
  ExecutionDuration=2340 (2.3 seconds)
```

**Why Separate from TenantMetrics?**
- Detailed logging (execution time, errors, debugging)
- Multiple checks per day (TenantMetrics = one value per period)
- After successful check → Copies to TenantMetrics for reporting

---

## 3. FIVE SOURCE TYPES DEEP DIVE

### **3.1 UserInput - Direct from Form Submissions**

**When to use:** Metric value comes directly from a single form field answer.

**How it works:**
```
User fills form field → Value stored in FormTemplateResponses
                      → FormItemMetricMappings links field to metric
                      → Value copied to TenantMetrics
```

**Example:**

```
Form Field: "Is LAN working?"
  Type: Boolean (Yes/No)
  Answer: "Yes"

Metric Mapping:
  ItemId=45 (LAN field) → MetricId=10 (LAN_STATUS)
  MappingType='Direct'

Result in TenantMetrics:
  MetricId=10, NumericValue=1, TextValue='Yes'
  SourceType='UserInput', SourceReferenceId=12345 (SubmissionId)
```

**Real KTDA Examples:**
- LAN Status (Yes/No)
- WAN Type (Fiber/Microwave/Hybrid)
- Chaipro Financials Version (Text: "5.2.1.4")
- Total number of computers (Number: 25)
- Last backup date (Date: "2025-11-05")

---

### **3.2 SystemCalculated - Formula from Multiple Fields**

**When to use:** Metric requires calculation from 2+ form fields.

**How it works:**
```
User fills multiple fields → Formula defined in FormItemMetricMappings
                          → System calculates result
                          → Stores in TenantMetrics
```

**Example:**

```
Form Fields:
  - "Total computers" (ItemId=20): Answer = 25
  - "Operational computers" (ItemId=21): Answer = 23

Metric Mapping:
  ItemId=21 → MetricId=5 (COMPUTER_AVAILABILITY_PCT)
  MappingType='SystemCalculated'
  TransformationLogic='{
    "formula": "(operational / total) * 100",
    "sourceItems": [21, 20],
    "roundTo": 2
  }'

Calculation:
  (23 / 25) * 100 = 92.00%

Result in TenantMetrics:
  MetricId=5, NumericValue=92.00, TextValue='92.00%'
  SourceType='SystemCalculated'
```

**Real KTDA Examples:**
- Computer Availability % = (Operational / Total) × 100
- Printer Uptime % = (Working Printers / Total Printers) × 100
- Network Uptime % = (Days Online / Days in Month) × 100
- Budget Utilization % = (Spent / Budgeted) × 100

**Formula Syntax (JSON):**
```json
{
  "formula": "(item21 / item20) * 100",
  "sourceItems": [20, 21],
  "roundTo": 2,
  "minValue": 0,
  "maxValue": 100
}
```

---

### **3.3 ExternalSystem - Pull from APIs**

**When to use:** Metric data comes from external monitoring tools/systems.

**How it works:**
```
Hangfire job runs → Calls external API (e.g., PRTG, monitoring tool)
                  → Parses response
                  → Stores in SystemMetricLogs
                  → Copies to TenantMetrics
```

**Example:**

```
External System: PRTG Network Monitor
API Endpoint: https://prtg.ktda.co.ke/api/getsensordata

Hangfire Job: 'PullNetworkUptime' (runs daily)

Pseudocode:
  var response = await prtgApi.GetSensorData(sensorId: "factory50_wan");
  var uptimePct = response.uptime_percentage; // 98.5

  SystemMetricLogs.Insert({
    TenantId: 50,
    MetricId: 18 (NETWORK_UPTIME_PCT),
    NumericValue: 98.5,
    JobName: 'PullNetworkUptime',
    Status: 'Success'
  });

  TenantMetrics.Upsert({
    TenantId: 50,
    MetricId: 18,
    NumericValue: 98.5,
    SourceType: 'ExternalSystem'
  });
```

**Real KTDA Examples:**
- Network uptime from PRTG
- Server CPU usage from monitoring
- Disk space from file servers
- Bandwidth utilization from routers
- Email queue length from Lotus Domino

**Benefits:**
- No manual data entry
- Real-time or near-real-time data
- Automatic alerting if thresholds breached

---

### **3.4 ComplianceTracking - Deadline Monitoring**

**When to use:** Metric tracks whether something was done on time.

**How it works:**
```
System monitors submission deadlines → Checks if submitted by deadline
                                     → Records compliance (Yes/No)
                                     → Stores in TenantMetrics
```

**Example:**

```
Business Rule: "Factory monthly reports must be submitted by 5th of month"

MetricDefinition:
  MetricCode: 'REPORT_SUBMISSION_TIMELINESS'
  SourceType: 'ComplianceTracking'
  DataType: 'Boolean'
  ExpectedValue: 'TRUE'
  ComplianceRule: '{
    "type": "deadline",
    "daysAfterPeriodEnd": 5,
    "templateId": 1 (Monthly Factory Report)
  }'

Check Logic (runs on 6th of month):
  var submissions = GetSubmissions(templateId: 1, period: 'October 2025');
  foreach (var submission in submissions) {
    var dueDate = new DateTime(2025, 11, 5); // 5th of next month
    var isCompliant = submission.SubmittedDate <= dueDate;

    TenantMetrics.Insert({
      TenantId: submission.TenantId,
      MetricId: 25 (REPORT_TIMELINESS),
      NumericValue: isCompliant ? 1 : 0,
      TextValue: isCompliant ? 'On Time' : 'Late',
      SourceType: 'ComplianceTracking'
    });
  }
```

**Result:**
```
Factory 50: Submitted Oct report on Nov 3 → Compliant (1, 'On Time')
Factory 52: Submitted Oct report on Nov 7 → Non-compliant (0, 'Late')
```

**Real KTDA Examples:**
- Report submission timeliness (by 5th of month)
- License renewal before expiry (30 days warning)
- Backup verification daily (must run every day)
- Security patch installation (within 7 days of release)
- Training completion (before deadline)

---

### **3.5 AutomatedCheck - Background Job Health Checks**

**When to use:** System actively checks infrastructure health without user input.

**How it works:**
```
Hangfire job runs (daily/hourly) → Checks system status
                                 → Records result in SystemMetricLogs
                                 → If success, updates TenantMetrics
```

**Example:**

```
Job: 'DailyBackupVerification' (runs 2 AM daily)

Pseudocode:
  foreach (var tenant in GetActiveTenants()) {
    var backupPath = $"\\backup\{tenant.Code}";
    var latestBackup = GetLatestFile(backupPath);

    var isRecent = latestBackup.Date >= DateTime.Today.AddDays(-1);
    var status = isRecent ? 'Success' : 'Failed';

    SystemMetricLogs.Insert({
      TenantId: tenant.Id,
      MetricId: 15 (BACKUP_STATUS),
      CheckDate: DateTime.Now,
      Status: status,
      NumericValue: isRecent ? 1 : 0,
      TextValue: isRecent ? 'Backup current' : 'Backup outdated',
      Details: JSON.Serialize(new {
        backupDate = latestBackup.Date,
        backupSize = latestBackup.Size,
        backupPath = backupPath
      }),
      JobName: 'DailyBackupVerification'
    });

    if (status == 'Success') {
      TenantMetrics.Upsert({
        TenantId: tenant.Id,
        MetricId: 15,
        NumericValue: 1,
        TextValue: 'Backup current',
        SourceType: 'AutomatedCheck'
      });
    } else {
      // Trigger alert
      CreateAlert(tenantId: tenant.Id, message: 'Backup failed/outdated');
    }
  }
```

**Real KTDA Examples:**
- Daily backup verification (check if backup file exists and is recent)
- License expiry checks (30/7/1 day warnings)
- Disk space monitoring (alert if <10% free)
- Service availability (ping servers, check ports)
- Certificate expiry (SSL certificates)
- Antivirus definition updates (check last update date)

**Benefits:**
- Proactive monitoring (catch issues before users report)
- Consistent checks (no human error)
- Historical data (trend analysis)
- Automatic alerting

---

## 4. FOUR MAPPING TYPES FOR FORM INTEGRATION

This is the CRITICAL connection between Form Templates and Metrics.

**Table:** `FormItemMetricMappings`

**Purpose:** Defines HOW a form field (or multiple fields) populates a metric.

### **4.1 Direct Mapping**

**When to use:** One form field directly maps to one metric. No transformation needed.

**Example:**

```sql
Form Field: "Is LAN working?" (ItemId=45)
  DataType: Boolean
  Options: Yes / No

Metric: LAN_STATUS (MetricId=10)
  DataType: Boolean
  ExpectedValue: 'TRUE'

Mapping:
  INSERT INTO FormItemMetricMappings VALUES (
    ItemId: 45,
    MetricId: 10,
    MappingType: 'Direct',
    TransformationLogic: NULL, -- No transformation
    ExpectedValue: 'TRUE', -- For threshold check
    IsActive: 1
  );

User selects "Yes" →
  TenantMetrics: NumericValue=1, TextValue='Yes'

Traffic Light Logic:
  IF TextValue = ExpectedValue ('TRUE'/'Yes') → Green
  ELSE → Red
```

**More Examples:**
```
Direct Mappings:
├─ "Total computers" (Number) → TOTAL_COMPUTERS metric
├─ "WAN Type" (Dropdown) → WAN_TYPE metric (stores text)
├─ "Last backup date" (Date) → LAST_BACKUP_DATE metric
├─ "Chaipro version" (Text) → CHAIPRO_VERSION metric
└─ "Firewall status" (Dropdown: OK/Issues) → FIREWALL_STATUS metric
```

---

### **4.2 Calculated Mapping**

**When to use:** Metric needs calculation from 2+ form fields.

**Example:**

```sql
Form Fields:
  - "Total computers" (ItemId=20): Number
  - "Operational computers" (ItemId=21): Number

Metric: COMPUTER_AVAILABILITY_PCT (MetricId=5)
  DataType: Percentage
  ThresholdGreen: 95, ThresholdYellow: 85

Mapping:
  INSERT INTO FormItemMetricMappings VALUES (
    ItemId: 21, -- Target field (operational)
    MetricId: 5,
    MappingType: 'SystemCalculated',
    TransformationLogic: '{
      "formula": "(operational / total) * 100",
      "sourceItems": [21, 20],
      "itemAliases": {
        "operational": 21,
        "total": 20
      },
      "roundTo": 2
    }',
    IsActive: 1
  );

User enters:
  Total: 25
  Operational: 23

System calculates:
  (23 / 25) * 100 = 92.00%

Stores in TenantMetrics:
  NumericValue: 92.00

Traffic Light:
  92.00 >= 85 AND < 95 → Yellow (warning)
```

**Formula Capabilities:**

```javascript
Supported Operations:
  - Arithmetic: +, -, *, /, %
  - Comparison: <, >, <=, >=, ==, !=
  - Logical: AND, OR, NOT
  - Functions: ROUND, FLOOR, CEIL, MIN, MAX, AVG, SUM

Example Formulas:
  "(operational / total) * 100"  // Percentage
  "total - operational"  // Difference
  "(item1 + item2 + item3) / 3"  // Average
  "ROUND((value / max) * 100, 2)"  // Rounded percentage
  "IF(backup_status == 'Yes', 100, 0)"  // Conditional
```

---

### **4.3 BinaryCompliance Mapping**

**When to use:** Metric checks if a condition is met (compliance: Yes/No, Pass/Fail).

**Example:**

```sql
Form Field: "Is backup in place?" (ItemId=60)
  DataType: Boolean
  Options: Yes / No

Metric: BACKUP_COMPLIANCE (MetricId=30)
  DataType: Percentage
  ExpectedValue: 'YES'

Mapping:
  INSERT INTO FormItemMetricMappings VALUES (
    ItemId: 60,
    MetricId: 30,
    MappingType: 'BinaryCompliance',
    ExpectedValue: 'YES', -- Expected answer
    TransformationLogic: NULL, -- Simple binary check
    IsActive: 1
  );

User selects "Yes" →
  TenantMetrics: NumericValue=100 (100% compliant), TextValue='Compliant'

User selects "No" →
  TenantMetrics: NumericValue=0 (0% compliant), TextValue='Non-compliant'

Dashboard shows:
  Green: 100% compliance
  Red: 0% compliance
```

**Why Convert to Percentage?**
- Standardizes reporting (all compliance metrics 0-100%)
- Easy aggregation across tenants (regional compliance %)
- Consistent traffic light logic

**More Examples:**
```
BinaryCompliance Mappings:
├─ "Is backup in place?" → BACKUP_COMPLIANCE (0% or 100%)
├─ "Firewall enabled?" → FIREWALL_COMPLIANCE (0% or 100%)
├─ "Antivirus up-to-date?" → ANTIVIRUS_COMPLIANCE (0% or 100%)
├─ "Report submitted on time?" → SUBMISSION_COMPLIANCE (0% or 100%)
└─ "All licenses valid?" → LICENSE_COMPLIANCE (0% or 100%)
```

---

### **4.4 Derived Mapping**

**When to use:** Complex logic involving multiple conditions, nested formulas, or cross-section data.

**Example:**

```sql
Metric: OVERALL_SYSTEM_HEALTH (MetricId=50)
  DataType: Percentage
  ThresholdGreen: 90, ThresholdYellow: 75

Mapping:
  INSERT INTO FormItemMetricMappings VALUES (
    ItemId: 999, -- Placeholder (no single source item)
    MetricId: 50,
    MappingType: 'Derived',
    TransformationLogic: '{
      "type": "weighted_average",
      "components": [
        {
          "metricId": 5,
          "weight": 0.3,
          "label": "Hardware Availability"
        },
        {
          "metricId": 10,
          "weight": 0.2,
          "label": "Network Status"
        },
        {
          "metricId": 15,
          "weight": 0.2,
          "label": "Backup Status"
        },
        {
          "metricId": 20,
          "weight": 0.15,
          "label": "Software Compliance"
        },
        {
          "metricId": 25,
          "weight": 0.15,
          "label": "Submission Timeliness"
        }
      ],
      "formula": "SUM(metric_value * weight)"
    }',
    IsActive: 1
  );

Calculation:
  Hardware: 92% × 0.3 = 27.6
  Network: 100% × 0.2 = 20.0
  Backup: 100% × 0.2 = 20.0
  Software: 85% × 0.15 = 12.75
  Timeliness: 90% × 0.15 = 13.5
  ──────────────────────────
  Overall: 93.85% → Green (>90%)
```

**Use Cases:**
- Composite KPIs (weighted average of multiple metrics)
- Health scores (combine multiple factors)
- Risk scores (weighted sum)
- Maturity levels (based on multiple compliance checks)

---

## 5. HOW METRICS GET POPULATED (THE COMPLETE FLOW)

### **Flow 1: User Submits Form → Metrics Auto-Populated**

```
┌─────────────────────────────────────────────────────────────────┐
│ USER SUBMITS FACTORY MONTHLY REPORT                              │
└─────────────────────────────────────────────────────────────────┘
                              ↓
                    [FormTemplateSubmissions]
                    SubmissionId=12345, Status='Submitted'
                              ↓
                    [FormTemplateResponses]
                    ItemId=20, ResponseText='25' (Total computers)
                    ItemId=21, ResponseText='23' (Operational)
                    ItemId=45, ResponseText='Yes' (LAN working)
                              ↓
                 ┌────────────┴────────────┐
                 │  METRIC POPULATION      │
                 │  BACKGROUND PROCESSOR   │
                 └────────────┬────────────┘
                              ↓
         ┌──────────────────────────────────────────┐
         │ FOR EACH FormItemMetricMapping:          │
         ├──────────────────────────────────────────┤
         │ 1. Get mapping for ItemId=21             │
         │    → Mapping Type: 'SystemCalculated'    │
         │    → MetricId: 5 (COMPUTER_AVAIL_PCT)    │
         │                                          │
         │ 2. Execute transformation:               │
         │    Formula: "(item21 / item20) * 100"    │
         │    Get item20 value: 25                  │
         │    Get item21 value: 23                  │
         │    Calculate: (23 / 25) * 100 = 92.00    │
         │                                          │
         │ 3. Store in TenantMetrics:               │
         │    TenantId=50, MetricId=5               │
         │    ReportingPeriod='2025-11-01'          │
         │    NumericValue=92.00                    │
         │    SourceType='SystemCalculated'         │
         │    SourceReferenceId=12345 (SubmissionId)│
         │                                          │
         │ 4. Log in MetricPopulationLog:           │
         │    SubmissionId=12345, MetricId=5        │
         │    SourceValue='23', CalculatedValue=92.00│
         │    Status='Success'                      │
         └──────────────────────────────────────────┘
                              ↓
         ┌──────────────────────────────────────────┐
         │ FOR ItemId=45 (LAN Status):              │
         ├──────────────────────────────────────────┤
         │ 1. Get mapping for ItemId=45             │
         │    → Mapping Type: 'Direct'              │
         │    → MetricId: 10 (LAN_STATUS)           │
         │                                          │
         │ 2. Direct copy (no transformation):      │
         │    Value: 'Yes'                          │
         │                                          │
         │ 3. Store in TenantMetrics:               │
         │    TenantId=50, MetricId=10              │
         │    NumericValue=1, TextValue='Yes'       │
         │    SourceType='UserInput'                │
         └──────────────────────────────────────────┘
                              ↓
              ┌───────────────────────────┐
              │  CHECK THRESHOLDS         │
              ├───────────────────────────┤
              │ COMPUTER_AVAIL: 92%       │
              │ Threshold: 85-95 → YELLOW │
              │                           │
              │ LAN_STATUS: 'Yes'         │
              │ Expected: 'Yes' → GREEN   │
              └───────────────────────────┘
                              ↓
              ┌───────────────────────────┐
              │  TRIGGER ALERTS (if needed)│
              ├───────────────────────────┤
              │ If Yellow/Red:            │
              │ → Create notification     │
              │ → Email regional manager  │
              └───────────────────────────┘
                              ↓
              ┌───────────────────────────┐
              │  UPDATE DASHBOARD         │
              ├───────────────────────────┤
              │ Factory 50 KPI Widget:    │
              │ ⚫ Computer Avail: 92% ⚠️ │
              │ 🟢 LAN Status: OK         │
              └───────────────────────────┘
```

---

### **Flow 2: Automated Background Job → Metrics Updated**

```
┌────────────────────────────────────────────────────────────┐
│ HANGFIRE JOB: 'DailyBackupVerification'                    │
│ Trigger: Every day at 2:00 AM                              │
└────────────────────────────────────────────────────────────┘
                              ↓
          ┌───────────────────────────────────┐
          │ FOR EACH Active Tenant:           │
          ├───────────────────────────────────┤
          │ 1. Check backup location:         │
          │    Path: \\backup\factory50       │
          │                                   │
          │ 2. Get latest backup file:        │
          │    File: factory50_2025-11-05.bak │
          │    Date: 2025-11-05               │
          │    Size: 150 GB                   │
          │                                   │
          │ 3. Validate:                      │
          │    Is file date within 24 hours?  │
          │    → Yes (backup is current)      │
          │                                   │
          │ 4. Log result:                    │
          │    INSERT INTO SystemMetricLogs   │
          │    TenantId=50, MetricId=15       │
          │    Status='Success'               │
          │    NumericValue=1                 │
          │    TextValue='Backup current'     │
          │    Details=JSON(backupInfo)       │
          │    JobName='DailyBackupVerification'│
          │                                   │
          │ 5. Update TenantMetrics:          │
          │    TenantId=50, MetricId=15       │
          │    NumericValue=1                 │
          │    SourceType='AutomatedCheck'    │
          │    SourceReferenceId=789 (LogId)  │
          └───────────────────────────────────┘
                              ↓
          ┌───────────────────────────────────┐
          │ IF BACKUP FAILED/OUTDATED:        │
          ├───────────────────────────────────┤
          │ 1. Log failure:                   │
          │    Status='Failed'                │
          │    TextValue='Backup outdated'    │
          │    ErrorMessage='Last backup: 3 days'│
          │                                   │
          │ 2. Trigger CRITICAL alert:        │
          │    → Email factory ICT staff      │
          │    → Email regional manager       │
          │    → Dashboard shows RED alert    │
          │                                   │
          │ 3. Update TenantMetrics:          │
          │    NumericValue=0 (failed)        │
          └───────────────────────────────────┘
                              ↓
          ┌───────────────────────────────────┐
          │ JOB COMPLETION SUMMARY:           │
          ├───────────────────────────────────┤
          │ Checked: 69 factories             │
          │ Success: 67 (97%)                 │
          │ Failed: 2 (3%)                    │
          │                                   │
          │ Failed Tenants:                   │
          │ - Factory 52: Backup 3 days old   │
          │ - Factory 65: Backup file missing │
          │                                   │
          │ → Email summary to Head Office    │
          └───────────────────────────────────┘
```

---

## 6. DEFINING METRICS - STEP BY STEP

### **Step 1: Identify the Metric**

**Questions to ask:**
1. What do we want to measure?
2. Why is this metric important?
3. Who will use this metric?
4. How often should it be measured?
5. What's a "good" value? What's "bad"?

**Example:**

```
Metric: Computer Availability Percentage
Purpose: Track how many computers are operational vs broken
Users: Factory ICT, Regional Managers, Head Office
Frequency: Monthly (from monthly report)
Good value: ≥95% (green)
Warning: 85-94% (yellow)
Critical: <85% (red)
```

---

### **Step 2: Choose Source Type**

**Decision Tree:**

```
Does value come from form submission?
├─ Yes: Is it a single field?
│  ├─ Yes → 'UserInput' (Direct mapping)
│  └─ No → 'SystemCalculated' (Formula mapping)
│
├─ No: Does it come from external API?
│  └─ Yes → 'ExternalSystem' (API integration)
│
├─ No: Is it checking a deadline/rule?
│  └─ Yes → 'ComplianceTracking' (Deadline monitoring)
│
└─ No: Is it an automated health check?
   └─ Yes → 'AutomatedCheck' (Background job)
```

**For our example:**
- Comes from form submission ✓
- Needs calculation from 2 fields (Total & Operational)
- **Source Type: 'SystemCalculated'**

---

### **Step 3: Define Data Type & Unit**

**Data Types:**
- `Integer`: Whole numbers (0, 1, 25, 100)
- `Decimal`: Numbers with decimals (92.5, 15.75)
- `Percentage`: 0-100 with % display (92.5%)
- `Boolean`: True/False, Yes/No (1/0)
- `Text`: Status values, versions ("Operational", "5.2.1")
- `Duration`: Time periods (45 minutes, 3 days)
- `Date/DateTime`: Specific dates/times

**Units:**
- Count, Percentage, Version, Status, Days, Hours, GB, MB, etc.

**For our example:**
- **Data Type:** Percentage
- **Unit:** Percentage
- **Display:** "92.5%"

---

### **Step 4: Set Thresholds (for KPIs)**

**Traffic Light System:**

```
Green (Good):     Value >= ThresholdGreen
Yellow (Warning): Value >= ThresholdYellow AND < ThresholdGreen
Red (Critical):   Value < ThresholdRed (same as Yellow threshold)

Inverted (for metrics where lower is better):
Green:  Value <= ThresholdGreen
Yellow: Value > ThresholdGreen AND <= ThresholdYellow
Red:    Value > ThresholdRed
```

**For our example:**
- **ThresholdGreen:** 95.0 (≥95% = excellent)
- **ThresholdYellow:** 85.0 (85-94% = needs attention)
- **ThresholdRed:** 85.0 (<85% = critical issue)

**Visual:**
```
  0% ──────────────── 85% ────── 95% ────── 100%
  │         🔴         │    ⚠️    │    🟢    │
  └─── Critical ───────┴─ Warning ┴── Good ──┘
```

---

### **Step 5: Create Metric Definition (SQL)**

```sql
INSERT INTO MetricDefinitions (
    MetricCode,
    MetricName,
    Category,
    SourceType,
    DataType,
    Unit,
    AggregationType,
    IsKPI,
    ThresholdGreen,
    ThresholdYellow,
    ThresholdRed,
    ExpectedValue,
    ComplianceRule,
    Description,
    IsActive
) VALUES (
    'COMPUTER_AVAILABILITY_PCT',  -- Unique code
    'Computer Availability Percentage',  -- Display name
    'Hardware',  -- Category
    'SystemCalculated',  -- Calculated from form fields
    'Percentage',  -- Data type
    'Percentage',  -- Unit
    'AVG',  -- Average across tenants for regional summary
    1,  -- Yes, this is a KPI
    95.0,  -- Green threshold
    85.0,  -- Yellow threshold
    85.0,  -- Red threshold (same as yellow)
    NULL,  -- No expected value (not binary)
    NULL,  -- No compliance rule
    'Percentage of computers that are operational vs total inventory. ' +
    'Calculated as: (Operational Computers / Total Computers) × 100',
    1  -- Active
);
```

---

### **Step 6: Create Form-to-Metric Mapping**

**This happens AFTER form template is created.**

**Prerequisite:**
- Form template exists with fields:
  - ItemId=20: "Total computers" (Number)
  - ItemId=21: "Operational computers" (Number)

**SQL:**

```sql
INSERT INTO FormItemMetricMappings (
    ItemId,
    MetricId,
    MappingType,
    TransformationLogic,
    IsActive
) VALUES (
    21,  -- Target item (Operational computers)
    (SELECT MetricId FROM MetricDefinitions WHERE MetricCode = 'COMPUTER_AVAILABILITY_PCT'),
    'SystemCalculated',  -- Mapping type
    '{
        "formula": "(operational / total) * 100",
        "sourceItems": [21, 20],
        "itemAliases": {
            "operational": 21,
            "total": 20
        },
        "roundTo": 2,
        "minValue": 0,
        "maxValue": 100
    }',  -- Transformation logic (JSON)
    1  -- Active
);
```

---

## 7. CONNECTING FORMS TO METRICS

### **During Form Template Creation**

**Workflow: Admin Creates "Factory Monthly Report" Template**

```
Step 1: Create Form Template
  TemplateId=1, TemplateName="Factory Monthly Report"
  Category="Infrastructure"

Step 2: Add Section "Hardware Status"
  SectionId=10, SectionName="Hardware Status"

Step 3: Add Form Fields
  ItemId=20: "Total computers"
    DataType=Number, IsRequired=1

  ItemId=21: "Operational computers"
    DataType=Number, IsRequired=1
    Validation: Must be ≤ Total computers

  ItemId=22: "Broken computers (for repair)"
    DataType=Number, IsRequired=0

Step 4: ADD METRIC MAPPINGS (This is KEY!)

  a) Direct Mapping (Total Computers):
     ItemId=20 → MetricId=1 (TOTAL_COMPUTERS)
     MappingType='Direct'

  b) Calculated Mapping (Availability %):
     ItemId=21 → MetricId=5 (COMPUTER_AVAILABILITY_PCT)
     MappingType='SystemCalculated'
     TransformationLogic='{
       "formula": "(operational / total) * 100",
       "sourceItems": [21, 20]
     }'

  c) Calculated Mapping (Broken Count):
     ItemId=22 → MetricId=6 (COMPUTERS_UNDER_REPAIR)
     MappingType='Direct'

Step 5: Configure Auto-Calculation
  FormItemCalculations:
    TargetItemId=22 (Broken computers)
    CalculationFormula='{
      "formula": "total - operational",
      "sourceItems": [20, 21]
    }'

  → User enters Total=25, Operational=23
  → System auto-fills Broken=2
```

---

### **UI/UX: Metric Mapping Interface**

**When admin creates form field, show "Link to Metric" section:**

```
┌─────────────────────────────────────────────────────────────┐
│ FORM FIELD CONFIGURATION                                     │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ Field Name: [Operational Computers]                         │
│ Data Type: [Number ▼]                                       │
│ Is Required: [✓]                                            │
│                                                              │
├─────────────────────────────────────────────────────────────┤
│ 📊 METRIC INTEGRATION                                       │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ Link this field to a metric? [✓] Yes  [ ] No               │
│                                                              │
│ Select Metric: [Computer Availability Percentage ▼]         │
│                                                              │
│ Mapping Type: [ ] Direct                                    │
│               [✓] Calculated                                 │
│               [ ] Binary Compliance                          │
│               [ ] Derived                                    │
│                                                              │
│ ┌──────────────────────────────────────────────────────┐   │
│ │ CALCULATION SETUP                                     │   │
│ ├──────────────────────────────────────────────────────┤   │
│ │                                                       │   │
│ │ Formula: [(operational / total) * 100]               │   │
│ │                                                       │   │
│ │ Source Fields:                                        │   │
│ │   operational: [Operational Computers ▼]             │   │
│ │   total:       [Total Computers ▼]                   │   │
│ │                                                       │   │
│ │ Round to: [2] decimal places                         │   │
│ │                                                       │   │
│ │ [Preview Calculation]                                 │   │
│ │                                                       │   │
│ │ Example: If total=25, operational=23                 │   │
│ │ Result: 92.00%                                        │   │
│ │                                                       │   │
│ └──────────────────────────────────────────────────────┘   │
│                                                              │
│ [Save Field]  [Cancel]                                      │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 8. REPORTING & VISUALIZATION

### **How Metrics Flow to Reports**

```
TenantMetrics (raw data)
        ↓
TenantPerformanceSnapshot (monthly rollup)
        ↓
RegionalMonthlySnapshot (regional aggregation)
        ↓
Reports & Dashboards (visualization)
```

---

### **Dashboard Widgets**

**1. KPI Scorecard (Factory Dashboard)**

```
┌─────────────────────────────────────────────────────┐
│ KIAMBU FACTORY - NOVEMBER 2025 KPIs                 │
├─────────────────────────────────────────────────────┤
│                                                      │
│ 🟢 LAN Status                            OK         │
│    Last checked: 2 hours ago                        │
│                                                      │
│ ⚠️ Computer Availability                92.0%       │
│    23 of 25 computers operational                   │
│    Target: 95% | Warning: <95%                      │
│                                                      │
│ 🟢 Backup Status                         Current    │
│    Last backup: Nov 5, 2025 (1 day ago)            │
│                                                      │
│ 🔴 Report Submission                     OVERDUE    │
│    Oct report due: Nov 5 | Submitted: Not yet      │
│                                                      │
└─────────────────────────────────────────────────────┘
```

**2. Trend Chart**

```
┌─────────────────────────────────────────────────────┐
│ COMPUTER AVAILABILITY TREND (Last 6 Months)         │
├─────────────────────────────────────────────────────┤
│                                                      │
│ 100% ┼                                              │
│      │      ●━━━●                                   │
│  95% ┼ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─  (Green)        │
│      │  ●           ●━━━●                           │
│  90% ┼                                              │
│      │                      ●                       │
│  85% ┼ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─  (Yellow)       │
│      │                                              │
│  80% ┼━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  │
│      └───┬────┬────┬────┬────┬────┬───            │
│         Jun  Jul  Aug  Sep  Oct  Nov              │
│                                                      │
│  Trend: ↗ Improving (+3% vs last month)            │
│  Action: Add 2 more computers to reach 95% target  │
│                                                      │
└─────────────────────────────────────────────────────┘
```

**3. Regional Comparison**

```
┌──────────────────────────────────────────────────────────────┐
│ REGION 3 - COMPUTER AVAILABILITY COMPARISON                   │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│ Factory          Availability   Status   Trend              │
│ ──────────────────────────────────────────────────────────  │
│ Kiambu           92.0% ⚠️       ▲ +2%                       │
│ Thika            96.5% 🟢       ▲ +1%                       │
│ Murang'a         88.0% ⚠️       ▼ -3%                       │
│ Sagana           98.0% 🟢       → 0%                        │
│ Kangema          82.0% 🔴       ▼ -5%  ← CRITICAL           │
│                                                               │
│ Regional Average: 91.3% ⚠️                                   │
│ Target: 95% | Gap: -3.7%                                    │
│                                                               │
│ [View Details] [Export Report]                              │
│                                                               │
└──────────────────────────────────────────────────────────────┘
```

---

### **Report Generation (SQL Queries)**

**Query 1: Current Month KPIs for Tenant**

```sql
SELECT
    md.MetricName,
    md.Category,
    md.Unit,
    tm.NumericValue,
    tm.TextValue,
    tm.CapturedDate,
    CASE
        WHEN md.IsKPI = 1 AND tm.NumericValue >= md.ThresholdGreen THEN 'Green'
        WHEN md.IsKPI = 1 AND tm.NumericValue >= md.ThresholdYellow THEN 'Yellow'
        WHEN md.IsKPI = 1 THEN 'Red'
        ELSE NULL
    END AS ThresholdStatus
FROM TenantMetrics tm
INNER JOIN MetricDefinitions md ON tm.MetricId = md.MetricId
WHERE tm.TenantId = 50  -- Kiambu Factory
  AND tm.ReportingPeriod = '2025-11-01'  -- November 2025
  AND md.IsKPI = 1  -- Only KPIs
ORDER BY md.Category, md.DisplayOrder;
```

**Query 2: Trend Analysis (6 months)**

```sql
SELECT
    tm.ReportingPeriod,
    AVG(tm.NumericValue) AS AvgValue,
    MIN(tm.NumericValue) AS MinValue,
    MAX(tm.NumericValue) AS MaxValue
FROM TenantMetrics tm
WHERE tm.TenantId = 50
  AND tm.MetricId = 5  -- Computer Availability %
  AND tm.ReportingPeriod >= DATEADD(MONTH, -6, GETDATE())
GROUP BY tm.ReportingPeriod
ORDER BY tm.ReportingPeriod;
```

**Query 3: Regional Comparison**

```sql
SELECT
    t.TenantName,
    tm.NumericValue AS Availability,
    CASE
        WHEN tm.NumericValue >= 95 THEN 'Green'
        WHEN tm.NumericValue >= 85 THEN 'Yellow'
        ELSE 'Red'
    END AS Status,
    LAG(tm.NumericValue) OVER (PARTITION BY t.TenantId ORDER BY tm.ReportingPeriod) AS PrevMonth,
    tm.NumericValue - LAG(tm.NumericValue) OVER (PARTITION BY t.TenantId ORDER BY tm.ReportingPeriod) AS Trend
FROM Tenants t
INNER JOIN TenantMetrics tm ON t.TenantId = tm.TenantId
WHERE t.RegionId = 3  -- Region 3
  AND tm.MetricId = 5  -- Computer Availability %
  AND tm.ReportingPeriod = '2025-11-01'
ORDER BY tm.NumericValue DESC;
```

---

## 9. REAL KTDA EXAMPLES

### **Example 1: Infrastructure Metrics**

```sql
-- Metric 1: LAN Status
INSERT INTO MetricDefinitions VALUES (
    MetricCode: 'LAN_STATUS',
    MetricName: 'LAN Network Status',
    Category: 'Infrastructure',
    SourceType: 'UserInput',
    DataType: 'Boolean',
    Unit: 'Status',
    IsKPI: 1,
    ExpectedValue: 'TRUE',
    ThresholdGreen: 1,  -- Operational
    ThresholdRed: 0,    -- Down
    Description: 'Status of factory LAN (Local Area Network)'
);

-- Form Field: "Is LAN working?" (Yes/No dropdown)
-- Mapping: Direct (ItemId → MetricId)

-- Metric 2: WAN Type
INSERT INTO MetricDefinitions VALUES (
    MetricCode: 'WAN_TYPE',
    MetricName: 'WAN Connection Type',
    Category: 'Infrastructure',
    SourceType: 'UserInput',
    DataType: 'Text',
    Unit: 'None',
    IsKPI: 0,  -- Not a KPI, just informational
    Description: 'Type of WAN connection (Fiber/Microwave/Hybrid)'
);

-- Form Field: "WAN Type" (Dropdown: Fiber, Microwave, Hybrid)
-- Mapping: Direct

-- Metric 3: Network Uptime %
INSERT INTO MetricDefinitions VALUES (
    MetricCode: 'NETWORK_UPTIME_PCT',
    MetricName: 'Network Uptime Percentage',
    Category: 'Infrastructure',
    SourceType: 'ExternalSystem',  -- Pull from PRTG
    DataType: 'Percentage',
    Unit: 'Percentage',
    IsKPI: 1,
    ThresholdGreen: 99.0,
    ThresholdYellow: 95.0,
    Description: 'Network availability % pulled from PRTG monitoring'
);

-- Hangfire Job: Calls PRTG API daily
-- No form mapping
```

---

### **Example 2: Hardware Metrics**

```sql
-- Metric 1: Total Computers
INSERT INTO MetricDefinitions VALUES (
    MetricCode: 'TOTAL_COMPUTERS',
    MetricName: 'Total Computer Count',
    Category: 'Hardware',
    SourceType: 'UserInput',
    DataType: 'Integer',
    Unit: 'Count',
    AggregationType: 'SUM',  -- Sum across all factories
    IsKPI: 0,
    Description: 'Total number of computers in inventory'
);

-- Metric 2: Operational Computers
INSERT INTO MetricDefinitions VALUES (
    MetricCode: 'OPERATIONAL_COMPUTERS',
    MetricName: 'Operational Computer Count',
    Category: 'Hardware',
    SourceType: 'UserInput',
    DataType: 'Integer',
    Unit: 'Count',
    AggregationType: 'SUM',
    IsKPI: 0,
    Description: 'Number of computers that are operational'
);

-- Metric 3: Computer Availability % (CALCULATED)
INSERT INTO MetricDefinitions VALUES (
    MetricCode: 'COMPUTER_AVAILABILITY_PCT',
    MetricName: 'Computer Availability Percentage',
    Category: 'Hardware',
    SourceType: 'SystemCalculated',
    DataType: 'Percentage',
    Unit: 'Percentage',
    AggregationType: 'AVG',  -- Average % across factories
    IsKPI: 1,
    ThresholdGreen: 95.0,
    ThresholdYellow: 85.0,
    Description: 'Percentage of computers operational vs total'
);

-- Form Fields:
--   ItemId=20: "Total computers" (Number)
--   ItemId=21: "Operational computers" (Number)

-- Mapping:
INSERT INTO FormItemMetricMappings VALUES (
    ItemId: 21,
    MetricId: 'COMPUTER_AVAILABILITY_PCT',
    MappingType: 'SystemCalculated',
    TransformationLogic: '{
        "formula": "(operational / total) * 100",
        "sourceItems": [21, 20]
    }'
);
```

---

### **Example 3: Software Metrics**

```sql
-- Metric 1: Chaipro Version
INSERT INTO MetricDefinitions VALUES (
    MetricCode: 'CHAIPRO_FINANCIALS_VERSION',
    MetricName: 'Chaipro Financials Version',
    Category: 'Software',
    SourceType: 'UserInput',
    DataType: 'Text',
    Unit: 'Version',
    IsKPI: 0,
    ExpectedValue: '5.2.1.4',  -- Current version
    Description: 'Installed version of Chaipro Financials module'
);

-- Form Field: "Chaipro Financials Version" (Text input)
-- Mapping: Direct

-- Metric 2: Software Compliance % (CALCULATED)
INSERT INTO MetricDefinitions VALUES (
    MetricCode: 'SOFTWARE_COMPLIANCE_PCT',
    MetricName: 'Software Version Compliance',
    Category: 'Software',
    SourceType: 'SystemCalculated',
    DataType: 'Percentage',
    Unit: 'Percentage',
    IsKPI: 1,
    ThresholdGreen: 90.0,
    ThresholdYellow: 75.0,
    Description: 'Percentage of software installations on current version'
);

-- Calculation: Count of factories with current version / Total factories
-- Mapping: Derived (complex aggregation across multiple metrics)
```

---

### **Example 4: Backup Metrics**

```sql
-- Metric 1: Backup Status (AUTOMATED CHECK)
INSERT INTO MetricDefinitions VALUES (
    MetricCode: 'BACKUP_STATUS',
    MetricName: 'Backup Status',
    Category: 'Compliance',
    SourceType: 'AutomatedCheck',
    DataType: 'Boolean',
    Unit: 'Status',
    IsKPI: 1,
    ExpectedValue: 'TRUE',
    ThresholdGreen: 1,
    ThresholdRed: 0,
    Description: 'Daily automated check of backup file existence and recency'
);

-- Hangfire Job: 'DailyBackupVerification'
-- Checks: \\backup\{tenantCode}\ for recent .bak files
-- No form mapping

-- Metric 2: Last Backup Date
INSERT INTO MetricDefinitions VALUES (
    MetricCode: 'LAST_BACKUP_DATE',
    MetricName: 'Last Successful Backup Date',
    Category: 'Compliance',
    SourceType: 'UserInput',  -- User reports from form
    DataType: 'Date',
    Unit: 'None',
    IsKPI: 0,
    Description: 'Date of last successful backup as reported in monthly form'
);

-- Form Field: "Last backup date" (Date picker)
-- Mapping: Direct
```

---

### **Example 5: Compliance Metrics**

```sql
-- Metric: Report Submission Timeliness (COMPLIANCE TRACKING)
INSERT INTO MetricDefinitions VALUES (
    MetricCode: 'REPORT_SUBMISSION_TIMELINESS',
    MetricName: 'Report Submission Timeliness',
    Category: 'Compliance',
    SourceType: 'ComplianceTracking',
    DataType: 'Boolean',
    Unit: 'Status',
    IsKPI: 1,
    ExpectedValue: 'TRUE',
    ThresholdGreen: 1,
    ThresholdRed: 0,
    ComplianceRule: '{
        "type": "deadline",
        "templateId": 1,
        "templateName": "Factory Monthly Report",
        "daysAfterPeriodEnd": 5,
        "description": "Report must be submitted by 5th of following month"
    }',
    Description: 'Tracks whether monthly report was submitted by deadline'
);

-- System Job: Runs on 6th of month
-- Checks: FormTemplateSubmissions.SubmittedDate <= Deadline
-- Populates: TenantMetrics with 1 (on time) or 0 (late)
```

---

## 10. WORKFLOWS & ACTIONS

### **WF-3.1: Define New Metric**

```
Trigger: System Admin needs to track new KPI
Steps:
  1. Navigate to "System Configuration → Metrics"
  2. Click "Add New Metric"
  3. Fill form:
     - Metric Code (unique): PRINTER_AVAILABILITY_PCT
     - Metric Name: Printer Availability Percentage
     - Category: Hardware
     - Description: % of printers operational vs total

  4. Select Source Type:
     ( ) User Input (direct from form)
     (•) System Calculated (formula)
     ( ) External System (API)
     ( ) Compliance Tracking (deadline)
     ( ) Automated Check (job)

  5. Configure Data Type:
     Data Type: [Percentage ▼]
     Unit: [Percentage ▼]
     Aggregation: [AVG ▼]

  6. Set KPI Thresholds:
     [✓] This is a KPI
     Green (Good): [90]
     Yellow (Warning): [75]
     Red (Critical): [75]

  7. Preview Traffic Light:
     100% ─── 90% ─── 75% ─── 0%
      🟢      │   ⚠️   │   🔴

  8. Click "Save Metric"
  9. System assigns MetricId=25
  10. Redirect to "Create Form Mapping"
```

---

### **WF-3.2: Map Form Field to Metric (Direct)**

```
Trigger: Admin creating form template, wants field to populate metric
Steps:
  1. In Form Builder, editing field "Total Printers"
  2. Expand "Metric Integration" section
  3. Toggle: [✓] Link to metric
  4. Select Metric: [Total Printer Count ▼]
  5. Select Mapping Type: (•) Direct
  6. Preview:
     When user enters value → Directly stores in TenantMetrics
     No transformation needed
  7. Click "Save"

  System creates:
    INSERT INTO FormItemMetricMappings (
      ItemId=50, MetricId=20, MappingType='Direct'
    );
```

---

### **WF-3.3: Map Form Fields to Metric (Calculated)**

```
Trigger: Admin creating calculated metric from multiple fields
Steps:
  1. In Form Builder, editing field "Operational Printers"
  2. Expand "Metric Integration" section
  3. Toggle: [✓] Link to metric
  4. Select Metric: [Printer Availability Percentage ▼]
  5. Select Mapping Type: (•) Calculated
  6. Configure Calculation:

     Formula Builder:
     ┌────────────────────────────────────────┐
     │ (operational / total) * 100            │
     └────────────────────────────────────────┘

     Source Fields:
     operational: [Operational Printers ▼]
     total:       [Total Printers ▼]

     Round to: [2] decimals

     Test Calculation:
     If total=10, operational=9
     Result: 90.00%

  7. Click "Save"

  System creates:
    INSERT INTO FormItemMetricMappings (
      ItemId=51,
      MetricId=25,
      MappingType='SystemCalculated',
      TransformationLogic='{...}'
    );
```

---

### **WF-3.4: User Submits Form → Metrics Auto-Populated**

```
Trigger: Factory ICT staff submits monthly report
Steps:
  1. User fills form:
     - Total Printers: 10
     - Operational Printers: 9
     - LAN Status: Yes

  2. User clicks "Submit"

  3. System validates all required fields

  4. System saves:
     FormTemplateSubmissions (SubmissionId=789)
     FormTemplateResponses (Item 50: "10", Item 51: "9", Item 45: "Yes")

  5. Background Processor (triggered immediately):

     a) Query: Get all FormItemMetricMappings for this template

     b) For each mapping:

        Mapping 1: ItemId=50 → MetricId=20 (Total Printers)
          Type: Direct
          Action: Copy value "10"
          Insert/Update TenantMetrics:
            TenantId=50, MetricId=20, NumericValue=10

        Mapping 2: ItemId=51 → MetricId=25 (Printer Avail %)
          Type: SystemCalculated
          Formula: "(operational / total) * 100"
          Get values: operational=9, total=10
          Calculate: (9 / 10) * 100 = 90.00
          Insert/Update TenantMetrics:
            TenantId=50, MetricId=25, NumericValue=90.00

        Mapping 3: ItemId=45 → MetricId=10 (LAN Status)
          Type: Direct
          Action: Copy value "Yes" (convert to 1)
          Insert/Update TenantMetrics:
            TenantId=50, MetricId=10, NumericValue=1, TextValue='Yes'

     c) Log all in MetricPopulationLog for audit

  6. Check Thresholds:
     Printer Avail: 90.00% >= 90% → Green ✓
     LAN Status: 'Yes' = Expected → Green ✓

  7. Update Dashboard (real-time via SignalR)

  8. Show success to user: "Report submitted. All KPIs green!"
```

---

### **WF-3.5: Background Job Checks Backup Status**

```
Trigger: Hangfire job 'DailyBackupVerification' runs at 2 AM
Steps:
  1. Get all active tenants

  2. For each tenant:

     a) Determine backup path:
        Path: \\backup\factory50\

     b) Get latest .bak file:
        File: factory50_2025-11-06.bak
        Date: 2025-11-06 01:30:00
        Size: 150 GB

     c) Check if recent (within 24 hours):
        Now: 2025-11-06 02:00:00
        File age: 30 minutes → ✓ Current

     d) Log result:
        INSERT INTO SystemMetricLogs (
          TenantId=50,
          MetricId=15 (BACKUP_STATUS),
          CheckDate='2025-11-06 02:00:00',
          Status='Success',
          NumericValue=1,
          TextValue='Backup current',
          Details='{"size": "150GB", "age": "30min"}',
          JobName='DailyBackupVerification',
          ExecutionDuration=1200
        );

     e) Update metric:
        INSERT/UPDATE TenantMetrics (
          TenantId=50,
          MetricId=15,
          NumericValue=1,
          SourceType='AutomatedCheck',
          SourceReferenceId=LogId
        );

  3. Summary:
     Checked: 69 factories
     Success: 67
     Failed: 2 (Factory 52, Factory 65)

  4. For failed tenants:
     a) Update metric with NumericValue=0
     b) Trigger CRITICAL alert
     c) Email factory ICT + regional manager
     d) Show RED on dashboard

  5. Email summary to Head Office ICT Manager
```

---

### **WF-3.6: View Metrics Dashboard**

```
Trigger: User navigates to Dashboard
Steps:
  1. System identifies user's tenant(s):
     - User role: FACTORY_ICT (Level 3)
     - Primary tenant: Factory 50 (Kiambu)

  2. Query current period metrics:
     SELECT * FROM TenantMetrics
     WHERE TenantId = 50
       AND ReportingPeriod = '2025-11-01'
       AND MetricId IN (SELECT MetricId FROM MetricDefinitions WHERE IsKPI = 1)

  3. For each metric, calculate threshold status:
     Metric: COMPUTER_AVAILABILITY_PCT
     Value: 92.00%
     ThresholdGreen: 95.0
     ThresholdYellow: 85.0
     Result: 92 >= 85 AND < 95 → Yellow ⚠️

  4. Render KPI widgets:
     ┌─────────────────────────────────────┐
     │ ⚠️ Computer Availability    92.0%   │
     │    Target: 95% | Gap: -3%          │
     │    [View Details]                   │
     └─────────────────────────────────────┘

     ┌───────────────────────────────────��─┐
     │ 🟢 LAN Status              OK       │
     │    Last checked: 2 hours ago        │
     └─────────────────────────────────────┘

  5. Render trend charts (query last 6 months)

  6. Show alerts if any Red metrics
```

---

### **WF-3.7: Generate Monthly Report**

```
Trigger: Regional Manager clicks "Generate Regional Report"
Steps:
  1. Select parameters:
     Region: [Region 3 ▼]
     Period: [November 2025 ▼]
     Include: [✓] All KPIs  [✓] Trends  [ ] Raw Data

  2. Query data:
     -- Get all metrics for all tenants in Region 3 for Nov 2025
     SELECT
       t.TenantName,
       md.MetricName,
       tm.NumericValue,
       tm.TextValue,
       CASE
         WHEN tm.NumericValue >= md.ThresholdGreen THEN 'Green'
         WHEN tm.NumericValue >= md.ThresholdYellow THEN 'Yellow'
         ELSE 'Red'
       END AS Status
     FROM TenantMetrics tm
     INNER JOIN Tenants t ON tm.TenantId = t.TenantId
     INNER JOIN MetricDefinitions md ON tm.MetricId = md.MetricId
     WHERE t.RegionId = 3
       AND tm.ReportingPeriod = '2025-11-01'
       AND md.IsKPI = 1
     ORDER BY md.Category, t.TenantName;

  3. Calculate regional averages:
     AVG(NumericValue) GROUP BY MetricId

  4. Identify outliers:
     Tenants with Red status
     Tenants trending down

  5. Generate report (Excel):
     Sheet 1: Summary (averages, counts)
     Sheet 2: By Tenant (detailed grid)
     Sheet 3: Trends (6-month comparison)
     Sheet 4: Recommendations (auto-generated)

  6. Download: Region3_November2025_KPI_Report.xlsx
```

---

## 11. BEST PRACTICES & RECOMMENDATIONS

### **Metric Design Principles**

1. **Keep It Simple**
   - Start with 5-10 core KPIs
   - Add more as needed, don't overwhelm users
   - Each metric should answer: "What action do we take if this goes red?"

2. **Actionable Metrics**
   ```
   Good:  Computer Availability % (can add more, repair existing)
   Bad:   Weather conditions (can't control)

   Good:  Report submission timeliness (can remind users)
   Bad:   Number of employees (can't directly influence)
   ```

3. **Meaningful Thresholds**
   - Base on historical data or industry standards
   - Green should be achievable with good performance
   - Yellow should trigger attention, not panic
   - Red should require immediate action

4. **Consistent Naming**
   ```
   MetricCode Convention:
   {CATEGORY}_{METRIC}_{UNIT}

   Examples:
   COMPUTER_AVAILABILITY_PCT
   LAN_STATUS
   BACKUP_SUCCESS_COUNT
   CHAIPRO_VERSION
   REPORT_SUBMISSION_TIMELINESS
   ```

---

### **Performance Optimization**

1. **Indexing Strategy**
   ```sql
   -- Critical indexes already in schema:
   IX_TenantMetrics_TimeSeries (ReportingPeriod DESC, TenantId, MetricId)
   IX_TenantMetrics_Tenant (TenantId, MetricId, ReportingPeriod DESC)

   -- For dashboard queries:
   CREATE INDEX IX_TenantMetrics_Dashboard
   ON TenantMetrics(TenantId, ReportingPeriod, MetricId)
   WHERE ReportingPeriod >= DATEADD(MONTH, -6, GETDATE());
   ```

2. **Caching**
   ```csharp
   // Cache current period metrics for 1 hour
   var cacheKey = $"TenantMetrics_{tenantId}_{period}";
   var metrics = await _cache.GetOrCreateAsync(cacheKey,
       entry => {
           entry.SlidingExpiration = TimeSpan.FromHours(1);
           return GetTenantMetricsAsync(tenantId, period);
       });
   ```

3. **Pre-Aggregation**
   ```sql
   -- Use TenantPerformanceSnapshot for monthly rollups
   -- Avoids expensive JOINs in reports
   -- Updated once per month after all submissions
   ```

---

### **Error Handling**

1. **Metric Population Failures**
   ```csharp
   try {
       var value = CalculateMetric(formula, sourceValues);
       InsertTenantMetric(value);
       LogMetricPopulation(status: "Success");
   }
   catch (DivideByZeroException) {
       LogMetricPopulation(status: "Failed", error: "Total is zero");
       // Don't fail entire submission - continue with other metrics
   }
   catch (FormulaException ex) {
       LogMetricPopulation(status: "Failed", error: ex.Message);
       AlertAdmin("Invalid formula in MetricId=X");
   }
   ```

2. **Background Job Failures**
   ```csharp
   // Hangfire will retry 3 times automatically
   // Log failures in SystemMetricLogs
   // Send alert after 3 consecutive failures
   if (consecutiveFailures >= 3) {
       CreateAlert(severity: "Critical",
                   message: "Backup check failing for 3 days");
   }
   ```

---

### **Security Considerations**

1. **Metric Access Control**
   ```csharp
   // Users can only view metrics for tenants they have access to
   var userTenants = GetUserTenantAccess(userId);
   var metrics = GetMetrics()
       .Where(m => userTenants.Contains(m.TenantId));
   ```

2. **Audit Trail**
   ```sql
   -- MetricPopulationLog tracks:
   - Who populated metric (PopulatedBy)
   - When (PopulatedDate)
   - Source (SubmissionId or JobName)
   - Calculation used (CalculationFormula)

   -- SystemMetricLogs tracks:
   - All automated checks
   - Success/failure status
   - Execution time
   ```

---

### **Testing Strategy**

1. **Unit Tests**
   ```csharp
   [Test]
   public void CalculateAvailabilityPercentage_ValidInputs_ReturnsCorrectValue()
   {
       // Arrange
       var total = 25;
       var operational = 23;
       var formula = "(operational / total) * 100";

       // Act
       var result = FormulaEngine.Calculate(formula,
           new { operational, total });

       // Assert
       Assert.That(result, Is.EqualTo(92.00).Within(0.01));
   }
   ```

2. **Integration Tests**
   ```csharp
   [Test]
   public async Task SubmitForm_WithMetricMappings_PopulatesMetrics()
   {
       // Arrange
       var submission = CreateTestSubmission(total: 25, operational: 23);

       // Act
       await _service.ProcessSubmissionAsync(submission);

       // Assert
       var metric = await _context.TenantMetrics
           .Where(m => m.TenantId == 50 && m.MetricId == 5)
           .FirstOrDefaultAsync();

       Assert.That(metric.NumericValue, Is.EqualTo(92.00));
       Assert.That(metric.SourceType, Is.EqualTo("SystemCalculated"));
   }
   ```

---

## SUMMARY

**Section 3: Metrics & KPI Tracking** is the **intelligence layer** of the KTDA ICT Reporting System.

**Key Capabilities:**
1. ✅ **Automated Data Collection** - No manual metric entry
2. ✅ **Real-time Calculation** - Instant KPI updates on form submission
3. ✅ **Historical Tracking** - Time-series data for trend analysis
4. ✅ **Threshold Monitoring** - Traffic light alerts (Green/Yellow/Red)
5. ✅ **Multi-Source Support** - Forms, APIs, background jobs, compliance checks
6. ✅ **Flexible Formulas** - Complex calculations from multiple fields
7. ✅ **Complete Audit Trail** - Track how every metric value was calculated

**Benefits:**
- **97% time savings** - Metrics auto-populated from forms (vs manual compilation)
- **Real-time visibility** - Instant dashboards (vs 7-14 day lag)
- **Proactive monitoring** - Automated checks catch issues before users report
- **Data-driven decisions** - Historical trends inform planning
- **Accountability** - Complete audit trail of who entered what and when

**Next Steps:**
1. Define your top 10-15 KPIs (start small!)
2. Create metric definitions in database
3. Map form fields to metrics during template creation
4. Set up 2-3 background jobs for critical checks (backup, licenses)
5. Build initial dashboards
6. Train users on interpreting traffic lights
7. Iterate based on feedback

---

**End of Section 3 Deep Dive**
