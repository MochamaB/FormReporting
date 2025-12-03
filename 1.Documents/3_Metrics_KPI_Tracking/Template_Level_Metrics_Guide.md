# TEMPLATE-LEVEL METRICS GUIDE

**Version:** 1.0  
**Last Updated:** December 3, 2025  

---

## PURPOSE

Template-level metrics provide holistic view of entire forms, combining **content metrics** (aggregated scores) and **process metrics** (workflow, timing, engagement).

---

## TWO CATEGORIES

**Category A: Content Metrics**
- Aggregate section scores into overall assessment
- Examples: Overall ICT Health Score, Total Assessment Points, Compliance Rate

**Category B: Process Metrics**
- Measure form submission behavior and performance
- Examples: Submission Timeline, Completion Rate, Form Engagement, Edit Count

---

## DATABASE TABLE: TemplateMetricMappings

**Purpose:** Links form templates to template-level metrics

**Key Columns:**

- **TemplateMappingId** - Primary key
- **TemplateId** - Foreign key to FormTemplates
- **MetricId** - Foreign key to MetricDefinitions (with MetricScope="Template")
- **MetricType** - Type: "Aggregated", "Workflow", "Timeline", "Engagement"
- **MetricConfig** - JSON configuration (varies by MetricType)

---

## CONTENT METRICS - CONFIGURATION

**Scenario:** Overall ICT Health Score from section scores

**MetricDefinition:**
- MetricCode: "ICT_OVERALL_ASSESSMENT_SCORE"
- MetricScope: "Template"
- DataType: "Percentage"
- ThresholdGreen: 85.0
- ThresholdYellow: 70.0

**TemplateMetricMapping:**
- TemplateId: 5 (Monthly ICT Report)
- MetricId: 203
- MetricType: "Aggregated"
- MetricConfig:
```json
{
  "type": "section_aggregate",
  "aggregation": "weighted_average",
  "sources": [
    {"sectionId": 1, "metricId": 201, "weight": 0.5},
    {"sectionId": 2, "metricId": 202, "weight": 0.3},
    {"sectionId": 3, "metricId": 203, "weight": 0.2}
  ]
}
```

**Calculation:**
```
Overall = (Infrastructure×0.5) + (Software×0.3) + (Hardware×0.2)
        = (72.5×0.5) + (85×0.3) + (90×0.2)
        = 36.25 + 25.5 + 18
        = 79.75%
```

---

## PROCESS METRICS - EXAMPLES

**Example 1: Submission Timeline Compliance**

**MetricDefinition:**
- MetricCode: "FORM_SUBMISSION_TIMELINE_COMPLIANCE"
- MetricScope: "Template"
- DataType: "Boolean"
- ExpectedValue: "2.0" (days)

**TemplateMetricMapping:**
- MetricType: "Timeline"
- MetricConfig:
```json
{
  "type": "submission_timeline",
  "startEvent": "FormAssigned",
  "endEvent": "FormSubmitted",
  "expectedDuration": 2,
  "unit": "days"
}
```

**Calculation:**
- Assigned: Dec 1, 09:00
- Submitted: Dec 3, 14:30
- Actual: 2.23 days
- Expected: 2.0 days
- Result: 0 (non-compliant)

**Example 2: Completion Rate**

**MetricDefinition:**
- MetricCode: "FORM_COMPLETION_RATE"
- MetricScope: "Template"
- DataType: "Percentage"

**TemplateMetricMapping:**
- MetricType: "Workflow"
- MetricConfig:
```json
{
  "type": "completion_rate",
  "numerator": "submitted_count",
  "denominator": "assigned_count",
  "period": "monthly"
}
```

**Calculation:**
- Assigned: 50 forms
- Submitted: 47 forms
- Rate: 47/50 = 94%

---

## DATA FLOW

**Content Metric Processing:**

1. **Wait for section metrics** to be calculated
2. **Retrieve section values** from TenantMetrics where MetricScope="Section"
3. **Apply aggregation formula** (weighted average, sum, etc.)
4. **Save to TenantMetrics**:
   - MetricScope: "Template"
   - SourceTemplateId: {TemplateId}
   - DrillDownData: Contains section breakdown

**Process Metric Processing:**

1. **Query form submission metadata** (timestamps, status changes)
2. **Calculate metric** based on MetricType
3. **Save to TenantMetrics** with MetricScope="Template"

---

## COMPLETE EXAMPLE

**Template:** "Monthly ICT Report"

**Template Metrics:**

**1. Overall ICT Health Score (Content)**
- Aggregates: Infrastructure (50%), Software (30%), Hardware (20%)
- Value: 79.75%
- Status: 🟡 Yellow (below 85%)

**2. Submission Timeline (Process)**
- Expected: 2 days
- Actual: 2.3 days
- Value: Non-compliant
- Status: 🔴 Red

**3. Completion Rate (Process)**
- Assigned: 50, Submitted: 47
- Value: 94%
- Status: 🟢 Green

**Result in TenantMetrics:**
Three records with MetricScope="Template", SourceTemplateId=5

**Dashboard Display:**
```
Monthly ICT Report (November 2024)
├─ Health Score: 79.75% 🟡 [Click to drill down]
├─ Timeline: Late by 0.3 days 🔴
└─ Completion: 94% (47/50) 🟢
```

---

## BEST PRACTICES

1. **Content metrics** should aggregate from section metrics, not individual fields
2. **Use weighted aggregation** to reflect section importance
3. **Process metrics** track form lifecycle and engagement
4. **Link to sections** via ParentMetricId for drill-down
5. **Store breakdown** in DrillDownData for UI navigation
6. **Name consistently** - suffix with "_TEMPLATE_SCORE" or "_TEMPLATE_METRIC"
