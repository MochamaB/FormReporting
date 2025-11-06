# SECTION 3: METRICS & KPI TRACKING - Workflows & Actions

**Module:** Performance Metrics & KPI Tracking
**Tables:** 5 (MetricDefinitions, TenantMetrics, SystemMetricLogs, FormItemMetricMappings, MetricPopulationLog)

---

## 1. METRIC DEFINITIONS

### **CRUD Operations:**
- **CREATE** Metric Definition
- **READ** Metric Definition (Single)
- **READ** All Metrics (List, filter by Category/SourceType)
- **READ** KPI Metrics Only (IsKPI = 1)
- **UPDATE** Metric Details (thresholds, description)
- **DELETE** Metric (Check for dependencies first)

### **Business Rules:**
- MetricCode must be unique
- SourceType must be: 'UserInput', 'SystemCalculated', 'ExternalSystem', 'ComplianceTracking', or 'AutomatedCheck'
- DataType must be: Integer, Decimal, Percentage, Boolean, Text, Duration, Date, DateTime
- If IsKPI = 1, thresholds should be defined (Green, Yellow, Red)
- ThresholdYellow and ThresholdRed typically have same value (boundary between yellow/red)
- Cannot delete if FormItemMetricMappings or TenantMetrics exist (FK constraint)

### **Workflows:**

#### **WF-3.1: Create Metric Definition**
```
Trigger: Admin needs to track new KPI
Steps:
  1. Navigate to "System Configuration → Metrics"
  2. Click "Add New Metric"
  3. Enter basic details:
     - MetricCode: PRINTER_AVAILABILITY_PCT
     - MetricName: Printer Availability Percentage
     - Category: Hardware
     - Description: % of printers operational vs total

  4. Select Source Type:
     ( ) UserInput - Direct from single form field
     (•) SystemCalculated - Formula from multiple fields
     ( ) ExternalSystem - Pull from API
     ( ) ComplianceTracking - Deadline monitoring
     ( ) AutomatedCheck - Background job

  5. Configure Data Type:
     Data Type: [Percentage ▼]
     Unit: [Percentage ▼]
     Aggregation: [AVG ▼] (for regional summaries)

  6. Set KPI Configuration:
     [✓] This is a KPI
     Threshold Green (Good): 90.0
     Threshold Yellow (Warning): 75.0
     Threshold Red (Critical): 75.0

  7. Preview traffic light:
     100% ──── 90% ──── 75% ──── 0%
      🟢       │   ⚠️    │   🔴

  8. Click "Save Metric"
  9. System validates uniqueness of MetricCode
  10. System assigns MetricId
  11. Redirect to "Create Form Mapping" (if SystemCalculated)
```

#### **WF-3.2: Update Metric Thresholds**
```
Trigger: Admin needs to adjust KPI targets based on performance data
Steps:
  1. Navigate to "Metrics → Edit {MetricName}"
  2. View current thresholds:
     Green: 95% | Yellow: 85% | Red: 85%
  3. View historical data:
     Last 6 months average: 88%
     Max achieved: 94%
     Min achieved: 82%
  4. Adjust thresholds:
     Green: 92% (more achievable)
     Yellow: 80% (wider warning range)
     Red: 80%
  5. Preview impact:
     - 5 factories move from Yellow to Green
     - 2 factories remain Yellow
     - 0 factories in Red
  6. Add change reason: "Based on 6-month performance review"
  7. Click "Update"
  8. System logs change in AuditLogs
  9. Recalculate all current period metrics' threshold status
  10. Update dashboards
  11. Notify affected tenants if status changed
```

#### **WF-3.3: Delete Metric Definition**
```
Trigger: Admin attempts to delete metric
Steps:
  1. Navigate to "Metrics → Delete {MetricName}"
  2. System checks dependencies:
     a. FormItemMetricMappings: 2 mappings found
     b. TenantMetrics: 450 historical records
     c. ReportDefinitions: Used in 3 reports
  3. Show warning:
     "Cannot delete. Metric is in use:
      - 2 form fields mapped
      - 450 historical records
      - 3 reports reference this metric

     Options:
     [ ] Deactivate instead (sets IsActive = 0)
     [ ] Force delete (removes all data - CANNOT UNDO)"
  4. If user selects "Deactivate":
     - Set IsActive = 0
     - Hide from form builder
     - Preserve all historical data
     - Keep in reports (read-only)
  5. If user selects "Force delete" (requires SYSADMIN):
     - Confirm with password
     - DELETE from FormItemMetricMappings
     - DELETE from TenantMetrics (all historical data)
     - DELETE from MetricDefinitions
     - Log in AuditLogs with Reason
```

---

## 2. TENANT METRICS

### **CRUD Operations:**
- **CREATE** Metric Value (via form submission or background job)
- **READ** Metric Value (Single tenant, single metric, single period)
- **READ** Time-Series Data (trend for specific metric)
- **READ** All Metrics for Tenant-Period
- **UPDATE** Metric Value (overwrite existing)
- **DELETE** Metric Value (rare - requires audit approval)

### **Business Rules:**
- Unique constraint: (TenantId, MetricId, ReportingPeriod) - one value per tenant per metric per period
- NumericValue OR TextValue must be populated (based on MetricDefinition.DataType)
- SourceType must match expected values
- SourceReferenceId should link back to source (SubmissionId or LogId)
- ReportingPeriod typically 1st of month (e.g., '2025-11-01' for November)

