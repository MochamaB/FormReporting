# Form Builder: Finalization & Publishing

**Purpose:** Metric mapping, template preview, and publishing workflow  
**Users:** Head Office ICT Managers  
**Components:** Metric Mapping, Template Preview, Publishing Workflow

---

## 7. Metric Mapping Interface

**Component Type:** Modal with Formula Builder

**Reusable Components:**
- **Modal** - Mapping configuration
- **Formula Builder** - Visual formula editor
- **Dropdown** - Metric selection (filtered)

### Mapping Types Overview

| Type | Use Case | Example |
|------|----------|---------|
| **Direct** | Field value copied as-is | "Number of Computers" → "Total Workstations" metric |
| **Calculated** | Formula using multiple fields | "(Operational Hours / Total Hours) * 100" → "Uptime %" |
| **BinaryCompliance** | Yes/No check → 100 or 0 | "Is backup running?" = "Yes" → 100% compliance |

### Interface

```
┌─ Metric Mapping for: "Number of Computers" ───────────────────┐
│                                                                 │
│  Select Metric: [Dropdown filtered by KPI Category]           │
│    → Available: Metrics from "IT Operations" category         │
│    → Selected: "Total Workstations"                            │
│                                                                 │
│  Mapping Type:                                                 │
│    ● Direct         - Copy field value as-is                   │
│    ○ Calculated     - Use formula with other fields            │
│    ○ BinaryCompliance - Check if matches expected value       │
│                                                                 │
│  ┌─ Direct Mapping Configuration ──────────────────────────┐  │
│  │                                                           │  │
│  │  Field Value → Metric NumericValue                       │  │
│  │  No transformation needed                                │  │
│  │                                                           │  │
│  │  Example: User enters 25 → Metric stores 25             │  │
│  │                                                           │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│  [Save Mapping] [Cancel]                                       │
└─────────────────────────────────────────────────────────────────┘
```

---

### 7.1 Direct Mapping

**Configuration:** None needed

**Logic:**
```
When submission approved:
1. Read field value from FormTemplateResponses
2. Insert/Update TenantMetrics
   - MetricId = mapped metric
   - NumericValue = field value (direct copy)
   - SourceType = 'UserInput'
   - SourceReferenceId = SubmissionId
```

**Use Cases:**
- Counts: Number of computers, employees, licenses
- Quantities: Storage capacity, bandwidth, inventory
- Pre-calculated percentages: User enters "85%" directly

---

### 7.2 Calculated Mapping

**Interface:**
```
┌─ Calculated Mapping Configuration ────────────────────────────┐
│                                                                 │
│  Formula Builder:                                               │
│                                                                 │
│  Available Fields (drag to formula or click to insert):        │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │ • Operational Hours (item21) [Number]                   │  │
│  │ • Total Hours (item20) [Number]                         │  │
│  │ • Downtime Hours (item22) [Number]                      │  │
│  │ • Number of Users (item15) [Number]                     │  │
│  └─────────────────────────────────────────────────────────┘  │
│                                                                 │
│  Operators: + - * / ( )                                        │
│                                                                 │
│  Formula: [                                                ]    │
│  (item21 / item20) * 100                                        │
│                                                                 │
│  Test Formula (optional):                                       │
│  item21 = [700]  item20 = [800]                               │
│  → Result: 87.5  ✓ Valid                                       │
│                                                                 │
│  Description (optional):                                        │
│  [Calculate system uptime percentage from operational hours]   │
│                                                                 │
│  Formula Validation:                                            │
│  ✓ Syntax valid                                                │
│  ✓ All referenced items exist                                  │
│  ✓ No circular dependencies                                    │
│  ✓ Test calculation successful                                 │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**JSON Storage:**
```json
{
  "formula": "(item21 / item20) * 100",
  "items": [21, 20],
  "itemCodes": ["SEC3_021", "SEC3_020"],
  "description": "Calculate uptime percentage"
}
```

**Validation Rules:**
```
On Save:
1. Parse formula, extract item references (item21, item20)
2. Check all referenced items exist in template
3. Check all referenced items have DataType = Number
4. Check no circular dependencies (metric → field → same metric)
5. Test formula with sample data
6. If validation fails, block save with specific error

