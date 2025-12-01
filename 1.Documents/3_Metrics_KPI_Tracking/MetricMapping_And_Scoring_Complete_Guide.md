# METRIC MAPPING & SCORING SYSTEM - Complete Architecture Guide

**Version:** 1.0  
**Last Updated:** November 29, 2025  
**Module:** Metrics & KPI Tracking (Section 3) + Form Templates (Section 4)  

---

## TABLE OF CONTENTS

1. [System Overview](#1-system-overview)
2. [Core Database Architecture](#2-core-database-architecture)
3. [Metric Definitions Explained](#3-metric-definitions-explained)
4. [Form Item Metric Mapping](#4-form-item-metric-mapping)
5. [The Four Mapping Types](#5-the-four-mapping-types)
6. [Scoring System Architecture](#6-scoring-system-architecture)
7. [Complete Data Flow](#7-complete-data-flow)
8. [Implementation Examples](#8-implementation-examples)
9. [Advanced Scenarios](#9-advanced-scenarios)
10. [Implementation Sequence](#10-implementation-sequence)

---

## 1. SYSTEM OVERVIEW

### 1.1 The Big Picture

The Metric Mapping & Scoring System transforms **form submission data** into **measurable KPIs** and **assessment scores** that power dashboards, reports, and analytics.

```
┌─────────────────────────────────────────────────────────────────┐
│                   COMPLETE SYSTEM ARCHITECTURE                   │
└─────────────────────────────────────────────────────────────────┘

LAYER 1: METRIC DEFINITIONS (Master Catalog)
├─ Pre-defined KPIs and performance indicators
├─ Thresholds for traffic light indicators
└─ Data types, units, aggregation rules

LAYER 2: FORM TEMPLATES (Data Collection)
├─ Sections and Fields designed in Form Builder
├─ Field types: Text, Number, Dropdown, Radio, etc.
└─ Field Options with optional score values

LAYER 3: METRIC MAPPINGS (The Bridge)
├─ Links form fields to metrics
├─ Defines transformation logic
└─ Supports 4 mapping types: Direct, Calculated, BinaryCompliance, ScoreMapping

LAYER 4: FORM SUBMISSIONS (User Input)
├─ Users fill out forms with actual data
└─ Responses stored in FormTemplateResponses

LAYER 5: METRIC POPULATION (Automation)
├─ Service extracts response data
├─ Applies transformation logic
├─ Calculates formulas and scores
└─ Populates TenantMetrics table

LAYER 6: REPORTING & ANALYTICS (Output)
├─ Dashboards display metric values
├─ Traffic lights based on thresholds
├─ Trend analysis over time
└─ Comparative reports across tenants
```

### 1.2 Key Problems Solved

**Problem 1: Manual KPI Calculation**
- **Before:** Regional managers manually compile metrics from Excel sheets
- **After:** System automatically extracts and calculates KPIs from form submissions

**Problem 2: Inconsistent Scoring**
- **Before:** Assessment forms graded manually with subjective criteria
- **After:** Predefined scoring rules ensure consistent evaluation

**Problem 3: No Historical Trends**
- **Before:** Data scattered across spreadsheets, hard to track over time
- **After:** Time-series data in TenantMetrics enables trend analysis

**Problem 4: Delayed Reporting**
- **Before:** Wait weeks for regional reports compilation
- **After:** Real-time dashboard updates as forms are submitted

---

## 2. CORE DATABASE ARCHITECTURE

### 2.1 Entity Relationship Diagram

```
┌──────────────────────┐
│  MetricDefinitions   │  ← What metrics exist (Master catalog)
│  (Master Catalog)    │
├──────────────────────┤
│ PK: MetricId         │
│ MetricCode          │
│ MetricName          │
│ Category            │
│ SourceType          │
│ DataType            │
│ ThresholdGreen      │
│ ThresholdYellow     │
│ ThresholdRed        │
└──────┬───────────────┘
       │
       │ 1:N
       │
┌──────▼───────────────────────────┐
│  FormItemMetricMappings          │  ← How fields map to metrics
│  (The Bridge)                    │
├──────────────────────────────────┤
│ PK: MappingId                    │
│ FK: ItemId → FormTemplateItems   │
│ FK: MetricId → MetricDefinitions │
│ MappingType                      │
│ TransformationLogic (JSON)       │
│ ExpectedValue                    │
└──────┬───────────────────────────┘
       │
       │ N:1
       │
┌──────▼───────────────┐
│  FormTemplateItems   │  ← Form fields
├──────────────────────┤
│ PK: ItemId           │
│ ItemName             │
│ DataType             │
│ SectionId            │
└──────┬───────────────┘
       │
       │ 1:N
       │
┌──────▼───────────────┐
│  FormItemOptions     │  ← Dropdown/Radio options
├──────────────────────┤
│ PK: OptionId         │
│ FK: ItemId           │
│ OptionValue          │
│ OptionLabel          │
│ ScoreValue ⭐ NEW    │  ← Scoring support
└──────────────────────┘

SUBMISSION & POPULATION FLOW:

┌──────────────────────────┐
│ FormTemplateSubmissions  │  ← User submits form
├──────────────────────────┤
│ PK: SubmissionId         │
│ TemplateId               │
│ TenantId                 │
│ Status                   │
└──────┬───────────────────┘
       │
       │ 1:N
       │
┌──────▼───────────────────┐
│  FormTemplateResponses   │  ← User's answers
├──────────────────────────┤
│ PK: ResponseId           │
│ FK: SubmissionId         │
│ FK: ItemId               │
│ ResponseValue            │
└──────────────────────────┘
       │
       │ Triggers
       │
┌──────▼───────────────────┐
│  MetricPopulationService │  ← Automation engine
└──────┬───────────────────┘
       │
       │ Creates
       │
┌──────▼───────────────────┐
│  TenantMetrics           │  ← Final metric values
├──────────────────────────┤
│ PK: MetricValueId        │
│ FK: TenantId             │
│ FK: MetricId             │
│ ReportingPeriod          │
│ NumericValue             │
│ SourceReferenceId        │
└──────┬───────────────────┘
       │
       │ Logs
       │
┌──────▼───────────────────────┐
│  MetricPopulationLog         │  ← Audit trail
├──────────────────────────────┤
│ PK: LogId                    │
│ FK: SubmissionId             │
│ FK: MetricId                 │
│ FK: MappingId                │
│ SourceValue                  │
│ CalculatedValue              │
│ CalculationFormula           │  ← NEW: Formula used
│ TransformationLogic (JSON)   │  ← NEW: Full logic applied
│ Status                       │
│ ErrorMessage                 │  ← NEW: Error details
│ ProcessedDate                │  ← NEW: Timestamp
└──────────────────────────────┘
```

---

## 3. METRIC DEFINITIONS EXPLAINED

### 3.1 What is a Metric Definition?

A **Metric Definition** is a template that describes:
- **WHAT** is being measured (e.g., "Computer Availability %")
- **HOW** it should be measured (data type, unit)
- **WHERE** the data comes from (user input, calculation, external system)
- **WHAT** constitutes good/bad performance (thresholds)

### 3.2 Database Schema

```sql
CREATE TABLE MetricDefinitions (
    MetricId INT PRIMARY KEY IDENTITY(1,1),
    
    -- Identity
    MetricCode NVARCHAR(50) NOT NULL UNIQUE,          -- e.g., 'COMPUTER_AVAILABILITY_PCT'
    MetricName NVARCHAR(200) NOT NULL,                 -- e.g., 'Computer Availability Percentage'
    Category NVARCHAR(100),                            -- e.g., 'Hardware', 'Network', 'Compliance'
    Description NVARCHAR(500),                         -- Human-readable explanation
    
    -- Source Configuration
    SourceType NVARCHAR(30) NOT NULL,                  -- HOW data is obtained
    -- Values: 'UserInput', 'SystemCalculated', 'ExternalSystem', 
    --         'ComplianceTracking', 'AutomatedCheck'
    
    -- Data Type
    DataType NVARCHAR(20) NOT NULL,                    -- WHAT format
    -- Values: 'Integer', 'Decimal', 'Percentage', 'Boolean', 
    --         'Text', 'Duration', 'Date', 'DateTime'
    
    Unit NVARCHAR(50),                                 -- HOW to display
    -- Values: 'Count', 'Percentage', 'GB', 'MB', 'Days', 'Hours', etc.
    
    AggregationType NVARCHAR(20),                      -- HOW to aggregate
    -- Values: 'SUM', 'AVG', 'MAX', 'MIN', 'LAST_VALUE', 'COUNT', 'NONE'
    
    -- KPI Thresholds (Traffic Light Indicators)
    IsKPI BIT DEFAULT 0,                               -- Is this a Key Performance Indicator?
    ThresholdGreen DECIMAL(18,4),                      -- ≥ this value = Green (Good)
    ThresholdYellow DECIMAL(18,4),                     -- ≥ this value = Yellow (Warning)
    ThresholdRed DECIMAL(18,4),                        -- NOT USED in calculation (kept for reference/minimum acceptable)
    
    -- Binary/Compliance Metrics
    ExpectedValue NVARCHAR(100),                       -- For binary checks: 'TRUE', 'Yes', 'Operational'
    ComplianceRule NVARCHAR(MAX),                      -- JSON: Deadline rules, validation logic
    
    -- Audit
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2,
    CreatedBy INT,
    ModifiedBy INT
)
```

### 3.3 Threshold Logic (Traffic Light Indicators)

**How Thresholds Work:**

The traffic light indicator is calculated using **ONLY TWO thresholds**: `ThresholdGreen` and `ThresholdYellow`.

```
TRAFFIC LIGHT LOGIC:
├─ IF value >= ThresholdGreen  → 🟢 GREEN (Good performance)
├─ ELSE IF value >= ThresholdYellow → 🟡 YELLOW (Warning)
└─ ELSE → 🔴 RED (Critical - below acceptable level)
```

**Important:** `ThresholdRed` is NOT used in the calculation logic. It exists in the schema for:
- Documentation purposes (to show minimum acceptable value)
- Future extensions (e.g., different visualization needs)
- Historical data (some systems may reference it)

**Recommendation:** When defining metrics, you can either:
1. **Set ThresholdRed = ThresholdYellow** (indicating red is anything below yellow)
2. **Set ThresholdRed = NULL** (indicating it's not used)
3. **Set ThresholdRed to a reference value** (e.g., absolute minimum tolerable)

**Example:**
```sql
-- Computer Availability Metric
ThresholdGreen: 95.0    -- ≥95% = Green
ThresholdYellow: 85.0   -- 85-94% = Yellow
ThresholdRed: 70.0      -- Reference only: 70% is absolute minimum

-- Actual values and their colors:
-- 97% → GREEN (≥95)
-- 90% → YELLOW (≥85 but <95)
-- 75% → RED (<85) ← ThresholdRed NOT used in this comparison
-- 60% → RED (<85)
```

### 3.5 The Five Source Types

#### **SourceType: 'UserInput'**
- Data comes directly from form field responses
- No transformation needed
- Example: "Total Computers" field → TOTAL_COMPUTERS metric

#### **SourceType: 'SystemCalculated'**
- Computed from multiple form fields using formulas
- Transformation logic required
- Example: Availability % = (Operational / Total) × 100

#### **SourceType: 'ExternalSystem'**
- Pulled from external APIs or monitoring tools
- Background jobs fetch data
- Example: Network uptime from PRTG monitoring

#### **SourceType: 'ComplianceTracking'**
- Deadline monitoring and compliance checks
- Compares submission dates against rules
- Example: "Was report submitted on time?"

#### **SourceType: 'AutomatedCheck'**
- Hangfire background jobs perform checks
- System validates conditions automatically
- Example: Daily backup success check

### 3.6 Example Metric Definitions

```sql
-- Example 1: Direct Input Metric
INSERT INTO MetricDefinitions VALUES (
    MetricCode: 'TOTAL_COMPUTERS',
    MetricName: 'Total Computers per Factory',
    Category: 'Hardware',
    SourceType: 'UserInput',                    -- Comes from form field
    DataType: 'Integer',
    Unit: 'Count',
    AggregationType: 'SUM',                     -- Sum across all factories
    IsKPI: 0,                                   -- Not a KPI, just a count
    Description: 'Total number of computers in factory inventory'
)

-- Example 2: Calculated Metric (KPI)
INSERT INTO MetricDefinitions VALUES (
    MetricCode: 'COMPUTER_AVAILABILITY_PCT',
    MetricName: 'Computer Availability Percentage',
    Category: 'Hardware',
    SourceType: 'SystemCalculated',             -- Computed from other fields
    DataType: 'Percentage',
    Unit: 'Percentage',
    AggregationType: 'AVG',                     -- Average across factories
    IsKPI: 1,                                   -- This is a KPI!
    ThresholdGreen: 95.0,                       -- ≥95% = Green
    ThresholdYellow: 85.0,                      -- ≥85% = Yellow (85-94%)
    ThresholdRed: 70.0,                         -- Reference only: absolute minimum
    Description: 'Percentage of operational computers vs total inventory'
)

-- Example 3: Binary Compliance Metric
INSERT INTO MetricDefinitions VALUES (
    MetricCode: 'LAN_OPERATIONAL_STATUS',
    MetricName: 'LAN Network Operational',
    Category: 'Network',
    SourceType: 'UserInput',
    DataType: 'Boolean',
    Unit: 'Status',
    ExpectedValue: 'Yes',                       -- Expected answer
    IsKPI: 1,
    ThresholdGreen: 1.0,                        -- 1.0 = Yes (Green)
    ThresholdYellow: 0.5,                       -- Not applicable for binary (never yellow)
    ThresholdRed: NULL,                         -- Reference only: not used
    Description: 'Is the LAN network operational?'
)

-- Example 4: Assessment Score Metric
INSERT INTO MetricDefinitions VALUES (
    MetricCode: 'ICT_INFRASTRUCTURE_SCORE',
    MetricName: 'ICT Infrastructure Assessment Score',
    Category: 'Assessment',
    SourceType: 'SystemCalculated',             -- Sum of all question scores
    DataType: 'Decimal',
    Unit: 'Points',
    AggregationType: 'AVG',
    IsKPI: 1,
    ThresholdGreen: 45.0,                       -- ≥45 = Green (≥90% of max 50)
    ThresholdYellow: 35.0,                      -- ≥35 = Yellow (≥70% of max)
    ThresholdRed: 25.0,                         -- Reference: 25 (50% of max) is absolute minimum
    Description: 'Total score from infrastructure assessment form (max 50 points)'
)
```

---

## 4. FORM ITEM METRIC MAPPING

### 4.1 What is FormItemMetricMapping?

The **bridge table** that connects form fields to metrics and defines how to transform response data into metric values.

### 4.2 Database Schema

```sql
CREATE TABLE FormItemMetricMappings (
    MappingId INT PRIMARY KEY IDENTITY(1,1),
    
    -- Relationships
    ItemId INT NOT NULL,                        -- FK to FormTemplateItems
    MetricId INT NOT NULL,                      -- FK to MetricDefinitions
    
    -- Mapping Configuration
    MappingType NVARCHAR(30) NOT NULL,          -- Type of transformation
    -- Values: 'Direct', 'Calculated', 'BinaryCompliance', 'ScoreMapping'
    
    TransformationLogic NVARCHAR(MAX),          -- JSON configuration
    ExpectedValue NVARCHAR(100),                -- For BinaryCompliance
    
    -- Audit
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    
    -- Constraints
    CONSTRAINT FK_ItemMetricMap_Item FOREIGN KEY (ItemId)
        REFERENCES FormTemplateItems(ItemId) ON DELETE CASCADE,
    CONSTRAINT FK_ItemMetricMap_Metric FOREIGN KEY (MetricId)
        REFERENCES MetricDefinitions(MetricId),
    CONSTRAINT UQ_ItemMetricMap UNIQUE (ItemId, MetricId)  -- One mapping per item-metric pair
)
```

### 4.3 Mapping Relationship

```
Form Template: "Monthly ICT Report"
│
├─ Section: Hardware Inventory
│  ├─ Field (ItemId: 45): "Total Computers" [Number]
│  │  │
│  │  └─ MAPPING 1 (Direct):
│  │     ├─ MetricId: 12 → TOTAL_COMPUTERS
│  │     ├─ MappingType: 'Direct'
│  │     └─ TransformationLogic: null
│  │
│  ├─ Field (ItemId: 46): "Operational Computers" [Number]
│  │  │
│  │  ├─ MAPPING 2 (Direct):
│  │  │  ├─ MetricId: 13 → OPERATIONAL_COMPUTERS
│  │  │  ├─ MappingType: 'Direct'
│  │  │  └─ TransformationLogic: null
│  │  │
│  │  └─ MAPPING 3 (Calculated):
│  │     ├─ MetricId: 14 → COMPUTER_AVAILABILITY_PCT
│  │     ├─ MappingType: 'Calculated'
│  │     └─ TransformationLogic: '{"formula": "(item46/item45)*100", "items": [45,46]}'
│  │
│  └─ Field (ItemId: 47): "Hardware Condition" [Dropdown]
│     │  Options: Excellent (10 pts), Good (7 pts), Fair (5 pts), Poor (2 pts)
│     │
│     └─ MAPPING 4 (ScoreMapping):
│        ├─ MetricId: 15 → ICT_INFRASTRUCTURE_SCORE
│        ├─ MappingType: 'ScoreMapping'
│        └─ TransformationLogic: '{"scoreRules": [...]}'
```

---

## 5. THE FOUR MAPPING TYPES

### 5.1 Type 1: Direct Mapping

**Use Case:** Field value becomes metric value with no transformation.

**Schema:**
```json
{
  "itemId": 45,
  "metricId": 12,
  "mappingType": "Direct",
  "transformationLogic": null
}
```

**Example:**
- Field: "Number of Computers" = 25
- Metric: TOTAL_COMPUTERS = 25

**Processing Logic:**
```csharp
if (mapping.MappingType == "Direct")
{
    var responseValue = submission.GetResponse(mapping.ItemId).ResponseValue;
    var metricValue = Convert.ToDecimal(responseValue);
    
    await SaveToTenantMetrics(
        tenantId: submission.TenantId,
        metricId: mapping.MetricId,
        value: metricValue,
        sourceReferenceId: submission.SubmissionId
    );
}
```

---

### 5.2 Type 2: Calculated Mapping

**Use Case:** Compute metric from multiple fields using a formula.

**Schema:**
```json
{
  "itemId": 45,
  "metricId": 14,
  "mappingType": "Calculated",
  "transformationLogic": {
    "formula": "(item46 / item45) * 100",
    "items": [45, 46],
    "description": "Operational / Total * 100",
    "roundTo": 2
  }
}
```

**Example:**
- Field 45: "Total Computers" = 25
- Field 46: "Operational Computers" = 20
- Metric: COMPUTER_AVAILABILITY_PCT = (20 / 25) × 100 = 80.0%

**Processing Logic:**
```csharp
if (mapping.MappingType == "Calculated")
{
    var logic = JsonConvert.DeserializeObject<CalculatedLogic>(mapping.TransformationLogic);

    // Fetch all source item values
    var values = new Dictionary<string, decimal>();
    foreach (var itemId in logic.Items)
    {
        var response = submission.GetResponse(itemId);

        // Handle null/empty responses
        if (string.IsNullOrWhiteSpace(response?.ResponseValue))
        {
            await LogMetricError(submission.SubmissionId, mapping.MetricId,
                $"Missing response for item {itemId}");
            continue; // Skip this mapping
        }

        values[$"item{itemId}"] = Convert.ToDecimal(response.ResponseValue);
    }

    // Validate division by zero BEFORE evaluating formula
    if (logic.ValidateDivisionByZero && formula.Contains("/"))
    {
        var denominators = ExtractDenominators(logic.Formula, values);
        if (denominators.Any(d => d == 0))
        {
            await LogMetricError(submission.SubmissionId, mapping.MetricId,
                "Division by zero - denominator is 0", logic.Formula);
            continue; // Skip this mapping
        }
    }

    // Evaluate formula (using expression evaluator)
    var formula = logic.Formula; // "(item46 / item45) * 100"
    decimal result;

    try
    {
        result = EvaluateFormula(formula, values);
    }
    catch (DivideByZeroException ex)
    {
        await LogMetricError(submission.SubmissionId, mapping.MetricId,
            $"Division by zero during formula evaluation: {ex.Message}", logic.Formula);
        continue; // Skip this mapping
    }
    catch (Exception ex)
    {
        await LogMetricError(submission.SubmissionId, mapping.MetricId,
            $"Formula evaluation error: {ex.Message}", logic.Formula);
        continue; // Skip this mapping
    }

    if (logic.RoundTo.HasValue)
        result = Math.Round(result, logic.RoundTo.Value);

    await SaveToTenantMetrics(
        tenantId: submission.TenantId,
        metricId: mapping.MetricId,
        value: result,
        sourceReferenceId: submission.SubmissionId
    );
}
```

---

### 5.3 Type 3: BinaryCompliance Mapping

**Use Case:** Convert Yes/No answers to 100% or 0% for compliance tracking.

**Schema:**
```json
{
  "itemId": 47,
  "metricId": 16,
  "mappingType": "BinaryCompliance",
  "expectedValue": "Yes",
  "transformationLogic": null
}
```

**Example:**
- Field: "Is LAN Operational?" = "Yes"
- Expected: "Yes"
- Metric: LAN_OPERATIONAL_STATUS = 1.0 (100%)

**Processing Logic:**
```csharp
if (mapping.MappingType == "BinaryCompliance")
{
    var responseValue = submission.GetResponse(mapping.ItemId).ResponseValue;
    var expectedValue = mapping.ExpectedValue;
    
    // Check if response matches expected value (case-insensitive)
    var isCompliant = string.Equals(responseValue, expectedValue, 
                                    StringComparison.OrdinalIgnoreCase);
    
    var metricValue = isCompliant ? 1.0m : 0.0m;  // 100% or 0%
    
    await SaveToTenantMetrics(
        tenantId: submission.TenantId,
        metricId: mapping.MetricId,
        value: metricValue,
        sourceReferenceId: submission.SubmissionId
    );
}
```

---

### 5.4 Type 4: ScoreMapping (NEW - For Assessment Forms)

**Use Case:** Map option selections to score values and aggregate for total score.

**TWO APPROACHES:**

#### **Approach A: Option-Based Scoring (Recommended - Simpler)**

Use the `ScoreValue` column in `FormItemOptions` table. Scores are stored with the options themselves.

**Schema:**
```json
{
  "itemId": 48,
  "metricId": 17,
  "mappingType": "ScoreMapping",
  "transformationLogic": {
    "scoreSource": "option",       // Use FormItemOptions.ScoreValue
    "contributesTo": "total",       // Aggregates to total score
    "weight": 1.0                   // Optional: weight multiplier
  }
}
```

**Database Setup:**
```sql
-- Options are pre-configured with scores
INSERT INTO FormItemOptions (ItemId: 48, OptionValue: 'excellent', OptionLabel: 'Excellent', ScoreValue: 10)
INSERT INTO FormItemOptions (ItemId: 48, OptionValue: 'good', OptionLabel: 'Good', ScoreValue: 7)
INSERT INTO FormItemOptions (ItemId: 48, OptionValue: 'fair', OptionLabel: 'Fair', ScoreValue: 5)
INSERT INTO FormItemOptions (ItemId: 48, OptionValue: 'poor', OptionLabel: 'Poor', ScoreValue: 2)
```

**Example:**
- Field: "Hardware Condition" = "good"
- System looks up option with OptionValue = "good"
- Finds ScoreValue = 7
- Metric: ICT_INFRASTRUCTURE_SCORE += 7 points

---

#### **Approach B: Rule-Based Scoring (Advanced - More Flexible)**

Define scoring rules directly in TransformationLogic JSON. Useful when you need dynamic scoring or want to override option scores.

**Schema:**
```json
{
  "itemId": 48,
  "metricId": 17,
  "mappingType": "ScoreMapping",
  "transformationLogic": {
    "scoreSource": "rules",        // Use rules defined here
    "scoreRules": [
      {"optionValue": "excellent", "scoreValue": 10, "weight": 1.0},
      {"optionValue": "good", "scoreValue": 7, "weight": 1.0},
      {"optionValue": "fair", "scoreValue": 5, "weight": 1.0},
      {"optionValue": "poor", "scoreValue": 2, "weight": 1.0}
    ],
    "aggregation": "sum",
    "maxScore": 10
  }
}
```

**Example:**
- Field: "Hardware Condition" = "good"
- System searches scoreRules array for "good"
- Finds scoreValue = 7
- Applies weight: 7 × 1.0 = 7
- Metric: ICT_INFRASTRUCTURE_SCORE += 7 points

---

**Processing Logic (Handles Both Approaches):**
```csharp
if (mapping.MappingType == "ScoreMapping")
{
    var logic = JsonConvert.DeserializeObject<ScoreMappingLogic>(mapping.TransformationLogic);
    var responseValue = submission.GetResponse(mapping.ItemId).ResponseValue;

    decimal scoreValue = 0;
    decimal weight = logic.Weight ?? 1.0m;

    // Determine which approach to use
    if (logic.ScoreSource == "option")
    {
        // APPROACH A: Get score from FormItemOptions.ScoreValue
        var option = await _context.FormItemOptions
            .FirstOrDefaultAsync(o => o.ItemId == mapping.ItemId &&
                                     o.OptionValue.Equals(responseValue, StringComparison.OrdinalIgnoreCase));

        if (option?.ScoreValue == null)
        {
            await LogMetricError(submission.SubmissionId, mapping.MetricId,
                $"No score defined for option '{responseValue}' in FormItemOptions");
            continue; // Skip this mapping
        }

        scoreValue = option.ScoreValue.Value;
    }
    else // "rules" or unspecified
    {
        // APPROACH B: Get score from TransformationLogic.ScoreRules
        var scoreRule = logic.ScoreRules?
            .FirstOrDefault(r => r.OptionValue.Equals(responseValue,
                                                      StringComparison.OrdinalIgnoreCase));

        if (scoreRule == null)
        {
            await LogMetricError(submission.SubmissionId, mapping.MetricId,
                $"No score rule defined for option '{responseValue}'");
            continue; // Skip this mapping
        }

        scoreValue = scoreRule.ScoreValue;
        weight *= scoreRule.Weight; // Combine weights
    }

    // Calculate final score
    var finalScore = scoreValue * weight;

    // NOTE: This saves INDIVIDUAL item score.
    // Aggregation to total score happens in a separate step (see Section 5.5)
    await SaveToTenantMetrics(
        tenantId: submission.TenantId,
        metricId: mapping.MetricId,
        value: finalScore,
        sourceReferenceId: submission.SubmissionId,
        sourceType: "ScoreMapping",
        metadataJson: $"{{\"itemId\": {mapping.ItemId}, \"responseValue\": \"{responseValue}\"}}"
    );
}
```

---

### 5.5 Score Aggregation Strategy

**The Critical Question:** When multiple fields map to the same metric with ScoreMapping, how are the scores combined?

**ANSWER: The service aggregates all ScoreMapping results for the same MetricId into a SINGLE TenantMetrics entry.**

#### **Aggregation Process:**

```csharp
public async Task ProcessScoreMappings(int submissionId)
{
    var submission = await GetSubmissionWithResponses(submissionId);
    var scoreMappings = await GetMappingsByType(submission.TemplateId, "ScoreMapping");

    // Group mappings by MetricId (all mappings contributing to same metric)
    var groupedMappings = scoreMappings.GroupBy(m => m.MetricId);

    foreach (var metricGroup in groupedMappings)
    {
        var metricId = metricGroup.Key;
        decimal totalScore = 0;
        var contributingItems = new List<object>();

        // Calculate score from each mapped field
        foreach (var mapping in metricGroup)
        {
            var itemScore = await CalculateItemScore(mapping, submission);

            if (itemScore.HasValue)
            {
                totalScore += itemScore.Value;

                // Track which items contributed (for audit trail)
                contributingItems.Add(new
                {
                    itemId = mapping.ItemId,
                    itemName = mapping.Item.ItemName,
                    score = itemScore.Value
                });
            }
        }

        // Save ONE aggregated entry to TenantMetrics
        await SaveToTenantMetrics(
            tenantId: submission.TenantId,
            metricId: metricId,
            value: totalScore,
            sourceReferenceId: submission.SubmissionId,
            sourceType: "ScoreMapping",
            metadataJson: JsonConvert.SerializeObject(new
            {
                totalScore = totalScore,
                itemCount = contributingItems.Count,
                breakdown = contributingItems
            })
        );

        // Log aggregation details
        await LogMetricPopulation(submission.SubmissionId, metricId,
            $"Aggregated {contributingItems.Count} item scores to total: {totalScore}",
            "Success");
    }
}
```

#### **Example: ICT Assessment Form**

```
Template: "ICT Infrastructure Assessment"
├─ Q1 (ItemId 45): "LAN Status" [Radio] → Selected: "Partial" (5 pts)
├─ Q2 (ItemId 46): "Network Speed" [Radio] → Selected: "Good" (7 pts)
├─ Q3 (ItemId 47): "Hardware Condition" [Radio] → Selected: "Fair" (5 pts)
├─ Q4 (ItemId 48): "Software Updates" [Radio] → Selected: "Good" (7 pts)
└─ Q5 (ItemId 49): "License Compliance" [Radio] → Selected: "Excellent" (10 pts)

All map to: MetricId 101 (ICT_INFRASTRUCTURE_SCORE)

RESULT:
TenantMetrics table gets ONE entry:
├─ MetricId: 101
├─ NumericValue: 34.0 (5 + 7 + 5 + 7 + 10)
├─ MetadataJson: {"totalScore": 34, "itemCount": 5, "breakdown": [...]}
└─ SourceReferenceId: 789 (SubmissionId)

NOT five separate entries!
```

#### **Weighted Aggregation Example:**

```
Q1: "Critical Infrastructure" (Weight: 2.0) → Selected: "Good" (7 pts) = 7 × 2.0 = 14 pts
Q2: "Standard Equipment" (Weight: 1.0) → Selected: "Fair" (5 pts) = 5 × 1.0 = 5 pts
Q3: "Optional Feature" (Weight: 0.5) → Selected: "Good" (7 pts) = 7 × 0.5 = 3.5 pts

Total Score: 14 + 5 + 3.5 = 22.5 points
```

#### **Section Scores AND Total Scores:**

If you want both section subtotals and grand total:

```sql
-- Create hierarchy of metrics
INSERT INTO MetricDefinitions (MetricId: 201, MetricCode: 'NETWORK_SECTION_SCORE')
INSERT INTO MetricDefinitions (MetricId: 202, MetricCode: 'HARDWARE_SECTION_SCORE')
INSERT INTO MetricDefinitions (MetricId: 203, MetricCode: 'TOTAL_ASSESSMENT_SCORE')

-- Map section questions to section metrics
INSERT INTO FormItemMetricMappings (ItemId: 45, MetricId: 201, MappingType: 'ScoreMapping')  -- Q1 → Network
INSERT INTO FormItemMetricMappings (ItemId: 46, MetricId: 201, MappingType: 'ScoreMapping')  -- Q2 → Network
INSERT INTO FormItemMetricMappings (ItemId: 47, MetricId: 202, MappingType: 'ScoreMapping')  -- Q3 → Hardware
INSERT INTO FormItemMetricMappings (ItemId: 48, MetricId: 202, MappingType: 'ScoreMapping')  -- Q4 → Hardware

-- THEN create calculated mapping for total (processed AFTER section scores)
INSERT INTO FormItemMetricMappings (
    ItemId: 45,  -- Representative item (required but not actually used)
    MetricId: 203,
    MappingType: 'Calculated',
    TransformationLogic: '{"formula": "metric201 + metric202", "sourceMetrics": [201, 202]}'
)

RESULT:
TenantMetrics table:
├─ MetricId 201: 12.0 (Network Section)
├─ MetricId 202: 15.0 (Hardware Section)
└─ MetricId 203: 27.0 (Total = 12 + 15)
```

---

### 5.6 Mapping Dependencies & Processing Order

**The Critical Challenge:** Some mappings depend on other mappings being processed first. Processing them in the wrong order will cause failures.

#### **Dependency Types:**

```
┌─────────────────────────────────────────────────────────────────┐
│                    MAPPING DEPENDENCIES                          │
└─────────────────────────────────────────────────────────────────┘

LEVEL 0: DIRECT MAPPINGS (No dependencies)
├─ Direct mappings only depend on form field responses
├─ Can be processed in any order or parallel
└─ Example: Item 45 → Metric 12 (TOTAL_COMPUTERS)

LEVEL 1: CALCULATED FROM FORM FIELDS (Depends on form data only)
├─ Depends on: Form field responses
├─ Can be processed after form is submitted
└─ Example: (item46 / item45) * 100 → Metric 14 (AVAILABILITY_PCT)

LEVEL 2: AGGREGATED SCORES (Depends on multiple field mappings)
├─ Depends on: Multiple ScoreMapping results
├─ Must be processed after all contributing ScoreMappings complete
└─ Example: Sum of Q1+Q2+Q3 scores → Metric 101 (SECTION_SCORE)

LEVEL 3: CALCULATED FROM OTHER METRICS (Depends on other metrics)
├─ Depends on: Other metric values in TenantMetrics
├─ Must be processed AFTER dependent metrics are saved
└─ Example: metric201 + metric202 → Metric 203 (TOTAL_SCORE)
```

#### **Processing Sequence Strategy:**

**Option A: Sequential Processing (Simple, Safer)**

Process all mappings for a submission in dependency order:

```csharp
public async Task PopulateMetrics(int submissionId)
{
    var submission = await GetSubmissionWithResponses(submissionId);
    var mappings = await GetAllMappingsForTemplate(submission.TemplateId);

    // PHASE 1: Direct mappings (no dependencies)
    var directMappings = mappings.Where(m => m.MappingType == "Direct");
    foreach (var mapping in directMappings)
    {
        await ProcessDirectMapping(mapping, submission);
    }

    // PHASE 2: BinaryCompliance mappings (only depend on form data)
    var complianceMappings = mappings.Where(m => m.MappingType == "BinaryCompliance");
    foreach (var mapping in complianceMappings)
    {
        await ProcessBinaryComplianceMapping(mapping, submission);
    }

    // PHASE 3: Calculated mappings that use form fields (item{Id} variables)
    var calculatedFieldMappings = mappings
        .Where(m => m.MappingType == "Calculated" && UsesFormFields(m.TransformationLogic));
    foreach (var mapping in calculatedFieldMappings)
    {
        await ProcessCalculatedMapping(mapping, submission);
    }

    // PHASE 4: ScoreMapping (individual item scores)
    var scoreMappings = mappings.Where(m => m.MappingType == "ScoreMapping");
    await ProcessScoreMappingsWithAggregation(scoreMappings, submission);

    // PHASE 5: Calculated mappings that use other metrics (metric{Id} variables)
    var calculatedMetricMappings = mappings
        .Where(m => m.MappingType == "Calculated" && UsesOtherMetrics(m.TransformationLogic));

    // Sort by dependency depth (metrics that depend on fewer others first)
    var orderedMappings = TopologicalSort(calculatedMetricMappings);

    foreach (var mapping in orderedMappings)
    {
        await ProcessCalculatedMapping(mapping, submission);
    }
}
```

**Option B: Dependency Graph with Retry (Advanced, More Flexible)**

Build a dependency graph and process mappings when their dependencies are satisfied:

```csharp
public async Task PopulateMetricsWithDependencyGraph(int submissionId)
{
    var submission = await GetSubmissionWithResponses(submissionId);
    var mappings = await GetAllMappingsForTemplate(submission.TemplateId);

    // Build dependency graph
    var graph = BuildDependencyGraph(mappings);
    var processedMetrics = new HashSet<int>();
    var pendingMappings = new Queue<FormItemMetricMapping>(mappings);
    var failedAttempts = new Dictionary<int, int>();

    while (pendingMappings.Any())
    {
        var mapping = pendingMappings.Dequeue();

        // Check if dependencies are satisfied
        var dependencies = GetDependencies(mapping);
        if (dependencies.All(d => processedMetrics.Contains(d)))
        {
            try
            {
                await ProcessMapping(mapping, submission);
                processedMetrics.Add(mapping.MetricId);
            }
            catch (Exception ex)
            {
                await LogMappingError(mapping.MappingId, ex.Message);
            }
        }
        else
        {
            // Dependencies not ready, re-queue if not exceeded max attempts
            failedAttempts.TryGetValue(mapping.MappingId, out var attempts);
            attempts++;

            if (attempts < 5) // Max 5 retry attempts
            {
                pendingMappings.Enqueue(mapping);
                failedAttempts[mapping.MappingId] = attempts;
            }
            else
            {
                await LogMappingError(mapping.MappingId,
                    $"Could not resolve dependencies after {attempts} attempts. " +
                    $"Missing: {string.Join(", ", dependencies.Where(d => !processedMetrics.Contains(d)))}");
            }
        }
    }
}

private List<int> GetDependencies(FormItemMetricMapping mapping)
{
    var dependencies = new List<int>();

    if (mapping.MappingType == "Calculated" && !string.IsNullOrEmpty(mapping.TransformationLogic))
    {
        var logic = JsonConvert.DeserializeObject<CalculatedLogic>(mapping.TransformationLogic);

        // Check if formula references other metrics (metric{Id} syntax)
        if (logic.SourceMetrics != null)
        {
            dependencies.AddRange(logic.SourceMetrics);
        }
    }

    return dependencies;
}
```

#### **Variable Syntax Convention:**

To distinguish between form field values and metric values in formulas:

```json
{
  "formula": "(item46 / item45) * 100",      // Uses form field values
  "items": [45, 46]                           // ItemIds from FormTemplateItems
}
```

vs.

```json
{
  "formula": "metric201 + metric202",         // Uses metric values
  "sourceMetrics": [201, 202]                 // MetricIds from TenantMetrics
}
```

**Detailed explanation in Section 5.7 below.**

#### **Best Practice Recommendations:**

1. **Avoid Circular Dependencies:**
   - Metric A depends on Metric B, which depends on Metric A = ERROR
   - Validate mappings during configuration to prevent cycles

2. **Explicit Dependency Declaration:**
   - Add `dependsOnMetrics: [201, 202]` to TransformationLogic
   - System can automatically determine processing order

3. **Error Handling:**
   - If a dependency fails, mark dependent mappings as "Blocked"
   - Log clear error messages indicating which dependency is missing

4. **Performance Optimization:**
   - Process independent mappings in parallel
   - Only wait for dependencies when required

5. **Testing:**
   - Create test submissions with complex dependency chains
   - Verify correct processing order
   - Ensure metrics populate correctly even with failures

#### **Example: Complex Dependency Chain**

```
Submission for Factory A, November 2024:

LEVEL 0: Direct Mappings (Processed First)
├─ Field: "Total Computers" (Item 45) = 25 → Metric 12: TOTAL_COMPUTERS = 25
├─ Field: "Operational" (Item 46) = 20 → Metric 13: OPERATIONAL_COMPUTERS = 20
└─ Field: "LAN Status" (Item 47) = "partial" → (No direct mapping, used in later calculations)

LEVEL 1: Calculated from Form Fields
└─ Mapping: (item46/item45)*100 → Metric 14: AVAILABILITY_PCT = 80.0%

LEVEL 2: ScoreMapping (Aggregated)
├─ Field 47 "LAN Status" = "partial" (5 pts) → Contributing to Metric 101
├─ Field 48 "Network Speed" = "good" (7 pts) → Contributing to Metric 101
└─ AGGREGATED → Metric 101: NETWORK_SECTION_SCORE = 12.0

LEVEL 3: Score from Hardware Section (Parallel with Level 2)
├─ Field 49 "Hardware Condition" = "fair" (5 pts) → Contributing to Metric 102
└─ AGGREGATED → Metric 102: HARDWARE_SECTION_SCORE = 5.0

LEVEL 4: Total Score from Sections (Depends on Level 2 & 3)
└─ Mapping: metric101 + metric102 → Metric 103: TOTAL_ASSESSMENT_SCORE = 17.0

FINAL RESULT: TenantMetrics table
├─ Metric 12: 25.0 (Total Computers)
├─ Metric 13: 20.0 (Operational)
├─ Metric 14: 80.0 (Availability %)
├─ Metric 101: 12.0 (Network Section Score)
├─ Metric 102: 5.0 (Hardware Section Score)
└─ Metric 103: 17.0 (Total Assessment Score)
```

**Processing took 4 phases because of dependencies.**
**If processed in wrong order, Level 4 would fail (missing metric values).**

---

### 5.7 Variable Syntax in Formulas (item vs metric)

**The Key Question:** How does the system know whether a formula references a form field value or another metric value?

#### **Variable Naming Convention:**

The system uses **different syntax** to distinguish between two types of variables:

```
┌─────────────────────────────────────────────────────────────────┐
│                    VARIABLE SYNTAX RULES                         │
└─────────────────────────────────────────────────────────────────┘

item{ItemId}    →  References a form field response value
                   Retrieved from: FormTemplateResponses.ResponseValue
                   Example: item45, item46, item123

metric{MetricId} → References another metric's calculated value
                   Retrieved from: TenantMetrics.NumericValue
                   Example: metric201, metric202, metric14
```

#### **When to Use Each Syntax:**

**Use `item{ItemId}` when:**
- Calculating a metric directly from user-submitted form data
- The formula needs the raw response values
- Processing happens immediately after form submission
- Example: Computer availability = operational / total

**Use `metric{MetricId}` when:**
- Calculating a metric from OTHER metrics (meta-metric)
- Combining section scores into total scores
- Creating composite KPIs from multiple existing metrics
- Processing happens AFTER dependent metrics are calculated

#### **Detailed Examples:**

**Example 1: Formula Using Form Fields Only**

```json
{
  "itemId": 45,
  "metricId": 14,
  "mappingType": "Calculated",
  "transformationLogic": {
    "formula": "(item46 / item45) * 100",
    "items": [45, 46],
    "description": "Computer availability percentage"
  }
}
```

**Data Flow:**
```
FormTemplateResponses table:
├─ Item 45: ResponseValue = "25"  → item45 = 25
└─ Item 46: ResponseValue = "20"  → item46 = 20

Formula evaluation:
(item46 / item45) * 100
= (20 / 25) * 100
= 80.0

Save to TenantMetrics:
└─ MetricId 14: NumericValue = 80.0
```

---

**Example 2: Formula Using Other Metrics**

```json
{
  "itemId": 47,  // Representative item (required but not actually used in formula)
  "metricId": 203,
  "mappingType": "Calculated",
  "transformationLogic": {
    "formula": "metric201 + metric202",
    "sourceMetrics": [201, 202],
    "description": "Total assessment score from all sections"
  }
}
```

**Data Flow:**
```
TenantMetrics table (ALREADY populated from earlier mappings):
├─ MetricId 201: NumericValue = 12.0  → metric201 = 12.0 (Network section score)
└─ MetricId 202: NumericValue = 15.0  → metric202 = 15.0 (Hardware section score)

Formula evaluation:
metric201 + metric202
= 12.0 + 15.0
= 27.0

Save to TenantMetrics:
└─ MetricId 203: NumericValue = 27.0
```

**Critical Requirement:** MetricIds 201 and 202 MUST be populated in TenantMetrics BEFORE this mapping runs.

---

**Example 3: Mixed Formula (Form Fields + Metrics)**

```json
{
  "itemId": 48,
  "metricId": 205,
  "mappingType": "Calculated",
  "transformationLogic": {
    "formula": "(item48 * 0.3) + (metric201 * 0.7)",
    "items": [48],
    "sourceMetrics": [201],
    "description": "Weighted score: 30% user rating + 70% calculated section score"
  }
}
```

**Data Flow:**
```
FROM FormTemplateResponses:
└─ Item 48: ResponseValue = "8"  → item48 = 8

FROM TenantMetrics (must exist already):
└─ MetricId 201: NumericValue = 12.0  → metric201 = 12.0

Formula evaluation:
(item48 * 0.3) + (metric201 * 0.7)
= (8 * 0.3) + (12.0 * 0.7)
= 2.4 + 8.4
= 10.8

Save to TenantMetrics:
└─ MetricId 205: NumericValue = 10.8
```

---

#### **JSON Schema Definition:**

**For Form Field Calculations:**
```json
{
  "formula": "string containing item{Id} variables",
  "items": [array of ItemIds referenced in formula],
  "roundTo": number,  // Optional: decimal places
  "validateDivisionByZero": boolean  // Optional: pre-check denominators
}
```

**For Metric Calculations:**
```json
{
  "formula": "string containing metric{Id} variables",
  "sourceMetrics": [array of MetricIds referenced in formula],
  "roundTo": number,  // Optional: decimal places
  "validateDivisionByZero": boolean  // Optional
}
```

**For Mixed Calculations:**
```json
{
  "formula": "string containing BOTH item{Id} AND metric{Id}",
  "items": [array of ItemIds],
  "sourceMetrics": [array of MetricIds],
  "roundTo": number,
  "validateDivisionByZero": boolean
}
```

---

#### **Implementation: Formula Evaluator**

```csharp
public decimal EvaluateFormula(string formula, int submissionId, CalculatedLogic logic)
{
    var variables = new Dictionary<string, decimal>();

    // Load form field values (item{Id})
    if (logic.Items != null && logic.Items.Any())
    {
        var responses = await _context.FormTemplateResponses
            .Where(r => r.SubmissionId == submissionId && logic.Items.Contains(r.ItemId))
            .ToListAsync();

        foreach (var response in responses)
        {
            var key = $"item{response.ItemId}";
            if (decimal.TryParse(response.ResponseValue, out var value))
            {
                variables[key] = value;
            }
        }
    }

    // Load metric values (metric{Id})
    if (logic.SourceMetrics != null && logic.SourceMetrics.Any())
    {
        var submission = await _context.FormTemplateSubmissions
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

        var metrics = await _context.TenantMetrics
            .Where(m => m.TenantId == submission.TenantId &&
                       m.ReportingPeriod == submission.ReportingPeriod &&
                       logic.SourceMetrics.Contains(m.MetricId))
            .ToListAsync();

        foreach (var metric in metrics)
        {
            var key = $"metric{metric.MetricId}";
            variables[key] = metric.NumericValue;
        }
    }

    // Evaluate formula using expression evaluator library
    // (e.g., NCalc, DynamicExpresso, or custom parser)
    var expression = new Expression(formula);

    foreach (var variable in variables)
    {
        expression.Parameters[variable.Key] = variable.Value;
    }

    var result = Convert.ToDecimal(expression.Evaluate());
    return result;
}
```

---

#### **Common Pitfalls & Solutions:**

**Pitfall 1: Using wrong syntax**
```json
// ❌ WRONG: Using "item" for a metric reference
{"formula": "item201 + item202", "items": [201, 202]}

// ✅ CORRECT: Use "metric" for metric references
{"formula": "metric201 + metric202", "sourceMetrics": [201, 202]}
```

**Pitfall 2: Processing order error**
```json
// ❌ WRONG: Trying to use metric202 before it exists
// Mapping processed BEFORE metric202 is calculated
{"formula": "metric202 * 1.5", "sourceMetrics": [202]}

// ✅ CORRECT: Ensure metric202 mapping is processed first
// Use dependency ordering (Section 5.6)
```

**Pitfall 3: Forgetting to declare sources**
```json
// ❌ WRONG: Using item45 but not declaring it
{"formula": "item45 * 100", "items": []}  // Missing items array!

// ✅ CORRECT: Always declare sources
{"formula": "item45 * 100", "items": [45]}
```

**Pitfall 4: Circular dependencies**
```
// ❌ WRONG: Circular reference
Metric 201 formula: "metric202 + 10"  →  Depends on 202
Metric 202 formula: "metric201 * 2"   →  Depends on 201
// DEADLOCK! Neither can be calculated

// ✅ CORRECT: Linear dependency chain
Metric 201 formula: "item45 + item46"  →  Depends on form fields only
Metric 202 formula: "metric201 * 2"    →  Depends on 201 (OK)
```

---

#### **Validation During Mapping Configuration:**

When admin creates a mapping with `MappingType: "Calculated"`:

```csharp
public async Task<ValidationResult> ValidateCalculatedMapping(CreateMappingDto dto)
{
    var errors = new List<string>();
    var logic = JsonConvert.DeserializeObject<CalculatedLogic>(dto.TransformationLogic);

    // 1. Check formula syntax
    if (string.IsNullOrWhiteSpace(logic.Formula))
    {
        errors.Add("Formula is required");
    }

    // 2. Extract variables from formula
    var itemVariables = Regex.Matches(logic.Formula, @"item(\d+)")
        .Select(m => int.Parse(m.Groups[1].Value))
        .ToList();

    var metricVariables = Regex.Matches(logic.Formula, @"metric(\d+)")
        .Select(m => int.Parse(m.Groups[1].Value))
        .ToList();

    // 3. Validate items array matches formula
    if (itemVariables.Any() && (logic.Items == null || !logic.Items.Any()))
    {
        errors.Add($"Formula uses item variables but 'items' array is empty");
    }

    var undeclaredItems = itemVariables.Except(logic.Items ?? new List<int>()).ToList();
    if (undeclaredItems.Any())
    {
        errors.Add($"Formula references undeclared items: {string.Join(", ", undeclaredItems)}");
    }

    // 4. Validate sourceMetrics array matches formula
    if (metricVariables.Any() && (logic.SourceMetrics == null || !logic.SourceMetrics.Any()))
    {
        errors.Add($"Formula uses metric variables but 'sourceMetrics' array is empty");
    }

    var undeclaredMetrics = metricVariables.Except(logic.SourceMetrics ?? new List<int>()).ToList();
    if (undeclaredMetrics.Any())
    {
        errors.Add($"Formula references undeclared metrics: {string.Join(", ", undeclaredMetrics)}");
    }

    // 5. Check for circular dependencies
    if (logic.SourceMetrics != null && logic.SourceMetrics.Contains(dto.MetricId))
    {
        errors.Add("Circular dependency: Metric cannot reference itself");
    }

    // 6. Validate ItemIds exist
    if (logic.Items != null)
    {
        var validItems = await _context.FormTemplateItems
            .Where(i => logic.Items.Contains(i.ItemId))
            .Select(i => i.ItemId)
            .ToListAsync();

        var invalidItems = logic.Items.Except(validItems).ToList();
        if (invalidItems.Any())
        {
            errors.Add($"Invalid ItemIds: {string.Join(", ", invalidItems)}");
        }
    }

    // 7. Validate MetricIds exist
    if (logic.SourceMetrics != null)
    {
        var validMetrics = await _context.MetricDefinitions
            .Where(m => logic.SourceMetrics.Contains(m.MetricId))
            .Select(m => m.MetricId)
            .ToListAsync();

        var invalidMetrics = logic.SourceMetrics.Except(validMetrics).ToList();
        if (invalidMetrics.Any())
        {
            errors.Add($"Invalid MetricIds: {string.Join(", ", invalidMetrics)}");
        }
    }

    return new ValidationResult
    {
        IsValid = !errors.Any(),
        Errors = errors
    };
}
```

---

**Summary Table:**

| Variable Type | Syntax | Source Table | When Available | Use Case |
|--------------|--------|--------------|----------------|----------|
| Form Field | `item{ItemId}` | FormTemplateResponses | After submission | Direct calculations from user input |
| Metric Value | `metric{MetricId}` | TenantMetrics | After metric population | Meta-metrics, aggregations, composite KPIs |
| Mixed | Both in same formula | Both tables | After dependencies resolved | Weighted combinations of input + calculated values |

---

## 6. SCORING SYSTEM ARCHITECTURE

### 6.1 Hybrid Scoring Approach

**Combines two strategies:**

#### **Strategy A: Option-Level Scores (Simple)**
- Add `ScoreValue` column to `FormItemOptions` table
- Admin assigns scores directly when creating field options
- Easy to configure, no complex mappings needed

#### **Strategy B: Metric-Based Scoring (Advanced)**
- Use `FormItemMetricMappings` with type `ScoreMapping`
- Supports weighted scoring, section scores, total scores
- Integrates with reporting/dashboard system

### 6.2 Enhanced FormItemOption Schema

```sql
ALTER TABLE FormItemOptions
ADD ScoreValue DECIMAL(10,2) NULL,              -- Points for selecting this option
    ScoreWeight DECIMAL(10,2) NULL DEFAULT 1.0; -- Multiplier for weighted scoring
```

**Updated Model:**
```csharp
public class FormItemOption
{
    public int OptionId { get; set; }
    public int ItemId { get; set; }
    public string OptionValue { get; set; }
    public string OptionLabel { get; set; }
    public int DisplayOrder { get; set; }
    
    // ===== SCORING FIELDS =====
    public decimal? ScoreValue { get; set; }     // NEW: Direct score assignment
    public decimal? ScoreWeight { get; set; }    // NEW: Weight multiplier
    
    // Navigation
    public virtual FormTemplateItem Item { get; set; }
}
```

### 6.3 Scoring Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    SCORING DATA FLOW                             │
└─────────────────────────────────────────────────────────────────┘

STEP 1: FORM DESIGN (Admin)
├─ Create assessment form
├─ Add questions with scored options
│  └─ Q1: "LAN Status"
│     ├─ Fully Operational → ScoreValue: 10
│     ├─ Partially Operational → ScoreValue: 5
│     └─ Not Operational → ScoreValue: 0

STEP 2: METRIC MAPPING (Admin - Optional)
├─ Create score aggregation metric
│  └─ Metric: "ICT_INFRASTRUCTURE_SCORE"
│     ├─ Category: "Assessment"
│     ├─ DataType: "Decimal"
│     ├─ Unit: "Points"
│     └─ Thresholds: Green=45, Yellow=35, Red=25
│
└─ Map all scored fields to metric
   └─ MappingType: "ScoreMapping"

STEP 3: FORM SUBMISSION (User)
├─ User fills assessment form
│  ├─ Q1: "LAN Status" = "Partially Operational"
│  ├─ Q2: "Network Speed" = "Good"
│  └─ Q3: "Hardware Condition" = "Fair"

STEP 4: SCORE CALCULATION (Automatic)
├─ FormScoringService.CalculateScore(submissionId)
│  ├─ Extract selected options
│  ├─ Sum ScoreValue for each response
│  │  ├─ Q1: 5 points
│  │  ├─ Q2: 7 points
│  │  └─ Q3: 5 points
│  ├─ Total: 17 / 30 = 56.7%
│  └─ Grade: "Fair" (Yellow threshold)

STEP 5: METRIC POPULATION (If mapped)
└─ Save to TenantMetrics
   └─ ICT_INFRASTRUCTURE_SCORE = 17.0

STEP 6: DISPLAY RESULTS
├─ Submission confirmation shows score
├─ Dashboard displays metric with traffic light
└─ Reports compare scores across tenants
```

### 6.4 Example: ICT Assessment Form

```sql
-- Metric Definition
INSERT INTO MetricDefinitions (
    MetricCode: 'ICT_INFRASTRUCTURE_ASSESSMENT_SCORE',
    MetricName: 'ICT Infrastructure Assessment Total Score',
    Category: 'Assessment',
    SourceType: 'SystemCalculated',
    DataType: 'Decimal',
    Unit: 'Points',
    IsKPI: true,
    ThresholdGreen: 45.0,  -- 90% of max (50)
    ThresholdYellow: 35.0, -- 70% of max
    ThresholdRed: 25.0     -- 50% of max
)

-- Form Structure
Template: "ICT Infrastructure Assessment"
├─ Section 1: Network Infrastructure (Max: 20 points)
│  ├─ Q1: "LAN Status" [Radio] (10 pts max)
│  │  ├─ Option: "Fully Operational" (value="fully", score=10)
│  │  ├─ Option: "Partially Operational" (value="partial", score=5)
│  │  └─ Option: "Not Operational" (value="not", score=0)
│  │
│  └─ Q2: "Network Speed" [Radio] (10 pts max)
│     ├─ Option: "Excellent >1Gbps" (value="excellent", score=10)
│     ├─ Option: "Good 500Mbps-1Gbps" (value="good", score=7)
│     ├─ Option: "Fair 100-500Mbps" (value="fair", score=5)
│     └─ Option: "Poor <100Mbps" (value="poor", score=2)
│
├─ Section 2: Hardware (Max: 20 points)
│  ├─ Q3: "Computer Availability" [Radio] (10 pts max)
│  └─ Q4: "Hardware Age" [Radio] (10 pts max)
│
└─ Section 3: Software (Max: 10 points)
   └─ Q5: "License Compliance" [Radio] (10 pts max)

-- Metric Mappings (Created in Step 3: Metric Mapping)
INSERT INTO FormItemMetricMappings (
    ItemId: 45,  -- Q1: LAN Status
    MetricId: 101,
    MappingType: 'ScoreMapping',
    TransformationLogic: '{
        "scoreSource": "option",  -- Use FormItemOption.ScoreValue
        "contributesTo": "total"
    }'
)

-- Alternative: Store scores directly in mapping (if not using FormItemOption.ScoreValue)
INSERT INTO FormItemMetricMappings (
    ItemId: 45,
    MetricId: 101,
    MappingType: 'ScoreMapping',
    TransformationLogic: '{
        "scoreRules": [
            {"optionValue": "fully", "scoreValue": 10},
            {"optionValue": "partial", "scoreValue": 5},
            {"optionValue": "not", "scoreValue": 0}
        ]
    }'
)
```

---

## 7. COMPLETE DATA FLOW

### 7.1 End-to-End Example

**Scenario:** Kangaita Factory submits November 2024 ICT report

```
┌─────────────────────────────────────────────────────────────────┐
│          COMPLETE FLOW: FORM SUBMISSION → METRIC VALUES         │
└─────────────────────────────────────────────────────────────────┘

[1] USER SUBMISSION
    ├─ Factory: Kangaita (TenantId: 5)
    ├─ Period: November 2024
    ├─ Template: "Monthly ICT Report"
    └─ Responses:
       ├─ Q1 (ItemId: 45): "Total Computers" = 25
       ├─ Q2 (ItemId: 46): "Operational Computers" = 20
       ├─ Q3 (ItemId: 47): "LAN Status" = "partial"
       └─ Q4 (ItemId: 48): "Hardware Condition" = "good"

[2] FORM APPROVAL (Triggers Metric Population)
    └─ Manager approves submission → Status: "Approved"

[3] METRIC POPULATION SERVICE RUNS
    ├─ Load all mappings for this template
    │  └─ Found 5 mappings:
    │     ├─ Mapping 1: Item 45 → Metric 12 (Direct)
    │     ├─ Mapping 2: Item 46 → Metric 13 (Direct)
    │     ├─ Mapping 3: Item 45+46 → Metric 14 (Calculated)
    │     ├─ Mapping 4: Item 47 → Metric 15 (BinaryCompliance)
    │     └─ Mapping 5: Items 47+48 → Metric 16 (ScoreMapping)
    │
    ├─ PROCESS MAPPING 1 (Direct)
    │  ├─ Extract: Item 45 = "25"
    │  ├─ Transform: None (direct)
    │  └─ Save: TenantMetrics
    │     ├─ TenantId: 5
    │     ├─ MetricId: 12 (TOTAL_COMPUTERS)
    │     ├─ ReportingPeriod: 2024-11-01
    │     ├─ NumericValue: 25.0
    │     └─ SourceType: "UserInput"
    │
    ├─ PROCESS MAPPING 2 (Direct)
    │  └─ Save: MetricId 13 (OPERATIONAL_COMPUTERS) = 20.0
    │
    ├─ PROCESS MAPPING 3 (Calculated)
    │  ├─ Extract: Item 45 = 25, Item 46 = 20
    │  ├─ Transform: (20 / 25) * 100 = 80.0
    │  └─ Save: MetricId 14 (COMPUTER_AVAILABILITY_PCT) = 80.0
    │     └─ Log: MetricPopulationLog
    │        ├─ SourceValue: "25, 20"
    │        ├─ CalculationFormula: "(item46/item45)*100"
    │        ├─ CalculatedValue: 80.0
    │        └─ Status: "Success"
    │
    ├─ PROCESS MAPPING 4 (BinaryCompliance)
    │  ├─ Extract: Item 47 = "partial"
    │  ├─ Expected: "fully"
    │  ├─ Transform: "partial" ≠ "fully" → 0.0
    │  └─ Save: MetricId 15 (LAN_OPERATIONAL_STATUS) = 0.0 (Red)
    │
    └─ PROCESS MAPPING 5 (ScoreMapping)
       ├─ Extract: Item 47 = "partial", Item 48 = "good"
       ├─ Lookup scores:
       │  ├─ "partial" → FormItemOption.ScoreValue = 5
       │  └─ "good" → FormItemOption.ScoreValue = 7
       ├─ Transform: 5 + 7 = 12.0
       └─ Save: MetricId 16 (ICT_INFRASTRUCTURE_SCORE) = 12.0

[4] FINAL TENANTMETRICS TABLE
    
    MetricValueId | TenantId | MetricId | ReportingPeriod | NumericValue | SourceReferenceId
    --------------|----------|----------|-----------------|--------------|-------------------
    1001          | 5        | 12       | 2024-11-01      | 25.0         | 789 (SubmissionId)
    1002          | 5        | 13       | 2024-11-01      | 20.0         | 789
    1003          | 5        | 14       | 2024-11-01      | 80.0         | 789
    1004          | 5        | 15       | 2024-11-01      | 0.0          | 789
    1005          | 5        | 16       | 2024-11-01      | 12.0         | 789

[5] DASHBOARD DISPLAY
    
    ┌──────────────────────────────────────────────────┐
    │ Kangaita Factory - November 2024                 │
    ├──────────────────────────────────────────────────┤
    │ Total Computers: 25                              │
    │ Operational: 20                                  │
    │ Availability: 80% 🟡 (Below 95% target)         │
    │ LAN Status: Not Operational 🔴                  │
    │ Assessment Score: 12/50 (24%) 🔴                │
    └──────────────────────────────────────────────────┘
```

---

## 8. IMPLEMENTATION EXAMPLES

### 8.1 Example 1: Simple Inventory Tracking

**Goal:** Track total computers across factories.

```sql
-- Step 1: Create Metric
INSERT INTO MetricDefinitions (
    MetricCode: 'TOTAL_COMPUTERS',
    MetricName: 'Total Computers',
    Category: 'Hardware',
    SourceType: 'UserInput',
    DataType: 'Integer',
    Unit: 'Count',
    AggregationType: 'SUM'
)

-- Step 2: Create Form Field (In Form Builder)
-- ItemId: 45, ItemName: "Number of Computers", DataType: "Number"

-- Step 3: Create Mapping
INSERT INTO FormItemMetricMappings (
    ItemId: 45,
    MetricId: 12,
    MappingType: 'Direct'
)

-- Result: When user enters "25", metric value = 25
```

### 8.2 Example 2: Calculated KPI

**Goal:** Calculate computer availability percentage.

```sql
-- Step 1: Create Metrics
INSERT INTO MetricDefinitions (MetricCode: 'TOTAL_COMPUTERS', ...)
INSERT INTO MetricDefinitions (MetricCode: 'OPERATIONAL_COMPUTERS', ...)
INSERT INTO MetricDefinitions (
    MetricCode: 'COMPUTER_AVAILABILITY_PCT',
    SourceType: 'SystemCalculated',
    DataType: 'Percentage',
    IsKPI: 1,
    ThresholdGreen: 95.0,
    ThresholdYellow: 85.0,
    ThresholdRed: 70.0  -- Reference only
)

-- Step 2: Create Form Fields
-- ItemId: 45, ItemName: "Total Computers"
-- ItemId: 46, ItemName: "Operational Computers"

-- Step 3: Create Mappings
INSERT INTO FormItemMetricMappings (ItemId: 45, MetricId: 12, MappingType: 'Direct')
INSERT INTO FormItemMetricMappings (ItemId: 46, MetricId: 13, MappingType: 'Direct')
INSERT INTO FormItemMetricMappings (
    ItemId: 45,
    MetricId: 14,
    MappingType: 'Calculated',
    TransformationLogic: '{"formula": "(item46/item45)*100", "items": [45,46], "roundTo": 2}'
)

-- Result: Total=25, Operational=20 → Availability=80.0% (Yellow)
```

### 8.3 Example 3: Assessment Form with Scoring

**Goal:** Create ICT infrastructure assessment form.

```sql
-- Step 1: Create Score Metric
INSERT INTO MetricDefinitions (
    MetricCode: 'ICT_ASSESSMENT_TOTAL_SCORE',
    Category: 'Assessment',
    SourceType: 'SystemCalculated',
    DataType: 'Decimal',
    Unit: 'Points',
    ThresholdGreen: 45.0,
    ThresholdYellow: 35.0,
    ThresholdRed: 25.0
)

-- Step 2: Create Form Fields with Options
-- ItemId: 47, ItemName: "LAN Status", DataType: "Radio"

-- Step 3: Create Options with Scores
INSERT INTO FormItemOptions (ItemId: 47, OptionValue: 'fully', OptionLabel: 'Fully Operational', ScoreValue: 10)
INSERT INTO FormItemOptions (ItemId: 47, OptionValue: 'partial', OptionLabel: 'Partially Operational', ScoreValue: 5)
INSERT INTO FormItemOptions (ItemId: 47, OptionValue: 'not', OptionLabel: 'Not Operational', ScoreValue: 0)

-- Step 4: Create Score Mapping
INSERT INTO FormItemMetricMappings (
    ItemId: 47,
    MetricId: 101,
    MappingType: 'ScoreMapping',
    TransformationLogic: '{"scoreSource": "option", "contributesTo": "total"}'
)

-- Result: User selects "Partially Operational" → Score += 5 points
```

---

## 9. ADVANCED SCENARIOS

### 9.1 Weighted Scoring

**Scenario:** Some questions are more important than others.

```json
{
  "mappingType": "ScoreMapping",
  "transformationLogic": {
    "scoreSource": "option",
    "weight": 1.5,  // This question worth 1.5x normal points
    "contributesTo": "total"
  }
}
```

**Example:**
- Q1: LAN Status (Weight: 1.5) → Selected "Good" (7 pts) = 7 × 1.5 = 10.5 pts
- Q2: Hardware Age (Weight: 1.0) → Selected "Fair" (5 pts) = 5 × 1.0 = 5.0 pts
- Total: 15.5 pts

### 9.2 Section Scores

**Scenario:** Calculate subtotals for each section.

```sql
-- Create section metrics
INSERT INTO MetricDefinitions (MetricCode: 'NETWORK_SECTION_SCORE', ...)
INSERT INTO MetricDefinitions (MetricCode: 'HARDWARE_SECTION_SCORE', ...)
INSERT INTO MetricDefinitions (MetricCode: 'TOTAL_ASSESSMENT_SCORE', ...)

-- Map section questions to section metric
INSERT INTO FormItemMetricMappings (ItemId: 47, MetricId: 201, MappingType: 'ScoreMapping')  -- Q1 → Network Section
INSERT INTO FormItemMetricMappings (ItemId: 48, MetricId: 201, MappingType: 'ScoreMapping')  -- Q2 → Network Section
INSERT INTO FormItemMetricMappings (ItemId: 49, MetricId: 202, MappingType: 'ScoreMapping')  -- Q3 → Hardware Section

-- Aggregate to total
INSERT INTO FormItemMetricMappings (
    ItemId: 47,
    MetricId: 203,
    MappingType: 'Calculated',
    TransformationLogic: '{"formula": "metric201 + metric202", "sourceMetrics": [201, 202]}'
)
```

### 9.3 Conditional Scoring

**Scenario:** Score only applies if certain conditions are met.

```json
{
  "mappingType": "ScoreMapping",
  "transformationLogic": {
    "scoreSource": "option",
    "conditions": [
      {
        "itemId": 45,
        "operator": "equals",
        "value": "Yes"
      }
    ],
    "message": "Score only applies if item 45 = Yes"
  }
}
```

### 9.4 Multiple Forms → Same Metric

**Scenario:** Different forms collect the same metric.

```
Form A: "Monthly Report" → Field: "Total Computers" → Metric: TOTAL_COMPUTERS
Form B: "Quarterly Audit" → Field: "Computer Count" → Metric: TOTAL_COMPUTERS
Form C: "Annual Inventory" → Field: "Number of PCs" → Metric: TOTAL_COMPUTERS

Dashboard shows trend combining data from all three forms!
```

---

## 10. IMPLEMENTATION SEQUENCE

### Phase 1: Foundation (Weeks 1-2)

**1. Metric Management Module**
```
/Administration/Metrics
├─ Index (List all metrics with search/filter)
├─ Create (Add new metric definition)
├─ Edit (Modify metric properties)
└─ Delete/Archive (Deactivate metrics)
```

**2. Seed Core Metrics**
```sql
-- Create 20-30 standard KTDA metrics
-- Categories: Hardware, Network, Software, Performance, Compliance, Assessment
```

**3. Database Updates**
```sql
-- Add ScoreValue to FormItemOptions
ALTER TABLE FormItemOptions ADD ScoreValue DECIMAL(10,2) NULL;
```

### Phase 2: Form Builder Integration (Weeks 3-4)

**1. Enhance Form Builder (Step 2)**
```
When creating field options:
├─ Add "Score Value" input field
├─ Preview total max score for form
└─ Validation: Ensure score values are numeric
```

**2. Implement Metric Mapping UI (Step 3)**
```
/Forms/FormTemplates/MetricMapping/{id}
├─ List all form fields
├─ Browse available metrics
├─ Create mappings with type selection
├─ Configure transformation logic
└─ Preview/test mappings
```

### Phase 3: Metric Population Service (Weeks 5-6)

**1. Service Implementation**
```csharp
// Services/Metrics/MetricPopulationService.cs
public class MetricPopulationService
{
    public async Task PopulateMetrics(int submissionId) { }
    private async Task ProcessDirectMapping(...) { }
    private async Task ProcessCalculatedMapping(...) { }
    private async Task ProcessBinaryComplianceMapping(...) { }
    private async Task ProcessScoreMapping(...) { }
}
```

**2. Integrate with Approval Workflow**
```csharp
// Trigger after approval
if (submission.Status == "Approved")
{
    await _metricPopulationService.PopulateMetrics(submission.SubmissionId);
}
```

**3. Error Handling & Logging**
```
MetricPopulationLog tracks:
├─ Which mappings succeeded
├─ Which failed (with error messages)
├─ Calculation details for audit trail
└─ Processing time for performance monitoring
```

### Phase 4: Reporting Integration (Weeks 7-8)

**1. Dashboard Widgets**
```
Display metrics with:
├─ Current value
├─ Traffic light (Green/Yellow/Red based on thresholds)
├─ Trend arrow (↑ improving, ↓ declining)
└─ Historical sparkline chart
```

**2. Metric Reports**
```
/Reports/Metrics
├─ Metric comparison across tenants
├─ Time-series trend analysis
├─ Threshold breach alerts
└─ Score leaderboards
```

---

## APPENDIX A: JSON Schema Reference

### A.1 Calculated Mapping

```json
{
  "formula": "(item46 / item45) * 100",
  "items": [45, 46],
  "description": "Operational / Total * 100",
  "roundTo": 2,
  "validateDivisionByZero": true
}
```

### A.2 Score Mapping (Option-Based)

```json
{
  "scoreSource": "option",
  "contributesTo": "total",
  "weight": 1.0,
  "conditions": []
}
```

### A.3 Score Mapping (Rule-Based)

```json
{
  "scoreRules": [
    {
      "optionValue": "excellent",
      "scoreValue": 10,
      "weight": 1.0
    },
    {
      "optionValue": "good",
      "scoreValue": 7,
      "weight": 1.0
    }
  ],
  "aggregation": "sum",
  "maxScore": 10
}
```

---

## APPENDIX B: Complete SQL Schema

### MetricPopulationLog Table

```sql
CREATE TABLE MetricPopulationLog (
    LogId INT PRIMARY KEY IDENTITY(1,1),

    -- References
    SubmissionId INT NOT NULL,                         -- FK to FormTemplateSubmissions
    MetricId INT NOT NULL,                             -- FK to MetricDefinitions
    MappingId INT NOT NULL,                            -- FK to FormItemMetricMappings

    -- Processing Details
    SourceValue NVARCHAR(MAX),                         -- Original values extracted (e.g., "25, 20")
    CalculatedValue DECIMAL(18,4) NULL,                -- Final computed value
    CalculationFormula NVARCHAR(MAX) NULL,             -- Formula used (e.g., "(item46/item45)*100")
    TransformationLogic NVARCHAR(MAX) NULL,            -- Full JSON logic that was applied

    -- Result Status
    Status NVARCHAR(20) NOT NULL,                      -- 'Success', 'Failed', 'Skipped', 'Blocked'
    ErrorMessage NVARCHAR(MAX) NULL,                   -- Error details if Status = 'Failed'

    -- Audit
    ProcessedDate DATETIME2 DEFAULT GETUTCDATE(),      -- When processing occurred
    ProcessingDurationMs INT NULL,                     -- How long processing took (optional)

    -- Constraints
    CONSTRAINT FK_MetricLog_Submission FOREIGN KEY (SubmissionId)
        REFERENCES FormTemplateSubmissions(SubmissionId) ON DELETE CASCADE,
    CONSTRAINT FK_MetricLog_Metric FOREIGN KEY (MetricId)
        REFERENCES MetricDefinitions(MetricId),
    CONSTRAINT FK_MetricLog_Mapping FOREIGN KEY (MappingId)
        REFERENCES FormItemMetricMappings(MappingId)
)

-- Indexes for performance
CREATE INDEX IX_MetricPopulationLog_Submission ON MetricPopulationLog(SubmissionId)
CREATE INDEX IX_MetricPopulationLog_Metric ON MetricPopulationLog(MetricId)
CREATE INDEX IX_MetricPopulationLog_Status ON MetricPopulationLog(Status)
CREATE INDEX IX_MetricPopulationLog_ProcessedDate ON MetricPopulationLog(ProcessedDate)
```

### Status Values

| Status | Description | When Used |
|--------|-------------|-----------|
| `Success` | Metric populated successfully | Processing completed without errors |
| `Failed` | Error occurred during processing | Formula evaluation failed, division by zero, missing dependencies |
| `Skipped` | Mapping intentionally not processed | Conditional logic not met, field optional and empty |
| `Blocked` | Could not process due to missing dependencies | Required metrics not yet calculated |

### Usage in Service

```csharp
// Log successful metric population
await LogMetricPopulation(new MetricPopulationLog
{
    SubmissionId = submission.SubmissionId,
    MetricId = mapping.MetricId,
    MappingId = mapping.MappingId,
    SourceValue = $"item45={values["item45"]}, item46={values["item46"]}",
    CalculatedValue = result,
    CalculationFormula = logic.Formula,
    TransformationLogic = mapping.TransformationLogic,
    Status = "Success",
    ProcessedDate = DateTime.UtcNow
});

// Log error
await LogMetricPopulation(new MetricPopulationLog
{
    SubmissionId = submission.SubmissionId,
    MetricId = mapping.MetricId,
    MappingId = mapping.MappingId,
    SourceValue = $"item45={values["item45"]}, item46={values["item46"]}",
    CalculationFormula = logic.Formula,
    TransformationLogic = mapping.TransformationLogic,
    Status = "Failed",
    ErrorMessage = $"Division by zero: denominator (item45) = 0",
    ProcessedDate = DateTime.UtcNow
});
```

### Other Key Tables

```sql
-- See database schema files for:
-- 1. MetricDefinitions (Section 3.2)
-- 2. FormItemMetricMappings (Section 4.2)
-- 3. FormItemOptions with ScoreValue (Section 6.2)
-- 4. TenantMetrics (holds final metric values)
```

---

## APPENDIX C: Service Interfaces

```csharp
public interface IMetricPopulationService
{
    Task PopulateMetrics(int submissionId);
    Task ReprocessMetrics(int submissionId);
    Task<List<MetricPopulationResult>> ValidateMappings(int templateId);
}

public interface IFormScoringService
{
    Task<FormScore> CalculateScore(int submissionId);
    Task<decimal> CalculateItemScore(int responseId);
    Task<ScoringBreakdown> GetScoreBreakdown(int submissionId);
}

public interface IMetricDefinitionService
{
    Task<MetricDefinition> CreateMetric(MetricDefinitionDto dto);
    Task<List<MetricDefinition>> GetMetricsByCategory(string category);
    Task<bool> ValidateMetricMapping(int itemId, int metricId);
}
```

---

**END OF DOCUMENT**
