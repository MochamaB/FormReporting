# Index Page Implementation Guide - KTDA Form Reporting System

## 📋 Overview

This guide demonstrates the **complete pattern** for implementing Index pages with **Statistic Cards** and **DataTables** using the three-layer component architecture.

**Pattern:** ViewModels → Extensions → Partial Views

---

## 🎯 Three-Layer Architecture

### **Layer 1: ViewModels** (WHAT data is needed)
- Pure POCOs (Plain Old CLR Objects)
- No business logic
- Configuration objects

### **Layer 2: Extensions** (HOW to transform data)
- All transformation logic
- Validation and defaults
- Fluent API methods

### **Layer 3: Partial Views** (HOW to render)
- Velzon HTML markup only
- Minimal C# logic
- Reusable components

---

## 📁 File Structure for Index Pages

```
Controllers/
└── Identity/
    └── RolesController.cs          # Controller with Index action

Views/
└── Identity/
    └── Roles/
        └── Index.cshtml             # View using components

Models/ViewModels/
├── Identity/
│   └── RolesIndexViewModel.cs      # Page-specific ViewModel
└── Components/
    ├── StatCardViewModel.cs         # Stat card configuration
    └── DataTableConfig.cs           # DataTable configuration

Extensions/
├── StatCardExtensions.cs            # Stat card transformation
└── DataTableExtensions.cs           # DataTable transformation

Views/Shared/Components/
├── StatisticCards/
│   └── _IconLeftCard.cshtml        # Stat card rendering
└── DataTable/
    ├── _SearchBox.cshtml            # Search component
    └── _FilterDropdown.cshtml       # Filter component
```

---

## 🔧 Complete Implementation Example

### **STEP 1: Controller - Calculate Statistics**

```csharp
// Controllers/Identity/RolesController.cs

using FormReporting.Data;
using FormReporting.Models.Entities.Identity;
using FormReporting.Models.ViewModels.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Controllers.Identity
{
    [Route("Identity/[controller]")]
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Display the roles index page with statistics and datatable
        /// </summary>
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? search, string? status, int? page)
        {
            // 1. BUILD QUERY with filters
            var query = _context.Roles
                .Include(r => r.ScopeLevel)
                .Include(r => r.UserRoles)
                .AsQueryable();

            // 2. APPLY SEARCH FILTER
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => 
                    r.RoleName.Contains(search) || 
                    r.RoleCode.Contains(search) ||
                    r.Description!.Contains(search));
            }

            // 3. APPLY STATUS FILTER
            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status.ToLower() == "active";
                query = query.Where(r => r.IsActive == isActive);
            }

            // 4. CALCULATE STATISTICS (for stat cards)
            var allRoles = await _context.Roles.ToListAsync();
            var totalRoles = allRoles.Count;
            var activeRoles = allRoles.Count(r => r.IsActive);
            var inactiveRoles = allRoles.Count(r => !r.IsActive);

            // 5. PAGINATION
            var pageSize = 10;
            var totalRecords = await query.CountAsync();
            var currentPage = page ?? 1;
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            var skip = (currentPage - 1) * pageSize;

            // 6. GET PAGINATED DATA
            var roles = await query
                .OrderBy(r => r.ScopeLevel.Level)
                .ThenBy(r => r.RoleName)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            // 7. BUILD VIEW MODEL
            var viewModel = new RolesIndexViewModel
            {
                Roles = roles.Select(r => new RoleViewModel
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    RoleCode = r.RoleCode,
                    Description = r.Description,
                    ScopeLevelName = r.ScopeLevel.ScopeName,
                    ScopeCode = r.ScopeLevel.ScopeCode,
                    Level = r.ScopeLevel.Level,
                    IsActive = r.IsActive,
                    UserCount = r.UserRoles.Count,
                    CreatedDate = r.CreatedDate
                }),
                TotalRoles = totalRoles,
                ActiveRoles = activeRoles,
                InactiveRoles = inactiveRoles
            };

            // 8. PASS PAGINATION DATA TO VIEW
            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;

            return View(viewModel);
        }
    }
}
```

