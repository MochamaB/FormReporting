# Metric System - Simplified Explanation

## Overview

The metric system connects form templates to KPI reporting through a hierarchical structure:
- **Field Level** → Individual form field values/scores
- **Section Level** → Aggregated field metrics within a section
- **Template Level** → Overall form score from section aggregates

---

## Core Concepts

### **1. MetricDefinition** = Central Catalog (Optional)
The **reporting layer** - a catalog of metrics that can be displayed, tracked, and reported on.

| Purpose | Description |
|---------|-------------|
| Identity | Metric name, code, description |
| Data Type | Integer, Decimal, Percentage, Boolean |
| KPI Thresholds | Green > 90, Yellow > 70, Red < 70 |
| Units | Count, Percentage, Days, etc. |

**Key Point:** MetricDefinition is OPTIONAL for mappings. You can create mappings first, then link to MetricDefinition later for reporting.

### **2. Mapping Tables** = Configuration
Define **WHERE** metric values come from and **HOW** to calculate them.

| Table | Links | Purpose |
|-------|-------|---------|
| `FormItemMetricMapping` | Field → Metric | "This field produces this value" |
| `FormSectionMetricMapping` | Section → Metric | "This section aggregates field values" |
| `FormTemplateMetricMapping` | Template → Metric | "This template produces overall score" |

### **3. Junction Tables** = Source Links (No JSON)
Define which sources feed into each aggregation. Proper relational design instead of JSON arrays.

| Table | Purpose |
|-------|---------|
| `FormSectionMetricSource` | Links section mapping ← item mappings |
| `FormTemplateMetricSource` | Links template mapping ← section mappings |

### **4. TenantMetric** = Actual Values (Storage)
Stores the **calculated values** when forms are submitted.

### **5. Reporting** = Display
Queries `TenantMetric` to show dashboards, charts, comparisons.
Regional/Global aggregations computed via SQL GROUP BY - no separate snapshot tables needed.

---

## Database Model Structure

### **FormItemMetricMapping** (Field Level)

| Column | Type | Required | Description |
|--------|------|----------|-------------|
| `MappingId` | int | PK | Primary key |
| `ItemId` | int | FK, Yes | Links to FormTemplateItem |
| `MetricId` | int? | FK, No | Links to MetricDefinition (OPTIONAL) |
| `MappingName` | string(100) | Yes | Name for identification |
| `MappingType` | string(30) | Yes | Direct, Calculated, BinaryCompliance |
| `AggregationType` | string(20) | Yes | Direct, Sum, Count, Avg |
| `TransformationLogic` | string? | No | Formula for calculated types |
| `ExpectedValue` | string(100)? | No | For binary compliance |
| `IsActive` | bool | Yes | Active flag |
| `CreatedDate` | DateTime | Yes | Created timestamp |

### **FormSectionMetricMapping** (Section Level)

| Column | Type | Required | Description |
|--------|------|----------|-------------|
| `MappingId` | int | PK | Primary key |
| `SectionId` | int | FK, Yes | Links to FormTemplateSection |
| `MetricId` | int? | FK, No | Links to MetricDefinition (OPTIONAL) |
| `MappingName` | string(100) | Yes | Name for identification |
| `MappingType` | string(30) | Yes | Aggregated, Calculated |
| `AggregationType` | string(20) | Yes | AVG, SUM, COUNT, WeightedAverage |
| `IsActive` | bool | Yes | Active flag |
| `CreatedDate` | DateTime | Yes | Created timestamp |

### **FormSectionMetricSource** (Junction Table)

| Column | Type | Required | Description |
|--------|------|----------|-------------|
| `Id` | int | PK | Primary key |
| `SectionMappingId` | int | FK, Yes | Links to FormSectionMetricMapping |
| `ItemMappingId` | int | FK, Yes | Links to FormItemMetricMapping |
| `Weight` | decimal? | No | For weighted averages |
| `DisplayOrder` | int | Yes | Order of sources |

### **FormTemplateMetricMapping** (Template Level)

