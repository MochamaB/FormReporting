# Form Builder: Structure & Configuration

**Purpose:** Build template structure (sections, fields, validation, conditional logic)  
**Users:** Head Office ICT Managers  
**Components:** Section Builder, Field Builder, Validation Builder, Conditional Logic Builder

---

## 3. Section Builder

**Component Type:** Draggable List + Modal + Accordion

**Reusable Components:**
- **Draggable List** - Reorder sections
- **Modal** - Add/edit section forms
- **Accordion** - Collapse/expand sections
- **Confirmation Dialog** - Delete warnings

### Section Properties

```
SectionName (nvarchar 100, required)
SectionDescription (nvarchar 300)
DisplayOrder (int, auto-assigned)
IsActive (bit, default 1)
```

### Interface Layout

```
Template: "Factory Monthly ICT Report" v1.0 [Draft]

[+ Add Section] [Preview Template] [Continue to Publish →]

┌─ Section 1: General Information ──────────── [Edit] [Delete] [↕]
│  Description: Basic factory details
│  Display Order: 1 | Fields: 3
│  
│  ├─ Field: Factory Name (Text, Required)
│  ├─ Field: Reporting Period (Date, Required)
│  ├─ Field: Region (Dropdown, Required, Pre-fill)
│  │  └─ Has: 1 Validation, Pre-fill from Tenant
│  └─ [+ Add Field to This Section]
└──────────────────────────────────────────────────────────────

┌─ Section 2: Hardware Inventory ────────────── [Edit] [Delete] [↕]
│  Description: Computer and network equipment
│  Display Order: 2 | Fields: 0
│  
│  └─ [+ Add Field to This Section]
└──────────────────────────────────────────────────────────────

[+ Add Another Section]
```

### Key Business Logic

**Add Section:**
```
POST /api/templates/{templateId}/sections
Body: {
  sectionName: "Hardware Inventory",
  sectionDescription: "Computer and network equipment",
  displayOrder: null  // Auto-assigned
}

Logic:
1. Validate: SectionName required, max 100 chars
2. Calculate DisplayOrder = MAX(DisplayOrder) + 1
3. INSERT FormTemplateSections
4. Return SectionId
5. Refresh UI, expand new section
```

**Edit Section:**
```
PUT /api/templates/{templateId}/sections/{sectionId}
Body: {
  sectionName: "Updated Name",
  sectionDescription: "Updated Description"
}

Logic:
1. Only Name and Description editable
2. DisplayOrder unchanged
3. UPDATE FormTemplateSections
4. Cannot edit if template is Published
```

**Delete Section:**
```
DELETE /api/templates/{templateId}/sections/{sectionId}

Validation:
IF section has fields:
  Show modal:
  "This section has X fields. Deleting will remove:
   - All fields in this section
   - All validation rules
   - All conditional logic
   - All metric mappings
   Continue?"
  Actions: [Delete Anyway] [Cancel]

Database Operations:
1. DELETE FROM FormItemMetricMappings WHERE ItemId IN (section fields)
2. DELETE FROM FormItemValidations WHERE ItemId IN (section fields)
3. DELETE FROM FormItemOptions WHERE ItemId IN (section fields)
4. DELETE FROM FormTemplateItems WHERE SectionId = @SectionId
5. DELETE FROM FormTemplateSections WHERE SectionId = @SectionId
6. Reorder remaining sections (close gaps in DisplayOrder)

Reorder Logic:
UPDATE FormTemplateSections
SET DisplayOrder = ROW_NUMBER() OVER (ORDER BY DisplayOrder)
WHERE TemplateId = @TemplateId
```

**Reorder Sections (Drag-and-Drop):**
```
PUT /api/templates/{templateId}/sections/reorder
Body: [
  { sectionId: 1, displayOrder: 2 },
  { sectionId: 2, displayOrder: 1 },
  { sectionId: 3, displayOrder: 3 }
]

Logic:
1. Validate all sectionIds belong to template
2. Validate displayOrders are sequential (1, 2, 3...)
3. UPDATE each section's DisplayOrder
4. No page refresh needed (update UI directly)
```