**Key Points:**
- ✅ Controller calculates statistics
- ✅ Handles filtering and pagination
- ✅ Passes data via ViewModel and ViewBag
- ✅ No HTML or rendering logic

---

### **STEP 2: ViewModel - Define Data Structure**

```csharp
// Models/ViewModels/Identity/RolesIndexViewModel.cs

namespace FormReporting.Models.ViewModels.Identity
{
    /// <summary>
    /// ViewModel for Roles Index page
    /// Contains statistics and list of roles
    /// </summary>
    public class RolesIndexViewModel
    {
        // Statistics for stat cards
        public int TotalRoles { get; set; }
        public int ActiveRoles { get; set; }
        public int InactiveRoles { get; set; }

        // List of roles for datatable
        public IEnumerable<RoleViewModel> Roles { get; set; } = new List<RoleViewModel>();
    }

    /// <summary>
    /// Individual role data for table rows
    /// </summary>
    public class RoleViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ScopeLevelName { get; set; } = string.Empty;
        public string ScopeCode { get; set; } = string.Empty;
        public int Level { get; set; }
        public bool IsActive { get; set; }
        public int UserCount { get; set; }
        public DateTime CreatedDate { get; set; }

        // Computed properties for rendering
        public string StatusBadge => IsActive 
            ? "<span class=\"badge bg-success-subtle text-success\">Active</span>" 
            : "<span class=\"badge bg-danger-subtle text-danger\">Inactive</span>";

        public string ScopeBadge => $"<span class=\"badge bg-primary-subtle text-primary\">{ScopeLevelName}</span>";
    }
}
```

---

### **STEP 3: View - Configure Components**