| Column | Type | Required | Description |
|--------|------|----------|-------------|
| `MappingId` | int | PK | Primary key |
| `TemplateId` | int | FK, Yes | Links to FormTemplate |
| `MetricId` | int? | FK, No | Links to MetricDefinition (OPTIONAL) |
| `MappingName` | string(100) | Yes | Name for identification |
| `MappingType` | string(30) | Yes | Aggregated, Calculated |
| `AggregationType` | string(20) | Yes | AVG, SUM, WeightedAverage |
| `IsActive` | bool | Yes | Active flag |
| `CreatedDate` | DateTime | Yes | Created timestamp |

### **FormTemplateMetricSource** (Junction Table)

| Column | Type | Required | Description |
|--------|------|----------|-------------|
| `Id` | int | PK | Primary key |
| `TemplateMappingId` | int | FK, Yes | Links to FormTemplateMetricMapping |
| `SectionMappingId` | int | FK, Yes | Links to FormSectionMetricMapping |
| `Weight` | decimal? | No | For weighted averages |
| `DisplayOrder` | int | Yes | Order of sources |

### **TenantMetric** (Value Storage)

| Column | Type | Required | Description |
|--------|------|----------|-------------|
| `MetricValueId` | long | PK | Primary key |
| `TenantId` | int | FK, Yes | Links to Tenant |
| `MetricId` | int? | FK, No | Links to MetricDefinition (OPTIONAL) |
| `ReportingPeriod` | date | Yes | Period for this value |
| `MetricScope` | string(30) | Yes | Field, Section, Template |
| `SourceMappingId` | int? | No | Polymorphic - the mapping that produced this |
| `NumericValue` | decimal? | No | The calculated value |
| `TextValue` | string? | No | Text representation |
| `SourceType` | string(30) | No | UserInput, SystemCalculated |
| `SourceReferenceId` | int? | No | SubmissionId |
| `CapturedDate` | DateTime | Yes | When captured |
| `CapturedBy` | int? | No | User who captured |

### **MetricDefinition** (Central Catalog)

| Column | Type | Required | Description |
|--------|------|----------|-------------|
| `MetricId` | int | PK | Primary key |
| `MetricCode` | string(50) | Yes | Unique code |
| `MetricName` | string(200) | Yes | Display name |
| `Category` | string(100) | No | Infrastructure, Software, etc. |
| `SourceType` | string(30) | Yes | UserInput, SystemCalculated |
| `DataType` | string(20) | Yes | Integer, Decimal, Percentage |
| `Unit` | string(50) | No | Count, Percentage, Days |
| `AggregationType` | string(20) | No | SUM, AVG, MAX, MIN |
| `MetricScope` | string(30) | No | Field, Section, Template |
| `HierarchyLevel` | int | No | 0=Field, 1=Section, 2=Template |
| `ParentMetricId` | int? | FK, No | Self-reference for hierarchy |
| `IsKPI` | bool | Yes | Is this a KPI? |
| `ThresholdGreen` | decimal? | No | Good threshold |
| `ThresholdYellow` | decimal? | No | Warning threshold |
| `ThresholdRed` | decimal? | No | Critical threshold |
| `IsActive` | bool | Yes | Active flag |
| `CreatedDate` | DateTime | Yes | Created timestamp |

---