### **Workflows:**

#### **WF-3.4: Auto-Populate Metric from Form Submission (Direct)**
```
Trigger: User submits form with metric-mapped fields
Example: User submits "Total Computers" = 25

Steps:
  1. FormTemplateSubmissions created (SubmissionId=12345)
  2. FormTemplateResponses created (ItemId=20, ResponseText='25')
  3. Background processor triggered
  4. Query: Get FormItemMetricMappings WHERE ItemId = 20
     Result: MetricId=1 (TOTAL_COMPUTERS), MappingType='Direct'
  5. Direct mapping - no transformation:
     Value = 25
  6. UPSERT TenantMetrics:
     IF EXISTS (TenantId=50, MetricId=1, ReportingPeriod='2025-11-01')
       → UPDATE NumericValue=25, CapturedDate=NOW(), SourceReferenceId=12345
     ELSE
       → INSERT (TenantId=50, MetricId=1, NumericValue=25, SourceType='UserInput')
  7. Log in MetricPopulationLog:
     SubmissionId=12345, MetricId=1, Status='Success'
  8. No threshold check (not a KPI)
```

#### **WF-3.5: Auto-Populate Metric from Form Submission (Calculated)**
```
Trigger: User submits form with calculated metric
Example: Total=25, Operational=23 → Availability%

Steps:
  1. FormTemplateSubmissions created (SubmissionId=12345)
  2. FormTemplateResponses created:
     ItemId=20, ResponseText='25' (Total)
     ItemId=21, ResponseText='23' (Operational)
  3. Background processor triggered
  4. Query: Get FormItemMetricMappings WHERE ItemId IN (20, 21)
     Result: MetricId=5 (COMPUTER_AVAILABILITY_PCT), MappingType='SystemCalculated'
  5. Get TransformationLogic JSON:
     {
       "formula": "(operational / total) * 100",
       "sourceItems": [21, 20],
       "itemAliases": {"operational": 21, "total": 20},
       "roundTo": 2
     }
  6. Retrieve source values:
     operational (ItemId=21): 23
     total (ItemId=20): 25
  7. Execute formula:
     (23 / 25) * 100 = 92.00
     Round to 2 decimals: 92.00
  8. Validate result:
     - Check >= 0 and <= 100 (percentage)
     - Check not NULL or NaN
  9. UPSERT TenantMetrics:
     TenantId=50, MetricId=5, NumericValue=92.00
     SourceType='SystemCalculated', SourceReferenceId=12345
  10. Log in MetricPopulationLog:
      SubmissionId=12345, MetricId=5
      SourceValue='23', CalculatedValue=92.00
      CalculationFormula='(operational / total) * 100'
      Status='Success', ProcessingTimeMs=45
  11. Check threshold (IsKPI=1):
      Value=92.00, ThresholdGreen=95.0, ThresholdYellow=85.0
      92 >= 85 AND < 95 → Yellow (Warning)
  12. Trigger alert if threshold breached:
      Status changed from Green to Yellow → Create notification
```

#### **WF-3.6: Manual Metric Entry (Override)**
```
Trigger: Admin needs to manually enter/correct metric value
Example: External system unavailable, need to manually record data

Steps:
  1. Navigate to "Metrics → Manual Entry"
  2. Select Tenant: [Factory 50 - Kiambu ▼]
  3. Select Period: [November 2025 ▼]
  4. Select Metric: [Network Uptime % ▼]
  5. View current value: 98.5% (from last automated check)
  6. Enter new value: 97.2
  7. Enter reason: "Correcting for downtime on Nov 15 (external system missed)"
  8. Click "Update"
  9. System validation:
     - Value within expected range (0-100 for percentage)
     - User has permission: Users.Manage OR TenantMetrics.Edit
  10. UPSERT TenantMetrics:
      NumericValue=97.2
      SourceType='Manual'
      CapturedBy=CurrentUserId
  11. Log in AuditLogs:
      Action='Update', TableName='TenantMetrics'
      OldValue='98.5', NewValue='97.2'
      Reason='Correcting for downtime...'
  12. Show success: "Metric updated. Dashboard will refresh."
```

#### **WF-3.7: Query Time-Series Data**
```
Trigger: User views trend chart on dashboard
Steps:
  1. User navigates to Factory Dashboard
  2. System queries last 6 months data:

     SELECT
       ReportingPeriod,
       NumericValue,
       TextValue
     FROM TenantMetrics
     WHERE TenantId = 50
       AND MetricId = 5 (COMPUTER_AVAILABILITY_PCT)
       AND ReportingPeriod >= DATEADD(MONTH, -6, GETDATE())
     ORDER BY ReportingPeriod DESC

  3. Result:
     2025-11-01: 92.00%
     2025-10-01: 90.00%
     2025-09-01: 94.00%
     2025-08-01: 96.00%
     2025-07-01: 93.00%
     2025-06-01: 88.00%

  4. Calculate trend:
     - Average: 92.17%
     - Trend: ↗ Improving (+2% vs last month)
     - Months above target (95%): 1 of 6
     - Months in warning (85-95%): 5 of 6
     - Months critical (<85%): 0 of 6

  5. Render line chart with threshold bands
  6. Show insight: "Improving trend, but still below target"
```

---

## 3. SYSTEM METRIC LOGS

