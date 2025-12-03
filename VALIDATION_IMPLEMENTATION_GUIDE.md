# ASP.NET Core MVC Validation - Complete Implementation Guide

## 📋 Overview

ASP.NET Core MVC provides a comprehensive validation system with **both server-side and client-side validation** out of the box. This guide explains how validation works globally in your project and how to implement it properly.

---

## 🌐 Global Validation Setup (Already Configured)

### **1. Server-Side Validation** ✅

**File:** `Program.cs`

```csharp
builder.Services.AddControllersWithViews();
```

This single line enables:
- ✅ **Model validation** using Data Annotations
- ✅ **ModelState** automatic population
- ✅ **Automatic 400 responses** for invalid API requests
- ✅ **Validation attributes** recognition

### **2. Client-Side Validation** ✅

**File:** `Views/Shared/_ValidationScriptsPartial.cshtml`

This partial includes jQuery validation libraries:

```html
<script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
<script src="~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js"></script>
```

**Usage in any view:**
```cshtml
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

✅ **Already included in your Create.cshtml!**

---

## 🎯 Validation Layers

### **Layer 1: Data Annotations (DTO Level)**

**File:** `Models/ViewModels/Metrics/CreateMetricDefinitionDto.cs`

```csharp
public class CreateMetricDefinitionDto
{
    [Required(ErrorMessage = "Metric Code is required")]
    [StringLength(50, ErrorMessage = "Metric Code cannot exceed 50 characters")]
    [Display(Name = "Metric Code")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Metric Code must contain only uppercase letters, numbers, and underscores")]
    public string MetricCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Metric Name is required")]
    [StringLength(200, ErrorMessage = "Metric Name cannot exceed 200 characters")]
    [Display(Name = "Metric Name")]
    public string MetricName { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Threshold must be a positive number")]
    [Display(Name = "Green Threshold")]
    public decimal? ThresholdGreen { get; set; }
}
```

#### **Available Validation Attributes:**

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `[Required]` | Field must have a value | `[Required(ErrorMessage = "Name is required")]` |
| `[StringLength]` | Max/Min string length | `[StringLength(100, MinimumLength = 3)]` |
| `[Range]` | Number within range | `[Range(0, 100)]` |
| `[RegularExpression]` | Pattern matching | `[RegularExpression(@"^[A-Z]+$")]` |
| `[EmailAddress]` | Valid email format | `[EmailAddress]` |
| `[Phone]` | Valid phone format | `[Phone]` |
| `[Url]` | Valid URL format | `[Url]` |
| `[Compare]` | Compare with another property | `[Compare("Password")]` |
| `[Display]` | Friendly name for labels | `[Display(Name = "Full Name")]` |
| `[DataType]` | Data type hint | `[DataType(DataType.Password)]` |

---

### **Layer 2: Controller Validation (Server-Side)**

**File:** `Controllers/Metrics/MetricDefinitionsController.cs`

```csharp
[HttpPost("Create")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreateMetricDefinitionDto model)
{
    // 1. AUTOMATIC VALIDATION - ASP.NET Core checks ModelState
    if (!ModelState.IsValid)
    {
        // Return to form with error messages
        return View("~/Views/Metrics/MetricDefinitions/Create.cshtml", model);
    }

    // 2. CUSTOM BUSINESS LOGIC VALIDATION
    var exists = await _context.MetricDefinitions
        .AnyAsync(m => m.MetricCode == model.MetricCode);

    if (exists)
    {
        ModelState.AddModelError("MetricCode", "A metric with this code already exists.");
        return View("~/Views/Metrics/MetricDefinitions/Create.cshtml", model);
    }

    // 3. PROCEED WITH SAVE
    // ... save logic
}
```

#### **ModelState Methods:**

```csharp
// Check if model is valid
if (ModelState.IsValid) { }

// Add error for specific field
ModelState.AddModelError("FieldName", "Error message");

// Add general error (not tied to field)
ModelState.AddModelError(string.Empty, "General error message");

// Clear all errors
ModelState.Clear();

// Remove error for specific field
ModelState.Remove("FieldName");

// Get all errors
var errors = ModelState.Values.SelectMany(v => v.Errors);
```

---

### **Layer 3: View Validation Helpers**

**File:** `Views/Metrics/MetricDefinitions/_BasicInfo.cshtml`

```cshtml
<div class="mb-3">
    <label asp-for="MetricCode" class="form-label">
        Metric Code <span class="text-danger">*</span>
    </label>
    <input asp-for="MetricCode" class="form-control" />
    <span asp-validation-for="MetricCode" class="text-danger"></span>
    <small class="text-muted">Helper text here</small>
</div>
```

#### **Tag Helpers:**

| Tag Helper | Purpose |
|------------|---------|
| `asp-for` | Binds input to model property |
| `asp-validation-for` | Shows validation errors for property |
| `asp-validation-summary` | Shows all validation errors |

#### **Validation Summary Types:**

```cshtml
<!-- Show all errors -->
<div asp-validation-summary="All" class="text-danger"></div>

<!-- Show only model-level errors (not field-specific) -->
<div asp-validation-summary="ModelOnly" class="text-danger"></div>

<!-- Show none (default) -->
<div asp-validation-summary="None"></div>
```

---

## 🔧 Enhanced Validation Implementation

### **Step 1: Update DTO with Validation** ✅ (DONE)

File already updated with:
- ✅ `[Required]` with custom error messages
- ✅ `[StringLength]` with max lengths
- ✅ `[Display]` names for labels
- ✅ `[RegularExpression]` for MetricCode format
- ✅ `[Range]` for threshold values

---

### **Step 2: Add Validation Summary to View**

**File:** `Views/Metrics/MetricDefinitions/Create.cshtml`

Add at the top of the form (after opening `<form>` tag):

```cshtml
<form asp-action="Create" asp-controller="MetricDefinitions" method="post" id="metricDefinitionWizard">
    @Html.AntiForgeryToken()
    
    <!-- Add Validation Summary -->
    <div asp-validation-summary="ModelOnly" class="alert alert-danger" role="alert"></div>
    
    @if (wizard != null)
    {
        <partial name="~/Views/Shared/Components/Wizards/_VerticalWizard.cshtml" />
    }
</form>
```

---

### **Step 3: Ensure Validation Messages Display in Partials**

Each partial (`_BasicInfo.cshtml`, `_DataConfiguration.cshtml`, etc.) already has:

```cshtml
<span asp-validation-for="MetricCode" class="text-danger"></span>
```

✅ **Already implemented!**

---

## 🎨 Validation CSS Classes

ASP.NET Core automatically adds CSS classes to inputs based on validation state:

### **Bootstrap 5 Classes (Your Theme)**

```css
/* Valid field */
.is-valid {
    border-color: #198754;
}

/* Invalid field */
.is-invalid {
    border-color: #dc3545;
}

/* Error message styling */
.text-danger {
    color: #dc3545;
}

/* Validation summary */
.validation-summary-errors {
    color: #dc3545;
}
```

These classes are automatically applied by:
1. **Server-side:** ASP.NET Core adds classes based on ModelState
2. **Client-side:** jQuery Validation adds/removes classes dynamically

---

## 🔍 Client-Side Validation Behavior

### **When User Interacts:**

1. **On blur (leaving field):** Validates field
2. **On submit:** Validates entire form
3. **Real-time:** Updates as user types (configurable)

### **JavaScript Configuration:**

```javascript
// Default settings (already working)
$.validator.setDefaults({
    errorClass: 'is-invalid',
    validClass: 'is-valid',
    errorElement: 'span',
    errorPlacement: function(error, element) {
        error.addClass('text-danger small');
        error.insertAfter(element);
    }
});

// Custom validation rule
$.validator.addMethod("metricCode", function(value, element) {
    return /^[A-Z0-9_]+$/.test(value);
}, "Metric Code must contain only uppercase letters, numbers, and underscores");
```

---

## 📦 Custom Validation Attribute (Advanced)

For complex validation logic, create custom attributes:

**File:** `Validators/MetricCodeValidator.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace FormReporting.Validators
{
    public class MetricCodeValidatorAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var code = value.ToString();
            
            // Custom logic
            if (!code.StartsWith("MTR_"))
            {
                return new ValidationResult("Metric Code must start with 'MTR_'");
            }

            return ValidationResult.Success;
        }
    }
}
```

**Usage:**

```csharp
[MetricCodeValidator]
public string MetricCode { get; set; } = string.Empty;
```

---

## 🧪 Testing Validation

### **Test Scenarios:**

1. ✅ **Submit empty form** - Should show "required" errors
2. ✅ **Submit with invalid MetricCode** - Should show regex error
3. ✅ **Submit with duplicate code** - Should show business logic error
4. ✅ **Submit with too-long strings** - Should show length errors
5. ✅ **Submit with negative thresholds** - Should show range errors
6. ✅ **Submit valid form** - Should save successfully

### **Testing Client-Side:**

1. Open browser DevTools (F12)
2. Disable JavaScript
3. Submit form - server-side validation should still work
4. Enable JavaScript
5. Submit form - errors should show without page reload

---

## 🚀 Global Validation Best Practices

### **1. Always Validate on Both Sides**

```
Client-Side (UX) ←→ Server-Side (Security)
```

- **Client-side:** Fast feedback, better UX
- **Server-side:** Security, cannot be bypassed

### **2. Consistent Error Messages**

```csharp
// ❌ BAD
[Required]
[StringLength(50)]

