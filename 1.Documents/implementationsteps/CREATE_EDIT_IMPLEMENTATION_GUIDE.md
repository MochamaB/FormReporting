# Create & Edit Page Implementation Guide - KTDA Form Reporting System

## 📋 Overview

Complete pattern for implementing **Create** and **Edit** pages using **SimpleForms** components with the three-layer architecture.

**Pattern:** ViewModels → Extensions → Partial Views

---

## 🎯 Three-Layer Architecture

### **Layer 1: ViewModels** (WHAT data is needed)
- `SimpleFormConfig` - Configuration object (what you create in views)
- `SimpleFormViewModel` - Renderable object (created by extensions)
- `SimpleFormFieldConfig` - Individual field configuration
- `SimpleFormFieldViewModel` - Individual field for rendering

### **Layer 2: Extensions** (HOW to transform data)
- `SimpleFormExtensions.BuildForm()` - Main transformation method
- Fluent API methods: `WithTextField()`, `WithSelectField()`, etc.
- CSS class generation based on form style

### **Layer 3: Partial Views** (HOW to render)
- `_SimpleForm.cshtml` - Main form wrapper
- `_SimpleFormContent.cshtml` - Form content and buttons
- `_SimpleFormField.cshtml` - Individual field rendering

---

## 📁 File Structure

```
Controllers/Identity/
└── RolesController.cs
    ├── Create() GET
    ├── Create(model) POST
    ├── Edit(id) GET
    └── Edit(model) POST

Views/Identity/Roles/
├── Create.cshtml
└── Edit.cshtml

Models/ViewModels/Identity/
└── RoleEditViewModel.cs

Models/ViewModels/Components/
└── SimpleFormConfig.cs

Extensions/
└── SimpleFormExtensions.cs

Views/Shared/Components/SimpleForms/
├── _SimpleForm.cshtml
├── _SimpleFormContent.cshtml
└── _SimpleFormField.cshtml
```

---

## 🔧 Complete Implementation Example

### **STEP 1: Controller - Create Actions**

```csharp
// Controllers/Identity/RolesController.cs

/// <summary>
/// Show create role page
/// </summary>
[HttpGet("Create")]
public async Task<IActionResult> Create()
{
    var model = new RoleEditViewModel
    {
        IsActive = true  // Default value
    };

    // Load dropdown options
    ViewBag.ScopeLevels = await _context.ScopeLevels
        .Where(s => s.IsActive)
        .OrderBy(s => s.Level)
        .Select(s => new SelectOption
        {
            Value = s.ScopeLevelId.ToString(),
            Text = s.ScopeName
        })
        .ToListAsync();

    return View(model);
}

/// <summary>
/// Handle create role submission
/// </summary>
[HttpPost("Create")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(RoleEditViewModel model)
{
    if (ModelState.IsValid)
    {
        // Check for duplicates
        if (await _context.Roles.AnyAsync(r => r.RoleCode == model.RoleCode))
        {
            ModelState.AddModelError("RoleCode", "A role with this code already exists.");
            await LoadDropdownOptions();
            return View(model);
        }

        // Create entity
        var role = new Role
        {
            RoleName = model.RoleName,
            RoleCode = model.RoleCode.ToUpper(),
            Description = model.Description,
            ScopeLevelId = model.ScopeLevelId,
            IsActive = model.IsActive,
            CreatedDate = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Role '{role.RoleName}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    await LoadDropdownOptions();
    return View(model);
}
```

### **STEP 2: Controller - Edit Actions**