## Complete Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                    FORM BUILDER (Configuration)                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  1. Create Field with Options                                       │
│     └─ Option: "Operational" → ScoreValue: 100                      │
│     └─ Option: "Faulty" → ScoreValue: 50                            │
│                                                                     │
│  2. Create Field Mapping (FormItemMetricMapping)                    │
│     └─ MappingName: "LAN Health"                                    │
│     └─ MappingType: Direct (use ScoreValue)                         │
│     └─ MetricId: NULL (optional, link later)                        │
│                                                                     │
│  3. Create Section Mapping (FormSectionMetricMapping)               │
│     └─ MappingName: "Infrastructure Score"                          │
│     └─ AggregationType: AVG                                         │
│                                                                     │
│  4. Link Sources (FormSectionMetricSource)                          │
│     └─ SectionMappingId → ItemMappingId (LAN Health)                │
│     └─ SectionMappingId → ItemMappingId (WAN Health)                │
│     └─ SectionMappingId → ItemMappingId (Server Health)             │
│                                                                     │
│  5. Create Template Mapping (FormTemplateMetricMapping)             │
│     └─ MappingName: "ICT Overall Score"                             │
│     └─ AggregationType: AVG                                         │
│                                                                     │
│  6. Link Sources (FormTemplateMetricSource)                         │
│     └─ TemplateMappingId → SectionMappingId (Infrastructure)        │
│     └─ TemplateMappingId → SectionMappingId (Software)              │
│     └─ TemplateMappingId → SectionMappingId (Hardware)              │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    FORM SUBMISSION (Runtime)                        │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  User at Factory X fills form:                                      │
│  - LAN Status: "Operational" (ScoreValue = 100)                     │
│  - WAN Status: "Faulty" (ScoreValue = 50)                           │
│                                                                     │
│  MetricPopulationService processes:                                 │
│                                                                     │
│  Step 1: Field Metrics                                              │
│    → Get FormItemMetricMappings for template                        │
│    → Calculate field values from responses                          │
│    → INSERT TenantMetric (Scope=Field, SourceMappingId=X, Value=100)│
│    → INSERT TenantMetric (Scope=Field, SourceMappingId=Y, Value=50) │
│                                                                     │
│  Step 2: Section Metrics                                            │
│    → Get FormSectionMetricMappings for template                     │
│    → Get sources from FormSectionMetricSource                       │
│    → Aggregate field values: AVG(100, 50) = 75                      │
│    → INSERT TenantMetric (Scope=Section, SourceMappingId=Z, Value=75)│
│                                                                     │
│  Step 3: Template Metrics                                           │
│    → Get FormTemplateMetricMappings for template                    │
│    → Get sources from FormTemplateMetricSource                      │
│    → Aggregate section values: AVG(75, 88, 92) = 85                 │
│    → INSERT TenantMetric (Scope=Template, SourceMappingId=W, Value=85)│
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    REPORTING (Query)                                │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  Option 1: Query by MappingName (no MetricDefinition needed)        │
│  SELECT t.TenantName, tm.NumericValue                               │
│  FROM TenantMetric tm                                               │
│  JOIN FormTemplateMetricMapping m ON tm.SourceMappingId = m.MappingId│
│  JOIN Tenants t ON tm.TenantId = t.TenantId                         │
│  WHERE m.MappingName = 'ICT Overall Score'                          │
│    AND tm.MetricScope = 'Template'                                  │
│                                                                     │
│  Option 2: Query by MetricDefinition (for central reporting)        │
│  SELECT r.RegionName, AVG(tm.NumericValue)                          │
│  FROM TenantMetric tm                                               │
│  JOIN MetricDefinitions md ON tm.MetricId = md.MetricId             │
│  JOIN Tenants t ON tm.TenantId = t.TenantId                         │
│  JOIN Regions r ON t.RegionId = r.RegionId                          │
│  WHERE md.MetricCode = 'ICT_OVERALL'                                │
│  GROUP BY r.RegionId                                                │
│                                                                     │
│  Result:                                                            │
│  ┌─────────────┬───────────────┐                                    │
│  │ Region      │ Avg ICT Score │                                    │
│  ├─────────────┼───────────────┤                                    │
│  │ Central     │ 87.5          │                                    │
│  │ Western     │ 82.3          │                                    │
│  │ Eastern     │ 79.1          │                                    │
│  └─────────────┴───────────────┘                                    │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Key Design Decisions

### 1. MetricId is OPTIONAL
- Create mappings first, link to MetricDefinition later
- Enables form-level reporting without central metric catalog
- MetricDefinition only needed for cross-form/dashboard reporting

### 2. No JSON Columns
- EF Core doesn't query JSON efficiently
- Proper FK relationships via junction tables
- Supports weighted averages via Weight column

### 3. Polymorphic TenantMetric
- `MetricScope` indicates type (Field/Section/Template)
- `SourceMappingId` references the mapping that produced the value
- Single table for all metric values

