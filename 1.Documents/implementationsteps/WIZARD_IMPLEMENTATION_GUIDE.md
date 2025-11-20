# Wizard Implementation Guide - KTDA Form Reporting System

## 📋 Overview

Wizards break complex workflows into **manageable multi-step processes** with validation, navigation, and flexible content.

**Pattern:** ViewModels → Extensions → Partial Views (Step Content)

**Key Innovation:** Step content = separate partial views that can use ANY component (SimpleForms, DataTables, StatCards, or plain HTML)

---

## 🎯 Three-Layer Architecture

### **Layer 1: ViewModels**
- `WizardConfig` - Configuration (what you create)
- `WizardStep` - Individual step config
- `WizardViewModel` - Renderable (created by extensions)

### **Layer 2: Extensions**
- `WizardExtensions.BuildWizard()` - Transformation

### **Layer 3: Partial Views**
- `_VerticalWizard.cshtml` - **2-column layout** (steps + content)
- `_HorizontalWizard.cshtml` - 2-row layout
- **Step Content Partials** - Individual `.cshtml` files per step

---

## 📐 **Vertical Wizard Layout (Current Implementation)**

```
┌──────────────────────────────────────────────────┐
│  ┌──────────┬───────────────────────────────┐   │
│  │  Steps   │     Content Area              │   │
│  │ (col-4)  │     (col-8)                   │   │
│  ├──────────┼───────────────────────────────┤   │
│  │ ○ Step 1 │  Step Content Partial         │   │
│  │ ◉ Step 2 │  (Plain HTML, SimpleForms,    │   │
│  │ ○ Step 3 │   DataTables, or mixed)       │   │
│  │ ○ Step 4 │                               │   │
│  │          │  [Back]  [Next]               │   │
│  └──────────┴───────────────────────────────┘   │
└──────────────────────────────────────────────────┘
```

**Note:** ✅ No summary column - clean 2-column design
**Best for:** Simple to moderate workflows (3-6 steps)

---

## 🔧 Complete Implementation

### **STEP 1: Controller**

```csharp
[HttpGet("Create")]
public async Task<IActionResult> Create()
{
    var model = new TenantCreateViewModel { IsActive = true };
    
    // Load dropdown data for step partials
    ViewBag.Regions = await _context.Regions.ToListAsync();
    ViewBag.TenantGroups = await _context.TenantGroups.ToListAsync();
    
    return View(model);
}

[HttpPost("Create")]
public async Task<IActionResult> Create(TenantCreateViewModel model)
{
    if (ModelState.IsValid)
    {
        // Process all step data
        var tenant = new Tenant { /* map properties */ };
        _context.Add(tenant);
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "Tenant created successfully.";
        return RedirectToAction(nameof(Index));
    }
    
    await LoadDropdownData();
    return View(model);
}
```

### **STEP 2: ViewModel**

```csharp
public class TenantCreateViewModel
{
    // Step 1 fields
    public string TenantCode { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string TenantType { get; set; } = string.Empty;
    
    // Step 2 fields
    public bool CreateDefaultDepartments { get; set; } = true;
    public List<DepartmentInputModel>? Departments { get; set; }
    
    // Step 3 fields
    public List<int>? SelectedGroupIds { get; set; }
}
```

### **STEP 3: Main View - Configure Wizard**

