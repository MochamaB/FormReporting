# FormBuilder Metric Mapping - Complete Implementation Plan

**Retrieved:** December 2, 2025  
**Status:** Step 1-2 Implemented, Step 3 (Metric Mapping) Pending

---

## 📊 **7-Step FormBuilder Wizard Overview**

```
Step 1: TemplateSetup ✅ DONE       → Basic Info + Settings (metadata only)
Step 2: FormBuilder ✅ DONE         → Sections + Fields + Validation + Logic
Step 3: MetricMapping ❌ PENDING    → Map fields to KPIs (THIS STEP)
Step 4: ApprovalWorkflow            → Define approval levels
Step 5: FormAssignments             → Assign to tenants/roles/users (ACCESS CONTROL)
Step 6: ReportConfiguration         → Configure reporting/dashboards
Step 7: ReviewPublish               → Validate + Publish
```

---

## 🎯 **Step 3: Metric Mapping - Detailed Breakdown**

### **What This Step Does:**

Links form fields to predefined metrics/KPIs so that when forms are submitted:
1. System automatically extracts field values
2. Applies transformation logic (if needed)
3. Calculates KPIs and scores
4. Populates `TenantMetrics` table for reporting

---

## 🔧 **Metric Mapping Architecture**

### **4 Mapping Types:**

| Type | Description | Use Case | Example |
|------|-------------|----------|---------|
| **Direct** | Field value → Metric | Simple data collection | "Number of Computers" (25) → Total Computers Metric = 25 |
| **Calculated** | Formula using multiple fields | KPI calculation | Availability % = (Operational / Total) × 100 |
| **BinaryCompliance** | Yes/No → 100% or 0% | Compliance tracking | "Is LAN Operational?" = Yes → 100% |
| **ScoreMapping** | Options have score values | Assessment forms | "LAN Status" = Fully Operational → 10 points |

---

## 🏗️ **Implementation Plan: 4 Phases**

### **Phase 1: Foundation (Weeks 1-2)** ✅ **MOSTLY DONE**

#### **1.1 Metric Management CRUD** ✅ 70% COMPLETE

**Status:**
- ✅ Index (READ) - Complete
- ✅ Create - Complete (Wizard pattern)
- ✅ Edit - Complete (Tabs pattern)
- ❌ Details - Missing
- ❌ Delete - Missing

**Files:**
- ✅ `Controllers/Metrics/MetricDefinitionsController.cs`
- ✅ `Views/Metrics/MetricDefinitions/Index.cshtml`
- ✅ `Views/Metrics/MetricDefinitions/Create.cshtml` (Wizard)
- ✅ `Views/Metrics/MetricDefinitions/Edit.cshtml` (Tabs)
- ❌ `Views/Metrics/MetricDefinitions/Details.cshtml` (TO DO)

**Next:**
- [ ] Implement Details page
- [ ] Implement Delete action (soft delete recommended)
- [ ] Test all CRUD operations

---

#### **1.2 Seed Core Metrics** ⚠️ PARTIALLY DONE

**Status:** Seeder created but temporarily commented out

**File:** `Data/Seeders/MetricDefinitionSeeder.cs`

**Seeded Metrics:**
- Hardware: Computers, Servers, Printers, Scanners
- Network: LAN Switches, Routers, Availability %
- Software: Licensed, Unlicensed
- Personnel: ICT Staff
- Performance: Uptime %, Response Time
- Compliance: License Compliance %, Policy Adherence %
- And more...

**Next:**
- [ ] Uncomment seeder in `Program.cs`
- [ ] Run application to populate metrics
- [ ] Verify all metrics created successfully

---

#### **1.3 Database Updates** ✅ DONE

**Status:** All tables and columns exist

**Key Tables:**
- ✅ `MetricDefinitions` - Master catalog of metrics
- ✅ `FormItemMetricMappings` - Links fields to metrics
- ✅ `FormItemOptions.ScoreValue` - For scoring assessments
- ✅ `TenantMetrics` - Stores calculated metric values
- ✅ `MetricPopulationLog` - Audit trail

---

### **Phase 2: Form Builder Integration (Weeks 3-4)** ❌ **NOT STARTED**

#### **2.1 Enhance Form Builder (Step 2)** ❌ TO DO