// ✅ GOOD
[Required(ErrorMessage = "Metric Code is required")]
[StringLength(50, ErrorMessage = "Metric Code cannot exceed 50 characters")]
```

### **3. Use Display Names**

```csharp
// ✅ GOOD
[Display(Name = "Metric Code")]
public string MetricCode { get; set; }
```

Now `@Html.LabelFor(m => m.MetricCode)` displays "Metric Code" instead of "MetricCode"

### **4. Group Related Validations**

```csharp
// Validation for thresholds only if IsKPI is true
public class ConditionalThresholdValidation : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var model = (CreateMetricDefinitionDto)validationContext.ObjectInstance;
        
        if (model.IsKPI && !model.ThresholdGreen.HasValue)
        {
            return new ValidationResult("Green threshold is required for KPI metrics");
        }
        
        return ValidationResult.Success;
    }
}
```

---

## ✅ Current Status for Metric Definitions

### **What's Already Working:**

1. ✅ **Global validation enabled** (Program.cs)
2. ✅ **Client-side validation scripts included** (_ValidationScriptsPartial)
3. ✅ **DTO has validation attributes** (with error messages)
4. ✅ **Controller checks ModelState**
5. ✅ **Views have validation helpers** (asp-validation-for)
6. ✅ **Business logic validation** (duplicate check)

### **What Was Just Added:**

1. ✅ **Custom error messages** for all required fields
2. ✅ **Display names** for better labels
3. ✅ **RegularExpression** for MetricCode format
4. ✅ **Range validation** for thresholds

---

## 📄 Quick Reference

### **Common Validation Patterns:**

```csharp
// Required field
[Required(ErrorMessage = "{0} is required")]
[Display(Name = "Field Name")]
public string FieldName { get; set; }

// String with length
[StringLength(100, MinimumLength = 3, ErrorMessage = "{0} must be between {2} and {1} characters")]
public string Description { get; set; }

// Number range
[Range(1, 100, ErrorMessage = "{0} must be between {1} and {2}")]
public int Age { get; set; }

// Email
[EmailAddress(ErrorMessage = "Invalid email address")]
public string Email { get; set; }

// Pattern
[RegularExpression(@"^[A-Z]+$", ErrorMessage = "Only uppercase letters allowed")]
public string Code { get; set; }

// Compare fields (e.g., password confirmation)
[Compare("Password", ErrorMessage = "Passwords do not match")]
public string ConfirmPassword { get; set; }
```

---

## 🎯 Summary

Your project **already has global validation configured**. You just needed to:

1. ✅ Add validation attributes to DTOs (DONE)
2. ✅ Check ModelState in controllers (DONE)
3. ✅ Include validation scripts in views (DONE)
4. ✅ Use validation tag helpers (DONE)

**Validation now works automatically across your entire application!**

Any new DTO with validation attributes will automatically get both client-side and server-side validation without additional setup.