### Validation Rules

**Pre-Publish Validation:**
```
- Minimum 1 section required
- Each section should have at least 1 field (warning, not blocker)
- Section names must be unique within template
- DisplayOrder must be sequential with no gaps
```

---

## 4. Field Builder Wizard

**Component Type:** 3-Step Horizontal Wizard in Modal

**Reusable Components:**
- **Horizontal Wizard** - Step indicator and navigation
- **Standard Forms** - Within each step
- **Dynamic Form Rows** - For dropdown options
- **Conditional Display** - Show/hide based on data type

### Step 1: Field Basics

**Item Name*** (The question shown to users)
```
Type: Text input
Max Length: 200 characters
Required: Yes
Example: "How many computers are operational?"
Help Text: "This is the question users will answer"
```

**Item Code*** (Internal identifier)
```
Type: Text input + Auto-generate button
Max Length: 50 characters
Pattern: ^[A-Z0-9_]+$
Required: Yes
Auto-Format: {SectionCode}_{Sequence}
Examples:
- Section 1, Field 1: SEC1_001
- Section 2, Field 15: SEC2_015

Auto-Generate Logic:
SectionCode = "SEC" + SectionDisplayOrder
Sequence = (MAX(sequence in section) + 1).ToString("D3")
```

**Data Type***
```
Dropdown Options:
1. Text - Single line text (max 255 chars)
2. TextArea - Multi-line text (max 2000 chars)
3. Number - Decimal number with min/max
4. Date - Calendar picker
5. Boolean - Yes/No, checkbox, or toggle
6. Dropdown - Single-select from options
7. FileUpload - File attachments

On Change: Step 2 configuration changes based on type
```

**Is Required***
```
Type: Checkbox
Default: false
Help Text: "If checked, users must provide a value"
```

**Display Order**
```
Type: Number input
Auto-assigned: MAX(DisplayOrder in section) + 1
Can override: Yes
Help Text: "Order field appears in section (drag-and-drop available)"
```

**Placeholder Text**
```
Type: Text input
Max Length: 100 characters
Optional: Yes
Example: "Enter number of computers..."
Help Text: "Hint text shown in empty field"
```

**Help Text**
```
Type: Textarea
Max Length: 300 characters
Optional: Yes
Example: "Count only functional workstations, not servers"
Help Text: "Additional guidance shown as tooltip"
```

---

### Step 2: Type-Specific Configuration

**IF DataType = 'Text':**
```
Min Length (number, optional, default: 0)
Max Length (number, optional, default: 255)
Default Value (text, optional)
```

**IF DataType = 'TextArea':**
```
Min Length (number, optional, default: 0)
Max Length (number, optional, default: 2000)
Rows (number, default: 4, range: 2-10)
Default Value (text, optional)
```

**IF DataType = 'Number':**
```
Min Value (decimal, optional)
Max Value (decimal, optional)
Decimal Places (dropdown: 0, 1, 2, 3, 4)
Unit (text, optional)
  Examples: "KSh", "Hours", "%", "GB", "Units"
  Displayed after input: [_______] Hours
Default Value (number, optional)

Validation:
IF Min and Max provided: Max must be > Min
```

**IF DataType = 'Date':**
```
Min Date (dropdown or custom):
  Options:
  - None (no minimum)
  - Today
  - First day of current month
  - First day of current year
  - Custom date

Max Date (dropdown or custom):
  Options:
  - None (no maximum)
  - Today
  - Last day of current month
  - Last day of current year
  - Custom date

Date Format (dropdown):
  - dd/MM/yyyy (31/12/2025)
  - MM/dd/yyyy (12/31/2025)
  - yyyy-MM-dd (2025-12-31)

Default Value (dropdown):
  - None
  - Today
  - First of month
  - Custom date
```

**IF DataType = 'Boolean':**
```
Display Style (dropdown):
  - Checkbox (traditional)
  - Toggle Switch (modern, mobile-friendly)
  - Yes/No Buttons (radio buttons)

True Label (text, default: "Yes")
False Label (text, default: "No")

Default Value (dropdown):
  - None (null, no selection)
  - True
  - False
```