```csharp
/// <summary>
/// Show edit role page
/// </summary>
[HttpGet("Edit/{id}")]
public async Task<IActionResult> Edit(int id)
{
    var role = await _context.Roles.FindAsync(id);
    if (role == null)
    {
        TempData["ErrorMessage"] = "Role not found.";
        return RedirectToAction(nameof(Index));
    }

    var model = new RoleEditViewModel
    {
        RoleId = role.RoleId,
        RoleName = role.RoleName,
        RoleCode = role.RoleCode,
        Description = role.Description,
        ScopeLevelId = role.ScopeLevelId,
        IsActive = role.IsActive
    };

    await LoadDropdownOptions();
    return View(model);
}

/// <summary>
/// Handle edit role submission
/// </summary>
[HttpPost("Edit")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(RoleEditViewModel model)
{
    if (ModelState.IsValid)
    {
        var role = await _context.Roles.FindAsync(model.RoleId);
        if (role == null)
        {
            TempData["ErrorMessage"] = "Role not found.";
            return RedirectToAction(nameof(Index));
        }

        // Check for duplicates (excluding current role)
        if (await _context.Roles.AnyAsync(r => r.RoleCode == model.RoleCode && r.RoleId != model.RoleId))
        {
            ModelState.AddModelError("RoleCode", "A role with this code already exists.");
            await LoadDropdownOptions();
            return View(model);
        }

        // Update entity
        role.RoleName = model.RoleName;
        role.RoleCode = model.RoleCode.ToUpper();
        role.Description = model.Description;
        role.ScopeLevelId = model.ScopeLevelId;
        role.IsActive = model.IsActive;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Role '{role.RoleName}' updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    await LoadDropdownOptions();
    return View(model);
}

/// <summary>
/// Helper method to load dropdown options
/// </summary>
private async Task LoadDropdownOptions()
{
    ViewBag.ScopeLevels = await _context.ScopeLevels
        .Where(s => s.IsActive)
        .OrderBy(s => s.Level)
        .Select(s => new SelectOption
        {
            Value = s.ScopeLevelId.ToString(),
            Text = s.ScopeName
        })
        .ToListAsync();
}
```

### **STEP 3: ViewModel**

```csharp
// Models/ViewModels/Identity/RoleEditViewModel.cs

using System.ComponentModel.DataAnnotations;

namespace FormReporting.Models.ViewModels.Identity
{
    public class RoleEditViewModel
    {
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Role name is required")]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role code is required")]
        [StringLength(50)]
        public string RoleCode { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Scope level is required")]
        public int ScopeLevelId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
```

### **STEP 4: Create View**

```cshtml
@* Views/Identity/Roles/Create.cshtml *@

@using FormReporting.Extensions
@using FormReporting.Models.ViewModels.Components
@model FormReporting.Models.ViewModels.Identity.RoleEditViewModel

@{
    ViewData["Title"] = "Create Role";

    // ═══════════════════════════════════════════════════════════
    // CONFIGURE FORM
    // ═══════════════════════════════════════════════════════════
    
    var formConfig = new SimpleFormConfig
    {
        FormId = "createRoleForm",
        FormAction = Url.Action("Create", "Roles") ?? "/Identity/Roles/Create",
        FormMethod = "POST",
        SubmitButtonText = "Create Role",
        CancelButtonUrl = Url.Action("Index", "Roles") ?? "/Identity/Roles",
        ShowCancelButton = true,
        ShowResetButton = false,
        StyleType = FormStyleType.Standard,
        CardTitle = "Create New Role",
        CardSubtitle = "Add a new role to the system",
        WrapInCard = true,
        Fields = new List<SimpleFormFieldConfig>
        {
            // Hidden field for RoleId (always 0 for create)
            new SimpleFormFieldConfig
            {
                PropertyName = "RoleId",
                FieldType = SimpleFieldType.Hidden,
                Value = 0,
                DisplayOrder = 1
            },
            // Role Name
            new SimpleFormFieldConfig
            {
                PropertyName = "RoleName",
                Label = "Role Name",
                FieldType = SimpleFieldType.Text,
                Value = Model.RoleName,
                IsRequired = true,
                PlaceholderText = "Enter role name",
                HelpText = "Descriptive name for the role",
                ColumnClass = "col-md-6",
                DisplayOrder = 2
            },
            // Role Code
            new SimpleFormFieldConfig
            {
                PropertyName = "RoleCode",
                Label = "Role Code",
                FieldType = SimpleFieldType.Text,
                Value = Model.RoleCode,
                IsRequired = true,
                PlaceholderText = "Enter role code (e.g., ADMIN)",
                HelpText = "Unique code for the role (uppercase)",
                ColumnClass = "col-md-6",
                DisplayOrder = 3
            },
            // Scope Level
            new SimpleFormFieldConfig
            {
                PropertyName = "ScopeLevelId",
                Label = "Scope Level",
                FieldType = SimpleFieldType.Select,
                Value = Model.ScopeLevelId,
                IsRequired = true,
                Options = ViewBag.ScopeLevels,
                HelpText = "Select the scope level for this role",
                ColumnClass = "col-md-6",
                DisplayOrder = 4
            },
            // Is Active
            new SimpleFormFieldConfig
            {
                PropertyName = "IsActive",
                Label = "Active",
                FieldType = SimpleFieldType.Checkbox,
                Value = Model.IsActive,
                HelpText = "Check to make this role active",
                ColumnClass = "col-md-6",
                DisplayOrder = 5
            },
            // Description
            new SimpleFormFieldConfig
            {
                PropertyName = "Description",
                Label = "Description",
                FieldType = SimpleFieldType.TextArea,
                Value = Model.Description,
                Rows = 4,
                PlaceholderText = "Enter role description",
                HelpText = "Optional description of the role's purpose",
                ColumnClass = "col-12",
                DisplayOrder = 6
            }
        }
    };

    // Transform config into renderable form
    var form = formConfig.BuildForm();
}

<!-- Page Header -->
<div class="row">
    <div class="col-12">
        <div class="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 class="mb-sm-0">Create Role</h4>
            <div class="page-title-right">
                <ol class="breadcrumb m-0">
                    <li class="breadcrumb-item"><a href="@Url.Action("Index", "Dashboard")">Dashboard</a></li>
                    <li class="breadcrumb-item"><a href="@Url.Action("Index", "Roles")">Roles</a></li>
                    <li class="breadcrumb-item active">Create</li>
                </ol>
            </div>
        </div>
    </div>
</div>

<!-- Render Form -->
<div class="row">
    <div class="col-lg-8 offset-lg-2">
        <partial name="~/Views/Shared/Components/SimpleForms/_SimpleForm.cshtml" model="form" />
    </div>
</div>
```

