# Form Item Metric Mapping - Complete Guide

## Overview

The **Form Item Metric Mapping** system connects form fields to KPI metrics through a two-stage configuration process. This allows form submissions to automatically populate metrics for reporting and analytics.

---

## Database Tables

### 1. **FormItemMetricMappings** (Configuration)
Stores the mapping between form fields and metrics.

```sql
CREATE TABLE FormItemMetricMappings (
    MappingId INT PRIMARY KEY,
    ItemId INT,              -- Links to FormTemplateItem
    MetricId INT,            -- Links to MetricDefinition
    MappingType VARCHAR(30), -- Direct, SystemCalculated, BinaryCompliance
    TransformationLogic TEXT,-- JSON formula for calculated metrics
    ExpectedValue VARCHAR,   -- For binary compliance
    IsActive BIT
)
```

### 2. **MetricPopulationLog** (Audit Trail)
Logs every time a metric is populated from a form submission.

```sql
CREATE TABLE MetricPopulationLog (
    LogId BIGINT PRIMARY KEY,
    SubmissionId INT,        -- Form submission that triggered this
    MetricId INT,
    MappingId INT,
    SourceItemId INT,
    SourceValue VARCHAR,     -- Raw value from form
    CalculatedValue DECIMAL, -- Final calculated value
    Status VARCHAR(20),      -- Success, Failed, Skipped
    PopulatedDate DATETIME
)
```

### 3. **TenantMetrics** (Final Data)
Stores the actual metric values for reporting.

```sql
CREATE TABLE TenantMetrics (
    MetricValueId BIGINT PRIMARY KEY,
    TenantId INT,
    MetricId INT,
    ReportingPeriod DATE,
    NumericValue DECIMAL,    -- The actual metric value
    TextValue VARCHAR,       -- Original text for reference
    SourceType VARCHAR,      -- UserInput, SystemCalculated
    SourceReferenceId INT,   -- Links to SubmissionId
    CapturedDate DATETIME
)
```

### 4. **MetricDefinition** (Metric Catalog)
Defines all available metrics in the system.

```sql
CREATE TABLE MetricDefinitions (
    MetricId INT PRIMARY KEY,
    MetricCode VARCHAR(50),
    MetricName VARCHAR(200),
    Category VARCHAR(100),   -- Hardware, Software, Infrastructure
    SourceType VARCHAR(30),  -- UserInput, SystemCalculated
    DataType VARCHAR(20),    -- Integer, Decimal, Percentage
    Unit VARCHAR(50),        -- Count, Percentage, GB, etc.
    AggregationType VARCHAR, -- SUM, AVG, MAX, MIN
    IsKPI BIT
)
```

---

## Two-Stage Configuration Process

### **Stage 1: Field Scoring Configuration**
Configure how the field produces measurable values.

### **Stage 2: Metric Mapping**
Connect the configured field to actual metrics.

---

## Stage 1: Field Scoring Configuration

### Purpose
Define how a form field produces a numeric or measurable value **without** selecting which metric it maps to.

### Configuration Types

#### **Type A: Option Scoring (Checkbox/Dropdown/Radio)**
For fields with predefined options.

**Example: Local Area Network (LAN)**
```
Field Type: Checkbox
Options:
  - Operational      → Score: 100
  - Faulty           → Score: 50
  - Non Operational  → Score: 0

Result: Field produces values 0-100
```

**Database Impact:**
```sql
-- Updates FormItemOptions table
UPDATE FormItemOptions 
SET ScoreValue = 100 
WHERE ItemId = 123 AND OptionValue = 'operational';

UPDATE FormItemOptions 
SET ScoreValue = 50 
WHERE ItemId = 123 AND OptionValue = 'faulty';
```

#### **Type B: Simple Count (Number Fields)**
For numeric input fields.

**Example: Total Desktop Computers**
```
Field Type: Number
Configuration: Use field value directly

Result: Field produces direct numeric input
```

**Database Impact:**
```
No database changes - just configuration state
Field validation ensures numeric input
```

#### **Type C: No Scoring (Text Only)**
For non-numeric fields.

**Example: Server Model Name**
```
Field Type: Text
Configuration: Text only, no scoring

Result: Field cannot produce numeric values
Can only be used for categorical/status metrics
```

---

## Stage 2: Metric Mapping

### Purpose
Connect the configured field to one or more `MetricDefinition` records.

### Smart Filtering

The system automatically filters compatible metrics based on:
- **DataType compatibility** (Numeric fields → Numeric metrics)
- **SourceType** (Form fields → "UserInput" metrics)
- **Category suggestions** (Based on form section/context)
- **AggregationType** (Count fields → SUM aggregation)

### Mapping Types

#### **1. Direct Mapping**
Copies field value (or score) directly to metric.

**Example:**
```
Field: LAN Status (scored 0-100)
Mapping Type: Direct
Metric: LAN Equipment Health (Percentage)

When user submits "Faulty" (score=50):
→ TenantMetrics.NumericValue = 50
```