```cshtml
@model TenantCreateViewModel
@using FormReporting.Extensions
@using FormReporting.Models.ViewModels.Components

@{
    var wizardConfig = new WizardConfig
    {
        FormId = "tenantCreationWizard",
        Layout = WizardLayout.Vertical,  // 2-column
        Steps = new List<WizardStep>
        {
            new WizardStep
            {
                StepId = "step1-basic",
                StepNumber = 1,
                Title = "Basic Details",
                Description = "Tenant information",
                Instructions = "Enter tenant code, name, and type",
                Icon = "ri-information-line",
                State = WizardStepState.Active,
                ContentPartialPath = "~/Views/Organizational/Tenants/Partials/_BasicDetails.cshtml",
                ShowPrevious = false,
                ShowNext = true,
                NextButtonText = "Next: Departments"
            },
            new WizardStep
            {
                StepId = "step2-departments",
                StepNumber = 2,
                Title = "Department Setup",
                Description = "Organizational structure",
                Icon = "ri-building-4-line",
                ContentPartialPath = "~/Views/Organizational/Tenants/Partials/_Departments.cshtml",
                ShowPrevious = true,
                ShowNext = true
            },
            new WizardStep
            {
                StepId = "step4-summary",
                StepNumber = 4,
                Title = "Review & Confirm",
                Icon = "ri-check-double-line",
                ContentPartialPath = "~/Views/Organizational/Tenants/Partials/_Summary.cshtml",
                ShowPrevious = true,
                ShowNext = false,
                CustomButtonHtml = "<button type='submit' class='btn btn-success ms-auto'><i class='ri-check-line me-1'></i>Create</button>"
            }
        }
    };

    var wizard = wizardConfig.BuildWizard();
    ViewData["WizardViewModel"] = wizard;
    ViewData["ParentModel"] = Model;
}

<div class="row">
    <div class="col-12">
        <div class="card">
            <div class="card-body">
                <form method="post" action="@Url.Action("Create")" id="@wizardConfig.FormId">
                    @Html.AntiForgeryToken()
                    <partial name="~/Views/Shared/Components/Wizards/_VerticalWizard.cshtml" />
                </form>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <script>
        $(document).ready(function () {
            // Step validation
            window.wizardValidationCallbacks = window.wizardValidationCallbacks || {};
            window.wizardValidationCallbacks['tenantCreationWizard'] = function (currentStepId) {
                if (currentStepId === 'step1-basic') {
                    if ($('#TenantCode').val().trim() === '') {
                        $('#TenantCode').addClass('is-invalid');
                        return false;
                    }
                }
                return true;
            };
        });
    </script>
}
```

### **STEP 4: Step Content Partials**

#### **Option A: Plain HTML**
```cshtml
@* _BasicDetails.cshtml *@
@model dynamic

<div class="row">
    <div class="col-md-6 mb-3">
        <label for="TenantCode">Tenant Code <span class="text-danger">*</span></label>
        <input type="text" class="form-control" id="TenantCode" name="TenantCode" value="@Model.TenantCode" required />
    </div>
    <div class="col-md-6 mb-3">
        <label for="TenantName">Tenant Name <span class="text-danger">*</span></label>
        <input type="text" class="form-control" id="TenantName" name="TenantName" value="@Model.TenantName" required />
    </div>
</div>
```

#### **Option B: Using SimpleForms**
```cshtml
@* _Step2_UsingSimpleForms.cshtml *@
@model dynamic
@using FormReporting.Extensions
@using FormReporting.Models.ViewModels.Components

@{
    // Create field configurations (don't wrap in <form> - wizard already has it)
    var fields = new List<SimpleFormFieldConfig>
    {
        new() { PropertyName = "CreateDefaultDepartments", Label = "Create Default Departments", 
                FieldType = SimpleFieldType.Checkbox, Value = Model.CreateDefaultDepartments }
    };
}

@foreach (var fieldConfig in fields)
{
    var field = new SimpleFormFieldViewModel
    {
        PropertyName = fieldConfig.PropertyName,
        Label = fieldConfig.Label,
        FieldType = fieldConfig.FieldType,
        Value = fieldConfig.Value?.ToString() ?? string.Empty,
        InputId = fieldConfig.PropertyName,
        InputName = fieldConfig.PropertyName,
        FormGroupClass = "mb-3",
        InputClass = "form-control"
    };
    
    <div class="col-12">
        <partial name="~/Views/Shared/Components/SimpleForms/_SimpleFormField.cshtml" model="field" />
    </div>
}
```

#### **Option C: Using DataTables**
```cshtml
@* _Step3_WithDataTable.cshtml *@
@{
    var tableConfig = new DataTableConfig { Columns = [...], Data = [...] };
    var table = tableConfig.BuildDataTable();
}
<partial name="~/Views/Shared/Components/DataTable/_DataTable.cshtml" model="table" />
```