### **CRUD Operations:**
- **CREATE** Log Entry (via background jobs)
- **READ** Log Entry (Single)
- **READ** Logs for Tenant (filter by date range)
- **READ** Logs for Job (filter by JobName)
- **READ** Failed Checks (Status='Failed')
- **DELETE** Old Logs (retention policy: 90 days)

### **Business Rules:**
- Created by Hangfire jobs or system processes only (not user-facing)
- Status must be: 'Success', 'Failed', or 'Warning'
- If Status='Failed', ErrorMessage should be populated
- Details field stores JSON for additional context
- ExecutionDuration in milliseconds (performance tracking)

### **Workflows:**

#### **WF-3.8: Daily Backup Verification Job**
```
Trigger: Hangfire job 'DailyBackupVerification' runs at 2:00 AM daily
Steps:
  1. Job starts, log: "DailyBackupVerification started"
  2. Get all active tenants:
     SELECT * FROM Tenants WHERE IsActive = 1
     Result: 69 factories + 1 head office + 9 subsidiaries = 79 tenants

  3. FOR EACH tenant:

     a) Determine backup path:
        BasePath = "\\backup\"
        TenantPath = BasePath + Tenant.TenantCode
        Example: "\\backup\factory50\"

     b) Get latest .bak file:
        Files = Directory.GetFiles(TenantPath, "*.bak")
        LatestFile = Files.OrderByDescending(f => f.LastWriteTime).First()
        Example: factory50_2025-11-06.bak
        LastBackupDate = File.LastWriteTime (2025-11-06 01:30:00)

     c) Check if recent (within 24 hours):
        Age = DateTime.Now - LastBackupDate
        IsRecent = Age.TotalHours < 24
        Example: 30 minutes → True (Success)

     d) Determine status:
        IF IsRecent THEN
          Status = 'Success'
          NumericValue = 1
          TextValue = 'Backup current'
        ELSE
          Status = 'Failed'
          NumericValue = 0
          TextValue = 'Backup outdated'
        END IF

     e) Log result:
        INSERT INTO SystemMetricLogs (
          TenantId = tenant.Id,
          MetricId = 15 (BACKUP_STATUS),
          CheckDate = NOW(),
          Status = Status,
          NumericValue = NumericValue,
          TextValue = TextValue,
          Details = JSON({
            "backupFile": LatestFile.Name,
            "backupDate": LastBackupDate,
            "backupSize": File.Size,
            "ageHours": Age.TotalHours
          }),
          JobName = 'DailyBackupVerification',
          ExecutionDuration = timer.ElapsedMilliseconds
        );

     f) IF Status = 'Success':
        - Update TenantMetrics:
          UPSERT (TenantId, MetricId=15, NumericValue=1, SourceType='AutomatedCheck')

     g) IF Status = 'Failed':
        - Update TenantMetrics:
          UPSERT (TenantId, MetricId=15, NumericValue=0, SourceType='AutomatedCheck')
        - Trigger CRITICAL alert:
          CreateAlert(tenantId, "Backup failed/outdated", severity='Critical')
        - Email factory ICT + regional manager
        - Dashboard shows RED status

  4. Job completion summary:
     TotalChecked = 79
     Successes = 77
     Failures = 2 (Factory 52, Factory 65)

  5. Email summary to Head Office ICT Manager:
     "Daily Backup Verification: 77/79 passed. 2 failures require attention."

  6. Job ends, log: "DailyBackupVerification completed in 45 seconds"
```

#### **WF-3.9: License Expiry Check Job**
```
Trigger: Hangfire job 'LicenseExpiryCheck' runs daily at 3:00 AM
Steps:
  1. Job starts
  2. Query all active licenses:
     SELECT * FROM SoftwareLicenses WHERE IsActive = 1
  3. FOR EACH license:
     a) Calculate days until expiry:
        DaysUntilExpiry = (ExpiryDate - TODAY()).TotalDays

     b) Determine alert level:
        IF DaysUntilExpiry <= 0:
          Status = 'Failed', AlertLevel = 'Critical', Message = 'License expired'
        ELSE IF DaysUntilExpiry <= 7:
          Status = 'Warning', AlertLevel = 'High', Message = 'Expires in 7 days'
        ELSE IF DaysUntilExpiry <= 30:
          Status = 'Warning', AlertLevel = 'Medium', Message = 'Expires in 30 days'
        ELSE:
          Status = 'Success', AlertLevel = 'None', Message = 'License valid'
        END IF

     c) Log result:
        INSERT INTO SystemMetricLogs (
          TenantId = license.TenantId,
          MetricId = 20 (LICENSE_COMPLIANCE),
          CheckDate = NOW(),
          Status = Status,
          Details = JSON({
            "licenseId": license.Id,
            "productName": license.ProductName,
            "expiryDate": license.ExpiryDate,
            "daysUntilExpiry": DaysUntilExpiry
          }),
          JobName = 'LicenseExpiryCheck'
        );

     d) IF Status != 'Success':
        - Create alert notification
        - Email factory manager + ICT
        - Update dashboard with warning/critical

  4. Summary:
     - Total licenses: 150
     - Valid: 145
     - Expiring soon (30 days): 3
     - Expiring very soon (7 days): 1
     - Expired: 1

  5. Email summary to procurement team
```