**Use Cases:**
- Status metrics
- Performance scores
- Count metrics
- Direct measurements

#### **2. System Calculated (Formula)**
Combines multiple fields using a formula.

**Example:**
```
Formula: (Operational_Count / Total_Count) * 100
Fields: 
  - Operational_Count (ItemId: 123)
  - Total_Count (ItemId: 124)
Metric: Infrastructure Availability %

When user submits: Operational=45, Total=50:
→ Calculation: (45/50) * 100 = 90%
→ TenantMetrics.NumericValue = 90
```

**Database Storage:**
```json
// FormItemMetricMapping.TransformationLogic
{
  "formula": "(item123 / item124) * 100",
  "sourceItems": [123, 124],
  "itemAliases": {
    "item123": "Operational_Count",
    "item124": "Total_Count"
  }
}
```

#### **3. Binary Compliance**
Converts field value to 100% (pass) or 0% (fail).

**Example:**
```
Field: Safety Check Completed
Expected Value: "Yes"
Metric: Safety Compliance Rate (%)

When user submits:
  "Yes" → 100%
  "No"  → 0%
```

---

## Complete Data Flow

### 1. Configuration Phase (Form Builder)

```
Administrator creates form template:
  
  Step 1: Create form field "LAN Status" (Checkbox)
  Step 2: Add options (Operational, Faulty, Non-Operational)
  
  Step 3a: Configure Scoring (TAB 1)
    → Set ScoreValue on each option
    → Operational = 100, Faulty = 50, Non-Operational = 0
  
  Step 3b: Map to Metric (TAB 2)
    → Select metric "LAN Equipment Health"
    → Or create new metric if needed
    → Set mapping type: "Direct"
    → Save creates FormItemMetricMapping record
```

### 2. Submission Phase (End User)

```
End user fills form:
  → Selects "Faulty" for LAN Status
  → Submits form
  
FormTemplateSubmission created:
  → SubmissionId: 555
  
FormTemplateResponse created:
  → ItemId: 123
  → ResponseValue: "faulty"
```

### 3. Population Phase (Automated)

```
MetricPopulationService.PopulateMetricsFromSubmission(555):

  A) Find all mappings for this template
     → FormItemMetricMappings WHERE TemplateId = X
  
  B) For each mapping (e.g., LAN field):
     1. Get form response: "faulty"
     2. Look up option score: ScoreValue = 50
     3. Create audit log:
        MetricPopulationLog {
          SubmissionId: 555,
          SourceValue: "faulty",
          CalculatedValue: 50,
          Status: "Success"
        }
     4. Update metric:
        TenantMetrics {
          MetricId: 456,
          NumericValue: 50,
          TextValue: "Faulty",
          SourceReferenceId: 555
        }
```

### 4. Reporting Phase (Dashboard)

```sql
-- Query average LAN health across all factories
SELECT 
    AVG(NumericValue) as AvgLANHealth,
    COUNT(*) as TotalSubmissions
FROM TenantMetrics
WHERE MetricId = 456
  AND ReportingPeriod = '2024-12-01';

-- Result: 66.7% average health (can track trends, alerts, etc.)
```

---

## UI Workflow

### Tab 1: Scoring Configuration

**Fields with Options (Checkbox/Dropdown):**
```
┌─────────────────────────────────────────┐
│ Enable Scoring: [✓] Use Option Scores  │
│                                         │
│ Option Scores:                          │
│ ┌──────────────┬──────┐                │
│ │ Operational  │ 100  │                │
│ │ Faulty       │  50  │                │
│ │ Non-Oper.    │   0  │                │
│ └──────────────┴──────┘                │
│                                         │
│ [Continue to Metric Mapping →]         │
└─────────────────────────────────────────┘
```

**Numeric Fields:**
```
┌─────────────────────────────────────────┐
│ Scoring Method:                         │
│ ⦿ Use field value directly (Count)     │
│ ○ No scoring                            │
│                                         │
│ [Continue to Metric Mapping →]         │
└─────────────────────────────────────────┘
```

### Tab 2: Metric Mapping

**Select Existing Metric:**
```
┌─────────────────────────────────────────┐
│ Compatible Metrics (Filtered):          │
│                                         │
│ ▼ Hardware (3)                          │
│   ○ LAN Equipment Health (Percentage)   │
│   ○ Network Status Score                │
│   ○ Hardware Availability               │
│                                         │
│ ▼ Infrastructure (2)                    │
│   ○ Infrastructure Health               │
│   ○ System Operational Score            │
│                                         │
│ Mapping Type: [Direct ▼]               │
│                                         │
│ [Create Mapping]                        │
└─────────────────────────────────────────┘
```

**Create New Metric:**
```
┌─────────────────────────────────────────┐
│ Metric Code:    [LAN_HEALTH_SCORE]     │
│ Metric Name:    [LAN Health Score]     │
│ Category:       [Hardware ▼]           │
│ Data Type:      [Percentage ▼]         │
│ Unit:           [Percentage]           │
│ Aggregation:    [AVG ▼]                │
│                                         │
│ [Create Metric & Map]                  │
└─────────────────────────────────────────┘
```