Validation Errors:
- "Referenced field 'item99' does not exist"
- "Field 'item15' is Text type, cannot use in numeric formula"
- "Circular dependency detected: Metric A depends on Field X, which maps to Metric A"
- "Division by zero in test calculation"
- "Invalid formula syntax: missing closing parenthesis"
```

**Runtime Logic (During Metric Population):**
```
When submission approved:
1. Parse formula from TransformationLogic JSON
2. For each referenced item, get value from FormTemplateResponses
3. Check all source values exist and are not null
4. Handle edge cases:
   - Division by zero → Return null, log warning
   - Null values → Skip calculation, log warning
   - Invalid data types → Log error, skip
5. Execute formula
6. Insert/Update TenantMetrics with calculated value
7. Log calculation in MetricPopulationLog:
   - SourceValue: "700 / 800"
   - CalculatedValue: 87.5
   - Formula: "(item21 / item20) * 100"
   - Status: Success/Failed
```

**Supported Operators:**
```
Arithmetic:
+ Addition
- Subtraction
* Multiplication
/ Division
( ) Parentheses for order of operations

Advanced (future):
ROUND(value, decimals)
MAX(value1, value2)
MIN(value1, value2)
IF(condition, trueValue, falseValue)
```

---

### 7.3 Binary Compliance Mapping

**Interface:**
```
┌─ Binary Compliance Configuration ──────────────────────────────┐
│                                                                 │
│  Expected Answer: [Dropdown populated from field options]     │
│    → Options: Yes, No                                          │
│    → Selected: "Yes"                                           │
│                                                                 │
│  Compliance Rule:                                              │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │ IF user's answer = "Yes" THEN metric value = 100        │  │
│  │ IF user's answer ≠ "Yes" THEN metric value = 0          │  │
│  └─────────────────────────────────────────────────────────┘  │
│                                                                 │
│  Use Case: Compliance checks                                   │
│  Examples:                                                      │
│  • Is backup running? (Yes = 100%, No = 0%)                   │
│  • Is antivirus updated? (Yes = compliant, No = non-compliant)│
│  • Is firewall enabled? (Yes = 100%, No = 0%)                 │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**Storage:**
```
Table: FormItemMetricMappings
Fields:
- MappingType = 'BinaryCompliance'
- ExpectedValue = 'Yes' (or other expected answer)
- TransformationLogic = null (not needed for binary)
```

**Runtime Logic:**
```
When submission approved:
1. Read field value from FormTemplateResponses
2. Read ExpectedValue from FormItemMetricMappings
3. Compare:
   IF value == ExpectedValue THEN metricValue = 100
   ELSE metricValue = 0
4. Insert/Update TenantMetrics
5. Log in MetricPopulationLog:
   - ExpectedValue: "Yes"
   - ActualValue: "No"
   - ComplianceResult: 0
   - Status: Success
```

**Use Cases:**
- All yes/no compliance questions
- Binary status checks
- Pass/fail assessments
- Presence/absence verification

---

### Multiple Mappings

**One field can map to multiple metrics:**

```
Field: "Number of Computers"
  ↓
Mapping 1: Direct → "Total Workstations" metric
Mapping 2: Direct → "IT Equipment Count" metric  
Mapping 3: Calculated → "Computer-to-Employee Ratio" metric
           Formula: (Number of Computers / Number of Employees)

All 3 metrics populated when submission approved
```

**Interface for Multiple Mappings:**
```
┌─ Metric Mappings for: "Number of Computers" ──────────────────┐
│                                                                 │
│  Existing Mappings:                                             │
│                                                                 │
│  [1] Total Workstations                                        │
│      Type: Direct                              [Edit] [Delete] │
│                                                                 │
│  [2] IT Equipment Count                                        │
│      Type: Direct                              [Edit] [Delete] │
│                                                                 │
│  [+ Add Another Mapping]                                        │
│                                                                 │
│  [Close]                                                        │
└─────────────────────────────────────────────────────────────────┘
```