```cshtml
@* Views/Identity/Roles/Index.cshtml *@

@using FormReporting.Extensions
@using FormReporting.Models.ViewModels.Components
@model FormReporting.Models.ViewModels.Identity.RolesIndexViewModel

@{
    ViewData["Title"] = "Roles Management";

    // ═══════════════════════════════════════════════════════════
    // SECTION 1: STATISTIC CARDS CONFIGURATION
    // ═══════════════════════════════════════════════════════════
    
    var statConfig = new StatsRowConfig
    {
        Titles = new List<string> { 
            "Total Roles", 
            "Active Roles", 
            "Inactive Roles" 
        },
        Values = new List<string> {
            Model.TotalRoles.ToString(),
            Model.ActiveRoles.ToString(),
            Model.InactiveRoles.ToString()
        },
        Icons = new List<string> {
            "ri-shield-user-line",
            "ri-checkbox-circle-line",
            "ri-close-circle-line"
        },
        ColorThemes = new List<string> { 
            "primary", 
            "success", 
            "danger" 
        },
        CardType = CardType.IconLeftCard,
        Subtitles = new List<string> {
            "System roles",
            "Currently active",
            "Deactivated roles"
        }
    };

    // Transform config into renderable cards
    var statCards = statConfig.BuildStatsRow();

    // ═══════════════════════════════════════════════════════════
    // SECTION 2: DATATABLE CONFIGURATION
    // ═══════════════════════════════════════════════════════════
    
    var tableConfig = new DataTableConfig
    {
        TableId = "rolesTable",
        Columns = new List<string> {
            "#",
            "Role Name",
            "Role Code",
            "Description",
            "Scope Level",
            "Level",
            "Users",
            "Status",
            "Actions"
        },
        EnableSearch = true,
        SearchBox = new SearchBoxConfig
        {
            ParameterName = "search",
            PlaceholderText = "Search roles...",
            CurrentValue = ViewBag.CurrentSearch,
            ActionUrl = Url.Action("Index", "Roles") ?? "/Identity/Roles"
        },
        FilterDropdowns = new List<FilterDropdownConfig>
        {
            new FilterDropdownConfig
            {
                Label = "Status",
                Options = new List<FilterOption>
                {
                    new FilterOption
                    {
                        Text = "All Status",
                        Value = "",
                        Url = Url.Action("Index", "Roles", new { search = ViewBag.CurrentSearch }),
                        IsActive = string.IsNullOrEmpty(ViewBag.CurrentStatus)
                    },
                    new FilterOption
                    {
                        Text = "Active",
                        Value = "active",
                        Url = Url.Action("Index", "Roles", new { search = ViewBag.CurrentSearch, status = "active" }),
                        IsActive = ViewBag.CurrentStatus == "active"
                    },
                    new FilterOption
                    {
                        Text = "Inactive",
                        Value = "inactive",
                        Url = Url.Action("Index", "Roles", new { search = ViewBag.CurrentSearch, status = "inactive" }),
                        IsActive = ViewBag.CurrentStatus == "inactive"
                    }
                }
            }
        },
        CreateButtonText = "Create New Role",
        CreateButtonUrl = Url.Action("Create", "Roles") ?? "/Identity/Roles/Create",
        ShowPagination = true,
        CurrentPage = ViewBag.CurrentPage,
        TotalPages = ViewBag.TotalPages,
        TotalRecords = ViewBag.TotalRecords,
        PageSize = ViewBag.PageSize
    };

    // Transform config into renderable table
    var table = tableConfig.BuildDataTable();

    // Calculate row number starting point for pagination
    int startingNumber = ((ViewBag.CurrentPage - 1) * ViewBag.PageSize) + 1;
}

<!-- ═══════════════════════════════════════════════════════════ -->
<!-- SECTION 3: RENDER STATISTIC CARDS -->
<!-- ═══════════════════════════════════════════════════════════ -->

<div class="row">
    @foreach (var card in statCards)
    {
        <partial name="~/Views/Shared/Components/StatisticCards/_IconLeftCard.cshtml" model="card" />
    }
</div>

<!-- ═══════════════════════════════════════════════════════════ -->
<!-- SECTION 4: RENDER DATATABLE -->
<!-- ═══════════════════════════════════════════════════════════ -->

<div class="row mt-3">
    <div class="col-xl-12">
        <div class="card">
            <!-- Card Header: Search, Filters, Create Button -->
            <div class="card-header">
                <div class="d-flex justify-content-between align-items-center flex-wrap w-100 gap-2">
                    <!-- Left: Search and Filters -->
                    <div class="d-flex gap-2 flex-wrap align-items-center">
                        @if (table.SearchBox != null)
                        {
                            <partial name="~/Views/Shared/Components/DataTable/_SearchBox.cshtml" model="table.SearchBox" />
                        }

                        @if (table.FilterDropdowns != null && table.FilterDropdowns.Any())
                        {
                            foreach (var filterDropdown in table.FilterDropdowns)
                            {
                                <partial name="~/Views/Shared/Components/DataTable/_FilterDropdown.cshtml" model="filterDropdown" />
                            }
                        }
                    </div>

                    <!-- Right: Create Button -->
                    <div class="d-flex gap-2">
                        @if (!string.IsNullOrEmpty(table.CreateButtonText))
                        {
                            <a href="@table.CreateButtonUrl" class="btn btn-primary">
                                <i class="ri-add-line me-1"></i>@table.CreateButtonText
                            </a>
                        }
                    </div>
                </div>
            </div>

            <!-- Card Body: Table -->
            <div class="card-body p-0">
                <div class="table-responsive">
                    <table class="@table.TableClasses" id="@table.TableId">
                        <thead class="bg-light">
                            <tr>
                                @foreach (var column in table.Columns)
                                {
                                    <th>@column</th>
                                }
                            </tr>
                        </thead>
                        <tbody>
                            @for (int i = 0; i < Model.Roles.Count(); i++)
                            {
                                var role = Model.Roles.ElementAt(i);
                                var rowNumber = startingNumber + i;
                                <tr>
                                    <td>@rowNumber</td>
                                    <td>@role.RoleName</td>
                                    <td><code>@role.RoleCode</code></td>
                                    <td>@role.Description</td>
                                    <td>@Html.Raw(role.ScopeBadge)</td>
                                    <td>Level @role.Level</td>
                                    <td>@role.UserCount</td>
                                    <td>@Html.Raw(role.StatusBadge)</td>
                                    <td>
                                        <!-- Action buttons -->
                                        <a href="@Url.Action("Edit", new { id = role.RoleId })" class="btn btn-sm btn-success">
                                            <i class="ri-pencil-line"></i>
                                        </a>
                                    </td>
                                </tr>
                            }
                        </tbody>
                    </table>
                </div>
            </div>

            <!-- Card Footer: Pagination -->
            @if (table.ShowPagination && table.TotalPages > 1)
            {
                <div class="card-footer">
                    <!-- Pagination markup -->
                </div>
            }
        </div>
    </div>
</div>
```