**Current Status:**
- ✅ Form Builder works (fields, sections, validation)
- ❌ No score value input for options yet

**What's Needed:**
```
When creating Radio/Dropdown/Checkbox options:
├─ Add "Score Value" input field (decimal)
├─ Preview total max score for section/form
├─ Validation: Ensure score values are numeric
└─ Display in Properties Panel
```

**Files to Update:**
- `Views/Forms/FormTemplates/Partials/_FieldProperties.cshtml`
- `Views/Forms/FormTemplates/Partials/_FieldOptions.cshtml`
- `wwwroot/js/form-builder.js`

---

#### **2.2 Implement Metric Mapping UI (Step 3)** ❌ TO DO

**URL:** `/Forms/FormTemplates/MetricMapping/{templateId}`

**UI Features:**

```
LEFT PANEL: Form Fields
┌────────────────────────────────┐
│ Section 1: Network             │
│   ├─ Total Switches [Link]     │ ← Click to map
│   ├─ Operational Switches      │
│   └─ LAN Status (Radio)        │
│                                 │
│ Section 2: Hardware            │
│   ├─ Total Computers [Mapped] │ ← Already mapped
│   └─ Operational Computers    │
└────────────────────────────────┘

CENTER: Mapping Configuration
┌────────────────────────────────┐
│ Mapping: Total Switches        │
│                                 │
│ Metric: [Search/Select]        │
│ ▼ TOTAL_NETWORK_SWITCHES       │
│                                 │
│ Mapping Type:                  │
│ ○ Direct                       │ ← Simple value copy
│ ○ Calculated                   │ ← Formula
│ ○ Binary Compliance            │ ← Yes/No
│ ○ Score Mapping                │ ← Assessment
│                                 │
│ [Save Mapping] [Cancel]        │
└────────────────────────────────┘

RIGHT PANEL: Available Metrics
┌────────────────────────────────┐
│ 🔍 Search metrics...           │
│                                 │
│ ▼ Hardware (12)                │
│   ├─ Total Computers           │
│   ├─ Operational Computers     │
│   └─ Computer Availability %   │
│                                 │
│ ▼ Network (8)                  │
│   ├─ Total Switches            │
│   └─ LAN Uptime %              │
└────────────────────────────────┘
```

**Mapping Type Details:**

1. **Direct Mapping** (Simplest)
   ```
   Field: "Number of Computers" (Integer)
   Metric: TOTAL_COMPUTERS
   Logic: None required
   Result: Value copied as-is
   ```

2. **Calculated Mapping** (Formula-Based)
   ```
   Field: "Total Switches" (used in formula)
   Metric: LAN_AVAILABILITY_PCT
   Logic: {
     "formula": "(item46 / item45) * 100",
     "items": [45, 46],
     "roundTo": 2,
     "validateDivisionByZero": true
   }
   Result: Calculated KPI
   ```

3. **Binary Compliance Mapping** (Yes/No)
   ```
   Field: "Is LAN Operational?" (Yes/No Radio)
   Metric: LAN_OPERATIONAL_COMPLIANCE
   Expected Value: "Yes"
   Logic: None required
   Result: Yes = 100%, No = 0%
   ```

4. **Score Mapping** (Assessment Forms)
   ```
   Field: "LAN Status" (Radio with score values)
   Options:
     - Fully Operational → ScoreValue: 10
     - Partially Operational → ScoreValue: 5
     - Not Operational → ScoreValue: 0
   Metric: ICT_ASSESSMENT_SCORE
   Logic: {
     "scoreSource": "option",
     "contributesTo": "total",
     "weight": 1.0
   }
   Result: Selected option's score
   ```

**Key Features:**
- ✅ Drag-and-drop field to metric
- ✅ Inline mapping type selector
- ✅ Formula builder for calculated mappings
- ✅ Live preview/test with sample data
- ✅ Validation (e.g., data type compatibility)
- ✅ Bulk mapping (map multiple fields at once)