#### **WF-3.10: Pull Network Uptime from External System**
```
Trigger: Hangfire job 'PullNetworkUptime' runs every 6 hours
Steps:
  1. Job starts
  2. FOR EACH tenant with network monitoring:

     a) Get PRTG sensor ID from configuration:
        SensorId = Tenant.NetworkMonitoringSensorId
        Example: "sensor_factory50_wan"

     b) Call PRTG API:
        Endpoint: https://prtg.ktda.co.ke/api/getsensordata
        Parameters: sensorId, period=24hours
        Response: { "uptime_percentage": 98.5, "downtime_minutes": 22 }

     c) Extract data:
        UptimePercentage = response.uptime_percentage
        DowntimeMinutes = response.downtime_minutes

     d) Log result:
        INSERT INTO SystemMetricLogs (
          TenantId = tenant.Id,
          MetricId = 18 (NETWORK_UPTIME_PCT),
          CheckDate = NOW(),
          Status = 'Success',
          NumericValue = UptimePercentage,
          TextValue = UptimePercentage + '%',
          Details = JSON({
            "sensorId": SensorId,
            "period": "24hours",
            "downtimeMinutes": DowntimeMinutes,
            "apiResponseTime": timer.ElapsedMs
          }),
          JobName = 'PullNetworkUptime',
          ExecutionDuration = timer.ElapsedMilliseconds
        );

     e) Update TenantMetrics:
        UPSERT (TenantId, MetricId=18, NumericValue=UptimePercentage)

     f) Check threshold:
        IF UptimePercentage < 95% THEN
          CreateAlert("Network uptime below target", severity='Warning')
        END IF

  3. Handle API failures:
     CATCH HttpException:
       Log: Status='Failed', ErrorMessage='PRTG API unavailable'
       No dashboard update (keep last known value)
       Email admin if 3 consecutive failures
```

#### **WF-3.11: Purge Old Logs**
```
Trigger: Hangfire job 'PurgeOldSystemLogs' runs monthly on 1st at 4:00 AM
Steps:
  1. Define retention policy:
     RetentionDays = 90 (keep 3 months)
     CutoffDate = TODAY - 90 days

  2. Count records to delete:
     SELECT COUNT(*)
     FROM SystemMetricLogs
     WHERE CheckDate < CutoffDate
     Result: 45,000 records

  3. Archive to file (optional):
     Export to: SystemMetricLogs_Archive_2025-11.csv
     Upload to: Azure Blob Storage (long-term archive)

  4. Delete old records:
     DELETE FROM SystemMetricLogs
     WHERE CheckDate < CutoffDate

  5. Log result:
     "Purged 45,000 records older than 90 days"
     "Database size reduced by 250 MB"

  6. Email summary to DBA
```

---

## 4. FORM ITEM METRIC MAPPINGS

### **CRUD Operations:**
- **CREATE** Mapping (during form template creation)
- **READ** Mappings for Form Template
- **READ** Mappings for Metric (which forms populate this metric)
- **UPDATE** Mapping Configuration (formula, transformation logic)
- **DELETE** Mapping (removes automatic population)

### **Business Rules:**
- Unique constraint: (ItemId, MetricId) - one mapping per field-metric pair
- MappingType must be: 'Direct', 'SystemCalculated', 'BinaryCompliance', or 'Derived'
- If MappingType='SystemCalculated', TransformationLogic (JSON) required
- If MappingType='BinaryCompliance', ExpectedValue required
- ItemId must reference FormTemplateItems
- MetricId must reference MetricDefinitions

### **Workflows:**

#### **WF-3.12: Create Direct Mapping**
```
Trigger: Admin creating form field that directly maps to metric
Example: "Total Computers" field → TOTAL_COMPUTERS metric

Steps:
  1. In Form Builder, editing field "Total Computers" (ItemId=20)
  2. Expand section: "📊 Metric Integration"
  3. Toggle: [✓] Link this field to a metric
  4. Select Metric:
     Search: [total comput...]
     Dropdown shows: TOTAL_COMPUTERS (Count) - Hardware
     Select: TOTAL_COMPUTERS

  5. Mapping Type:
     (•) Direct - Value goes straight to metric (no transformation)
     ( ) Calculated - Use formula from multiple fields
     ( ) Binary Compliance - Convert Yes/No to 0%/100%
     ( ) Derived - Complex logic

  6. Preview:
     User enters: 25
     Metric value: 25
     No transformation needed

  7. Click "Save"
  8. System creates:
     INSERT INTO FormItemMetricMappings (
       ItemId = 20,
       MetricId = (SELECT MetricId WHERE MetricCode='TOTAL_COMPUTERS'),
       MappingType = 'Direct',
       TransformationLogic = NULL,
       IsActive = 1
     );

  9. Show confirmation: "Field linked to metric. Values will auto-populate on submission."
```