### 4. No Hardcoded Snapshot Tables
- DELETE `RegionalMonthlySnapshot` - compute from TenantMetric via SQL
- DELETE `TenantPerformanceSnapshot` - use TenantMetric directly
- Regional aggregations: GROUP BY Region on TenantMetric JOIN Tenants

---

## Models Summary

### Models to UPDATE

| Model | Changes Needed |
|-------|----------------|
| `MetricDefinition` | Add: MetricScope, HierarchyLevel, ParentMetricId |
| `TenantMetric` | Add: MetricScope, SourceMappingId; Make MetricId optional |
| `FormItemMetricMapping` | Add: MappingName, AggregationType; Make MetricId optional |
| `ReportField` | Add: MetricScope, DrillDownEnabled |

### Models to CREATE

| Model | Purpose |
|-------|---------|
| `FormSectionMetricMapping` | Section → Metric configuration |
| `FormSectionMetricSource` | Junction: Section mapping ← Item mappings |
| `FormTemplateMetricMapping` | Template → Metric configuration |
| `FormTemplateMetricSource` | Junction: Template mapping ← Section mappings |

### Models to DELETE

| Model | Reason |
|-------|--------|
| `RegionalMonthlySnapshot` | Hardcoded fields; compute from TenantMetric via SQL |
| `TenantPerformanceSnapshot` | Hardcoded fields; use TenantMetric directly |

---

## Entity Relationship Diagram

```
┌─────────────────────┐
│  FormTemplate       │
└─────────┬───────────┘
          │ 1:N
          ▼
┌─────────────────────────────┐      ┌─────────────────────────────┐
│ FormTemplateMetricMapping   │──────│ FormTemplateMetricSource    │
│ - MappingId (PK)            │ 1:N  │ - TemplateMappingId (FK)    │
│ - TemplateId (FK)           │      │ - SectionMappingId (FK)     │
│ - MetricId (FK, optional)   │      │ - Weight                    │
│ - MappingName               │      └──────────────┬──────────────┘
│ - AggregationType           │                     │
└─────────────────────────────┘                     │ N:1
                                                    ▼
┌─────────────────────┐      ┌─────────────────────────────┐      ┌─────────────────────────────┐
│ FormTemplateSection │──────│ FormSectionMetricMapping    │──────│ FormSectionMetricSource     │
└─────────────────────┘ 1:N  │ - MappingId (PK)            │ 1:N  │ - SectionMappingId (FK)     │
                             │ - SectionId (FK)            │      │ - ItemMappingId (FK)        │
                             │ - MetricId (FK, optional)   │      │ - Weight                    │
                             │ - MappingName               │      └──────────────┬──────────────┘
                             │ - AggregationType           │                     │
                             └─────────────────────────────┘                     │ N:1
                                                                                 ▼
┌─────────────────────┐      ┌─────────────────────────────┐
│ FormTemplateItem    │──────│ FormItemMetricMapping       │
└─────────────────────┘ 1:N  │ - MappingId (PK)            │
                             │ - ItemId (FK)               │
                             │ - MetricId (FK, optional)   │
                             │ - MappingName               │
                             │ - MappingType               │
                             │ - AggregationType           │
                             └──────────────┬──────────────┘
                                            │
                                            │ All mappings can optionally link to:
                                            ▼
                             ┌─────────────────────────────┐
                             │ MetricDefinition            │
                             │ - MetricId (PK)             │
                             │ - MetricCode                │
                             │ - MetricName                │
                             │ - MetricScope               │
                             │ - ThresholdGreen/Yellow/Red │
                             └─────────────────────────────┘

                             All produce values stored in:
                                            ▼
                             ┌─────────────────────────────┐
                             │ TenantMetric                │
                             │ - MetricValueId (PK)        │
                             │ - TenantId (FK)             │
                             │ - MetricId (FK, optional)   │
                             │ - MetricScope               │
                             │ - SourceMappingId           │
                             │ - NumericValue              │
                             │ - ReportingPeriod           │
                             └─────────────────────────────┘
```