---

### Metric Selection Filtering

```
Load metrics based on:

1. Template's KPI Category (if set)
   IF template.KPICategoryId IS NOT NULL:
     Show only metrics WHERE MetricCategoryId = template.KPICategoryId
   ELSE:
     Show all active metrics

2. DataType compatibility
   IF field.DataType = 'Number':
     Show only metrics WHERE DataType = 'Numeric'
   IF field.DataType = 'Boolean' OR Dropdown:
     Show metrics WHERE DataType = 'Numeric' (for binary compliance)

3. SourceType filtering
   Exclude metrics WHERE SourceType = 'SystemCalculated'
   (Reserved for auto-calculated metrics like aggregations)

4. Active status
   Show only WHERE IsActive = 1
```

---

### Error Handling

**Failed Metric Population:**
```
Scenario: Formula references field that user skipped (optional field)

Action:
1. Do NOT block submission approval
2. Log error in MetricPopulationLog:
   - Status: Failed
   - ErrorMessage: "Source field 'item22' has no value (null)"
   - SubmissionId: 12345
   - MetricId: 67
3. Send notification to System Administrator
4. Show warning badge on submission: "Metric population incomplete"
5. Allow manual metric entry as fallback
```

**Division by Zero:**
```
Formula: (item21 / item20) * 100
item20 value = 0

Action:
1. Detect division by zero before execution
2. Return null for metric value
3. Log warning in MetricPopulationLog:
   - Status: Warning
   - ErrorMessage: "Division by zero: item20 = 0"
4. Metric not populated (remains null)
```

---

### API Endpoints

```
POST   /api/templates/{templateId}/fields/{itemId}/metric-mappings
       Body: { metricId, mappingType, transformationLogic, expectedValue }
       
PUT    /api/templates/{templateId}/metric-mappings/{mappingId}
       Body: { modified fields }
       
DELETE /api/templates/{templateId}/metric-mappings/{mappingId}

GET    /api/metrics/available?templateId={id}&fieldId={id}
       Returns: Filtered list of compatible metrics
```

---

## 8. Template Preview Component

**Component Type:** Full-Screen Modal with Form Renderer

**Reusable Components:**
- **Modal** (full-screen mode)
- **Tabs or Accordion** (for sections)
- **Dynamic Form Renderer** (same as submission component)

### Purpose
Test template before publishing - see exactly what end users will see during submission

### Features

**1. Section Navigation**
```
Display based on section count:
- 2-5 sections: Horizontal tabs
- 6+ sections: Vertical accordion
- Stepped mode: Wizard navigation (if enabled)
```

**2. Complete Field Rendering**
```
Each field rendered with:
✓ Same input controls as submission
✓ Validation rules (client-side only)
✓ Help text and tooltips
✓ Placeholder text
✓ Required field indicators (red asterisk)
✓ Conditional logic active
✓ Pre-fill simulations
✓ Metric mapping badges
```

**3. Pre-fill Simulation**
```
For fields with pre-fill sources:

Hardware Asset:
- Load 3-5 random assets from HardwareAssets (user's tenant)
- Display as dropdown
- Label: "📦 Pre-filled from Hardware Inventory"
- If no assets: Show "⚠️ No hardware assets found for your tenant"

Software Asset:
- Load 3-5 random assets from SoftwareAssets
- Display as dropdown  
- Label: "💿 Pre-filled from Software Inventory"

Tenant Property:
- Load actual tenant data (current user's tenant)
- Display as read-only field
- Label: "🏢 Auto-filled from Tenant Profile"
```

**4. Conditional Logic (Active)**
```
JavaScript evaluation enabled:
- Change source field value → Target fields show/hide immediately
- Visual feedback: Fields fade in/out
- Console logs: "Field 'LAN Issue Details' shown (condition met)"
```

**5. Validation Display**
```
Validation badges on fields:
- Required: Red asterisk *
- Has Rules: Blue info icon with count "3 rules"
- Hover: Shows validation rules in tooltip

Try validation:
- Enter invalid value → See error message
- Enter valid value → Error clears
- Required field empty → "This field is required"
```