#### **WF-3.13: Create Calculated Mapping**
```
Trigger: Admin creating calculated metric from multiple fields
Example: Computer Availability % = (Operational / Total) × 100

Steps:
  1. In Form Builder, editing field "Operational Computers" (ItemId=21)
  2. Expand: "📊 Metric Integration"
  3. Toggle: [✓] Link to metric
  4. Select Metric: COMPUTER_AVAILABILITY_PCT
  5. Mapping Type: (•) Calculated
  6. Formula Builder opens:

     ┌────────────────────────────────────────────────┐
     │ FORMULA BUILDER                                 │
     ├────────────────────────────────────────────────┤
     │                                                 │
     │ Formula: [(operational / total) * 100      ]   │
     │                                                 │
     │ Source Fields:                                  │
     │   Variable Name    Field                       │
     │   operational  →  [Operational Computers ▼]    │
     │   total        →  [Total Computers ▼]          │
     │                                                 │
     │ Options:                                        │
     │   Round to: [2] decimal places                 │
     │   Min value: [0  ]  Max value: [100]           │
     │                                                 │
     │ [Test Formula]                                  │
     │                                                 │
     │ Test Values:                                    │
     │   operational = [23]                           │
     │   total = [25]                                 │
     │                                                 │
     │ Result: 92.00%  ✓                              │
     │                                                 │
     └────────────────────────────────────────────────┘

  7. Click "Save Mapping"
  8. System creates:
     INSERT INTO FormItemMetricMappings (
       ItemId = 21,
       MetricId = (SELECT MetricId WHERE MetricCode='COMPUTER_AVAILABILITY_PCT'),
       MappingType = 'SystemCalculated',
       TransformationLogic = '{
         "formula": "(operational / total) * 100",
         "sourceItems": [21, 20],
         "itemAliases": {
           "operational": 21,
           "total": 20
         },
         "roundTo": 2,
         "minValue": 0,
         "maxValue": 100
       }',
       IsActive = 1
     );

  9. Validation:
     - Check all source fields exist in same template
     - Test formula with sample data
     - Validate JSON structure

  10. Show confirmation: "Calculated metric configured. Will compute on submission."
```

#### **WF-3.14: Create Binary Compliance Mapping**
```
Trigger: Admin mapping Yes/No question to compliance metric
Example: "Is backup in place?" → BACKUP_COMPLIANCE (0% or 100%)

Steps:
  1. In Form Builder, editing field "Is backup in place?" (ItemId=60)
     Field Type: Boolean (Yes/No dropdown)
  2. Expand: "📊 Metric Integration"
  3. Toggle: [✓] Link to metric
  4. Select Metric: BACKUP_COMPLIANCE
  5. Mapping Type: (•) Binary Compliance
  6. Configure compliance:

     ┌────────────────────────────────────────────────┐
     │ BINARY COMPLIANCE SETUP                         │
     ├────────────────────────────────────────────────┤
     │                                                 │
     │ Expected Answer: [Yes ▼]                       │
     │                                                 │
     │ If answer matches expected:                    │
     │   → 100% compliant (Green)                     │
     │                                                 │
     │ If answer does not match:                      │
     │   → 0% compliant (Red)                         │
     │                                                 │
     │ Preview:                                        │
     │   User selects "Yes" → 100% 🟢                 │
     │   User selects "No"  → 0%   🔴                 │
     │                                                 │
     └────────────────────────────────────────────────┘

  7. Click "Save"
  8. System creates:
     INSERT INTO FormItemMetricMappings (
       ItemId = 60,
       MetricId = (SELECT MetricId WHERE MetricCode='BACKUP_COMPLIANCE'),
       MappingType = 'BinaryCompliance',
       ExpectedValue = 'Yes',
       TransformationLogic = NULL,
       IsActive = 1
     );

  9. Show confirmation: "Binary compliance configured."
```

#### **WF-3.15: Test Metric Mapping**
```
Trigger: Admin wants to test mapping before publishing template
Steps:
  1. In Form Builder, click "Test Metric Mappings"
  2. System shows preview form:
     - All fields editable
     - Shows which fields map to metrics (badge/icon)
  3. Admin fills sample data:
     Total Computers: 25
     Operational Computers: 23
     LAN Status: Yes
     Backup in place: Yes
  4. Click "Calculate Metrics"
  5. System processes (without saving):
     a) Direct mapping: Total → 25
     b) Calculated: (23/25)*100 → 92.00%
     c) Binary: Yes = 100%
  6. Show results:
     ┌────────────────────────────────────────────────┐
     │ METRIC CALCULATION RESULTS                      │
     ├────────────────────────────────────────────────┤
     │ ✓ TOTAL_COMPUTERS: 25                          │
     │ ⚠️ COMPUTER_AVAILABILITY_PCT: 92.00% (Yellow)  │
     │ ✓ BACKUP_COMPLIANCE: 100% (Green)              │
     │                                                 │
     │ All mappings working correctly!                │
     └────────────────────────────────────────────────┘
  7. Admin can adjust and re-test
  8. Click "Publish Template" when satisfied
```

---

## 5. METRIC POPULATION LOG

### **CRUD Operations:**
- **CREATE** Log Entry (automatic on metric population)
- **READ** Logs for Submission
- **READ** Logs for Metric (audit trail)
- **READ** Failed Populations (Status='Failed')
- **DELETE** Old Logs (retention policy)

### **Business Rules:**
- Created automatically when metrics populated from form submissions
- Status must be: 'Success', 'Failed', 'Skipped', or 'Pending'
- If Status='Failed', ErrorMessage should be populated
- ProcessingTimeMs tracks performance (for optimization)
- CalculationFormula stored for audit trail (shows how value was calculated)

### **Workflows:**

