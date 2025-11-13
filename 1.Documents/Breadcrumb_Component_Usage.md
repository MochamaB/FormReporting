# Automatic Breadcrumb Component

## Overview
The breadcrumb component automatically generates navigation breadcrumbs from route data, eliminating the need to manually write breadcrumb HTML in every view.

## Three-Layer Architecture
1. **Config Layer**: `BreadcrumbConfig` (optional custom configuration)
2. **Extension Layer**: `BreadcrumbExtensions` (auto-generation logic)
3. **ViewModel Layer**: `BreadcrumbViewModel` (render-ready data)
4. **Partial Layer**: `_Breadcrumb.cshtml` (ZERO logic rendering)

---

## Usage Examples

### 1. Automatic Breadcrumb (Simplest - Recommended)
Just include the partial - breadcrumb will be auto-generated from route data:

```cshtml
@{
    ViewData["Title"] = "Create New Region";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

@* Automatic Breadcrumb *@
<partial name="_Breadcrumb" />
```

**Result:**
- URL: `/Regions/Create`
- Breadcrumb: **Home > Regions > Create**
- Page Title: "Create New Region"

---

### 2. Custom Breadcrumb with Fluent API
Configure custom breadcrumb items:

```cshtml
@using FormReporting.Models.ViewModels.Components
@using FormReporting.Extensions

@{
    ViewData["Title"] = "Region Details";

    var breadcrumbConfig = new BreadcrumbConfig()
        .WithPageTitle("Nairobi Region")
        .WithItem("Regions", Url.Action("Index", "Regions"))
        .WithItem("Nairobi");

    ViewData["BreadcrumbViewModel"] = breadcrumbConfig.BuildBreadcrumb(ViewContext);
}

<partial name="_Breadcrumb" />
```

**Result:**
- Breadcrumb: **Home > Regions > Nairobi**
- Page Title: "Nairobi Region"

---

### 3. Custom Breadcrumb Without Home Link

```cshtml
@{
    var breadcrumbConfig = new BreadcrumbConfig()
        .WithoutHome()
        .WithPageTitle("Dashboard")
        .WithItem("Analytics");

    ViewData["BreadcrumbViewModel"] = breadcrumbConfig.BuildBreadcrumb(ViewContext);
}

<partial name="_Breadcrumb" />
```

**Result:**
- Breadcrumb: **Analytics**
- Page Title: "Dashboard"
- No Home link

---

### 4. Multi-Level Navigation

```cshtml
@{
    var breadcrumbConfig = new BreadcrumbConfig()
        .WithPageTitle("User Profile")
        .WithItem("Settings", Url.Action("Index", "Settings"))
        .WithItem("User Management", Url.Action("Index", "Users"))
        .WithItem("John Doe");

    ViewData["BreadcrumbViewModel"] = breadcrumbConfig.BuildBreadcrumb(ViewContext);
}

<partial name="_Breadcrumb" />
```

**Result:**
- Breadcrumb: **Home > Settings > User Management > John Doe**
- Page Title: "User Profile"

---

## How Auto-Generation Works

The `BuildBreadcrumbFromRoute()` method automatically generates breadcrumbs based on:

1. **Home Link**: Always added (unless disabled)
2. **Area**: Added if present (e.g., "Organizational")
3. **Controller**: Added if not "Home" (e.g., "Regions")
4. **Action**: Added if not "Index" (e.g., "Create", "Edit", "Details")

### Examples of Auto-Generated Breadcrumbs

| URL | Auto-Generated Breadcrumb |
|-----|--------------------------|
| `/Home/Index` | Home |
| `/Regions/Index` | Home > Regions |
| `/Regions/Create` | Home > Regions > Create |
| `/Regions/Edit/5` | Home > Regions > Edit |
| `/Regions/Details/5` | Home > Regions > Details |
| `/Users/Profile/john` | Home > Users > Profile |

---

## Customization Options

### 1. Custom Home Link Text and URL

```cshtml
var breadcrumbConfig = new BreadcrumbConfig()
    .WithHome("Dashboard", Url.Action("Index", "Dashboard"))
    .WithItem("Reports");
```

**Result:** **Dashboard > Reports**

---

### 2. Page Title from ViewData

```cshtml
@{
    ViewData["Title"] = "My Custom Title";
}

<partial name="_Breadcrumb" />
```

The page title will be extracted from `ViewData["Title"]` automatically.

---

## Benefits

✅ **No Manual HTML**: Never write `<ol class="breadcrumb">` again
✅ **Automatic Generation**: Works out of the box for standard CRUD pages
✅ **Customizable**: Full control when needed via fluent API
✅ **Consistent**: Same pattern across entire application
✅ **DRY**: Single source of truth for breadcrumb rendering
✅ **Three-Layer Pattern**: Follows established component architecture

---

## Migration from Old Breadcrumbs

### Before (Manual):
```cshtml
<div class="row">
    <div class="col-12">
        <div class="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 class="mb-sm-0">Create Region</h4>
            <div class="page-title-right">
                <ol class="breadcrumb m-0">
                    <li class="breadcrumb-item"><a href="@Url.Action("Index", "Home")">Home</a></li>
                    <li class="breadcrumb-item"><a href="@Url.Action("Index", "Regions")">Regions</a></li>
                    <li class="breadcrumb-item active">Create</li>
                </ol>
            </div>
        </div>
    </div>
</div>
```

### After (Automatic):
```cshtml
<partial name="_Breadcrumb" />
```

**Lines of code reduced: 14 → 1** 🎉

---

## Files

- **Config/ViewModel**: `Models/ViewModels/Components/BreadcrumbComponents.cs`
- **Extensions**: `Extensions/BreadcrumbExtensions.cs`
- **Partial**: `Views/Shared/_Breadcrumb.cshtml`

---

## Notes

- The breadcrumb partial is already included in `_Layout.cshtml` in most cases
- Use automatic generation for 90% of pages
- Use custom configuration for special cases (multi-level nav, custom titles)
- CamelCase names are automatically split (e.g., "RegionDetails" → "Region Details")