### **STEP 5: Edit View**

```cshtml
@* Views/Identity/Roles/Edit.cshtml *@

@using FormReporting.Extensions
@using FormReporting.Models.ViewModels.Components
@model FormReporting.Models.ViewModels.Identity.RoleEditViewModel

@{
    ViewData["Title"] = "Edit Role";

    var formConfig = new SimpleFormConfig
    {
        FormId = "editRoleForm",
        FormAction = Url.Action("Edit", "Roles") ?? "/Identity/Roles/Edit",
        FormMethod = "POST",
        SubmitButtonText = "Update Role",
        CancelButtonUrl = Url.Action("Index", "Roles") ?? "/Identity/Roles",
        ShowCancelButton = true,
        CardTitle = $"Edit Role: {Model.RoleName}",
        CardSubtitle = "Update role information",
        WrapInCard = true,
        Fields = new List<SimpleFormFieldConfig>
        {
            new SimpleFormFieldConfig
            {
                PropertyName = "RoleId",
                FieldType = SimpleFieldType.Hidden,
                Value = Model.RoleId,
                DisplayOrder = 1
            },
            new SimpleFormFieldConfig
            {
                PropertyName = "RoleName",
                Label = "Role Name",
                FieldType = SimpleFieldType.Text,
                Value = Model.RoleName,
                IsRequired = true,
                ColumnClass = "col-md-6",
                DisplayOrder = 2
            },
            new SimpleFormFieldConfig
            {
                PropertyName = "RoleCode",
                Label = "Role Code",
                FieldType = SimpleFieldType.Text,
                Value = Model.RoleCode,
                IsRequired = true,
                ColumnClass = "col-md-6",
                DisplayOrder = 3
            },
            new SimpleFormFieldConfig
            {
                PropertyName = "ScopeLevelId",
                Label = "Scope Level",
                FieldType = SimpleFieldType.Select,
                Value = Model.ScopeLevelId,
                IsRequired = true,
                Options = ViewBag.ScopeLevels,
                ColumnClass = "col-md-6",
                DisplayOrder = 4
            },
            new SimpleFormFieldConfig
            {
                PropertyName = "IsActive",
                Label = "Active",
                FieldType = SimpleFieldType.Checkbox,
                Value = Model.IsActive,
                ColumnClass = "col-md-6",
                DisplayOrder = 5
            },
            new SimpleFormFieldConfig
            {
                PropertyName = "Description",
                Label = "Description",
                FieldType = SimpleFieldType.TextArea,
                Value = Model.Description,
                Rows = 4,
                ColumnClass = "col-12",
                DisplayOrder = 6
            }
        }
    };

    var form = formConfig.BuildForm();
}

<div class="row">
    <div class="col-12">
        <div class="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 class="mb-sm-0">Edit Role</h4>
            <div class="page-title-right">
                <ol class="breadcrumb m-0">
                    <li class="breadcrumb-item"><a href="@Url.Action("Index", "Dashboard")">Dashboard</a></li>
                    <li class="breadcrumb-item"><a href="@Url.Action("Index", "Roles")">Roles</a></li>
                    <li class="breadcrumb-item active">Edit</li>
                </ol>
            </div>
        </div>
    </div>
</div>

<div class="row">
    <div class="col-lg-8 offset-lg-2">
        <partial name="~/Views/Shared/Components/SimpleForms/_SimpleForm.cshtml" model="form" />
    </div>
</div>
```