#### **WF-3.16: Audit Metric Calculation**
```
Trigger: Admin investigating why metric value seems incorrect
Steps:
  1. Navigate to "Metrics → Audit Trail"
  2. Filter:
     Tenant: [Factory 50 - Kiambu ▼]
     Metric: [Computer Availability % ▼]
     Period: [November 2025 ▼]
  3. System queries:
     SELECT * FROM MetricPopulationLog
     WHERE SubmissionId IN (
       SELECT SubmissionId FROM FormTemplateSubmissions
       WHERE TenantId=50 AND ReportingPeriod='2025-11-01'
     )
     AND MetricId = 5
     ORDER BY PopulatedDate DESC

  4. Show audit trail:
     ┌──────────────────────────────────────────────────────┐
     │ METRIC POPULATION AUDIT TRAIL                         │
     ├──────────────────────────────────────────────────────┤
     │ Date: Nov 6, 2025 10:30 AM                           │
     │ Submission: #12345                                    │
     │ Submitted by: John Doe (Factory ICT)                 │
     │                                                       │
     │ Source Values:                                        │
     │   Total Computers (ItemId=20): 25                    │
     │   Operational Computers (ItemId=21): 23              │
     │                                                       │
     │ Calculation:                                          │
     │   Formula: (operational / total) * 100               │
     │   Substituted: (23 / 25) * 100                       │
     │   Result: 92.00                                       │
     │                                                       │
     │ Metric Value: 92.00%                                 │
     │ Status: Success ✓                                     │
     │ Processing Time: 45ms                                │
     │                                                       │
     │ Previous Value: 90.00% (Oct 2025)                    │
     │ Change: +2.00% (↗ Improved)                          │
     └──────────────────────────────────────────────────────┘

  5. Admin can:
     - View source submission (link)
     - Download calculation details (PDF)
     - Recalculate if formula changed
```

#### **WF-3.17: Investigate Failed Population**
```
Trigger: Admin sees "Failed" status in MetricPopulationLog
Steps:
  1. Navigate to "Metrics → Failed Calculations"
  2. Filter: Last 7 days
  3. System shows:
     ┌──────────────────────────────────────────────────────┐
     │ FAILED METRIC CALCULATIONS (Last 7 Days)             │
     ├──────────────────────────────────────────────────────┤
     │ 3 failures found                                      │
     │                                                       │
     │ 1. Factory 52 - Computer Availability %              │
     │    Date: Nov 5, 2025                                 │
     │    Error: "Divide by zero - Total Computers = 0"     │
     │    [View Details] [Retry]                            │
     │                                                       │
     │ 2. Factory 65 - Network Uptime %                     │
     │    Date: Nov 4, 2025                                 │
     │    Error: "Source field not found: ItemId=99"        │
     │    [View Details] [Fix Mapping]                      │
     │                                                       │
     │ 3. Factory 70 - Software Compliance                  │
     │    Date: Nov 3, 2025                                 │
     │    Error: "Invalid formula syntax"                   │
     │    [View Details] [Edit Formula]                     │
     └──────────────────────────────────────────────────────┘

  4. Admin clicks "View Details" for #1:
     SubmissionId: 12567
     Source Value: Total Computers = 0
     Calculation Attempted: (23 / 0) * 100
     Error: Division by zero

  5. Admin actions:
     Option A: Fix source data
       - Navigate to submission
       - Edit Total Computers: 0 → 25
       - Save (triggers recalculation)

     Option B: Add validation
       - Add FormItemValidation: Total must be > 0
       - Prevents future zero entries

     Option C: Update formula
       - Change formula to handle zero:
         "IF(total > 0, (operational / total) * 100, 0)"

  6. Click "Retry Calculation"
  7. System recalculates with fixed data
  8. Update log Status: 'Failed' → 'Success'
```

#### **WF-3.18: Performance Analysis**
```
Trigger: Admin optimizing metric population performance
Steps:
  1. Navigate to "System → Performance → Metric Population"
  2. Query average processing times:
     SELECT
       md.MetricName,
       fimm.MappingType,
       AVG(mpl.ProcessingTimeMs) AS AvgTimeMs,
       MAX(mpl.ProcessingTimeMs) AS MaxTimeMs,
       COUNT(*) AS TotalCalculations
     FROM MetricPopulationLog mpl
     INNER JOIN MetricDefinitions md ON mpl.MetricId = md.MetricId
     INNER JOIN FormItemMetricMappings fimm ON mpl.MappingId = fimm.MappingId
     WHERE mpl.PopulatedDate >= DATEADD(DAY, -30, GETDATE())
       AND mpl.Status = 'Success'
     GROUP BY md.MetricName, fimm.MappingType
     ORDER BY AVG(mpl.ProcessingTimeMs) DESC

  3. Show results:
     ┌────────────────────────────────────────────────────────────┐
     │ METRIC POPULATION PERFORMANCE (Last 30 Days)               │
     ├────────────────────────────────────────────────────────────┤
     │ Metric                        Avg Time  Max Time  Count    │
     │ ──────────────────────────────────────────────────────────│
     │ Overall System Health (Derived)  320ms    1200ms   150  ⚠️│
     │ Computer Availability (Calc)      45ms     120ms   2100  ✓│
     │ LAN Status (Direct)                5ms      15ms   2100  ✓│
     │ Total Computers (Direct)           3ms      10ms   2100  ✓│
     └────────────────────────────────────────────────────────────┘

  4. Identify bottleneck:
     "Overall System Health" metric taking 320ms average (too slow)
     MappingType: 'Derived' (complex weighted average)

  5. Optimization options:
     - Cache intermediate calculations
     - Pre-compute monthly snapshots
     - Simplify formula
     - Async processing (don't block submission)

  6. Implement optimization and re-measure
```

---

## CROSS-TABLE WORKFLOWS

