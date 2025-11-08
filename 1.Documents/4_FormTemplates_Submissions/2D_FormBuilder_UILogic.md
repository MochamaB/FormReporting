# Form Builder & Rendering UI Logic Documentation

**Document:** 2D_FormBuilder_UILogic.md
**Location:** 1.Documents/4_FormTemplates_Submissions/
**Related:** Complements 2B_FormBuilder_Structure.md
**Purpose:** Define UI component architecture for form builder interface and form rendering system

---

## Table of Contents
1. [System Architecture Analysis](#system-architecture-analysis)
2. [Component Set A: Form Builder Components (Admin Interface)](#component-set-a-form-builder-components-admin-interface)
3. [Component Set B: Form Rendering Components (User Interface)](#component-set-b-form-rendering-components-user-interface)
4. [Implementation Considerations](#implementation-considerations)
5. [Recommended Implementation Order](#recommended-implementation-order)

---

## System Architecture Analysis

### Form Data Model Structure (3-Tier Architecture)

The system uses a sophisticated **Entity-Attribute-Value (EAV)** pattern for dynamic form building:

#### **Tier 1: Form Definition Layer**

| Entity | Purpose | Key Features |
|--------|---------|--------------|
| `FormTemplate` | Main form container | Name, type, version, publish status (Draft/Published/Archived), workflow integration |
| `FormTemplateSection` | Groups fields into sections/pages | Collapsible, icons, display order, section routing |
| `FormTemplateItem` | Individual form fields | Rich configuration, conditional logic, matrix layouts, library integration |

#### **Tier 2: Field Configuration Layer**

| Entity | Purpose | Storage Format |
|--------|---------|----------------|
| `FormItemValidation` | Validation rules | Required, Email, Range, Regex, MinLength, MaxLength, Custom |
| `FormItemOption` | Dropdown/Radio/Checkbox options | Value, Label, DisplayOrder, IsDefault, ParentOptionId (for cascading) |
| `FormItemConfiguration` | Field-specific settings | Key-Value pairs (minValue, maxValue, allowedFileTypes, ratingMax) |
| `FormItemCalculation` | Calculated fields | Formula definitions for auto-calculations |
| `FormItemMetricMapping` | Metric integration | Maps form fields to system metrics (TenantMetric table) |

#### **Tier 3: Response/Submission Layer**

| Entity | Purpose | Data Storage |
|--------|---------|--------------|
| `FormTemplateSubmission` | Form submission instance | Workflow status, approvals, timestamps |
| `FormTemplateResponse` | EAV response storage | TextValue, NumericValue, DateValue, BooleanValue (flexible data capture) |

### Supported Field Types

From `FormFieldType` enum (Models/Common/Enums.cs):

```csharp
public enum FormFieldType
{
    Text = 1,           // Single-line text input
    TextArea = 2,       // Multi-line text input
    Number = 3,         // Numeric input
    Date = 4,           // Date picker
    DateTime = 5,       // Date and time picker
    Email = 6,          // Email input with validation
    Phone = 7,          // Phone number input
    Dropdown = 8,       // Single-select dropdown
    Radio = 9,          // Radio button group
    Checkbox = 10,      // Checkbox input
    MultiSelect = 11,   // Multi-select dropdown
    FileUpload = 12,    // File upload control
    Url = 13,           // URL input with validation
    Currency = 14,      // Currency input with formatting
    Percentage = 15     // Percentage input
}
```

### Advanced Features in the System

#### **Field-Level Features**

| Feature | Implementation | Example Use Case |
|---------|---------------|------------------|
| **Conditional Logic** | JSON in `FormTemplateItem.ConditionalLogic` | Show "Other Reason" field only if "Reason" = "Other" |
| **Matrix/Grid Layouts** | `MatrixGroupId`, `MatrixRowLabel` | Rate multiple items on same scale (Quality, Timeliness, Cost) |
| **Prefix/Suffix** | `PrefixText`, `SuffixText` | Currency symbols ("KES"), units ("%", "kg", "hrs") |
| **Help Text & Placeholders** | `HelpText`, `PlaceholderText` | User guidance and example values |
| **Field Library Integration** | `LibraryFieldId`, `IsLibraryOverride` | Reusable fields across multiple forms |
| **Default Values** | `DefaultValue` | Pre-populate fields with common values |
| **Cascading Dropdowns** | `FormItemOption.ParentOptionId` | Region → Factory → Department hierarchy |

#### **Section-Level Features**

| Feature | Implementation | Purpose |
|---------|---------------|---------|
| **Collapsible Sections** | `IsCollapsible`, `IsCollapsedByDefault` | Long forms, progressive disclosure |
| **Section Icons** | `IconClass` (Remix Icons) | Visual categorization (ri-computer-line, ri-network-line) |
| **Display Ordering** | `DisplayOrder` | Control section sequence |
| **Section Routing** | `SectionRouting` table | Conditional section branching |

#### **Form-Level Features**

| Feature | Purpose |
|---------|---------|
| **Multi-Version Support** | Track form template versions over time |
| **Publish Workflow** | Draft → Published → Archived status management |
| **Approval Workflows** | `RequiresApproval`, `WorkflowId` integration |
| **Multi-Tenant Assignments** | `FormTemplateAssignment` for tenant-specific forms |
| **Analytics** | `FormAnalytics` table for form performance tracking |

---

## Component Set A: Form Builder Components (Admin Interface)

### **Purpose**
Enable ICT administrators to visually create, configure, and manage form templates without writing code.

### **Target Users**
- ICT System Administrators
- Head Office Form Designers
- Regional Form Coordinators (with permissions)

### **Core Components**

#### **1. FormBuilderCanvas**
Main drag-and-drop canvas for form design.

**Features:**
- Section management (add, reorder, delete)
- Field drag-and-drop from palette
- Visual field reordering within sections
- Real-time preview toggle
- Undo/Redo functionality
- Auto-save to draft

**Technical Requirements:**
- SortableJS or similar drag-and-drop library
- State management (Vue.js/Alpine.js/React)
- WebSocket for collaborative editing (future)

**Data Binding:**
```
UI State ↔ FormTemplate + FormTemplateSection + FormTemplateItem
```

#### **2. SectionBuilder**
Visual editor for section configuration.

**Properties Panel:**
- Section Name (text input)
- Section Description (textarea)
- Icon Selector (icon picker with Remix Icons)
- Display Order (numeric)
- Collapsible (checkbox)
- Collapsed By Default (checkbox)

**Actions:**
- Add New Section
- Duplicate Section
- Delete Section
- Configure Section Routing (conditional branching)

**Database Mapping:**
```csharp
FormTemplateSection {
    SectionName,
    SectionDescription,
    IconClass,
    DisplayOrder,
    IsCollapsible,
    IsCollapsedByDefault
}
```

#### **3. FieldPalette**
Sidebar with draggable field types.

**Field Categories:**
- **Text Inputs:** Text, TextArea, Email, Phone, Url
- **Numeric:** Number, Currency, Percentage
- **Date/Time:** Date, DateTime
- **Choice:** Dropdown, Radio, Checkbox, MultiSelect
- **File:** FileUpload
- **Advanced:** Calculated Field, Conditional Field

**UI Layout:**
```
┌─────────────────────┐
│  Field Palette      │
├─────────────────────┤
│ [Text] Text Field   │
│ [123] Number Field  │
│ [▼] Dropdown        │
│ [○] Radio Buttons   │
│ [☑] Checkbox        │
│ [📅] Date Picker    │
│ [📎] File Upload    │
│ [∑] Calculated      │
└─────────────────────┘
```

**Interaction:**
```
Drag field type → Drop on canvas/section → Properties panel opens
```

#### **4. FieldPropertiesPanel**
Right sidebar for configuring selected field.

**Tabs:**

**Tab 1: Basic Properties**
```
- Field Name (required)
- Field Code (auto-generated, editable)
- Field Type (read-only, set on creation)
- Description (textarea)
- Display Order (numeric)
- Is Required (checkbox)
- Default Value (varies by type)
- Placeholder Text
- Help Text (shown below field)
```

**Tab 2: Display Options**
```
- Prefix Text (e.g., "KES", "$")
- Suffix Text (e.g., "%", "kg")
- Layout Type (Single, Matrix, Grid, Inline)
- Matrix Group ID (for grid layouts)
- Matrix Row Label
```

**Tab 3: Validation Rules**
```
For each validation type:
┌──────────────────────────────────┐
│ ☑ Required                       │
│ ☐ Email Format                   │
│ ☑ Min Length: [5]     Max: [100] │
│ ☐ Min Value:  [___]   Max: [___] │
│ ☐ Regex Pattern: [____________]  │
│   Error Message: [_____________] │
└──────────────────────────────────┘

Add Validation Rule [+]
```

**Tab 4: Options** (for Dropdown/Radio/Checkbox/MultiSelect)
```
Options List:
┌─────────────────────────────────────────┐
│ Label          | Value       | Default  │
├─────────────────────────────────────────┤
│ Option 1  [x]  | option_1    | ○        │
│ Option 2  [x]  | option_2    | ●        │
│ Option 3  [x]  | option_3    | ○        │
└─────────────────────────────────────────┘

[+ Add Option] [Import from List] [Cascading Setup]
```

**Tab 5: Conditional Logic**
```
Show this field when:
┌─────────────────────────────────────────┐
│ [Field Name ▼] [equals ▼] [Value]      │
│                           [+ Add Rule]  │
└─────────────────────────────────────────┘

Action: [Show ▼] / Hide
Logic: [All rules must match ▼] / Any rule matches
```

**Tab 6: Calculations** (for calculated fields)
```
Formula:
┌─────────────────────────────────────────┐
│ [Field1] + [Field2] * 1.16              │
└─────────────────────────────────────────┘

Insert Field: [Dropdown of numeric fields ▼]
Functions: [SUM] [AVG] [MIN] [MAX]
```

**Tab 7: Metric Mapping**
```
Map to System Metric:
[Metric Dropdown ▼] None, Server Uptime, Response Time, etc.

Aggregation: [Sum ▼] / Average / Count / Latest
```

**Database Mapping:**
All tabs map to:
```
FormTemplateItem
FormItemValidation (multiple)
FormItemOption (multiple)
FormItemConfiguration (multiple)
FormItemCalculation
FormItemMetricMapping
```

#### **5. FieldLibraryBrowser**
Modal for selecting pre-configured reusable fields.

**Features:**
- Search and filter library fields
- Preview field configuration
- Category filtering (Common, HR, ICT, Finance)
- One-click insertion
- Option to override library defaults

**UI Layout:**
```
┌────────────────────────────────────────────┐
│  Field Library                      [X]     │
├────────────────────────────────────────────┤
│ Search: [___________]  Category: [All ▼]   │
├────────────────────────────────────────────┤
│                                             │
│  ☐ Employee Name (Text)                    │
│     Common field for employee names        │
│                                             │
│  ☐ Department (Dropdown)                   │
│     Standard department list               │
│                                             │
│  ☐ Budget Amount (Currency)                │
│     Currency field with KES prefix         │
│                                             │
└────────────────────────────────────────────┘
              [Insert Selected]
```

**Database Query:**
```sql
SELECT * FROM FieldLibrary
WHERE IsActive = 1
  AND (Category = @category OR @category IS NULL)
  AND FieldName LIKE '%' + @search + '%'
ORDER BY FieldName
```

#### **6. FormPreview**
Live preview of how users will see the form.

**Features:**
- Real-time updates as fields are configured
- Toggle between desktop/tablet/mobile views
- Test validation rules
- Test conditional logic
- Test calculations

**Implementation:**
- Render using Form Rendering Components (Set B)
- Read-only mode with test data
- Side-by-side or overlay view

**UI Layout:**
```
┌──────────────────────────────────────────┐
│  Preview                 [Desktop ▼] [X] │
├──────────────────────────────────────────┤
│                                           │
│  [Form renders here using Set B]         │
│                                           │
└──────────────────────────────────────────┘
```

### **Component Set A Data Flow**

```
┌─────────────────────────────────────────────────────────┐
│                     Form Builder UI                      │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌────────────┐  ┌──────────────┐  ┌────────────────┐  │
│  │   Field    │  │ Form Builder │  │   Properties   │  │
│  │  Palette   │→ │    Canvas    │← │     Panel      │  │
│  └────────────┘  └──────────────┘  └────────────────┘  │
│                          ↓                               │
│                  ┌──────────────┐                       │
│                  │ Form Preview │                       │
│                  └──────────────┘                       │
│                          ↓                               │
└─────────────────────────────────────────────────────────┘
                           ↓
              [Save/Publish Button Click]
                           ↓
┌─────────────────────────────────────────────────────────┐
│                  Controller Action                       │
│  public async Task<IActionResult> SaveFormTemplate()    │
└─────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────┐
│                   Database Layer                         │
├─────────────────────────────────────────────────────────┤
│  FormTemplate (insert/update)                            │
│    ↓                                                     │
│  FormTemplateSection (bulk insert/update)                │
│    ↓                                                     │
│  FormTemplateItem (bulk insert/update)                   │
│    ↓                                                     │
│  FormItemValidation (bulk insert)                        │
│  FormItemOption (bulk insert)                            │
│  FormItemConfiguration (bulk insert)                     │
│  FormItemCalculation (bulk insert)                       │
│  FormItemMetricMapping (bulk insert)                     │
└─────────────────────────────────────────────────────────┘
```

### **Recommended Technologies for Component Set A**

| Layer | Technology | Purpose |
|-------|------------|---------|
| **Frontend Framework** | Vue.js 3 or Alpine.js | Reactive state management, component composition |
| **Drag & Drop** | SortableJS or Vue.Draggable | Field/section reordering |
| **Form State** | Pinia (Vue) or Simple JS Object | Manage form builder state |
| **Icon Library** | Remix Icons | Consistent with Velzon theme |
| **Rich Text Editor** | TinyMCE or Quill | For description/help text fields |
| **Validation** | Joi or Yup | Client-side validation before save |
| **API Communication** | Axios or Fetch | AJAX save operations |

---

## Component Set B: Form Rendering Components (User Interface)

### **Purpose**
Display published form templates to end users (factory staff, managers, regional coordinators) for data entry and submission.

### **Target Users**
- Factory Users (production data entry)
- Regional Managers (report submission)
- Head Office Staff (consolidated reporting)
- External Users (if forms are made public)

### **Architecture: Three-Layer Pattern**

Following the established pattern from StatCards and DataTable components:

```
Layer 1: ViewModels (Configuration → ViewModel)
         ↓
Layer 2: Extensions (Transformation + Fluent API)
         ↓
Layer 3: Partial Views (Pure Velzon HTML Rendering)
```

### **Layer 1: ViewModels**

**File:** `Models/ViewModels/Components/FormViewModel.cs`

#### **Core Classes:**

```csharp
/// <summary>
/// Configuration object for form rendering
/// </summary>
public class FormConfig
{
    public string FormId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public List<FormSectionConfig> Sections { get; set; } = new();
    public string? SubmitUrl { get; set; }
    public string? SaveDraftUrl { get; set; }
    public bool EnableAutoSave { get; set; } = false;
    public int AutoSaveIntervalSeconds { get; set; } = 30;
    public bool ShowProgressBar { get; set; } = true;
    public string CssClass { get; set; } = "";
}

/// <summary>
/// Section configuration
/// </summary>
public class FormSectionConfig
{
    public int SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public string? SectionDescription { get; set; }
    public string? IconClass { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsCollapsible { get; set; } = true;
    public bool IsCollapsedByDefault { get; set; } = false;
    public List<FormFieldConfig> Fields { get; set; } = new();
}

/// <summary>
/// Individual field configuration
/// </summary>
public class FormFieldConfig
{
    public string FieldId { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string? FieldDescription { get; set; }
    public FormFieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public string? PlaceholderText { get; set; }
    public string? HelpText { get; set; }
    public string? PrefixText { get; set; }
    public string? SuffixText { get; set; }
    public int DisplayOrder { get; set; }

    // Options for dropdown/radio/checkbox
    public List<FormFieldOption> Options { get; set; } = new();

    // Validations
    public List<FormFieldValidation> Validations { get; set; } = new();

    // Conditional logic
    public ConditionalLogic? ConditionalLogic { get; set; }

    // Matrix layout
    public string LayoutType { get; set; } = "Single";
    public int? MatrixGroupId { get; set; }
    public string? MatrixRowLabel { get; set; }

    // Current value (for edit mode)
    public object? CurrentValue { get; set; }
}

/// <summary>
/// Field option (dropdown/radio/checkbox)
/// </summary>
public class FormFieldOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsSelected { get; set; }
    public int? ParentOptionId { get; set; } // For cascading
}

/// <summary>
/// Validation rule configuration
/// </summary>
public class FormFieldValidation
{
    public string ValidationType { get; set; } = string.Empty;
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string? RegexPattern { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string Severity { get; set; } = "Error";
}

/// <summary>
/// Conditional logic configuration
/// </summary>
public class ConditionalLogic
{
    public string Action { get; set; } = "show"; // show, hide, enable, disable
    public string Logic { get; set; } = "all"; // all, any
    public List<ConditionalRule> Rules { get; set; } = new();
}

public class ConditionalRule
{
    public string TargetFieldId { get; set; } = string.Empty;
    public string Operator { get; set; } = "equals"; // equals, notEquals, contains, greaterThan, lessThan
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Final ViewModel (for rendering)
/// </summary>
public class FormViewModel
{
    public string FormId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<FormSectionViewModel> Sections { get; set; } = new();
    public string? SubmitUrl { get; set; }
    public string? SaveDraftUrl { get; set; }
    public bool EnableAutoSave { get; set; }
    public int AutoSaveIntervalSeconds { get; set; }
    public bool ShowProgressBar { get; set; }
    public int TotalFields { get; set; }
    public int RequiredFields { get; set; }
    public string CssClass { get; set; } = string.Empty;
}

public class FormSectionViewModel
{
    public int SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public string? SectionDescription { get; set; }
    public string? IconClass { get; set; }
    public bool IsCollapsible { get; set; }
    public bool IsCollapsedByDefault { get; set; }
    public List<FormFieldViewModel> Fields { get; set; } = new();
    public string CollapseId { get; set; } = string.Empty; // For Bootstrap collapse
}

public class FormFieldViewModel
{
    public string FieldId { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string? FieldDescription { get; set; }
    public FormFieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public string? PlaceholderText { get; set; }
    public string? HelpText { get; set; }
    public string? PrefixText { get; set; }
    public string? SuffixText { get; set; }
    public List<FormFieldOption> Options { get; set; } = new();
    public List<FormFieldValidation> Validations { get; set; } = new();
    public ConditionalLogic? ConditionalLogic { get; set; }
    public string LayoutType { get; set; } = "Single";
    public int? MatrixGroupId { get; set; }
    public string? MatrixRowLabel { get; set; }
    public object? CurrentValue { get; set; }
    public string InputName { get; set; } = string.Empty; // For form POST
    public string InputId { get; set; } = string.Empty; // For HTML id attribute
}
```

### **Layer 2: Extensions**

**File:** `Extensions/FormExtensions.cs`

#### **Key Methods:**

```csharp
public static class FormExtensions
{
    /// <summary>
    /// Main transformation: FormConfig → FormViewModel
    /// </summary>
    public static FormViewModel BuildForm(this FormConfig config)
    {
        var viewModel = new FormViewModel
        {
            FormId = config.FormId,
            Title = config.Title,
            Description = config.Description,
            SubmitUrl = config.SubmitUrl,
            SaveDraftUrl = config.SaveDraftUrl,
            EnableAutoSave = config.EnableAutoSave,
            AutoSaveIntervalSeconds = config.AutoSaveIntervalSeconds,
            ShowProgressBar = config.ShowProgressBar,
            CssClass = config.CssClass
        };

        // Transform sections
        viewModel.Sections = config.Sections
            .OrderBy(s => s.DisplayOrder)
            .Select(s => TransformSection(s, config.FormId))
            .ToList();

        // Calculate totals
        viewModel.TotalFields = viewModel.Sections.Sum(s => s.Fields.Count);
        viewModel.RequiredFields = viewModel.Sections
            .SelectMany(s => s.Fields)
            .Count(f => f.IsRequired);

        return viewModel;
    }

    /// <summary>
    /// Fluent API: Add section to form
    /// </summary>
    public static FormConfig WithSection(
        this FormConfig config,
        string sectionName,
        string? sectionDescription = null,
        Action<FormSectionConfig>? configureSection = null)
    {
        var section = new FormSectionConfig
        {
            SectionId = config.Sections.Count + 1,
            SectionName = sectionName,
            SectionDescription = sectionDescription,
            DisplayOrder = config.Sections.Count + 1
        };

        configureSection?.Invoke(section);
        config.Sections.Add(section);
        return config;
    }

    /// <summary>
    /// Fluent API: Add field to section
    /// </summary>
    public static FormSectionConfig WithField(
        this FormSectionConfig section,
        FormFieldConfig field)
    {
        field.DisplayOrder = section.Fields.Count + 1;
        section.Fields.Add(field);
        return section;
    }

    /// <summary>
    /// Fluent API: Add validation to field
    /// </summary>
    public static FormFieldConfig WithValidation(
        this FormFieldConfig field,
        string validationType,
        string errorMessage,
        decimal? minValue = null,
        decimal? maxValue = null,
        int? minLength = null,
        int? maxLength = null,
        string? regexPattern = null)
    {
        field.Validations.Add(new FormFieldValidation
        {
            ValidationType = validationType,
            ErrorMessage = errorMessage,
            MinValue = minValue,
            MaxValue = maxValue,
            MinLength = minLength,
            MaxLength = maxLength,
            RegexPattern = regexPattern
        });
        return field;
    }

    /// <summary>
    /// Helper: Load form from database template
    /// </summary>
    public static async Task<FormConfig> LoadFromTemplate(
        int templateId,
        ApplicationDbContext context)
    {
        var template = await context.FormTemplates
            .Include(t => t.Sections)
            .ThenInclude(s => s.Items)
            .ThenInclude(i => i.Validations)
            .Include(t => t.Sections)
            .ThenInclude(s => s.Items)
            .ThenInclude(i => i.Options)
            .FirstOrDefaultAsync(t => t.TemplateId == templateId);

        if (template == null)
            throw new Exception($"Form template {templateId} not found");

        var formConfig = new FormConfig
        {
            FormId = template.TemplateId.ToString(),
            Title = template.TemplateName,
            Description = template.Description
        };

        foreach (var section in template.Sections.OrderBy(s => s.DisplayOrder))
        {
            formConfig.WithSection(section.SectionName, section.SectionDescription, sectionConfig =>
            {
                sectionConfig.IconClass = section.IconClass;
                sectionConfig.IsCollapsible = section.IsCollapsible;
                sectionConfig.IsCollapsedByDefault = section.IsCollapsedByDefault;

                foreach (var item in section.Items.OrderBy(i => i.DisplayOrder))
                {
                    var fieldConfig = new FormFieldConfig
                    {
                        FieldId = item.ItemId.ToString(),
                        FieldName = item.ItemName,
                        FieldDescription = item.ItemDescription,
                        FieldType = Enum.Parse<FormFieldType>(item.DataType ?? "Text"),
                        IsRequired = item.IsRequired,
                        DefaultValue = item.DefaultValue,
                        PlaceholderText = item.PlaceholderText,
                        HelpText = item.HelpText,
                        PrefixText = item.PrefixText,
                        SuffixText = item.SuffixText,
                        LayoutType = item.LayoutType,
                        MatrixGroupId = item.MatrixGroupId,
                        MatrixRowLabel = item.MatrixRowLabel
                    };

                    // Add options
                    fieldConfig.Options = item.Options
                        .OrderBy(o => o.DisplayOrder)
                        .Select(o => new FormFieldOption
                        {
                            Value = o.OptionValue,
                            Label = o.OptionLabel,
                            IsDefault = o.IsDefault
                        }).ToList();

                    // Add validations
                    fieldConfig.Validations = item.Validations
                        .Where(v => v.IsActive)
                        .OrderBy(v => v.ValidationOrder)
                        .Select(v => new FormFieldValidation
                        {
                            ValidationType = v.ValidationType,
                            MinValue = v.MinValue,
                            MaxValue = v.MaxValue,
                            MinLength = v.MinLength,
                            MaxLength = v.MaxLength,
                            RegexPattern = v.RegexPattern,
                            ErrorMessage = v.ErrorMessage,
                            Severity = v.Severity
                        }).ToList();

                    // Parse conditional logic
                    if (!string.IsNullOrEmpty(item.ConditionalLogic))
                    {
                        try
                        {
                            fieldConfig.ConditionalLogic =
                                JsonSerializer.Deserialize<ConditionalLogic>(item.ConditionalLogic);
                        }
                        catch { /* Invalid JSON, skip */ }
                    }

                    sectionConfig.WithField(fieldConfig);
                }
            });
        }

        return formConfig;
    }

    /// <summary>
    /// Helper: Validate form submission
    /// </summary>
    public static Dictionary<string, List<string>> ValidateFormSubmission(
        this FormConfig config,
        Dictionary<string, object> formData)
    {
        var errors = new Dictionary<string, List<string>>();

        foreach (var section in config.Sections)
        {
            foreach (var field in section.Fields)
            {
                var fieldErrors = new List<string>();
                var value = formData.GetValueOrDefault(field.FieldId);

                foreach (var validation in field.Validations)
                {
                    var isValid = validation.ValidationType switch
                    {
                        "Required" => value != null && !string.IsNullOrWhiteSpace(value.ToString()),
                        "Email" => IsValidEmail(value?.ToString()),
                        "MinLength" => value?.ToString()?.Length >= validation.MinLength,
                        "MaxLength" => value?.ToString()?.Length <= validation.MaxLength,
                        "MinValue" => decimal.TryParse(value?.ToString(), out var d) && d >= validation.MinValue,
                        "MaxValue" => decimal.TryParse(value?.ToString(), out var d2) && d2 <= validation.MaxValue,
                        "Regex" => Regex.IsMatch(value?.ToString() ?? "", validation.RegexPattern ?? ""),
                        _ => true
                    };

                    if (!isValid)
                        fieldErrors.Add(validation.ErrorMessage);
                }

                if (fieldErrors.Any())
                    errors[field.FieldId] = fieldErrors;
            }
        }

        return errors;
    }

    // Private helper methods
    private static FormSectionViewModel TransformSection(FormSectionConfig section, string formId)
    {
        return new FormSectionViewModel
        {
            SectionId = section.SectionId,
            SectionName = section.SectionName,
            SectionDescription = section.SectionDescription,
            IconClass = section.IconClass,
            IsCollapsible = section.IsCollapsible,
            IsCollapsedByDefault = section.IsCollapsedByDefault,
            CollapseId = $"{formId}_section_{section.SectionId}",
            Fields = section.Fields
                .OrderBy(f => f.DisplayOrder)
                .Select(f => TransformField(f, formId))
                .ToList()
        };
    }

    private static FormFieldViewModel TransformField(FormFieldConfig field, string formId)
    {
        return new FormFieldViewModel
        {
            FieldId = field.FieldId,
            FieldName = field.FieldName,
            FieldDescription = field.FieldDescription,
            FieldType = field.FieldType,
            IsRequired = field.IsRequired,
            DefaultValue = field.DefaultValue,
            PlaceholderText = field.PlaceholderText,
            HelpText = field.HelpText,
            PrefixText = field.PrefixText,
            SuffixText = field.SuffixText,
            Options = field.Options,
            Validations = field.Validations,
            ConditionalLogic = field.ConditionalLogic,
            LayoutType = field.LayoutType,
            MatrixGroupId = field.MatrixGroupId,
            MatrixRowLabel = field.MatrixRowLabel,
            CurrentValue = field.CurrentValue,
            InputName = $"field_{field.FieldId}",
            InputId = $"{formId}_field_{field.FieldId}"
        };
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
```

### **Layer 3: Partial Views**

**File Structure:**
```
Views/Shared/Components/Form/
├── _Form.cshtml                    (Main form container)
├── _FormSection.cshtml              (Section wrapper)
├── _FormField.cshtml                (Field router)
├── _FormFieldLabel.cshtml           (Reusable label component)
├── _FormFieldHelp.cshtml            (Help text component)
├── _FormFieldValidation.cshtml      (Validation message component)
│
├── Fields/
│   ├── _TextField.cshtml
│   ├── _TextAreaField.cshtml
│   ├── _NumberField.cshtml
│   ├── _DateField.cshtml
│   ├── _DateTimeField.cshtml
│   ├── _EmailField.cshtml
│   ├── _PhoneField.cshtml
│   ├── _UrlField.cshtml
│   ├── _DropdownField.cshtml
│   ├── _RadioField.cshtml
│   ├── _CheckboxField.cshtml
│   ├── _MultiSelectField.cshtml
│   ├── _FileUploadField.cshtml
│   ├── _CurrencyField.cshtml
│   └── _PercentageField.cshtml
```

#### **_Form.cshtml** (Main Container)

```cshtml
@model FormReporting.Models.ViewModels.Components.FormViewModel

@*
    Form Component - Main Container

    Features:
    - Multi-section forms
    - Progress bar
    - Auto-save capability
    - Submit/Save Draft actions
    - Velzon theme styling
*@

<div class="form-container @Model.CssClass" id="@Model.FormId">
    @* Form Header *@
    <div class="card">
        <div class="card-header bg-primary-subtle">
            <div class="d-flex justify-content-between align-items-center">
                <div>
                    <h4 class="card-title mb-0">@Model.Title</h4>
                    @if (!string.IsNullOrEmpty(Model.Description))
                    {
                        <p class="text-muted mb-0 mt-1">@Model.Description</p>
                    }
                </div>
                @if (Model.ShowProgressBar)
                {
                    <div class="text-end">
                        <span class="badge bg-info fs-12">
                            <span class="completed-fields">0</span> / @Model.TotalFields completed
                        </span>
                    </div>
                }
            </div>
        </div>

        @if (Model.ShowProgressBar)
        {
            <div class="progress" style="height: 3px;">
                <div class="progress-bar" role="progressbar" style="width: 0%" id="@Model.FormId-progress"></div>
            </div>
        }

        <div class="card-body">
            <form id="@Model.FormId-form" method="post" enctype="multipart/form-data">
                @* Render all sections *@
                @foreach (var section in Model.Sections)
                {
                    <partial name="~/Views/Shared/Components/Form/_FormSection.cshtml" model="section" />
                }

                @* Form Actions *@
                <div class="form-actions mt-4 pt-3 border-top">
                    <div class="d-flex justify-content-between">
                        <div>
                            @if (!string.IsNullOrEmpty(Model.SaveDraftUrl))
                            {
                                <button type="button" class="btn btn-soft-secondary" id="@Model.FormId-save-draft">
                                    <i class="ri-save-line"></i> Save as Draft
                                </button>
                            }
                        </div>
                        <div class="d-flex gap-2">
                            <button type="button" class="btn btn-light" onclick="window.history.back()">
                                <i class="ri-arrow-left-line"></i> Cancel
                            </button>
                            <button type="submit" class="btn btn-primary" id="@Model.FormId-submit">
                                <i class="ri-check-line"></i> Submit Form
                            </button>
                        </div>
                    </div>
                </div>
            </form>
        </div>
    </div>
</div>

@section Scripts {
    <script src="~/js/form-validation.js"></script>
    <script src="~/js/form-conditional-logic.js"></script>
    @if (Model.EnableAutoSave)
    {
        <script src="~/js/form-autosave.js"></script>
    }
    <script>
        $(document).ready(function() {
            var formId = '@Model.FormId';
            var form = $('#' + formId + '-form');

            // Initialize form validation
            initializeFormValidation(formId);

            // Initialize conditional logic
            initializeConditionalLogic(formId);

            // Progress tracking
            updateProgress();
            form.on('change', 'input, select, textarea', updateProgress);

            function updateProgress() {
                var totalFields = @Model.TotalFields;
                var completedFields = 0;

                form.find('input, select, textarea').each(function() {
                    if ($(this).val() && $(this).val().length > 0) {
                        completedFields++;
                    }
                });

                var percentage = Math.round((completedFields / totalFields) * 100);
                $('#' + formId + '-progress').css('width', percentage + '%');
                $('.completed-fields').text(completedFields);
            }

            // Submit handler
            form.on('submit', function(e) {
                e.preventDefault();
                if (validateForm(formId)) {
                    // Submit via AJAX or allow normal form submission
                    this.submit();
                }
            });

            // Save draft handler
            $('#' + formId + '-save-draft').on('click', function() {
                saveDraft(formId, '@Model.SaveDraftUrl');
            });

            @if (Model.EnableAutoSave)
            {
                @:// Auto-save every @Model.AutoSaveIntervalSeconds seconds
                @:setInterval(function() {
                    @:saveDraft(formId, '@Model.SaveDraftUrl');
                @:}, @(Model.AutoSaveIntervalSeconds * 1000));
            }
        });
    </script>
}
```

#### **_FormSection.cshtml**

```cshtml
@model FormReporting.Models.ViewModels.Components.FormSectionViewModel

<div class="form-section mb-4" id="section-@Model.SectionId">
    @if (Model.IsCollapsible)
    {
        <div class="card">
            <div class="card-header" id="heading-@Model.CollapseId">
                <h5 class="mb-0">
                    <button class="btn btn-link w-100 text-start d-flex justify-content-between align-items-center"
                            type="button"
                            data-bs-toggle="collapse"
                            data-bs-target="#@Model.CollapseId">
                        <span>
                            @if (!string.IsNullOrEmpty(Model.IconClass))
                            {
                                <i class="@Model.IconClass me-2"></i>
                            }
                            @Model.SectionName
                        </span>
                        <i class="ri-arrow-down-s-line"></i>
                    </button>
                </h5>
                @if (!string.IsNullOrEmpty(Model.SectionDescription))
                {
                    <p class="text-muted mb-0 px-3 pb-2">@Model.SectionDescription</p>
                }
            </div>
            <div id="@Model.CollapseId"
                 class="collapse @(!Model.IsCollapsedByDefault ? "show" : "")"
                 aria-labelledby="heading-@Model.CollapseId">
                <div class="card-body">
                    @foreach (var field in Model.Fields)
                    {
                        <partial name="~/Views/Shared/Components/Form/_FormField.cshtml" model="field" />
                    }
                </div>
            </div>
        </div>
    }
    else
    {
        <div class="card">
            <div class="card-header bg-light">
                <h5 class="card-title mb-0">
                    @if (!string.IsNullOrEmpty(Model.IconClass))
                    {
                        <i class="@Model.IconClass me-2"></i>
                    }
                    @Model.SectionName
                </h5>
                @if (!string.IsNullOrEmpty(Model.SectionDescription))
                {
                    <p class="text-muted mb-0 mt-1">@Model.SectionDescription</p>
                }
            </div>
            <div class="card-body">
                @foreach (var field in Model.Fields)
                {
                    <partial name="~/Views/Shared/Components/Form/_FormField.cshtml" model="field" />
                }
            </div>
        </div>
    }
</div>
```

#### **_FormField.cshtml** (Router)

```cshtml
@model FormReporting.Models.ViewModels.Components.FormFieldViewModel
@using FormReporting.Models.Common

@*
    Form Field Router - Directs to specific field type partial
*@

<div class="form-field-wrapper mb-3"
     id="field-@Model.FieldId"
     data-field-type="@Model.FieldType"
     @if (Model.ConditionalLogic != null) { <text>data-conditional-logic='@Json.Serialize(Model.ConditionalLogic)'</text> }>

    @switch (Model.FieldType)
    {
        case FormFieldType.Text:
            <partial name="~/Views/Shared/Components/Form/Fields/_TextField.cshtml" model="Model" />
            break;

        case FormFieldType.TextArea:
            <partial name="~/Views/Shared/Components/Form/Fields/_TextAreaField.cshtml" model="Model" />
            break;

        case FormFieldType.Number:
            <partial name="~/Views/Shared/Components/Form/Fields/_NumberField.cshtml" model="Model" />
            break;

        case FormFieldType.Date:
            <partial name="~/Views/Shared/Components/Form/Fields/_DateField.cshtml" model="Model" />
            break;

        case FormFieldType.DateTime:
            <partial name="~/Views/Shared/Components/Form/Fields/_DateTimeField.cshtml" model="Model" />
            break;

        case FormFieldType.Email:
            <partial name="~/Views/Shared/Components/Form/Fields/_EmailField.cshtml" model="Model" />
            break;

        case FormFieldType.Phone:
            <partial name="~/Views/Shared/Components/Form/Fields/_PhoneField.cshtml" model="Model" />
            break;

        case FormFieldType.Url:
            <partial name="~/Views/Shared/Components/Form/Fields/_UrlField.cshtml" model="Model" />
            break;

        case FormFieldType.Dropdown:
            <partial name="~/Views/Shared/Components/Form/Fields/_DropdownField.cshtml" model="Model" />
            break;

        case FormFieldType.Radio:
            <partial name="~/Views/Shared/Components/Form/Fields/_RadioField.cshtml" model="Model" />
            break;

        case FormFieldType.Checkbox:
            <partial name="~/Views/Shared/Components/Form/Fields/_CheckboxField.cshtml" model="Model" />
            break;

        case FormFieldType.MultiSelect:
            <partial name="~/Views/Shared/Components/Form/Fields/_MultiSelectField.cshtml" model="Model" />
            break;

        case FormFieldType.FileUpload:
            <partial name="~/Views/Shared/Components/Form/Fields/_FileUploadField.cshtml" model="Model" />
            break;

        case FormFieldType.Currency:
            <partial name="~/Views/Shared/Components/Form/Fields/_CurrencyField.cshtml" model="Model" />
            break;

        case FormFieldType.Percentage:
            <partial name="~/Views/Shared/Components/Form/Fields/_PercentageField.cshtml" model="Model" />
            break;

        default:
            <div class="alert alert-warning">
                Unsupported field type: @Model.FieldType
            </div>
            break;
    }
</div>
```

#### **Example Field Partial: _TextField.cshtml**

```cshtml
@model FormReporting.Models.ViewModels.Components.FormFieldViewModel

<div class="mb-3">
    <label for="@Model.InputId" class="form-label">
        @Model.FieldName
        @if (Model.IsRequired)
        {
            <span class="text-danger">*</span>
        }
    </label>

    @if (!string.IsNullOrEmpty(Model.FieldDescription))
    {
        <small class="text-muted d-block mb-1">@Model.FieldDescription</small>
    }

    <div class="input-group">
        @if (!string.IsNullOrEmpty(Model.PrefixText))
        {
            <span class="input-group-text">@Model.PrefixText</span>
        }

        <input type="text"
               class="form-control"
               id="@Model.InputId"
               name="@Model.InputName"
               placeholder="@Model.PlaceholderText"
               value="@Model.CurrentValue"
               @if (Model.IsRequired) { <text>required</text> }
               data-validations='@Json.Serialize(Model.Validations)' />

        @if (!string.IsNullOrEmpty(Model.SuffixText))
        {
            <span class="input-group-text">@Model.SuffixText</span>
        }
    </div>

    @if (!string.IsNullOrEmpty(Model.HelpText))
    {
        <div class="form-text">@Model.HelpText</div>
    }

    <div class="invalid-feedback"></div>
</div>
```

### **Component Set B: Controller Usage Example**

```csharp
public class FormSubmissionController : Controller
{
    private readonly ApplicationDbContext _context;

    public FormSubmissionController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Display form for filling
    public async Task<IActionResult> FillForm(int templateId)
    {
        // Load form template from database
        var formConfig = await FormExtensions.LoadFromTemplate(templateId, _context);

        // Set submission URLs
        formConfig.SubmitUrl = Url.Action("SubmitForm", new { templateId });
        formConfig.SaveDraftUrl = Url.Action("SaveDraft", new { templateId });
        formConfig.EnableAutoSave = true;
        formConfig.ShowProgressBar = true;

        // Transform to view model
        var formViewModel = formConfig.BuildForm();

        return View(formViewModel);
    }

    // Handle form submission
    [HttpPost]
    public async Task<IActionResult> SubmitForm(int templateId, IFormCollection formData)
    {
        // Load form config for validation
        var formConfig = await FormExtensions.LoadFromTemplate(templateId, _context);

        // Convert IFormCollection to Dictionary
        var data = formData.Keys.ToDictionary(k => k, k => (object)formData[k].ToString());

        // Validate
        var errors = formConfig.ValidateFormSubmission(data);
        if (errors.Any())
        {
            TempData["Errors"] = JsonSerializer.Serialize(errors);
            return RedirectToAction("FillForm", new { templateId });
        }

        // Create submission
        var submission = new FormTemplateSubmission
        {
            TemplateId = templateId,
            TenantId = GetCurrentTenantId(),
            SubmittedBy = GetCurrentUserId(),
            SubmittedDate = DateTime.Now,
            Status = "Submitted"
        };

        _context.FormTemplateSubmissions.Add(submission);
        await _context.SaveChangesAsync();

        // Save responses
        foreach (var kvp in data)
        {
            var itemId = int.Parse(kvp.Key.Replace("field_", ""));
            var item = await _context.FormTemplateItems
                .FirstOrDefaultAsync(i => i.ItemId == itemId);

            if (item == null) continue;

            var response = new FormTemplateResponse
            {
                SubmissionId = submission.SubmissionId,
                ItemId = itemId
            };

            // Store in appropriate column based on data type
            switch (item.DataType)
            {
                case "Number":
                case "Currency":
                case "Percentage":
                    if (decimal.TryParse(kvp.Value?.ToString(), out var numValue))
                        response.NumericValue = numValue;
                    break;

                case "Date":
                case "DateTime":
                    if (DateTime.TryParse(kvp.Value?.ToString(), out var dateValue))
                        response.DateValue = dateValue;
                    break;

                case "Checkbox":
                    response.BooleanValue = kvp.Value?.ToString() == "true" || kvp.Value?.ToString() == "on";
                    break;

                default:
                    response.TextValue = kvp.Value?.ToString();
                    break;
            }

            _context.FormTemplateResponses.Add(response);
        }

        await _context.SaveChangesAsync();

        // Trigger metric population
        await PopulateMetrics(submission.SubmissionId);

        TempData["Success"] = "Form submitted successfully!";
        return RedirectToAction("SubmissionConfirmation", new { submissionId = submission.SubmissionId });
    }

    // Save draft
    [HttpPost]
    public async Task<IActionResult> SaveDraft(int templateId, IFormCollection formData)
    {
        // Similar to SubmitForm but with Status = "Draft"
        // ...

        return Json(new { success = true, message = "Draft saved" });
    }

    private async Task PopulateMetrics(int submissionId)
    {
        // Read FormItemMetricMapping and populate TenantMetric table
        // Log in MetricPopulationLog
    }
}
```

---

## Implementation Considerations

### **1. Conditional Logic Handling**

**Challenge:** `FormTemplateItem.ConditionalLogic` is JSON-based (`{"action": "show", "rules": [{"itemId": 45, "operator": "equals", "value": "Yes"}]}`)

**Solution:**

**JavaScript File:** `wwwroot/js/form-conditional-logic.js`

```javascript
function initializeConditionalLogic(formId) {
    var form = $('#' + formId + '-form');

    // Find all fields with conditional logic
    form.find('[data-conditional-logic]').each(function() {
        var $field = $(this);
        var logic = JSON.parse($field.attr('data-conditional-logic'));

        // Watch target fields
        logic.rules.forEach(function(rule) {
            var $targetField = $('#field-' + rule.targetFieldId).find('input, select, textarea');

            $targetField.on('change', function() {
                evaluateConditionalLogic($field, logic);
            });
        });

        // Initial evaluation
        evaluateConditionalLogic($field, logic);
    });
}

function evaluateConditionalLogic($field, logic) {
    var results = [];

    logic.rules.forEach(function(rule) {
        var $targetField = $('#field-' + rule.targetFieldId).find('input, select, textarea');
        var targetValue = $targetField.val();
        var ruleMatches = false;

        switch (rule.operator) {
            case 'equals':
                ruleMatches = targetValue == rule.value;
                break;
            case 'notEquals':
                ruleMatches = targetValue != rule.value;
                break;
            case 'contains':
                ruleMatches = targetValue.includes(rule.value);
                break;
            case 'greaterThan':
                ruleMatches = parseFloat(targetValue) > parseFloat(rule.value);
                break;
            case 'lessThan':
                ruleMatches = parseFloat(targetValue) < parseFloat(rule.value);
                break;
        }

        results.push(ruleMatches);
    });

    // Apply logic (all or any)
    var shouldApply = logic.logic === 'all'
        ? results.every(r => r === true)
        : results.some(r => r === true);

    // Apply action
    switch (logic.action) {
        case 'show':
            $field.toggle(shouldApply);
            break;
        case 'hide':
            $field.toggle(!shouldApply);
            break;
        case 'enable':
            $field.find('input, select, textarea').prop('disabled', !shouldApply);
            break;
        case 'disable':
            $field.find('input, select, textarea').prop('disabled', shouldApply);
            break;
    }
}
```

### **2. Matrix/Grid Layout**

**Challenge:** Fields with same `MatrixGroupId` should render as table/grid

**Solution:** In `FormExtensions.TransformSection()`, group fields by `MatrixGroupId` and render as table

```cshtml
@* In _FormSection.cshtml *@
@{
    var matrixGroups = Model.Fields
        .Where(f => f.MatrixGroupId.HasValue)
        .GroupBy(f => f.MatrixGroupId.Value);

    var singleFields = Model.Fields.Where(f => !f.MatrixGroupId.HasValue);
}

@* Render single fields *@
@foreach (var field in singleFields)
{
    <partial name="~/Views/Shared/Components/Form/_FormField.cshtml" model="field" />
}

@* Render matrix groups *@
@foreach (var group in matrixGroups)
{
    <div class="matrix-group mb-4">
        <table class="table table-bordered">
            <thead>
                <tr>
                    <th>Item</th>
                    @foreach (var field in group)
                    {
                        <th>@field.FieldName</th>
                    }
                </tr>
            </thead>
            <tbody>
                @foreach (var field in group)
                {
                    <tr>
                        <td>@field.MatrixRowLabel</td>
                        <td>
                            @* Render input without label *@
                            <input type="text" name="@field.InputName" class="form-control" />
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
}
```

### **3. Cascading Dropdowns**

**Challenge:** `FormItemOption.ParentOptionId` enables dependent dropdowns

**Solution:**

**In Partial View:**
```cshtml
<select id="@Model.InputId"
        name="@Model.InputName"
        class="form-select cascading-dropdown"
        data-cascade-target="@Model.CascadeTargetFieldId">
    @foreach (var option in Model.Options)
    {
        <option value="@option.Value"
                data-parent="@option.ParentOptionId">
            @option.Label
        </option>
    }
</select>
```

**JavaScript:**
```javascript
$('.cascading-dropdown').on('change', function() {
    var parentValue = $(this).val();
    var targetFieldId = $(this).data('cascade-target');
    var $targetSelect = $('#' + targetFieldId);

    // Hide all options
    $targetSelect.find('option').hide();

    // Show matching options
    $targetSelect.find('option[data-parent="' + parentValue + '"]').show();

    // Reset target value
    $targetSelect.val('');
});
```

### **4. Field Library Integration**

**Challenge:** When `LibraryFieldId` is set and `!IsLibraryOverride`, pull defaults from `FieldLibrary`

**Solution:** In `FormExtensions.LoadFromTemplate()`:

```csharp
if (item.LibraryFieldId.HasValue && !item.IsLibraryOverride)
{
    var libraryField = await context.FieldLibrary
        .FirstOrDefaultAsync(f => f.LibraryFieldId == item.LibraryFieldId.Value);

    if (libraryField != null && !string.IsNullOrEmpty(libraryField.DefaultConfiguration))
    {
        var defaultConfig = JsonSerializer.Deserialize<FieldDefaultConfiguration>(
            libraryField.DefaultConfiguration);

        // Apply library defaults
        fieldConfig.Validations = defaultConfig.Validations;
        fieldConfig.Options = defaultConfig.Options;
        // ... etc
    }
}
```

### **5. Client-Side Validation**

**JavaScript File:** `wwwroot/js/form-validation.js`

```javascript
function initializeFormValidation(formId) {
    var form = $('#' + formId + '-form');

    form.find('input, select, textarea').each(function() {
        var $input = $(this);
        var validations = JSON.parse($input.attr('data-validations') || '[]');

        $input.on('blur change', function() {
            validateField($input, validations);
        });
    });
}

function validateField($input, validations) {
    var value = $input.val();
    var errors = [];

    validations.forEach(function(validation) {
        var isValid = true;

        switch (validation.ValidationType) {
            case 'Required':
                isValid = value && value.trim() !== '';
                break;
            case 'Email':
                isValid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
                break;
            case 'MinLength':
                isValid = value.length >= validation.MinLength;
                break;
            case 'MaxLength':
                isValid = value.length <= validation.MaxLength;
                break;
            case 'MinValue':
                isValid = parseFloat(value) >= validation.MinValue;
                break;
            case 'MaxValue':
                isValid = parseFloat(value) <= validation.MaxValue;
                break;
            case 'Regex':
                isValid = new RegExp(validation.RegexPattern).test(value);
                break;
        }

        if (!isValid) {
            errors.push(validation.ErrorMessage);
        }
    });

    // Display errors
    var $feedback = $input.siblings('.invalid-feedback');
    if (errors.length > 0) {
        $input.addClass('is-invalid');
        $feedback.text(errors[0]).show();
    } else {
        $input.removeClass('is-invalid');
        $feedback.hide();
    }

    return errors.length === 0;
}

function validateForm(formId) {
    var form = $('#' + formId + '-form');
    var isValid = true;

    form.find('input, select, textarea').each(function() {
        var $input = $(this);
        var validations = JSON.parse($input.attr('data-validations') || '[]');
        if (!validateField($input, validations)) {
            isValid = false;
        }
    });

    return isValid;
}
```

### **6. File Upload Handling**

**Challenge:** `FileUpload` field type needs special handling

**Solution:**

**In Controller:**
```csharp
[HttpPost]
public async Task<IActionResult> UploadFile(IFormFile file, int itemId)
{
    // Upload to Azure Blob Storage or local storage
    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
    var filePath = Path.Combine("uploads", "forms", fileName);

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    // Return file path to store in FormTemplateResponse.TextValue
    return Json(new { success = true, filePath });
}
```

**In Partial View (_FileUploadField.cshtml):**
```cshtml
<input type="file"
       id="@Model.InputId"
       class="form-control file-upload-field"
       data-item-id="@Model.FieldId"
       accept="@Model.AcceptedFileTypes" />

<input type="hidden"
       name="@Model.InputName"
       id="@Model.InputId-path" />

<script>
$('#@Model.InputId').on('change', function() {
    var file = this.files[0];
    var formData = new FormData();
    formData.append('file', file);
    formData.append('itemId', '@Model.FieldId');

    $.ajax({
        url: '/FormSubmission/UploadFile',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function(response) {
            $('#@Model.InputId-path').val(response.filePath);
        }
    });
});
</script>
```

### **7. Calculated Fields**

**Challenge:** `FormItemCalculation` suggests auto-calculating fields

**Solution:**

**In Database:**
```sql
-- FormItemCalculation table
ItemCalculationId | ItemId | Formula                  | DependsOnItems
1                 | 45     | [Item43] * [Item44]      | 43,44
2                 | 46     | [Item45] * 1.16          | 45
```

**JavaScript:**
```javascript
// Parse formula and watch dependent fields
var formula = "[Item43] * [Item44]"; // From FormItemCalculation.Formula
var dependsOn = [43, 44]; // From FormItemCalculation.DependsOnItems

dependsOn.forEach(function(itemId) {
    $('#field-' + itemId).find('input').on('change', function() {
        calculateField(45, formula, dependsOn);
    });
});

function calculateField(targetItemId, formula, dependsOn) {
    var expression = formula;

    // Replace [ItemX] with actual values
    dependsOn.forEach(function(itemId) {
        var value = $('#field-' + itemId).find('input').val() || 0;
        expression = expression.replace('[Item' + itemId + ']', value);
    });

    // Evaluate (use a safe evaluator like math.js, not eval())
    var result = math.evaluate(expression);

    // Set calculated value
    $('#field-' + targetItemId).find('input').val(result);
}
```

### **8. Metric Population**

**Challenge:** `FormItemMetricMapping` links responses to metrics

**Solution:**

**After Form Submission (in Controller):**
```csharp
private async Task PopulateMetrics(int submissionId)
{
    var submission = await _context.FormTemplateSubmissions
        .Include(s => s.Responses)
        .ThenInclude(r => r.Item)
        .ThenInclude(i => i.MetricMappings)
        .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

    foreach (var response in submission.Responses)
    {
        var mappings = response.Item.MetricMappings.Where(m => m.IsActive);

        foreach (var mapping in mappings)
        {
            var metricValue = response.NumericValue ??
                              decimal.Parse(response.TextValue ?? "0");

            // Find or create tenant metric
            var tenantMetric = await _context.TenantMetrics
                .FirstOrDefaultAsync(tm =>
                    tm.MetricId == mapping.MetricId &&
                    tm.TenantId == submission.TenantId);

            if (tenantMetric == null)
            {
                tenantMetric = new TenantMetric
                {
                    MetricId = mapping.MetricId,
                    TenantId = submission.TenantId,
                    MetricValue = metricValue,
                    RecordedDate = DateTime.Now
                };
                _context.TenantMetrics.Add(tenantMetric);
            }
            else
            {
                // Apply aggregation
                switch (mapping.AggregationType)
                {
                    case "Sum":
                        tenantMetric.MetricValue += metricValue;
                        break;
                    case "Average":
                        // Calculate average
                        break;
                    case "Latest":
                        tenantMetric.MetricValue = metricValue;
                        break;
                }
            }

            // Log population
            _context.MetricPopulationLogs.Add(new MetricPopulationLog
            {
                SubmissionId = submissionId,
                MetricId = mapping.MetricId,
                PopulatedValue = metricValue,
                PopulatedDate = DateTime.Now
            });
        }
    }

    await _context.SaveChangesAsync();
}
```

---

## Recommended Implementation Order

### **Phase 1: Foundation (Component Set B - Form Rendering)**

**Why Start Here:**
- Follows established pattern (ViewModels → Extensions → Partials)
- Can be tested immediately with hand-crafted test data
- More critical for end users (factory staff need to fill forms)
- Reusable components for other parts of the system

**Steps:**
1. Create ViewModels (`FormViewModel.cs`)
2. Create Extensions (`FormExtensions.cs`) with `BuildForm()` and `LoadFromTemplate()`
3. Create basic partial views:
   - `_Form.cshtml` (container)
   - `_FormSection.cshtml` (sections)
   - `_FormField.cshtml` (router)
4. Create field type partials (start with most common):
   - `_TextField.cshtml`
   - `_NumberField.cshtml`
   - `_DateField.cshtml`
   - `_DropdownField.cshtml`
   - `_CheckboxField.cshtml`
5. Create test controller and view
6. Implement basic client-side validation
7. Implement form submission handling

**Estimated Time:** 3-5 days

### **Phase 2: Advanced Features (Component Set B)**

**Steps:**
1. Conditional logic JavaScript
2. Cascading dropdowns
3. Matrix/grid layouts
4. Calculated fields
5. File uploads
6. Auto-save functionality
7. Progress tracking
8. Metric population

**Estimated Time:** 3-4 days

### **Phase 3: Form Builder Interface (Component Set A)**

**Why Later:**
- Admins can manually create forms in database initially
- Requires more complex frontend framework (Vue.js/Alpine.js)
- Depends on Component Set B being complete (for preview)

**Steps:**
1. Choose frontend framework (Vue.js recommended)
2. Create FormBuilderCanvas component
3. Create SectionBuilder component
4. Create FieldPalette component
5. Create FieldPropertiesPanel component
6. Create FieldLibraryBrowser
7. Integrate FormPreview (uses Component Set B)
8. Implement save/publish functionality

**Estimated Time:** 7-10 days

### **Phase 4: Polish & Testing**

**Steps:**
1. Cross-browser testing
2. Mobile responsiveness
3. Accessibility (ARIA labels, keyboard navigation)
4. Performance optimization
5. User acceptance testing
6. Documentation

**Estimated Time:** 3-5 days

---

## Summary

This document outlines two complementary component sets:

**Component Set A (Form Builder - Admin Interface):**
- Visual drag-and-drop form designer
- Field property configuration panels
- Field library integration
- Live preview
- Target: ICT administrators

**Component Set B (Form Rendering - User Interface):**
- Three-layer architecture (ViewModels → Extensions → Partials)
- Dynamic form rendering from database templates
- EAV response storage
- Client-side validation
- Conditional logic
- Metric population
- Target: End users (factory staff, managers)

**Recommendation:** Start with **Component Set B** first, as it follows the established pattern, is more critical for operations, and provides reusable components that Component Set A will leverage for previews.

---

**Related Documentation:**
- `2B_FormBuilder_Structure.md` - Database schema and relationships
- `SYSTEM_OVERVIEW.md` - Overall system architecture
- `STATCARD_ARCHITECTURE.md` - Reference for three-layer pattern

**Last Updated:** 2025-11-07