**IF DataType = 'Dropdown':**
```
Add Options (Dynamic Rows):

Minimum 2 options required

┌─ Option 1 ───────────────────────────────────
│ Value*: yes
│   (Internal value, lowercase, no spaces)
│   Used in: database storage, conditional logic, API
│ 
│ Label*: Yes
│   (Displayed to users)
│ 
│ Display Order: 1 (auto-assigned, drag to reorder)
│ 
│ Is Default: [x] (Only one option can be default)
│ 
│ [Remove Option]
└───────────────────────────────────────────────

┌─ Option 2 ───────────────────────────────────
│ Value*: no
│ Label*: No
│ Display Order: 2
│ Is Default: [ ]
│ [Remove Option]
└───────────────────────────────────────────────

[+ Add Option]

Validation:
- Minimum 2 options
- Option values must be unique within field
- At most 1 option can have Is Default = true
- Option Value: alphanumeric + underscore only, no spaces
```

**IF DataType = 'FileUpload':**
```
Allowed Extensions* (multi-select checkboxes):
  □ .pdf (Documents)
  □ .jpg, .jpeg, .png (Images)
  □ .xlsx, .xls (Excel files)
  □ .docx, .doc (Word documents)
  □ .zip (Compressed files)

Max File Size (dropdown):
  - 1 MB
  - 5 MB (recommended)
  - 10 MB
  - 25 MB

Allow Multiple Files (checkbox, default: false)

Max Files (number, enabled if Allow Multiple = true)
  Min: 1, Max: 10, Default: 3
```

---

### Step 3: Advanced Settings

**Pre-fill Source** (Optional, auto-populate field value)
```
Dropdown Options:
1. None (manual entry by user)
2. Hardware Asset (link to HardwareAssets table)
3. Software Asset (link to SoftwareAssets table)
4. Tenant Property (link to Tenants table)

IF "Hardware Asset" selected:
  Map to Field (dropdown):
  - Asset Tag
  - Asset Name
  - Serial Number
  - Model
  - Manufacturer
  
  Data Load Logic:
  - Filter by submission's TenantId
  - Display as dropdown for user selection
  - If no assets found, show empty field with warning

IF "Software Asset" selected:
  Map to Field (dropdown):
  - Software Name
  - License Key
  - Version
  - Vendor
  
  Data Load Logic:
  - Filter by submission's TenantId
  - Display as dropdown for user selection

IF "Tenant Property" selected:
  Map to Field (dropdown):
  - Tenant Name
  - Tenant Code
  - Address
  - Phone
  - Email
  - Region Name
  
  Data Load Logic:
  - Use submission's TenantId
  - Auto-fill (read-only field)
```

**Additional Configuration Links:**
```
[Configure Validation Rules →] Opens Component 5
[Configure Conditional Logic →] Opens Component 6

Note: Metric Mapping done later in Component 7 (after field creation)
```

---

### Save Field Logic

```
POST /api/templates/{templateId}/sections/{sectionId}/fields
Body: {
  // Step 1
  itemName, itemCode, dataType, isRequired,
  displayOrder, placeholderText, helpText,
  
  // Step 2 (type-specific)
  minValue, maxValue, decimalPlaces, unit,        // Number
  minDate, maxDate, dateFormat,                    // Date
  minLength, maxLength, rows,                      // Text/TextArea
  displayStyle, trueLabel, falseLabel,             // Boolean
  allowedExtensions, maxFileSize, allowMultiple,   // FileUpload
  
  // Step 3
  preFillSource, preFillMapping,
  defaultValue
}

Database Operations:
1. INSERT FormTemplateItems
2. IF DataType = 'Dropdown':
     POST /api/templates/{templateId}/fields/{itemId}/options (bulk)
     Body: [
       { optionValue, optionLabel, displayOrder, isDefault },
       ...
     ]
3. Return ItemId
4. Close wizard modal
5. Refresh Section Builder, show new field in section

Success Message:
"Field '{ItemName}' added successfully!"
```

---

## 5. Validation Rules Builder

**Component Type:** Modal with Dynamic Form Rows