### **WF-3.19: Complete Metric Setup (End-to-End)**
```
Trigger: Admin setting up new "Printer Availability %" metric
Steps:
  1. STEP 1: Define Metric
     Navigate to: Metrics → Add New
     - MetricCode: PRINTER_AVAILABILITY_PCT
     - MetricName: Printer Availability Percentage
     - Category: Hardware
     - SourceType: SystemCalculated
     - DataType: Percentage
     - IsKPI: Yes
     - ThresholdGreen: 90, ThresholdYellow: 75
     Click "Save" → MetricId=30 assigned

  2. STEP 2: Add Form Fields
     Navigate to: Form Templates → Factory Monthly Report
     Add Section: "Printer Status"
     Add Fields:
       a) Total Printers (ItemId=50, DataType=Number)
       b) Operational Printers (ItemId=51, DataType=Number)
       c) Printers Under Repair (ItemId=52, DataType=Number)

  3. STEP 3: Create Mappings
     Field: Total Printers
       - Link to: TOTAL_PRINTERS metric (Direct)

     Field: Operational Printers
       - Link to: PRINTER_AVAILABILITY_PCT metric (Calculated)
       - Formula: "(operational / total) * 100"
       - Source: operational=51, total=50

  4. STEP 4: Add Validation
     Field: Operational Printers
       - Validation: Must be ≤ Total Printers
       - Error: "Operational cannot exceed total"

  5. STEP 5: Test
     Click "Test Form"
     Enter: Total=10, Operational=9
     Preview Metrics: 90.00% (Green ✓)

  6. STEP 6: Publish
     Click "Publish Template"
     Assign to: All Factories (TenantType='Factory')

  7. STEP 7: Create Dashboard Widget
     Navigate to: Dashboard → Add Widget
     - Type: KPI Scorecard
     - Metric: PRINTER_AVAILABILITY_PCT
     - Show: Current value + Trend
     - Threshold indicators: Green/Yellow/Red

  8. STEP 8: Create Report
     Navigate to: Reports → Add Report
     - Name: "Printer Status Summary"
     - Metrics: PRINTER_AVAILABILITY_PCT, TOTAL_PRINTERS
     - Grouping: By Region
     - Schedule: Monthly on 10th

  9. Complete! Metric is now:
     ✓ Defined in system
     ✓ Mapped to form fields
     ✓ Auto-populated on submission
     ✓ Visible on dashboards
     ✓ Included in reports
```

### **WF-3.20: Monthly Metric Aggregation**
```
Trigger: Scheduled job on 1st of month (after submission deadline)
Purpose: Create monthly snapshots for fast reporting

Steps:
  1. Job: 'MonthlyMetricAggregation' runs on 1st at 6:00 AM
  2. Parameters:
     PreviousMonth = LastMonth() (e.g., November 2025)

  3. FOR EACH Metric WHERE IsKPI = 1:

     a) Aggregate tenant-level data:
        INSERT INTO TenantPerformanceSnapshot (
          TenantId,
          ReportingPeriod,
          MetricId,
          MetricValue,
          ThresholdStatus,
          ComparedToPrevious
        )
        SELECT
          tm.TenantId,
          tm.ReportingPeriod,
          tm.MetricId,
          tm.NumericValue,
          CASE
            WHEN tm.NumericValue >= md.ThresholdGreen THEN 'Green'
            WHEN tm.NumericValue >= md.ThresholdYellow THEN 'Yellow'
            ELSE 'Red'
          END AS ThresholdStatus,
          tm.NumericValue - LAG(tm.NumericValue) OVER (
            PARTITION BY tm.TenantId, tm.MetricId
            ORDER BY tm.ReportingPeriod
          ) AS ComparedToPrevious
        FROM TenantMetrics tm
        INNER JOIN MetricDefinitions md ON tm.MetricId = md.MetricId
        WHERE tm.ReportingPeriod = PreviousMonth
          AND md.IsKPI = 1;

     b) Aggregate regional data:
        INSERT INTO RegionalMonthlySnapshot (
          RegionId,
          ReportingPeriod,
          MetricId,
          AvgValue,
          MinValue,
          MaxValue,
          TenantsGreen,
          TenantsYellow,
          TenantsRed
        )
        SELECT
          t.RegionId,
          tm.ReportingPeriod,
          tm.MetricId,
          AVG(tm.NumericValue) AS AvgValue,
          MIN(tm.NumericValue) AS MinValue,
          MAX(tm.NumericValue) AS MaxValue,
          SUM(CASE WHEN tm.NumericValue >= md.ThresholdGreen THEN 1 ELSE 0 END) AS TenantsGreen,
          SUM(CASE WHEN tm.NumericValue >= md.ThresholdYellow
                   AND tm.NumericValue < md.ThresholdGreen THEN 1 ELSE 0 END) AS TenantsYellow,
          SUM(CASE WHEN tm.NumericValue < md.ThresholdYellow THEN 1 ELSE 0 END) AS TenantsRed
        FROM TenantMetrics tm
        INNER JOIN Tenants t ON tm.TenantId = t.TenantId
        INNER JOIN MetricDefinitions md ON tm.MetricId = md.MetricId
        WHERE tm.ReportingPeriod = PreviousMonth
          AND t.RegionId IS NOT NULL
        GROUP BY t.RegionId, tm.ReportingPeriod, tm.MetricId;

  4. Summary:
     - 15 KPI metrics aggregated
     - 69 factories × 15 metrics = 1,035 tenant snapshots
     - 7 regions × 15 metrics = 105 regional snapshots

  5. Benefits:
     - Dashboard queries now use snapshots (fast)
     - Reports generated instantly (no JOINs)
     - Historical data preserved
```

