# Layout Files Guide

## Overview

The application has two main layout files to optimize the user experience for different types of pages.

---

## 📄 **Available Layouts**

### **1. _Layout.cshtml** (Default)
**File:** `Views/Shared/_Layout.cshtml`

**Purpose:** Standard layout for regular application pages

**Sidebar Behavior:**
- ✅ Sidebar is always expanded (250px width)
- ✅ Standard navigation experience
- ✅ Best for: Dashboards, lists, forms, reports, admin pages

**HTML Attribute:**
```html
data-sidebar-size="lg"
```

**Usage:**
This is the default layout. No need to specify unless overriding.
```cshtml
@{
    Layout = "~/Views/Shared/_Layout.cshtml";
}
```

**Used By:**
- Dashboard pages
- List/Index pages
- Standard CRUD forms
- Reports
- User management
- Most application pages

---

### **2. _BuilderLayout.cshtml** (Builder Views)
**File:** `Views/Shared/_BuilderLayout.cshtml`

**Purpose:** Optimized layout for builder interfaces (Form Builder, Role Builder, Workflow Builder)

**Sidebar Behavior:**
- ✅ Sidebar auto-collapses to ~70px width
- ✅ Expands to 250px on hover
- ✅ Toggle button to lock/unlock hover behavior
- ✅ Provides **180px more horizontal space** for builder canvases

**HTML Attribute:**
```html
data-sidebar-size="sm-hover"
```

**Usage:**
Explicitly specify this layout at the top of your view:
```cshtml
@{
    Layout = "~/Views/Shared/_BuilderLayout.cshtml";
    ViewData["Title"] = "Your Page Title";
}
```

**Used By:**
- ✅ Form Template Builder (`Forms/FormTemplates/Create.cshtml`)
- ✅ Form Builder Canvas (`Forms/FormTemplates/FormBuilder.cshtml`)
- ✅ Role Creation Wizard (`Identity/Roles/Create.cshtml`)
- ✅ Workflow Designer (if implemented)
- ✅ Report Designer (if implemented)
- ✅ Any interface with drag-and-drop or large canvas areas

---

## 🎯 **When to Use Which Layout**

### **Use _Layout.cshtml (Default) For:**
- 📊 Dashboards
- 📋 Data tables and lists
- 📝 Standard forms (Edit User, Add Tenant, etc.)
- 📈 Reports and analytics pages
- ⚙️ Settings and configuration pages
- 👥 User/Role/Department management (list views)

**Reason:** Users benefit from seeing the full navigation menu at all times.

---

### **Use _BuilderLayout.cshtml For:**
- 🎨 Form Template Builder (multi-step wizard)
- 🔧 Form Section/Field Builder (drag-and-drop canvas)
- 🛡️ Role Creation Wizard
- 🔀 Workflow Designer
- 📊 Report Configuration Builder
- 🎭 Any wizard or canvas-based interface

**Reason:** These interfaces need maximum horizontal space and minimal distractions. The auto-collapse sidebar provides:
- More room for the canvas/wizard
- Less visual clutter
- Quick access to menu on hover
- Professional UX similar to VS Code, Figma, etc.

---

## 🔧 **How the Hover Sidebar Works**

### **Collapsed State (Default)**
```
┌─────┐
│ [≡] │  ← Sidebar (~70px)
│ 🏠  │
│ 📝  │
│ 👥  │
└─────┘
```
- Only icons visible
- Text labels hidden
- ~70px width

### **Expanded State (On Hover)**
```
┌──────────────────┐
│ [≡] Dashboard    │  ← Sidebar (250px)
│ 🏠  Home          │
│ 📝  Forms         │
│ 👥  Users         │
└──────────────────┘
```
- Full menu visible
- Icons + text labels
- 250px width
- Smooth transition (0.3s)

### **Toggle Button**
Users can click the toggle button (🔘) in the sidebar to:
- ✅ Lock the expanded state (if they prefer it always open)
- ✅ Unlock back to auto-collapse mode

---

## 📝 **Implementation Example**