---

## 📊 Pattern Summary

### **Controller Responsibilities:**
1. ✅ Query database
2. ✅ Apply filters
3. ✅ Calculate statistics
4. ✅ Handle pagination
5. ✅ Build ViewModel
6. ✅ Pass data to view

### **View Responsibilities:**
1. ✅ Create configuration objects
2. ✅ Call extension methods
3. ✅ Render partial components
4. ✅ Loop through data

### **Extension Responsibilities:**
1. ✅ Transform config → viewmodels
2. ✅ Apply defaults
3. ✅ Validate data
4. ✅ Return renderable objects

### **Partial View Responsibilities:**
1. ✅ Render Velzon HTML
2. ✅ Use model properties
3. ✅ Minimal C# logic

---

## 🎯 Benefits

| Aspect | Traditional Approach | Component Pattern |
|--------|---------------------|-------------------|
| **Lines of Code** | 200+ per page | 50-80 per page |
| **Consistency** | Manual | Automatic |
| **Maintainability** | Update 77 views | Update 1 component |
| **Reusability** | Copy-paste | Import extension |
| **Type Safety** | Weak | Strong |
| **Testing** | Difficult | Easy |

---

## 🚀 Quick Start Template

```csharp
// 1. CONTROLLER
public async Task<IActionResult> Index()
{
    var data = await _context.YourEntity.ToListAsync();
    
    var viewModel = new YourIndexViewModel
    {
        TotalCount = data.Count,
        ActiveCount = data.Count(x => x.IsActive),
        Items = data
    };
    
    return View(viewModel);
}
```

```cshtml
@* 2. VIEW *@
@using FormReporting.Extensions
@using FormReporting.Models.ViewModels.Components

@{
    // Stat Cards
    var statConfig = new StatsRowConfig
    {
        Titles = new List<string> { "Total", "Active" },
        Values = new List<string> { Model.TotalCount.ToString(), Model.ActiveCount.ToString() },
        Icons = new List<string> { "ri-dashboard-line", "ri-checkbox-line" },
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

## 🔄 AJAX Mode DataTable (Alternative to TableContentRenderer)

For pages that need **dynamic data loading without page refresh** (e.g., tabbed interfaces, dashboards), use the **AJAX Mode** instead of `TableContentRenderer`.

### When to Use AJAX Mode

| Scenario | Use Server-Side | Use AJAX Mode |
|----------|-----------------|---------------|
| Simple list page | ✅ | |
| Tabbed interface | | ✅ |
| Dashboard panels | | ✅ |
| Frequent filtering | | ✅ |
| Large datasets | | ✅ |
| Real-time updates | | ✅ |

### AJAX Mode Features

- **Skeleton loader** shown during data fetch
- **No page reload** when filtering/paginating
- **API-driven** data loading
- **Column type rendering** (avatar, badge, date, actions)
- **Automatic pagination** controls

---

### AJAX Mode Implementation

#### **STEP 1: Ensure API Endpoint Exists**

The API must return data in this format:

```json
{
    "success": true,
    "data": [
        {
            "id": 1,
            "targetName": "Marketing Department",
            "targetDetail": "5 users",
            "targetIcon": "ri-building-line",
            "targetColor": "primary",
            "assignmentType": "Department",
            "typeColor": "info",
            "status": "Active",
            "statusColor": "success",
            "dueDate": "2024-12-31T00:00:00"
        }
    ],
    "pagination": {
        "page": 1,
        "pageSize": 10,
        "totalCount": 47,
        "totalPages": 5
    }
}
```

#### **STEP 2: Configure DataTable with AJAX Mode**

```cshtml
@using FormReporting.Models.ViewModels.Components
@using FormReporting.Extensions