**Reusable Components:**
- **Modal** - Full validation editor
- **Dynamic Form Rows** - Add/remove rules
- **Conditional Fields** - Show parameters based on type

### Validation Types & Parameters

| Type | Applies To | Parameters | Example |
|------|------------|------------|---------|
| Required | All | None | "This field is required" |
| MinLength | Text, TextArea | MinLength (int) | "Minimum 5 characters" |
| MaxLength | Text, TextArea | MaxLength (int) | "Maximum 100 characters" |
| MinValue | Number | MinValue (decimal) | "Must be at least 0" |
| MaxValue | Number | MaxValue (decimal) | "Cannot exceed 1000" |
| Range | Number | MinValue, MaxValue | "Must be between 0-100" |
| RegexPattern | Text, TextArea | Pattern, Description | "Must be valid email" |
| DateRange | Date | MinDate, MaxDate | "Date must be in 2025" |
| FileSize | FileUpload | MaxSizeMB | "File under 5MB" |
| FileExtension | FileUpload | AllowedExtensions | "Only PDF allowed" |

### Interface

```
┌─ Validation Rules for: "Number of Computers" ────────────────┐
│                                                                │
│  Existing Rules (checked in order):                           │
│                                                                │
│  [1] Required                                                  │
│      Error Message: "This field is required"                  │
│      Severity: Error                         [Edit] [Delete]  │
│                                                                │
│  [2] MinValue = 0                                              │
│      Error Message: "Number cannot be negative"              │
│      Severity: Error                         [Edit] [Delete]  │
│                                                                │
│  [3] MaxValue = 500                                            │
│      Error Message: "Exceeds reasonable factory limit"       │
│      Severity: Warning                       [Edit] [Delete]  │
│      ℹ️ Warnings allow submission but show alert              │
│                                                                │
│  [+ Add Validation Rule]                                       │
│                                                                │
│  [Save All Rules] [Close]                                      │
└────────────────────────────────────────────────────────────────┘
```

### Add/Edit Rule Form

```
Validation Type*: [Dropdown based on field's DataType]

// IF Type = Range
Min Value*: [______] (numeric input)
Max Value*: [______] (numeric input)

// IF Type = RegexPattern
Pattern*: [_______________________________________]
Description: [____________________________________]

[Use Template ▼] Quick regex patterns:
  - Email: ^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$
  - Kenya Phone: ^(\+254|0)[17]\d{8}$
  - Alphanumeric: ^[a-zA-Z0-9]+$
  - Alphanumeric + Spaces: ^[a-zA-Z0-9\s]+$
  - IP Address: ^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$
  - URL: ^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\...
  - Custom (manual entry)

Error Message*: [_______________________________________]
  Variables available: {min}, {max}, {field}, {value}, {length}
  Example: "Value must be between {min} and {max}"

Severity*: [Dropdown]
  - Error (blocks submission, red message, stops validation chain)
  - Warning (shows alert, allows submission, yellow message)
  - Info (tooltip only, blue icon, doesn't block)

Validation Order: [__] (auto-assigned, drag to reorder)

[Save Rule] [Cancel]
```

### Execution Logic

**Validation Order:**
```
Rules checked sequentially during form submission:

1. Check Rule 1 (Required) → Pass, continue
2. Check Rule 2 (MinValue) → Pass, continue
3. Check Rule 3 (MaxValue) → Fail, Severity=Error
   → Stop validation chain
   → Show error message
   → Block submission
   → Rule 4 not checked

IF Severity = Warning:
  → Show warning message (yellow alert)
  → Allow user to continue
  → Log warning in submission audit trail
  → Continue to next rule

IF Severity = Info:
  → Show as tooltip/help icon
  → No blocking or warning
  → Continue to next rule
```