### **Standard Page (Uses Default _Layout.cshtml)**
```cshtml
@* File: Views/Identity/Users/Index.cshtml *@
@model List<UserViewModel>

@{
    ViewData["Title"] = "User Management";
    @* No layout specified - uses default _Layout.cshtml *@
}

<h1>User Management</h1>
@* ... rest of the page *@
```

### **Builder Page (Uses _BuilderLayout.cshtml)**
```cshtml
@* File: Views/Forms/FormTemplates/Create.cshtml *@
@using FormReporting.Models.ViewModels.Components

@{
    Layout = "~/Views/Shared/_BuilderLayout.cshtml";  @* ← Specify builder layout *@
    ViewData["Title"] = "Create Form Template";
}

@* 7-step wizard with auto-collapse sidebar *@
<partial name="Components/FormBuilder/_FormBuilderProgress" />
@* ... wizard content *@
```

---

## ⚙️ **Technical Details**

### **What Makes It Work**

**1. HTML Data Attribute**
```html
<!-- _Layout.cshtml -->
<html data-sidebar-size="lg">

<!-- _BuilderLayout.cshtml -->
<html data-sidebar-size="sm-hover">
```

**2. CSS (Already in app.min.css)**
The Velzon theme CSS automatically handles the hover behavior based on the `data-sidebar-size` attribute.

**3. JavaScript (Already in app.js)**
- Handles hover events
- Manages toggle button clicks
- Persists user preference to localStorage

**4. Sidebar Toggle Button (Already in _Sidebar.cshtml)**
```cshtml
<button type="button" 
        class="btn btn-sm p-0 fs-20 header-item float-end btn-vertical-sm-hover" 
        id="vertical-hover">
    <i class="ri-record-circle-line"></i>
</button>
```

---

## 🚀 **Adding to New Views**

### **Step 1: Determine Layout Needed**
Ask yourself:
- Is this a builder/wizard/canvas interface? → Use `_BuilderLayout`
- Is this a standard page? → Use default `_Layout` (no action needed)

### **Step 2: Add Layout Declaration**
If using builder layout, add at the top of your view:
```cshtml
@{
    Layout = "~/Views/Shared/_BuilderLayout.cshtml";
    ViewData["Title"] = "Your Page Title";
}
```

### **Step 3: Test**
- ✅ Sidebar should be collapsed by default
- ✅ Sidebar should expand on hover
- ✅ Toggle button should lock/unlock behavior
- ✅ More horizontal space for your content

---

## 📋 **Current Usage**

### **Views Using _BuilderLayout.cshtml:**
1. ✅ `Views/Forms/FormTemplates/Create.cshtml` - Form Template Wizard
2. 🔄 `Views/Identity/Roles/Create.cshtml` - Role Creation Wizard (planned)
3. 🔄 `Views/Forms/FormTemplates/FormBuilder.cshtml` - Form Builder Canvas (when implemented)
4. 🔄 `Views/Forms/FormTemplates/MetricMapping.cshtml` - Metric Mapping (when implemented)

### **Views Using _Layout.cshtml (Default):**
- All other views (Dashboard, Lists, Standard Forms, etc.)

---

## 🎨 **Customization (Future)**

If you need to create additional layout variants:

1. Copy `_Layout.cshtml` or `_BuilderLayout.cshtml`
2. Rename (e.g., `_FullScreenLayout.cshtml`)
3. Modify the `data-*` attributes:
   - `data-sidebar-size`: "lg", "sm-hover", "sm", "md"
   - `data-layout`: "vertical", "horizontal", "semibox"
   - `data-topbar`: "light", "dark"

---

## ✅ **Summary**

| **Layout** | **Sidebar** | **Width** | **Best For** |
|------------|-------------|-----------|--------------|
| `_Layout.cshtml` | Always expanded | 250px | Standard pages, dashboards, lists |
| `_BuilderLayout.cshtml` | Auto-collapse + hover | 70px → 250px | Builders, wizards, designers |

**Rule of Thumb:**
- 📄 Standard page → Default layout
- 🎨 Builder/Wizard → Builder layout