**Controller Actions Needed:**
```csharp
[HttpGet("MetricMapping/{id}")]
public async Task<IActionResult> MetricMapping(int id)
{
    // Load template + existing mappings
    // Load available metrics
    // Build progress tracker
}

[HttpPost("SaveMetricMapping")]
public async Task<IActionResult> SaveMetricMapping(MetricMappingDto dto)
{
    // Validate mapping
    // Save to FormItemMetricMappings
    // Return success
}

[HttpDelete("DeleteMetricMapping/{mappingId}")]
public async Task<IActionResult> DeleteMetricMapping(int mappingId)
{
    // Remove mapping
}

[HttpPost("TestMetricMapping")]
public async Task<IActionResult> TestMetricMapping(TestMappingDto dto)
{
    // Preview calculation with sample data
}
```

---

### **Phase 3: Metric Population Service (Weeks 5-6)** ❌ **NOT STARTED**

#### **3.1 Service Implementation** ⚠️ PARTIALLY EXISTS

**Status:**
- ✅ Service interface defined
- ✅ Service registered in DI
- ⚠️ Implementation incomplete

**File:** `Services/Metrics/MetricPopulationService.cs`

**Core Methods:**
```csharp
public class MetricPopulationService : IMetricPopulationService
{
    // Main entry point
    public async Task PopulateMetrics(int submissionId)
    {
        // 1. Load submission + responses
        // 2. Load all mappings for this template
        // 3. Process mappings by type (order matters!)
        // 4. Log results to MetricPopulationLog
    }

    // Type-specific processors
    private async Task ProcessDirectMapping(...)
    {
        // Simple value copy
    }

    private async Task ProcessCalculatedMapping(...)
    {
        // Parse formula, evaluate, round
    }

    private async Task ProcessBinaryComplianceMapping(...)
    {
        // Check if value matches expected
    }

    private async Task ProcessScoreMapping(...)
    {
        // Lookup ScoreValue from option
        // Apply weight if configured
    }

    // Helper methods
    private async Task SaveToTenantMetrics(...)
    private async Task LogMetricPopulation(...)
    private async Task LogMetricError(...)
}
```

**Processing Order:**
```
1. Direct mappings (no dependencies)
2. Binary compliance (no dependencies)
3. Score mappings (no dependencies)
4. Calculated mappings (may depend on above)
```

---

#### **3.2 Integration with Submission Workflow** ❌ TO DO

**Trigger Point:**

After form submission is approved:
```csharp
// In FormSubmissionsController.cs
if (submission.Status == "Approved")
{
    await _metricPopulationService.PopulateMetrics(submission.SubmissionId);
}
```

**OR** If no approval workflow:
```csharp
// Immediately after submission
await _metricPopulationService.PopulateMetrics(submission.SubmissionId);
```

---

#### **3.3 Error Handling & Logging** ❌ TO DO

**Log to:** `MetricPopulationLog`

**Status Values:**
- `Success` - Processed successfully
- `Failed` - Error occurred (e.g., division by zero)
- `Skipped` - Conditional logic not met
- `Blocked` - Missing dependencies

**Example Log Entry:**
```json
{
  "submissionId": 123,
  "metricId": 14,
  "mappingId": 45,
  "sourceValue": "item45=25, item46=20",
  "calculatedValue": 80.0,
  "calculationFormula": "(item46/item45)*100",
  "status": "Success",
  "processedDate": "2025-12-02T08:30:00Z"
}
```

---

### **Phase 4: Reporting Integration (Weeks 7-8)** ❌ **NOT STARTED**

#### **4.1 Dashboard Widgets** ❌ TO DO

**Display Format:**
```
┌────────────────────────────┐
│ LAN Availability           │
│ 87.5% ●                    │ ← Yellow indicator
│ ↓ -2.3% from last month    │
│ ▁▂▃▅▄▃▅▇ (trend)           │
└────────────────────────────┘
```

**Features:**
- ✅ Current value
- ✅ Traffic light (Green/Yellow/Red)
- ✅ Trend arrow (↑ ↓ →)
- ✅ Sparkline chart

---

#### **4.2 Metric Reports** ❌ TO DO

**URL:** `/Reports/Metrics`

**Report Types:**
1. **Metric Comparison** - Compare metric across tenants
2. **Time Series** - Trend over time
3. **Threshold Alerts** - Metrics below threshold
4. **Score Leaderboards** - Rank tenants by assessment scores

---

## 📋 **Current Implementation Status**

### ✅ **What's Done:**