**6. Metric Mapping Indicators**
```
Fields with metric mappings show badge:
[Field: Number of Computers] 📊 Linked to 2 metrics

Hover to see:
- Total Workstations (Direct)
- Computer-to-Employee Ratio (Calculated)
```

### Interface

```
┌─ Template Preview: "Factory Monthly ICT Report" v1.0 ─────────┐
│                                                    [✕ Close]    │
│                                                                 │
│  ℹ️ This is a preview. No data will be saved.                 │
│                                                                 │
│  [Section 1: General Info] [Section 2: Hardware] [Section 3...]│
│                                                                 │
│  ┌─ Section 1: General Information ─────────────────────────┐ │
│  │                                                            │ │
│  │  Factory Name*                                            │ │
│  │  [Kangaita Factory                 ] 🏢 Pre-filled       │ │
│  │                                                            │ │
│  │  Reporting Period*                                        │ │
│  │  [November 2025                    ] 📅                  │ │
│  │                                                            │ │
│  │  Region*                                                  │ │
│  │  [Central Region ▼                 ] 🏢 Pre-filled       │ │
│  │                                                            │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                 │
│  ┌─ Section 2: Hardware Inventory ───────────────────────────┐ │
│  │                                                            │ │
│  │  Number of Computers* ℹ️ (3 validation rules)            │ │
│  │  [25                               ] 📊 2 metrics         │ │
│  │  💡 Count only functional workstations                    │ │
│  │                                                            │ │
│  │  Is LAN working?*                                         │ │
│  │  ● Yes  ○ No                                              │ │
│  │                                                            │ │
│  │  <!-- Conditional field (hidden until "No" selected) --> │ │
│  │                                                            │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                 │
│  [Previous Section] [Next Section]                             │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Key Logic

**Data Handling:**
```
NO database saves:
- All data kept in browser memory only
- On close, all test data discarded
- No FormTemplateSubmissions created
- No FormTemplateResponses saved