@{
    var tableConfig = new DataTableConfig
    {
        TableId = "assignmentsTable",
        Columns = new List<string> { "", "Target", "Type", "Due Date", "Status", "Actions" },
        EnableSearch = true,
        SearchBox = new SearchBoxConfig
        {
            ParameterName = "search",
            PlaceholderText = "Search..."
        },
        EnableBulkActions = true,
        BulkActions = new List<BulkActionConfig>
        {
            new BulkActionConfig 
            { 
                Text = "Delete Selected", 
                ActionUrl = "/api/items/bulk-delete", 
                IconClass = "ri-delete-bin-line",
                ColorClass = "danger",
                RequiresConfirmation = true
            }
        },
        CreateButtonText = "Create New",
        CreateButtonUrl = "/items/create",
        ShowPagination = true,
        PageSize = 10,
        EnableSorting = true,
        EnableHover = true
    }
    // ═══════════════════════════════════════════════════════════
    // AJAX MODE CONFIGURATION
    // ═══════════════════════════════════════════════════════════
    .WithAjaxLoading("/api/items", skeletonRowCount: 5)
    .WithAjaxParam("categoryId", "123")  // Additional query params
    
    // ═══════════════════════════════════════════════════════════
    // COLUMN DEFINITIONS (maps API response fields to columns)
    // ═══════════════════════════════════════════════════════════
    .WithAvatarColumn("targetName", 
        iconField: "targetIcon", 
        secondaryField: "targetDetail", 
        colorField: "targetColor")
    .WithBadgeColumn("assignmentType", colorField: "typeColor")
    .WithDateColumn("dueDate")
    .WithBadgeColumn("status", colorField: "statusColor")
    .WithActionsColumn(
        new AjaxRowActionConfig { Text = "Edit", IconClass = "ri-pencil-line", Url = "/items/{id}/edit" },
        new AjaxRowActionConfig { Text = "View", IconClass = "ri-eye-line", Url = "/items/{id}" },
        new AjaxRowActionConfig { IsDivider = true },
        new AjaxRowActionConfig { Text = "Delete", IconClass = "ri-delete-bin-line", ColorClass = "danger", OnClick = "deleteItem({id})" }
    )
    
    // ═══════════════════════════════════════════════════════════
    // FILTERS (work with AJAX - no page reload)
    // ═══════════════════════════════════════════════════════════
    .WithFilterDropdown("Status", new List<FilterOption>
    {
        new FilterOption { Text = "All Status", Value = "", IsActive = true },
        new FilterOption { Text = "Active", Value = "Active" },
        new FilterOption { Text = "Inactive", Value = "Inactive" }
    })
    .WithFilterSelect("category", "Category", categoryOptions);

    var tableViewModel = tableConfig.BuildDataTable();
}