**Storage:**
```
Table: FormItemValidations
Fields:
- ValidationId (PK)
- ItemId (FK to FormTemplateItems)
- ValidationType (nvarchar: Required, MinValue, MaxValue, etc.)
- MinValue (decimal, nullable)
- MaxValue (decimal, nullable)
- MinLength (int, nullable)
- MaxLength (int, nullable)
- RegexPattern (nvarchar, nullable)
- ErrorMessage (nvarchar, required, max 500)
- Severity (nvarchar: Error, Warning, Info)
- ValidationOrder (int)
- IsActive (bit, default 1)
- CreatedDate, CreatedBy

API:
POST   /api/templates/{templateId}/fields/{itemId}/validations
PUT    /api/templates/{templateId}/validations/{validationId}
DELETE /api/templates/{templateId}/validations/{validationId}
PUT    /api/templates/{templateId}/fields/{itemId}/validations/reorder
```

---

## 6. Conditional Logic Builder

**Component Type:** Modal with Visual Rule Builder

**Reusable Components:**
- **Modal** - Logic editor
- **Visual Rule Builder** - Drag-and-drop conditions
- **Dropdown Cascading** - Field → Operator → Value

### Supported Operators

| Operator | DataTypes | Description | Example |
|----------|-----------|-------------|---------|
| equals | All | Exact match | "Yes" = "Yes" |
| not_equals | All | Not equal | "No" ≠ "Yes" |
| contains | Text, TextArea | Substring match | "Error" in "Error Code 404" |
| not_contains | Text, TextArea | No substring | "Success" not in "Error" |
| greater_than | Number, Date | Value > | 100 > 50 |
| greater_than_or_equal | Number, Date | Value >= | 100 >= 100 |
| less_than | Number, Date | Value < | 50 < 100 |
| less_than_or_equal | Number, Date | Value <= | 50 <= 50 |
| is_empty | All | No value | field is null or "" |
| not_empty | All | Has value | field has data |
| in | Dropdown | Value in list | "red" in ["red", "blue"] |
| not_in | Dropdown | Not in list | "green" not in ["red", "blue"] |

### Actions

- **show** - Make field visible (rendered in DOM)
- **hide** - Make field invisible (not rendered, skipped in validation)
- **enable** - Make field editable (remove disabled attribute)
- **disable** - Make field read-only (grayed out, still visible)

### Interface

```
┌─ Conditional Logic for: "LAN Issue Details" ──────────────────┐
│                                                                 │
│  Show this field when:                                          │
│                                                                 │
│  ┌─ Condition 1 ──────────────────────────────────────────┐   │
│  │ Source Field: [Dropdown: All fields in template]      │   │
│  │   → Selected: "Is LAN working?" (SEC2_015)            │   │
│  │                                                         │   │
│  │ Operator: [Dropdown: Based on source field type]       │   │
│  │   → Selected: equals                                   │   │
│  │                                                         │   │
│  │ Value: [Dropdown: Options from source field]           │   │
│  │   → Selected: "No"                                     │   │
│  │                                                         │   │
│  │                                               [Remove]  │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  Logic Operator: [Dropdown: AND / OR]                          │
│                                                                 │
│  ┌─ Condition 2 ──────────────────────────────────────────┐   │
│  │ Source Field: [Dropdown]                               │   │
│  │ Operator: [Dropdown]                                    │   │
│  │ Value: [Input based on operator]                        │   │
│  │                                               [Remove]  │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  [+ Add Condition]                                              │
│                                                                 │
│  Action: [Dropdown: Show / Hide / Enable / Disable]            │
│    → Selected: Show                                             │
│                                                                 │
│  [Test Logic with Sample Data] [Save] [Cancel]                 │
└─────────────────────────────────────────────────────────────────┘
```

### JSON Structure (Stored in DB)

```json
{
  "action": "show",
  "logicOperator": "AND",
  "rules": [
    {
      "sourceItemId": 15,
      "sourceItemCode": "SEC2_015",
      "operator": "equals",
      "value": "No"
    },
    {
      "sourceItemId": 16,
      "sourceItemCode": "SEC2_016",
      "operator": "not_empty",
      "value": null
    }
  ]
}
```

### Dependency Validation Rules

