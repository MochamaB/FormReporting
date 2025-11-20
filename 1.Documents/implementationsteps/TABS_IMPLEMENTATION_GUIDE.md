# Tabs Implementation Guide - KTDA Form Reporting System

## 📋 Overview

Tabs organize related content into separate panels, commonly used for **Edit pages** to group information logically. Tabs use the **same step partials as Wizards** for maximum reusability.

**Pattern:** Same three-layer architecture as Wizards, but simpler navigation (click any tab)

---

## 🎯 Key Difference: Wizards vs Tabs

| Feature | Wizard | Tabs |
|---------|--------|------|
| **Navigation** | Sequential (Next/Previous) | Free (click any tab) |
| **Validation** | Per-step before proceeding | On form submit |
| **Use Case** | Create workflows | Edit pages |
| **Content** | Same partials! | Same partials! |

---

## 🔧 Implementation Example

### **Controller (Edit Action)**

```csharp
[HttpGet("Edit/{id}")]
public async Task<IActionResult> Edit(int id)
{
    var tenant = await _context.Tenants
        .Include(t => t.Departments)
        .Include(t => t.TenantGroupMembers)
        .FirstOrDefaultAsync(t => t.TenantId == id);
    
    if (tenant == null) return NotFound();
    
    var model = new TenantEditViewModel
    {
        TenantId = tenant.TenantId,
        TenantCode = tenant.TenantCode,
        TenantName = tenant.TenantName,
        // ... map all properties
    };
    
    // Load dropdown data
    ViewBag.Regions = await _context.Regions.ToListAsync();
    ViewBag.ExistingDepartments = tenant.Departments;
    
    return View(model);
}
```

### **View (Edit.cshtml)**

```cshtml
@model TenantEditViewModel

<div class="row">
    <div class="col-12">
        <div class="card">
            <div class="card-body">
                <form method="post" action="@Url.Action("Edit")">
                    @Html.AntiForgeryToken()
                    @Html.HiddenFor(m => m.TenantId)
                    
                    <!-- Nav Tabs -->
                    <ul class="nav nav-tabs nav-tabs-custom nav-success mb-3" role="tablist">
                        <li class="nav-item">
                            <a class="nav-link active" data-bs-toggle="tab" href="#basic" role="tab">
                                <i class="ri-information-line me-1"></i>Basic Details
                            </a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" data-bs-toggle="tab" href="#departments" role="tab">
                                <i class="ri-building-4-line me-1"></i>Departments
                            </a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" data-bs-toggle="tab" href="#groups" role="tab">
                                <i class="ri-group-line me-1"></i>Groups
                            </a>
                        </li>
                    </ul>
                    
                    <!-- Tab Content -->
                    <div class="tab-content">
                        <div class="tab-pane fade show active" id="basic" role="tabpanel">
                            @* Reuse same partial from wizard! *@
                            <partial name="~/Views/Organizational/Tenants/Partials/_BasicDetails.cshtml" model="Model" />
                        </div>
                        
                        <div class="tab-pane fade" id="departments" role="tabpanel">
                            <partial name="~/Views/Organizational/Tenants/Partials/_Departments.cshtml" model="Model" />
                        </div>
                        
                        <div class="tab-pane fade" id="groups" role="tabpanel">
                            <partial name="~/Views/Organizational/Tenants/Partials/_Groups.cshtml" model="Model" />
                        </div>
                    </div>
                    
                    <!-- Submit Button -->
                    <div class="mt-4">
                        <button type="submit" class="btn btn-primary">
                            <i class="ri-save-line me-1"></i>Update Tenant
                        </button>
                        <a href="@Url.Action("Index")" class="btn btn-secondary ms-2">Cancel</a>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>
```

---

## ✨ **Key Benefits**

1. ✅ **Reuse wizard partials** - No duplicate code
2. ✅ **Simple implementation** - Standard Bootstrap tabs
3. ✅ **Free navigation** - Users can jump to any tab
4. ✅ **Same content flexibility** - Plain HTML, SimpleForms, DataTables, StatCards

---

## 🎨 **Tab Content = Same Partials as Wizard**

```
Wizard (Create.cshtml)                    Tabs (Edit.cshtml)
├── Step 1 → _BasicDetails.cshtml    ←→  Tab 1 → _BasicDetails.cshtml
├── Step 2 → _Departments.cshtml     ←→  Tab 2 → _Departments.cshtml
└── Step 3 → _Groups.cshtml          ←→  Tab 3 → _Groups.cshtml
```

**Same partial file, different context!**

---

## 📊 **When to Use Tabs**

✅ **Use Tabs for:**
- Edit pages with multiple sections
- Viewing related information groups
- Settings/configuration pages

❌ **Use Wizard instead for:**
- Create workflows (guided step-by-step)
- Processes requiring validation per step
- Sequential data entry

---

**Tabs are perfect for Edit pages where users need quick access to different sections without sequential constraints.**