---

## 📊 Field Types Reference

```csharp
// Text Input
new SimpleFormFieldConfig
{
    PropertyName = "Name",
    Label = "Name",
    FieldType = SimpleFieldType.Text,
    IsRequired = true,
    ColumnClass = "col-md-6"
}

// Email Input
new SimpleFormFieldConfig
{
    PropertyName = "Email",
    Label = "Email Address",
    FieldType = SimpleFieldType.Email,
    IsRequired = true
}

// Number Input
new SimpleFormFieldConfig
{
    PropertyName = "Age",
    Label = "Age",
    FieldType = SimpleFieldType.Number,
    Min = 18,
    Max = 100,
    Step = 1
}

// Date Input
new SimpleFormFieldConfig
{
    PropertyName = "BirthDate",
    Label = "Birth Date",
    FieldType = SimpleFieldType.Date
}

// Dropdown/Select
new SimpleFormFieldConfig
{
    PropertyName = "CategoryId",
    Label = "Category",
    FieldType = SimpleFieldType.Select,
    Options = ViewBag.Categories,
    IsRequired = true
}

// Textarea
new SimpleFormFieldConfig
{
    PropertyName = "Description",
    Label = "Description",
    FieldType = SimpleFieldType.TextArea,
    Rows = 5
}

// Checkbox
new SimpleFormFieldConfig
{
    PropertyName = "IsActive",
    Label = "Active",
    FieldType = SimpleFieldType.Checkbox,
    Value = true
}

// Hidden Field
new SimpleFormFieldConfig
{
    PropertyName = "Id",
    FieldType = SimpleFieldType.Hidden,
    Value = Model.Id
}
```

---

## 🎨 Form Style Types

```csharp
// Standard (default)
StyleType = FormStyleType.Standard

// Floating Labels
StyleType = FormStyleType.FloatingLabels

// Horizontal (label left, input right)
StyleType = FormStyleType.Horizontal

// Inline (all fields in one row)
StyleType = FormStyleType.Inline
```

---

## ✅ Implementation Checklist

### **Create Page:**
- [ ] GET action returns empty model with defaults
- [ ] Load dropdown options in ViewBag
- [ ] POST action validates model
- [ ] Check for duplicates
- [ ] Create entity and save
- [ ] Redirect to Index with success message
- [ ] View configures SimpleFormConfig
- [ ] View calls BuildForm()
- [ ] View renders _SimpleForm partial

### **Edit Page:**
- [ ] GET action loads entity by ID
- [ ] Map entity to ViewModel
- [ ] Load dropdown options
- [ ] POST action validates model
- [ ] Check for duplicates (excluding current)
- [ ] Update entity and save
- [ ] Redirect to Index with success message
- [ ] View configures SimpleFormConfig with values
- [ ] Hidden field for ID included
- [ ] View renders _SimpleForm partial

---

## 🚀 Quick Start Template

```csharp
// Controller
[HttpGet("Create")]
public async Task<IActionResult> Create()
{
    var model = new YourViewModel();
    await LoadDropdownOptions();
    return View(model);
}

[HttpPost("Create")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(YourViewModel model)
{
    if (ModelState.IsValid)
    {
        var entity = new YourEntity { /* map properties */ };
        _context.Add(entity);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Created successfully.";
        return RedirectToAction(nameof(Index));
    }
    await LoadDropdownOptions();
    return View(model);
}
```

```cshtml
@* View *@
@{
    var formConfig = new SimpleFormConfig
    {
        FormAction = Url.Action("Create"),
        SubmitButtonText = "Create",
        CancelButtonUrl = Url.Action("Index"),
        CardTitle = "Create New Item",
        Fields = new List<SimpleFormFieldConfig>
        {
            new() { PropertyName = "Name", Label = "Name", FieldType = SimpleFieldType.Text, IsRequired = true }
        }
    };
    var form = formConfig.BuildForm();
}

<partial name="~/Views/Shared/Components/SimpleForms/_SimpleForm.cshtml" model="form" />
```

---

**This pattern is used for all Create/Edit pages across 77 tables in the KTDA system.**