Sample data sources:
- Hardware: SELECT TOP 5 * FROM HardwareAssets WHERE TenantId = @CurrentUserTenantId
- Software: SELECT TOP 5 * FROM SoftwareAssets WHERE TenantId = @CurrentUserTenantId
- Tenant: SELECT * FROM Tenants WHERE TenantId = @CurrentUserTenantId
```

**Validation Behavior:**
```
Client-side only:
- Validation runs on blur (field loses focus)
- Error messages shown below field
- Submit button disabled if errors
- No server-side validation
```

**Conditional Logic:**
```
Same JavaScript as actual submission:
- Runs on page load
- Re-evaluates on field change
- Shows/hides fields instantly
- No delays or API calls
```

### Access Points

**From Section Builder:**
```
[Preview Template] button → Opens preview modal
```

**From Template Dashboard:**
```
Actions column: [👁️ Preview] icon → Opens preview modal
```

---

## 9. Publishing Workflow

**Component Type:** Modal with Validation Checklist

**Reusable Components:**
- **Modal** - Publishing confirmation
- **Checklist** - Validation results with icons
- **Alert Badges** - Error/warning indicators
- **Accordion** - Group errors/warnings by category

### Pre-Publish Validation

**Critical Errors (Must Fix):**
```
❌ Template has no sections
❌ Template has no fields
❌ Dropdown field "Department" has no options
❌ Conditional logic in field "Issue Details" references deleted field (item42)
❌ Conditional logic has circular dependency: Field A → Field B → Field A
❌ Metric mapping formula in field "Uptime %" has syntax error
❌ If RequiresApproval=true, WorkflowId is null
❌ Template Code "TPL_IT_001" already exists (duplicate)
```

**Warnings (Can Publish with Confirmation):**
```
⚠️ No metric mappings configured (template won't populate KPIs)
⚠️ No validation rules on any field (may receive bad data)
⚠️ No conditional logic used (all fields always visible)
⚠️ No help text on 15 fields (users may be confused)
⚠️ Template has 67 fields (very long form, consider splitting)
⚠️ Section "Miscellaneous" has only 1 field (consider merging)
```

### Interface

```
┌─ Publish Template: "Factory Monthly ICT Report" v1.0 ─────────┐
│                                                                 │
│  Pre-Publish Validation Results:                               │
│                                                                 │
│  ✅ Structure                                                  │
│     ✓ Template has 5 sections                                  │
│     ✓ Template has 33 fields                                   │
│     ✓ All sections have fields                                 │
│                                                                 │
│  ✅ Fields                                                     │
│     ✓ All dropdown fields have options                         │
│     ✓ All required fields configured correctly                 │
│     ✓ Field codes are unique                                   │
│                                                                 │
│  ❌ Conditional Logic (1 error)                                │
│     ✗ Field "LAN Issue Details" references missing field       │
│        Referenced: item42 (SEC2_042)                           │
│        Status: Field was deleted                               │
│        [Go to Field] [Remove Logic]                            │
│                                                                 │
│  ✅ Validation Rules                                           │
│     ✓ All regex patterns are valid                            │
│     ✓ No conflicting rules                                     │
│                                                                 │
│  ⚠️ Metric Mappings (1 warning)                               │
│     ⚠ Only 3 of 33 fields mapped to metrics                   │
│        This may result in incomplete KPI data                  │
│        [Go to Metric Mappings] [Ignore]                        │
│                                                                 │
│  ✅ Settings                                                   │
│     ✓ Workflow assigned (2-Step Approval)                     │
│     ✓ Template code is unique                                  │
│     ✓ KPI category assigned (IT Operations)                    │
│                                                                 │
│  ───────────────────────────────────────────────────────────  │
│                                                                 │
│  Summary:                                                       │
│  • Errors: 1 (must fix before publishing)                     │
│  • Warnings: 1 (can publish but recommended to address)        │
│                                                                 │
│  ⚠️ IMPORTANT: Published templates cannot be edited.           │
│     To make changes later, you must create a new version.      │
│                                                                 │
│  [Fix Errors First] [Publish Template] [Cancel]                │
│                                                                 │
│  [Publish Template] button disabled until errors fixed         │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Validation Logic (Server-Side)

```csharp
public class TemplatePublishValidator
{
    public ValidationResult ValidateForPublish(int templateId)
    {
        var result = new ValidationResult();
        var template = GetTemplate(templateId);
        
        // Critical validations
        ValidateStructure(template, result);
        ValidateFields(template, result);
        ValidateConditionalLogic(template, result);
        ValidateMetricMappings(template, result);
        ValidateSettings(template, result);
        
        // Warning validations
        CheckMetricCoverage(template, result);
        CheckValidationRules(template, result);
        CheckHelpText(template, result);
        
        return result;
    }
    
    private void ValidateStructure(FormTemplate template, ValidationResult result)
    {
        var sections = GetSections(template.TemplateId);
        
        if (sections.Count == 0)
            result.AddError("Template has no sections");
        
        var totalFields = sections.Sum(s => s.Fields.Count);
        if (totalFields == 0)
            result.AddError("Template has no fields");
        
        // Check each section has fields
        foreach (var section in sections)
        {
            if (section.Fields.Count == 0)
                result.AddWarning($"Section '{section.SectionName}' has no fields");
        }
    }
    
    private void ValidateConditionalLogic(FormTemplate template, ValidationResult result)
    {
        var fields = GetAllFields(template.TemplateId);
        
        foreach (var field in fields.Where(f => f.ConditionalLogic != null))
        {
            var logic = JsonConvert.DeserializeObject<ConditionalLogic>(field.ConditionalLogic);
            
            foreach (var rule in logic.Rules)
            {
                // Check referenced field exists
                if (!fields.Any(f => f.ItemId == rule.SourceItemId))
                {
                    result.AddError(
                        $"Field '{field.ItemName}' references missing field (item{rule.SourceItemId})",
                        fixAction: $"Go to field '{field.ItemName}' and remove or update conditional logic"
                    );
                }
            }
            
            // Check for circular dependencies
            if (HasCircularDependency(field, fields))
            {
                result.AddError($"Circular dependency detected in field '{field.ItemName}'");
            }
        }
    }
    
    private void ValidateMetricMappings(FormTemplate template, ValidationResult result)
    {
        var mappings = GetMetricMappings(template.TemplateId);
        
        foreach (var mapping in mappings)
        {
            if (mapping.MappingType == "Calculated")
            {
                var logic = JsonConvert.DeserializeObject<TransformationLogic>(mapping.TransformationLogic);
                
                // Validate formula syntax
                if (!IsValidFormula(logic.Formula))
                {
                    result.AddError(
                        $"Metric mapping for field '{mapping.ItemName}' has invalid formula",
                        fixAction: "Edit metric mapping and fix formula syntax"
                    );
                }
                
                // Check all referenced items exist
                foreach (var itemId in logic.Items)
                {
                    if (!FieldExists(itemId, template.TemplateId))
                    {
                        result.AddError($"Formula references non-existent field (item{itemId})");
                    }
                }
            }
        }
    }
}
```

### Publishing Actions

**If No Errors:**
```
Click [Publish Template]:

1. Show final confirmation:
   "Are you sure you want to publish this template?
    Once published, it cannot be edited. You can only archive it or create a new version."
   [Yes, Publish] [Cancel]

2. On confirm:
   POST /api/templates/{id}/publish
   
   Database Updates:
   UPDATE FormTemplates
   SET PublishStatus = 'Published',
       PublishedDate = GETUTCDATE(),
       PublishedBy = @CurrentUserId
   WHERE TemplateId = @TemplateId
   
3. Create audit log:
   INSERT AuditLogs (EntityType, EntityId, Action, UserId, Timestamp)
   VALUES ('FormTemplate', @TemplateId, 'Published', @CurrentUserId, GETUTCDATE())
   
4. Send notification:
   - To: Users with role "Assignment Manager"
   - Title: "New Template Published"
   - Message: "'{TemplateName}' is now available for assignment"
   
5. Show success message:
   "Template '{TemplateName}' published successfully!
    You can now assign it to users."
   
6. Redirect options:
   [View Template] [Create Assignment] [Back to Dashboard]
```

**If Errors Exist:**
```
[Publish Template] button: Disabled (grayed out)
Tooltip: "Fix 1 error before publishing"

Quick fix buttons:
[Go to Field] → Navigate to field in Section Builder
[Remove Logic] → Delete problematic conditional logic
[Edit Mapping] → Open metric mapping modal
```

**If Only Warnings:**
```
[Publish Template] button: Enabled (yellow warning color)

Click behavior:
1. Show additional confirmation:
   "This template has 1 warning:
    • Only 3 of 33 fields mapped to metrics
    
    Are you sure you want to publish anyway?"
   
   [Yes, Publish Anyway] [Go Back and Fix]
   
2. If "Yes", proceed with publish
3. If "Go Back", close modal, stay in Section Builder
```

---

### Post-Publish Restrictions

**Template becomes read-only:**
```
Can do:
✓ View template details
✓ Preview template
✓ Create assignments
✓ View submissions
✓ Archive template
✓ Create new version (clone)

Cannot do:
✗ Edit sections
✗ Edit fields
✗ Edit validation rules
✗ Edit conditional logic
✗ Edit metric mappings
✗ Change template type or category
✗ Delete template

Limited edits allowed:
- Template Name (display name only)
- Description
- Instructions for Submitters
- Support Contact
```

### Version Management

**Create New Version:**
```
From Dashboard: [Create New Version] action

Logic:
1. Clone template to new record
   - Copy all sections, fields, validations, logic, mappings
   - Increment version: v1.0 → v1.1, v1.9 → v2.0
   - Set PublishStatus = 'Draft'
   - Set OriginalTemplateId = old template ID
   
2. Update original template:
   UPDATE FormTemplates
   SET PublishStatus = 'Deprecated'
   WHERE TemplateId = @OldTemplateId
   
3. Redirect to new template in Section Builder
   
4. Show message:
   "Version 1.1 created from version 1.0.
    Original version marked as Deprecated.
    You can now edit this new version."
```

---

**Implementation Complete:** All Form Builder components documented