| Component | Status | Notes |
|-----------|--------|-------|
| Metric Definitions CRUD | 70% | Missing Details & Delete |
| Metric Seeder | Ready | Needs to be run |
| Database Schema | 100% | All tables exist |
| Form Builder (Step 2) | 100% | Fields, sections, validation |
| Progress Tracker | 100% | Shows all 7 steps |

### ❌ **What's Pending:**

| Component | Priority | Est. Time |
|-----------|----------|-----------|
| **ScoreValue Input in Options** | High | 2-3 days |
| **Metric Mapping UI (Step 3)** | High | 1-2 weeks |
| **Metric Population Service** | High | 1 week |
| **Test Mapping Feature** | Medium | 2-3 days |
| **Dashboard Integration** | Low | 1 week |
| **Metric Reports** | Low | 1 week |

---

## 🎯 **Recommended Next Steps**

### **Immediate (This Week):**

1. ✅ **Complete Metric Definitions CRUD**
   - [ ] Implement Details page
   - [ ] Implement Delete action
   - [ ] Test all operations

2. ✅ **Run Metric Seeder**
   - [ ] Uncomment in Program.cs
   - [ ] Verify metrics populated

3. ✅ **Plan Metric Mapping UI**
   - [ ] Design wireframes
   - [ ] Define API endpoints
   - [ ] Create ViewModels

### **Short Term (Next 2 Weeks):**

4. **Add ScoreValue to Form Builder**
   - [ ] Update Field Options UI
   - [ ] Add validation
   - [ ] Test with Radio/Dropdown fields

5. **Implement Metric Mapping UI**
   - [ ] Create MetricMapping view
   - [ ] Implement controller actions
   - [ ] Build mapping configuration panel
   - [ ] Add test/preview feature

### **Medium Term (Next Month):**

6. **Complete Metric Population Service**
   - [ ] Implement all 4 mapping processors
   - [ ] Add error handling
   - [ ] Integrate with submission workflow
   - [ ] Test with real data

7. **Dashboard Integration**
   - [ ] Create metric widgets
   - [ ] Add traffic light indicators
   - [ ] Implement trend charts

---

## 🧪 **Testing Checklist**

### **Phase 2 Testing:**
- [ ] Can create mappings for all 4 types
- [ ] Data type validation works
- [ ] Formula builder saves correctly
- [ ] Test preview shows accurate results
- [ ] Can edit existing mappings
- [ ] Can delete mappings

### **Phase 3 Testing:**
- [ ] Direct mapping populates correctly
- [ ] Calculated mapping evaluates formulas
- [ ] Binary compliance converts Yes/No
- [ ] Score mapping sums correctly
- [ ] Error handling works (division by zero)
- [ ] Logs all operations to MetricPopulationLog

### **Phase 4 Testing:**
- [ ] Dashboard shows correct metric values
- [ ] Traffic lights match thresholds
- [ ] Trends show historical data
- [ ] Reports generate successfully

---

## 📚 **Key Documents**

| Document | Purpose |
|----------|---------|
| `IMPLEMENTATION_SUMMARY.md` | 7-step wizard overview |
| `MetricMapping_And_Scoring_Complete_Guide.md` | Complete architecture & examples |
| `Section3_Metrics_Deep_Dive.md` | Metric system deep dive |
| `Section3_Workflows_Actions.md` | Metric workflows & business rules |
| `4D_MetricPopulation_Service.md` | Service implementation guide |

---

## 🎉 **Summary**

**Current Phase:** Between Phase 1 and Phase 2

**Phase 1 Status:** ~80% complete (Metric CRUD mostly done)

**Next Major Milestone:** Implement Metric Mapping UI (Step 3 of FormBuilder)

**Estimated Time to Complete Step 3:** 2-3 weeks of focused development

**Key Dependencies:**
- ✅ Database schema ready
- ✅ Metrics seeded (or ready to seed)
- ✅ Form Builder working
- ❌ Score values in options (quick add)
- ❌ Metric Mapping UI (main work)
- ❌ Metric Population Service (backend logic)

---

**Would you like to start with:**
1. Adding ScoreValue to Form Builder options?
2. Creating the Metric Mapping UI wireframes/design?
3. Implementing the Metric Population Service?

