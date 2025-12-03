# SECTION-LEVEL METRICS GUIDE

**Version:** 1.0  
**Last Updated:** December 3, 2025  

---

## PURPOSE

Section-level metrics aggregate values from multiple fields within a form section, providing mid-level insights between individual fields and overall template scores.

---

## WHY SECTION METRICS?

**Organized Analysis**
- Instead of 20 individual field scores, see: Infrastructure 72%, Software 85%, Hardware 90%

**Targeted Improvements**
- Identify which sections need attention: "Infrastructure below target"

**Weighted Importance**
- Critical sections contribute more: Infrastructure 50%, Software 30%, Hardware 20%

**Drill-Down Navigation**
- Click section score to see contributing fields: Infrastructure 72% → LAN:50, WAN:100, Servers:75

---

## DATABASE TABLE: SectionMetricMappings

**Purpose:** Links form sections to aggregated metrics

**Key Columns:**

- **SectionMappingId** - Primary key
- **SectionId** - Foreign key to FormTemplateSections
- **MetricId** - Foreign key to MetricDefinitions (with MetricScope="Section")
- **AggregationType** - How to aggregate: "Sum", "Average", "Weighted", "Calculated"
- **AggregationConfig** - JSON with aggregation details
- **UseOnlyMappedFields** - Boolean: true = only fields with metric mappings

---

## AGGREGATION TYPES

**Type 1: Simple Average**
```
Formula: Sum all field scores / Count of fields
Example: (50 + 100 + 75) / 3 = 75%
Use When: All fields equally important
```

**Type 2: Weighted Average**
```
Formula: Sum of (field score × weight)
Example: (50×0.4) + (100×0.3) + (75×0.3) = 72.5%
Use When: Some fields more critical than others
```

**Type 3: Sum**
```
Formula: Add all field scores
Example: 10 + 7 + 5 = 22 points
Use When: Calculating total assessment points
```

**Type 4: Custom Formula**
```
Formula: Custom expression
Example: MAX(field1, field2) × field3 / 100
Use When: Complex business logic required
```

---

## CONFIGURATION EXAMPLE

**Scenario:** Infrastructure Health Score from LAN, WAN, and Server status fields

**MetricDefinition:**
- MetricCode: "INFRASTRUCTURE_SECTION_SCORE"
- MetricName: "Infrastructure Health Score"
- MetricScope: "Section"
- DataType: "Percentage"
- ThresholdGreen: 90.0
- ThresholdYellow: 75.0

**SectionMetricMapping:**
- SectionId: 1 (Infrastructure and Systems)
- MetricId: 201
- AggregationType: "Weighted"
- AggregationConfig:
```json
{
  "type": "weighted_average",
  "weights": {
    "45": 0.4,
    "46": 0.3,
    "47": 0.3
  }
}
```
- UseOnlyMappedFields: true

**Explanation:**
- ItemId 45 (LAN Status): 40% of section score
- ItemId 46 (WAN Status): 30% of section score
- ItemId 47 (Server Status): 30% of section score

---

## DATA FLOW

**User Submission:**
- LAN Status: "Faulty" → Score: 50
- WAN Status: "Operational" → Score: 100
- Server Status: "Partial" → Score: 75

**System Processing:**

1. **Retrieve field scores** from FormTemplateResponses or field-level calculations
2. **Apply aggregation formula**
   - (50 × 0.4) + (100 × 0.3) + (75 × 0.3)
   - = 20 + 30 + 22.5
   - = 72.5%

3. **Evaluate threshold**
   - 72.5 < 75.0 (Yellow threshold)
   - Result: 🔴 Red status

4. **Save to TenantMetrics**
   - MetricId: 201
   - MetricScope: "Section"
   - SourceSectionId: 1
   - NumericValue: 72.5
   - DrillDownData: '{"LAN":50, "WAN":100, "Server":75}'

**Dashboard Display:**
"Infrastructure Health: 72.5% 🔴" (Red because below 75% threshold)

---

## COMPLETE EXAMPLE

**Form Template:** "Monthly ICT Report"

**Section 1: Infrastructure and Systems**

**Fields:**
- Item 45: "LAN Equipment Status" (Checkbox: Operational=100, Faulty=50, Down=0)
- Item 46: "WAN Equipment Status" (Checkbox: Operational=100, Faulty=50, Down=0)
- Item 47: "Server Status" (Checkbox: Operational=100, Partial=75, Down=0)
- Item 48: "Switch Count" (Number field - NOT included in health score)

**Section Metric:**
- Code: INFRASTRUCTURE_SECTION_SCORE
- Calculation: Weighted average of Items 45, 46, 47
- Excludes: Item 48 (not relevant to health scoring)

**Result:**
Single metric value representing overall infrastructure health, used for:
- Dashboard KPI display
- Roll-up to template-level score
- Drill-down analysis to field details
- Historical trend tracking

---

## BEST PRACTICES

1. **Use Weighted Aggregation** when fields have different importance levels
2. **Set UseOnlyMappedFields=true** to exclude non-scored fields (like counts, text)
3. **Define clear thresholds** for traffic light indicators
4. **Store drill-down data** in DrillDownData JSON for UI navigation
5. **Link to parent metrics** using ParentMetricId for hierarchy
6. **Name consistently** - suffix with "_SECTION_SCORE" or "_SECTION_TOTAL"