---

## Key Benefits

### ✅ Separation of Concerns
- **Field Configuration**: How field produces values (Tab 1)
- **Metric Mapping**: Where values go (Tab 2)

### ✅ Flexibility
- Same field can map to multiple metrics
- Create metrics on-the-go
- Reuse existing metrics across forms

### ✅ Data Quality
- Smart filtering prevents incompatible mappings
- Validation ensures data integrity
- Audit trail tracks all population events

### ✅ Automation
- Metrics populate automatically on form submission
- No manual data entry required
- Real-time KPI updates

---

## Example Scenarios

### Scenario 1: Performance Scoring

**Field:** LAN Equipment Status  
**Configuration:**
- Tab 1: Set option scores (100, 50, 0)
- Tab 2: Map to "LAN Health Score" metric (Direct)

**Result:** Forms automatically populate LAN health percentages

### Scenario 2: Counting Hardware

**Field:** Total Desktop Computers  
**Configuration:**
- Tab 1: Enable counting (use direct value)
- Tab 2: Map to "TOTAL_COMPUTERS" metric (Direct)

**Result:** Can aggregate: "750 computers across all factories"

### Scenario 3: Availability Calculation

**Field 1:** Operational Servers (Number)  
**Field 2:** Total Servers (Number)  
**Configuration:**
- Tab 1: Both fields enable counting
- Tab 2: Create calculated mapping:
  - Formula: `(Field1 / Field2) * 100`
  - Map to "Server Availability %" metric

**Result:** Automatic availability % calculation

### Scenario 4: Compliance Tracking

**Field:** Safety Protocol Completed  
**Configuration:**
- Tab 1: Options with scores (Yes=100, No=0)
- Tab 2: Map to "Safety Compliance Rate" (Binary)
  - Expected Value: "Yes"

**Result:** Track compliance rates: "95% compliance this month"

---

## Technical Implementation

### API Endpoints

```
GET    /api/metric-mapping/template/{id}/fields
GET    /api/metric-mapping/field/{id}
GET    /api/metric-mapping/field/{id}/compatible-metrics
POST   /api/metric-mapping/field/{id}/configure-scoring
POST   /api/metric-mapping/create
POST   /api/metric-mapping/metrics/create-and-map
PUT    /api/metric-mapping/update/{id}
DELETE /api/metric-mapping/delete/{id}
```

### Service Layer

```csharp
// MetricMappingService
Task<List<MetricDefinitionViewModel>> GetCompatibleMetricsForFieldAsync(int itemId);
Task<bool> ConfigureFieldScoringAsync(int itemId, FieldScoringDto dto);
Task<MetricMappingViewModel> CreateMappingAsync(CreateMappingDto dto);

// MetricPopulationService
Task PopulateMetricsFromSubmissionAsync(int submissionId);
Task<decimal?> CalculateFormulaAsync(string formula, Dictionary<int, decimal> values);
```

---

## Validation Rules

### Field-Level Validation
- Numeric fields must have valid number ranges
- Option scores must be numeric
- At least 2 options required for scored fields

### Mapping-Level Validation
- Cannot map text fields to numeric metrics
- Formula fields must reference valid items
- Binary compliance requires expected value
- Metric code must be unique

### Submission-Level Validation
- Required fields must have values
- Numeric fields must be valid numbers
- Option values must exist in field configuration

---

## Troubleshooting

### Issue: Metric Not Populating

**Check:**
1. Is mapping active? (`FormItemMetricMapping.IsActive = true`)
2. Did user submit the field? (Check `FormTemplateResponse`)
3. Does field have scoring configured? (Check `FormItemOption.ScoreValue`)
4. Check `MetricPopulationLog` for errors

### Issue: Wrong Value Stored

**Check:**
1. Option scores are correct (`FormItemOption.ScoreValue`)
2. Mapping type is appropriate (Direct vs Calculated)
3. Formula is valid (for calculated mappings)
4. Check `MetricPopulationLog.CalculationFormula`

### Issue: Metric Not Showing in Dashboard

**Check:**
1. Is metric marked as KPI? (`MetricDefinition.IsKPI`)
2. Is data in correct reporting period?
3. Query aggregation type matches metric definition
4. Check `TenantMetrics` table directly

---

## Future Enhancements

- [ ] Formula builder UI with drag-and-drop
- [ ] Test mapping with sample values
- [ ] Bulk mapping operations
- [ ] Mapping templates (reuse across forms)
- [ ] Advanced transformations (conditional logic)
- [ ] Historical metric versioning
- [ ] Real-time metric preview during form design

---

## Summary

The Form Item Metric Mapping system provides a powerful, flexible way to automatically populate KPIs from form submissions. The two-stage configuration (Scoring → Mapping) ensures clean separation of concerns while maintaining data integrity and enabling complex metric calculations.

**Key Principle:** Configure how fields produce values first, then map those values to metrics second.