<!-- Render the DataTable -->
<partial name="~/Views/Shared/Components/DataTable/_DataTable.cshtml" model="tableViewModel" />
```

---

### Column Types Reference

| Type | Method | Description | Required Fields |
|------|--------|-------------|-----------------|
| **checkbox** | `WithCheckboxColumn("id")` | Row selection checkbox | `id` field |
| **text** | `WithTextColumn("name", "subtitle")` | Plain text with optional secondary | `name`, optional `subtitle` |
| **avatar** | `WithAvatarColumn("name", icon, secondary, color)` | Icon + text + secondary | `name`, `iconField`, `secondaryField`, `colorField` |
| **badge** | `WithBadgeColumn("status", "statusColor")` | Colored badge | `status`, `colorField` |
| **date** | `WithDateColumn("dueDate", "MMM dd, yyyy")` | Formatted date | `dueDate` |
| **actions** | `WithActionsColumn(...)` | Dropdown menu | Action configs |

---

### Action Configuration

```csharp
new AjaxRowActionConfig 
{ 
    Text = "Edit",                    // Display text
    IconClass = "ri-pencil-line",     // Remix icon class
    Url = "/items/{id}/edit",         // URL with {id} placeholder
    OnClick = "editItem({id})",       // JavaScript function (alternative to Url)
    ColorClass = "danger",            // For danger actions (red text)
    IsDivider = true                  // Renders <hr> divider
}
```

---

### Comparison: Server-Side vs AJAX Mode

```cshtml
@* ═══════════════════════════════════════════════════════════ *@
@* SERVER-SIDE MODE (Traditional) *@
@* ═══════════════════════════════════════════════════════════ *@

var tableConfig = new DataTableConfig
{
    TableId = "rolesTable",
    Columns = new List<string> { "Name", "Status", "Actions" },
    // ... other config
    
    // Server-side: Use TableContentRenderer
    TableContentRenderer = _ =>
    {
        var sb = new StringBuilder();
        foreach (var role in roles)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{role.Name}</td>");
            sb.Append($"<td><span class='badge bg-success'>{role.Status}</span></td>");
            sb.Append($"<td><a href='/roles/{role.Id}/edit'>Edit</a></td>");
            sb.Append("</tr>");
        }
        return new HtmlString(sb.ToString());
    }
};

@* ═══════════════════════════════════════════════════════════ *@
@* AJAX MODE (Dynamic) *@
@* ═══════════════════════════════════════════════════════════ *@

var tableConfig = new DataTableConfig
{
    TableId = "rolesTable",
    Columns = new List<string> { "Name", "Status", "Actions" },
    // ... other config
}
// AJAX mode: Define columns declaratively
.WithAjaxLoading("/api/roles")
.WithTextColumn("name")
.WithBadgeColumn("status", colorField: "statusColor")
.WithActionsColumn(
    new AjaxRowActionConfig { Text = "Edit", Url = "/roles/{id}/edit" }
);
```

---

### JavaScript API

The DataTable component exposes a reload function for each AJAX table:

```javascript
// Reload table data (e.g., after creating/deleting an item)
window.reloadTable_assignmentsTable();

// Or use the table ID
window['reloadTable_' + tableId]();
```

---

## ✅ Checklist for New Index Pages

- [ ] Controller calculates statistics
- [ ] Controller handles filtering
- [ ] Controller implements pagination
- [ ] ViewModel defined with statistics
- [ ] View creates StatsRowConfig
- [ ] View calls BuildStatsRow()
- [ ] View renders stat cards
- [ ] View creates DataTableConfig
- [ ] View renders table with data
- [ ] Pagination implemented
- [ ] Search functionality working
- [ ] Filter dropdowns configured

## ✅ Checklist for AJAX Mode DataTables

- [ ] API endpoint returns correct JSON format
- [ ] API supports filtering via query params
- [ ] API supports pagination (page, pageSize)
- [ ] DataTableConfig uses `.WithAjaxLoading(url)`
- [ ] Column mappings defined with `.With*Column()` methods
- [ ] Actions configured with `AjaxRowActionConfig`
- [ ] Filters use `.WithFilterDropdown()` or `.WithFilterSelect()`
- [ ] Bulk actions configured if needed
- [ ] Skeleton loader displays during loading
- [ ] Error state shows retry button

---

**This pattern is used across all 77 tables in the KTDA system for consistency and maintainability.**