#### **Option D: Using StatCards**
```cshtml
@* _Summary.cshtml *@
@{
    var statConfig = new StatsRowConfig
    {
        Titles = new List<string> { "Departments", "Groups", "Users" },
        Values = new List<string> { "4", "2", "0" },
        Icons = new List<string> { "ri-building-line", "ri-group-line", "ri-user-line" },
        CardType = CardType.IconLeftCard
    };
    var cards = statConfig.BuildStatsRow();
}
<div class="row">
    @foreach (var card in cards)
    {
        <partial name="~/Views/Shared/Components/StatisticCards/_IconLeftCard.cshtml" model="card" />
    }
</div>
```

---

## 🎨 **Step Content Can Use ANY Component**

| Content Type | Use Case | Example |
|--------------|----------|---------|
| **Plain HTML** | Basic forms | Text inputs, dropdowns, checkboxes |
| **SimpleForms** | Structured forms | Multi-field forms with validation |
| **DataTables** | Display lists | Show existing items, selection |
| **StatCards** | Summary stats | Overview dashboard, review step |
| **Mixed** | Complex UIs | Combine multiple components |
| **Custom JS** | Interactive | Dynamic form builders, calculators |

---

## ✅ **Key Features**

### **1. Navigation Control**
```csharp
ShowPrevious = false,              // Hide back button (first step)
ShowNext = true,                   // Show next button
NextButtonText = "Next: Details",  // Custom button text
CustomButtonHtml = "<button>...</button>"  // Custom buttons (submit, etc.)
```

### **2. Step Validation**
```javascript
window.wizardValidationCallbacks[wizardId] = function(currentStepId) {
    if (currentStepId === 'step1') {
        if ($('#Field').val() === '') return false;  // Block navigation
    }
    return true;  // Allow navigation
};
```

### **3. Step States**
```csharp
State = WizardStepState.Active   // Blue, current step
State = WizardStepState.Done     // Green checkmark
State = WizardStepState.Pending  // Gray, not started
```

### **4. Partial Reusability**
Same partial can be used in:
- **Wizard** (multi-step creation)
- **Tabs** (edit page with tabbed interface)

---

## 📊 **When to Use Wizards**

✅ **Use Wizards:**
- Multi-step entity creation (Tenant, User, Form Template)
- Complex workflows with logical grouping
- Processes requiring review before submission
- Guided data entry

❌ **Don't Use Wizards:**
- Simple single-page forms → Use SimpleForms
- Quick edits → Use Edit page
- Read-only views → Use Details page

---

## 🚀 **Quick Start Template**

```csharp
// Controller
[HttpGet("Create")]
public IActionResult Create()
{
    var model = new YourViewModel();
    await LoadDropdowns();
    return View(model);
}
```

```cshtml
@* View *@
@{
    var wizardConfig = new WizardConfig
    {
        FormId = "myWizard",
        Layout = WizardLayout.Vertical,
        Steps = new List<WizardStep>
        {
            new() { StepId = "step1", Title = "Step 1", ContentPartialPath = "~/Views/.../_ Step1.cshtml" }
        }
    };
    var wizard = wizardConfig.BuildWizard();
    ViewData["WizardViewModel"] = wizard;
    ViewData["ParentModel"] = Model;
}

<form method="post" id="@wizardConfig.FormId">
    <partial name="~/Views/Shared/Components/Wizards/_VerticalWizard.cshtml" />
</form>
```

---

## ✅ **Implementation Checklist**

- [ ] Controller loads ViewModel with dropdown data
- [ ] ViewModel contains fields for ALL steps
- [ ] WizardConfig defines all steps
- [ ] Step partials created in Partials folder
- [ ] BuildWizard() called and stored in ViewData
- [ ] _VerticalWizard component rendered
- [ ] JavaScript validation added
- [ ] Form submission handled in POST action
- [ ] Transaction used for multi-entity operations

---

**This pattern is used across complex workflows in the KTDA system for consistency and user experience.**