### **WF-3.21: Alert on Threshold Breach**
```
Trigger: Metric value crosses threshold (Green→Yellow or Yellow→Red)
Steps:
  1. Metric updated in TenantMetrics:
     TenantId=50, MetricId=5 (COMPUTER_AVAILABILITY_PCT)
     Previous: 95.0% (Green)
     Current: 92.0% (Yellow)

  2. System detects threshold change:
     IF (PreviousThreshold = 'Green' AND CurrentThreshold = 'Yellow')
        OR (CurrentThreshold = 'Red') THEN
       TriggerAlert()
     END IF

  3. Create alert:
     INSERT INTO AlertHistory (
       TenantId = 50,
       AlertDefinitionId = (SELECT Id WHERE MetricId=5 AND AlertType='ThresholdBreach'),
       Severity = 'Medium',
       Status = 'Triggered',
       Message = 'Computer Availability dropped to 92% (below 95% target)',
       TriggeredDate = NOW()
     );

  4. Create notifications:
     Recipients: Factory ICT, Regional Manager

     INSERT INTO Notifications (
       TenantId = 50,
       Title = '⚠️ Computer Availability Below Target',
       Message = 'Kiambu Factory computer availability is now 92% (target: 95%).
                  2 additional computers are operational, but 3 total.',
       NotificationType = 'Warning',
       Priority = 'Medium',
       EntityType = 'Metric',
       EntityId = 5
     );

     INSERT INTO NotificationRecipients (
       NotificationId = [above],
       UserId = [Factory ICT UserId],
       DeliveryChannel = 'Email, InApp'
     );

  5. Send email:
     To: factory50.ict@ktda.co.ke, region3.manager@ktda.co.ke
     Subject: "⚠️ Kiambu Factory - Computer Availability Below Target"
     Body:
       "Hello,

       Computer availability at Kiambu Factory has dropped to 92%,
       which is below the 95% target threshold.

       Current Status:
       - Total Computers: 25
       - Operational: 23
       - Availability: 92%
       - Status: ⚠️ Warning (Yellow)

       Recommended Actions:
       - Repair or replace 2 computers to reach 95% target (24 operational)
       - Review maintenance logs for recurring issues

       View Details: [Dashboard Link]

       This is an automated alert from KTDA ICT Reporting System."

  6. Update dashboard:
     - Show yellow warning icon
     - Display alert banner
     - Add to "Action Items" widget
```

---

## SUMMARY

### **Total Operations:**
- **CRUD Actions:** 40+ operations across 5 tables
- **Workflows:** 21 defined workflows
- **Business Rules:** 25+ validation rules

### **Key Integration Points:**
1. **FormTemplateItems ↔ FormItemMetricMappings** → Links form fields to metrics
2. **FormTemplateSubmissions → MetricPopulationLog → TenantMetrics** → Auto-population flow
3. **MetricDefinitions ↔ TenantMetrics** → Time-series storage
4. **Hangfire Jobs → SystemMetricLogs → TenantMetrics** → Automated checks
5. **TenantMetrics ↔ AlertDefinitions** → Threshold monitoring
6. **TenantMetrics → TenantPerformanceSnapshot** → Monthly aggregation
7. **TenantMetrics → Reports & Dashboards** → Visualization

### **Permissions Required:**
```
Metric Management:
  - Metrics.Define → SYSADMIN, HO_ICT_MGR
  - Metrics.Edit → SYSADMIN, HO_ICT_MGR
  - Metrics.Delete → SYSADMIN only
  - Metrics.ViewAll → SYSADMIN, HO_ICT_MGR, REGIONAL_MGR, AUDITOR

Metric Data:
  - TenantMetrics.View → All users (filtered by tenant access)
  - TenantMetrics.ManualEntry → SYSADMIN, HO_ICT_MGR (override capability)
  - TenantMetrics.Export → SYSADMIN, HO_ICT_MGR, REGIONAL_MGR

Form-Metric Mapping:
  - FormItemMetricMappings.Create → SYSADMIN, HO_ICT_MGR (during template design)
  - FormItemMetricMappings.Edit → SYSADMIN, HO_ICT_MGR
  - FormItemMetricMappings.Delete → SYSADMIN only

Audit & Logs:
  - MetricPopulationLog.View → SYSADMIN, HO_ICT_MGR, AUDITOR
  - SystemMetricLogs.View → SYSADMIN, HO_ICT_MGR
```

### **Automation Summary:**
```
Automated Processes:
├─ Form submission → Metric population (immediate)
├─ Threshold checking → Alert creation (immediate)
├─ Daily backup verification (2:00 AM daily)
├─ License expiry checks (3:00 AM daily)
├─ Network uptime pull (every 6 hours)
├─ Monthly aggregation (1st of month, 6:00 AM)
└─ Old log purging (1st of month, 4:00 AM)

Manual Processes:
├─ Metric definition (one-time setup)
├─ Form-to-metric mapping (during template creation)
├─ Threshold adjustment (as needed based on performance)
└─ Manual metric override (exception cases only)
```

---

**End of Section 3 Workflows**