```
1. Can only reference fields in SAME template
   → Cannot reference fields from other templates

2. Can only reference fields in PREVIOUS sections or SAME section
   → Section 1 field cannot depend on Section 3 field
   → Section 3 field CAN depend on Section 1 field
   → Fields in same section can depend on each other (if DisplayOrder allows)

3. Cannot reference self
   → Field cannot have conditional logic based on its own value

4. Cannot create circular dependencies
   → Field A depends on Field B
   → Field B cannot depend on Field A

Validation on Save:
- Check all sourceItemIds exist in template
- Check sourceItem DisplayOrder < targetItem DisplayOrder (in same section)
- Check no circular chains
- Build dependency graph, detect cycles
```

### Client-Side Evaluation (JavaScript)

```javascript
// On page load: Evaluate all conditional logic
function initializeConditionalLogic() {
    const fieldsWithLogic = fields.filter(f => f.conditionalLogic);
    
    fieldsWithLogic.forEach(field => {
        const shouldApply = evaluateLogic(field.conditionalLogic);
        applyAction(field.itemId, field.conditionalLogic.action, shouldApply);
    });
}

// On field value change: Re-evaluate dependent fields
function onFieldChange(changedItemId, newValue) {
    // Find fields that depend on this changed field
    const dependentFields = fields.filter(f => 
        f.conditionalLogic?.rules.some(r => r.sourceItemId === changedItemId)
    );
    
    dependentFields.forEach(field => {
        const shouldApply = evaluateLogic(field.conditionalLogic);
        applyAction(field.itemId, field.conditionalLogic.action, shouldApply);
    });
}

// Evaluate logic rules (AND/OR)
function evaluateLogic(logic) {
    const results = logic.rules.map(rule => {
        const sourceValue = getFieldValue(rule.sourceItemId);
        return checkCondition(sourceValue, rule.operator, rule.value);
    });
    
    if (logic.logicOperator === 'AND') {
        return results.every(r => r === true);
    } else {
        return results.some(r => r === true);
    }
}

// Check individual condition
function checkCondition(sourceValue, operator, expectedValue) {
    switch(operator) {
        case 'equals':
            return sourceValue == expectedValue;
        case 'not_equals':
            return sourceValue != expectedValue;
        case 'contains':
            return sourceValue && sourceValue.includes(expectedValue);
        case 'greater_than':
            return parseFloat(sourceValue) > parseFloat(expectedValue);
        case 'is_empty':
            return !sourceValue || sourceValue === '';
        case 'not_empty':
            return sourceValue && sourceValue !== '';
        // ... other operators
        default:
            return false;
    }
}

// Apply action to field
function applyAction(itemId, action, shouldApply) {
    const fieldElement = document.getElementById(`field_${itemId}`);
    const inputElement = fieldElement.querySelector('input, select, textarea');
    
    if (action === 'show') {
        fieldElement.style.display = shouldApply ? 'block' : 'none';
        if (!shouldApply) inputElement.required = false; // Skip validation if hidden
    } else if (action === 'hide') {
        fieldElement.style.display = shouldApply ? 'none' : 'block';
    } else if (action === 'enable') {
        inputElement.disabled = !shouldApply;
    } else if (action === 'disable') {
        inputElement.disabled = shouldApply;
    }
}
```

### Test Logic Feature

```
[Test Logic with Sample Data] button:

Opens mini-modal:
┌─ Test Conditional Logic ─────────────────────┐
│ Enter sample values for source fields:        │
│                                                │
│ Is LAN working?: [Dropdown: Yes / No]        │
│   → Select: No                                │
│                                                │
│ [Run Test]                                     │
│                                                │
│ Result:                                        │
│ ✓ Field "LAN Issue Details" will be SHOWN    │
│                                                │
│ [Close]                                        │
└────────────────────────────────────────────────┘
```

### Storage

```
Table: FormTemplateItems
Column: ConditionalLogic (nvarchar(MAX), JSON format)

Update API:
PUT /api/templates/{templateId}/fields/{itemId}/conditional-logic
Body: { action, logicOperator, rules: [...] }

Delete Logic:
PUT /api/templates/{templateId}/fields/{itemId}/conditional-logic
Body: null
```

---

**Next Steps:** After completing structure, proceed to Metric Mapping, Preview, and Publishing (Components 7-9)
