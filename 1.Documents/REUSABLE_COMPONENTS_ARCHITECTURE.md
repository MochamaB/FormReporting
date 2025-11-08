# Reusable Components Architecture - KTDA ICT Reporting System

## 🎯 Design Philosophy

**Three-Layer Pattern:**
1. **ViewModels** - Define WHAT data is needed (pure POCOs, no logic)
2. **Extensions** - Define HOW to transform data (all business logic here)
3. **Partial Views** - Define HOW to render (Velzon HTML only)

**Golden Rules:**
- Views only provide data configuration
- Extensions handle all transformation logic
- Partial views only contain rendering markup
- No logic in views, no HTML in extensions
- Reusable across all 12 modules (77 tables)

---

## 📁 Complete File Structure

```
FormReporting/
├── Models/ViewModels/Components/
│   ├── StatCardViewModel.cs              # Statistic cards
│   ├── DataTableConfig.cs                # Tables (future)
│   ├── TabConfig.cs                      # Tabs (future)
│   ├── FormConfig.cs                     # Forms (future)
│   └── WizardConfig.cs                   # Wizards (future)
│
├── Extensions/
│   ├── StatCardExtensions.cs             # ✅ Implemented
│   ├── DataTableExtensions.cs            # Future
│   ├── TabExtensions.cs                  # Future
│   ├── FormExtensions.cs                 # Future
│   └── WizardExtensions.cs               # Future
│
└── Views/Shared/Components/
    ├── StatisticCards/
    │   ├── _TileBoxCard.cshtml           # ✅ Implemented
    │   ├── _StatisticsCard.cshtml        # ✅ Implemented
    │   ├── _IconLeftCard.cshtml          # ✅ Implemented
    │   ├── _CRMWidgetCard.cshtml         # ✅ Implemented
    │   └── _TestAllCards.cshtml          # ✅ Test showcase
    │
    ├── DataTables/
    │   └── _DataTable.cshtml             # Future
    │
    ├── Tabs/
    │   ├── _HorizontalTabs.cshtml        # Future
    │   └── _VerticalTabs.cshtml          # Future
    │
    ├── Forms/
    │   └── _StandardForm.cshtml          # Future
    │
    └── Wizards/
        ├── _HorizontalWizard.cshtml      # Future
        └── _VerticalWizard.cshtml        # Future
```

---

## 🏗️ STATISTIC CARDS - Complete Implementation

### **Card Types Available**

| Card Type | Best For | Key Features | Example Use Case |
|-----------|----------|--------------|------------------|
| **TileBoxCard** | Dashboard overview | Trend indicator, link, icon right | Total Tenants, Active Users, Form Submissions |
| **StatisticsCard** | Analytics metrics | Comparison badge, "vs. previous month" | User Sessions, Page Views, Bounce Rate |
| **IconLeftCard** | Summary metrics | Icon left, subtitle, trend right | Total Revenue, Number of Stores, Sales |
| **CRMWidgetCard** | Grouped metrics | Multiple stats in one card | Campaign metrics, Sales pipeline stages |

### **Architecture Layers**

#### **Layer 1: ViewModel - Pure Data Structure**

```csharp
// Models/ViewModels/Components/StatCardViewModel.cs

// Configuration object (what you create in views)
public class StatsRowConfig
{
    public List<string> Titles { get; set; }           // Required
    public List<string> Values { get; set; }           // Required
    public List<string> Icons { get; set; }            // Required
    public List<string>? ColorThemes { get; set; }     // Optional
    public CardType CardType { get; set; }

    // Optional features
    public List<string>? LinkTexts { get; set; }
    public List<string>? LinkUrls { get; set; }
    public List<string>? TrendPercentages { get; set; }
    public List<TrendDirection>? TrendDirections { get; set; }
    // ... more optional properties
}

// Individual card model (created by extensions)
public class StatCardViewModel
{
    public string Title { get; set; }
    public string Value { get; set; }
    public string Icon { get; set; }
    public string ColorTheme { get; set; }
    public CardType CardType { get; set; }
    // ... all optional properties
}

// Enums
public enum CardType
{
    TileBoxCard,
    CRMWidgetCard,
    StatisticsCard,
    IconLeftCard
}

public enum TrendDirection { Up, Down, Neutral }
```

**Design Notes:**
- `StatsRowConfig` = What you build in views (easy to create)
- `StatCardViewModel` = What extensions produce (for rendering)
- Parallel arrays pattern (Titles[i], Values[i], Icons[i] go together)
- Optional properties default to null (extensions handle missing data)

---

#### **Layer 2: Extensions - All Transformation Logic**

```csharp
// Extensions/StatCardExtensions.cs

public static class StatCardExtensions
{
    // Main transformation method
    public static List<StatCardViewModel> BuildStatsRow(this StatsRowConfig config)
    {
        // 1. Validate inputs
        // 2. Ensure all arrays same length (pad with defaults)
        // 3. Loop through and create StatCardViewModel for each
        // 4. Apply optional properties
        // 5. Return list of viewmodels
    }

    // Fluent API methods
    public static StatsRowConfig WithCardType(this StatsRowConfig config, CardType type) { ... }
    public static StatsRowConfig WithTrends(this StatsRowConfig config, ...) { ... }
    public static StatsRowConfig WithLinks(this StatsRowConfig config, ...) { ... }
    public static StatsRowConfig WithComparisons(this StatsRowConfig config, ...) { ... }

    // Helper methods
    public static (string icon, string colorClass) GetTrendIndicator(this TrendDirection direction)
    {
        return direction switch
        {
            TrendDirection.Up => ("ri-arrow-right-up-line", "text-success"),
            TrendDirection.Down => ("ri-arrow-right-down-line", "text-danger"),
            TrendDirection.Neutral => ("ri-subtract-line", "text-muted")
        };
    }

    // Test data generator
    public static StatsRowConfig CreateDefaultTestConfig(CardType cardType)
    {
        // Returns fully populated config with realistic test data
    }
}
```

**Design Notes:**
- Extensions are **pure functions** (no side effects)
- All defaults handled here (views never worry about missing data)
- Fluent API enables chaining: `config.WithTrends(...).WithLinks(...)`
- Test data generator for rapid prototyping
- Private helper methods keep public API clean

---

#### **Layer 3: Partial Views - Pure Rendering**

```cshtml
@* Views/Shared/Components/StatisticCards/_TileBoxCard.cshtml *@

@model FormReporting.Models.ViewModels.Components.StatCardViewModel
@using FormReporting.Extensions

@{
    var (trendIcon, trendColor) = Model.TrendDirection.GetTrendIndicator();
    var hasBackground = Model.HasBackgroundColor;
    var bgClass = hasBackground ? $"bg-{Model.ColorTheme}" : "";
}

<div class="@Model.ColumnClass">
    <div class="card card-animate @bgClass">
        <div class="card-body">
            <!-- Velzon HTML markup only -->
            <!-- Use Model properties, no calculations -->
        </div>
    </div>
</div>
```

**Design Notes:**
- Receives `StatCardViewModel` (already fully prepared)
- Only contains Velzon HTML markup
- Minimal C# logic (only for CSS class building)
- Uses extension methods for helper calculations (trend indicators)
- Responsive classes from Bootstrap

---

### **Data Flow Diagram**

```
┌──────────────────────────────────────────────────────────┐
│ 1. CONTROLLER: Calculate Statistics                      │
│    ViewBag.TotalTenants = tenants.Count;                │
│    ViewBag.ActiveTenants = tenants.Count(t => t.IsActive); │
└─────────────────────┬────────────────────────────────────┘
                      ↓
┌──────────────────────────────────────────────────────────┐
│ 2. VIEW: Create StatsRowConfig (pure data)              │
│    var config = new StatsRowConfig                       │
│    {                                                      │
│        Titles = ["Total Tenants", "Active"],            │
│        Values = [ViewBag.TotalTenants, ViewBag.Active], │
│        Icons = ["ri-building-line", "ri-check-line"],   │
│        CardType = CardType.TileBoxCard                   │
│    };                                                     │
└─────────────────────┬────────────────────────────────────┘
                      ↓
┌──────────────────────────────────────────────────────────┐
│ 3. EXTENSION: config.BuildStatsRow()                     │
│    - Validates config                                     │
│    - Pads arrays to same length with defaults            │
│    - Creates List<StatCardViewModel>                     │
│    - Applies color themes (defaults if not provided)     │
│    - Returns ready-to-render viewmodels                  │
└─────────────────────┬────────────────────────────────────┘
                      ↓
┌──────────────────────────────────────────────────────────┐
│ 4. VIEW: Loop and render partials                        │
│    @foreach (var card in cards)                          │
│    {                                                      │
│        <partial name="_TileBoxCard" model="card" />      │
│    }                                                      │
└─────────────────────┬────────────────────────────────────┘
                      ↓
┌──────────────────────────────────────────────────────────┐
│ 5. PARTIAL: Render Velzon HTML                           │
│    <div class="card card-animate">                       │
│        <h4>@Model.Title</h4>                             │
│        <span>@Model.Value</span>                         │
│    </div>                                                 │
└──────────────────────────────────────────────────────────┘
```

---

## 📚 Usage Examples

### **Example 1: Simple Dashboard Cards (Minimal)**

```csharp
// Controller
public async Task<IActionResult> Dashboard()
{
    var tenants = await _context.Tenants.ToListAsync();
    ViewBag.TotalTenants = tenants.Count;
    ViewBag.ActiveTenants = tenants.Count(t => t.IsActive);
    ViewBag.InactiveTenants = tenants.Count(t => !t.IsActive);

    return View();
}
```

```cshtml
@* View *@
@using FormReporting.Extensions
@using FormReporting.Models.ViewModels.Components

@{
    var config = new StatsRowConfig
    {
        Titles = new List<string> { "Total Tenants", "Active", "Inactive" },
        Values = new List<string>
        {
            ViewBag.TotalTenants.ToString(),
            ViewBag.ActiveTenants.ToString(),
            ViewBag.InactiveTenants.ToString()
        },
        Icons = new List<string> { "ri-building-line", "ri-checkbox-circle-line", "ri-close-circle-line" },
        ColorThemes = new List<string> { "primary", "success", "danger" },
        CardType = CardType.TileBoxCard
    };

    var cards = config.BuildStatsRow();
}

<div class="row">
    @foreach (var card in cards)
    {
        <partial name="~/Views/Shared/Components/StatisticCards/_TileBoxCard.cshtml" model="card" />
    }
</div>
```

**Result:** 3 beautiful statistic cards showing tenant counts

---

### **Example 2: Advanced Cards with All Features**

```cshtml
@{
    var config = new StatsRowConfig
    {
        Titles = new List<string> { "Total Revenue", "New Users", "Conversions", "Active Sessions" },
        Values = new List<string> { "$471k", "245", "32.5%", "1,245" },
        Icons = new List<string> { "ri-money-dollar-circle-line", "ri-user-add-line", "ri-percent-line", "ri-pulse-line" },
        ColorThemes = new List<string> { "primary", "success", "warning", "info" },
        CardType = CardType.TileBoxCard,

        // Add trends
        TrendPercentages = new List<string> { "16.24", "12.5", "3.2", "8.7" },
        TrendDirections = new List<TrendDirection>
        {
            TrendDirection.Up,
            TrendDirection.Up,
            TrendDirection.Down,
            TrendDirection.Up
        },

        // Add bottom links
        LinkTexts = new List<string> { "View details", "See all users", "Analyze", "Monitor" },
        LinkUrls = new List<string> { "/revenue", "/users", "/conversions", "/sessions" }
    };

    var cards = config.BuildStatsRow();
}

<div class="row">
    @foreach (var card in cards)
    {
        <partial name="~/Views/Shared/Components/StatisticCards/_TileBoxCard.cshtml" model="card" />
    }
</div>
```

---

### **Example 3: Fluent API Style**

```cshtml
@{
    var cards = new StatsRowConfig
    {
        Titles = new List<string> { "Users", "Sessions" },
        Values = new List<string> { "28.05k", "97.66k" },
        Icons = new List<string> { "ri-user-line", "ri-pulse-line" }
    }
    .WithCardType(CardType.StatisticsCard)
    .WithComparisons(
        badgeValues: new List<string> { "16.24 %", "3.96 %" },
        badgeColors: new List<string> { "success", "danger" }
    )
    .BuildStatsRow();
}
```

---

### **Example 4: CRM Widget (Multiple Stats in One Card)**

```cshtml
@{
    var crmCards = new List<StatCardViewModel>
    {
        new() { Title = "Campaign Sent", Value = "197", Icon = "ri-space-ship-line", TrendDirection = TrendDirection.Up },
        new() { Title = "Annual Profit", Value = "$489.4k", Icon = "ri-exchange-dollar-line", TrendDirection = TrendDirection.Up },
        new() { Title = "Lead Conversion", Value = "32.89%", Icon = "ri-pulse-line", TrendDirection = TrendDirection.Down }
    };
}

<div class="row">
    <partial name="~/Views/Shared/Components/StatisticCards/_CRMWidgetCard.cshtml" model="crmCards" />
</div>
```

---

### **Example 5: Using Test Data (Rapid Prototyping)**

```cshtml
@* No controller data needed - uses built-in test data *@

@using FormReporting.Extensions

@{
    var testConfig = StatCardExtensions.CreateDefaultTestConfig(CardType.IconLeftCard);
    var cards = testConfig.BuildStatsRow();
}

<div class="row">
    @foreach (var card in cards)
    {
        <partial name="~/Views/Shared/Components/StatisticCards/_IconLeftCard.cshtml" model="card" />
    }
</div>
```

**Use Case:** Quickly build UI mockups before implementing backend logic

---

## 🎨 Customization Options

### **Color Themes**
- `primary` (blue)
- `secondary` (purple)
- `success` (green)
- `warning` (orange)
- `danger` (red)
- `info` (cyan)

### **Column Sizes (Responsive Grid)**
```csharp
// 4 cards per row on large screens
ColumnClass = "col-xl-3 col-md-6"

// 3 cards per row
ColumnClass = "col-xl-4 col-md-6"

// 2 cards per row
ColumnClass = "col-xl-6 col-md-12"

// Full width
ColumnClass = "col-12"
```

### **Background Colors**
```csharp
var cards = config.BuildStatsRow();
cards[1].HasBackgroundColor = true; // 2nd card gets bg-{ColorTheme} class
```

---

## 🔧 Integration with KTDA System

### **Multi-Tenant Filtering**
```csharp
// In controller - automatically filter by user's tenant access
var userTenantIds = GetUserTenantIds(User); // From claims
var tenants = await _context.Tenants
    .Where(t => userTenantIds.Contains(t.TenantId))
    .ToListAsync();

ViewBag.TotalTenants = tenants.Count;
```

### **Permission-Based Visibility**
```cshtml
@* Only show link if user has permission *@
@{
    var linkUrls = new List<string>();
    linkUrls.Add(User.HasClaim("Permissions", "Tenants.View") ? "/tenants" : "#");
}
```

### **Role-Level Statistics**
```csharp
// Show different stats based on user role level
var roleLevel = GetUserRoleLevel(User);

if (roleLevel == 1) // HeadOffice
{
    ViewBag.TotalTenants = _context.Tenants.Count();
    ViewBag.TotalRegions = _context.Regions.Count();
}
else if (roleLevel == 2) // Regional
{
    var userRegionId = GetUserRegionId(User);
    ViewBag.RegionalTenants = _context.Tenants.Count(t => t.RegionId == userRegionId);
}
else // Factory-level
{
    var userTenantId = GetUserTenantId(User);
    ViewBag.ActiveUsers = _context.Users.Count(u => u.TenantId == userTenantId && u.IsActive);
}
```

---

## 📊 Real KTDA Examples

### **Section 1: Organizational Dashboard**
```cshtml
@{
    var orgStats = new StatsRowConfig
    {
        Titles = new List<string> { "Total Regions", "Total Tenants", "Head Office", "Subsidiaries" },
        Values = new List<string>
        {
            ViewBag.TotalRegions.ToString(),
            ViewBag.TotalTenants.ToString(),
            "1",
            ViewBag.Subsidiaries.ToString()
        },
        Icons = new List<string> { "ri-map-pin-line", "ri-building-line", "ri-home-office-line", "ri-building-2-line" },
        ColorThemes = new List<string> { "primary", "success", "warning", "info" },
        CardType = CardType.TileBoxCard,
        LinkTexts = new List<string> { "View regions", "View tenants", "View details", "View all" },
        LinkUrls = new List<string> { "/regions", "/tenants", "/head-office", "/subsidiaries" }
    };
}
```

### **Section 2: Identity & Access Management Dashboard**
```cshtml
@{
    var iamStats = new StatsRowConfig
    {
        Titles = new List<string> { "Total Users", "Active Users", "Roles", "Permissions" },
        Values = new List<string>
        {
            ViewBag.TotalUsers.ToString(),
            ViewBag.ActiveUsers.ToString(),
            ViewBag.TotalRoles.ToString(),
            ViewBag.TotalPermissions.ToString()
        },
        Icons = new List<string> { "ri-user-line", "ri-checkbox-circle-line", "ri-shield-user-line", "ri-lock-line" },
        ColorThemes = new List<string> { "primary", "success", "info", "warning" },
        CardType = CardType.IconLeftCard,
        Subtitles = new List<string>
        {
            $"From {ViewBag.LastMonthUsers} last month",
            $"From {ViewBag.LastMonthActive} last month",
            $"{ViewBag.HeadOfficeRoles} HeadOffice roles",
            $"Across {ViewBag.ModulesCount} modules"
        }
    };
}
```

### **Section 3: Metrics & KPI Dashboard**
```cshtml
@{
    var metricsStats = new StatsRowConfig
    {
        Titles = new List<string> { "Total Metrics", "Auto-Calculated", "Compliance Checks", "Thresholds Exceeded" },
        Values = new List<string>
        {
            ViewBag.TotalMetrics.ToString(),
            ViewBag.AutoMetrics.ToString(),
            ViewBag.ComplianceMetrics.ToString(),
            ViewBag.ThresholdsExceeded.ToString()
        },
        Icons = new List<string> { "ri-dashboard-line", "ri-calculator-line", "ri-checkbox-circle-line", "ri-alert-line" },
        ColorThemes = new List<string> { "primary", "info", "success", "danger" },
        CardType = CardType.StatisticsCard,
        BadgeValues = new List<string>
        {
            $"{ViewBag.MetricsGrowth}%",
            $"{ViewBag.AutoGrowth}%",
            $"{ViewBag.ComplianceRate}%",
            $"{ViewBag.ThresholdRate}%"
        },
        BadgeColors = new List<string> { "success", "success", "success", "danger" }
    };
}
```

### **Section 4: Form Templates Dashboard**
```cshtml
@{
    var formStats = new StatsRowConfig
    {
        Titles = new List<string> { "Total Templates", "Active Templates", "Submissions (This Month)", "Completion Rate" },
        Values = new List<string>
        {
            ViewBag.TotalTemplates.ToString(),
            ViewBag.ActiveTemplates.ToString(),
            ViewBag.MonthlySubmissions.ToString(),
            $"{ViewBag.CompletionRate}%"
        },
        Icons = new List<string> { "ri-file-list-line", "ri-file-check-line", "ri-file-text-line", "ri-percent-line" },
        ColorThemes = new List<string> { "primary", "success", "info", "warning" },
        CardType = CardType.TileBoxCard,
        TrendPercentages = new List<string>
        {
            ViewBag.TemplateGrowth.ToString(),
            ViewBag.ActiveGrowth.ToString(),
            ViewBag.SubmissionGrowth.ToString(),
            ViewBag.CompletionGrowth.ToString()
        },
        TrendDirections = new List<TrendDirection>
        {
            ViewBag.TemplateGrowth > 0 ? TrendDirection.Up : TrendDirection.Down,
            ViewBag.ActiveGrowth > 0 ? TrendDirection.Up : TrendDirection.Down,
            ViewBag.SubmissionGrowth > 0 ? TrendDirection.Up : TrendDirection.Down,
            ViewBag.CompletionGrowth > 0 ? TrendDirection.Up : TrendDirection.Down
        }
    };
}
```

---

## 🧪 Testing Components

### **Quick Test URLs**

After running the application, navigate to these URLs to test the components:

1. **All Card Types Test:** `http://localhost:[PORT]/ComponentTest/StatCards`
   - Shows all 4 card types with test data
   - Includes variations (normal + background colors)
   - Contains usage examples and code snippets

2. **Real Data Example:** `http://localhost:[PORT]/ComponentTest/RealDataExample`
   - Shows cards with simulated KTDA data
   - Demonstrates real-world usage across all sections
   - Includes implementation notes

### **Test Controller**

The `ComponentTestController` is already created with these actions:

```csharp
// View all card types with test data
public IActionResult StatCards()
{
    return View("~/Views/Test/StatCards.cshtml");
}

// View cards with simulated real data
public IActionResult RealDataExample()
{
    ViewBag.TotalTenants = 77;
    ViewBag.ActiveTenants = 69;
    // ... more test data
    return View("~/Views/Test/RealDataExample.cshtml");
}
```

### **Run the Application**

```bash
dotnet run
```

Then navigate to `/ComponentTest/StatCards` to see all card types in action.

---

## 🚀 Benefits

| Feature | Old Way (Manual HTML) | New Way (Component Pattern) |
|---------|----------------------|----------------------------|
| **Development Speed** | 50+ lines HTML per card | 5 lines config per card |
| **Consistency** | Manual styling | Automatic from Velzon |
| **Maintainability** | Update 77 views | Update 1 partial |
| **Testability** | Hard to test | Easy with test data |
| **Type Safety** | Strings everywhere | Strongly-typed POCOs |
| **Reusability** | Copy-paste | Import extension |
| **Multi-Tenant** | Manual filtering | Built into pattern |
| **Permissions** | Manual checks | Integrated in config |

---

## 📝 Template for Creating New Components

Want to create DataTable, Tabs, Forms, or Wizard components? Follow this exact pattern:

### **Step 1: Create ViewModels**

```csharp
// Models/ViewModels/Components/YourComponentConfig.cs

// Config object (what user creates)
public class YourComponentConfig
{
    public List<string> RequiredProperty { get; set; } = new();
    public List<string>? OptionalProperty { get; set; }
    public ComponentType Type { get; set; }
}

// Individual item model (created by extensions)
public class YourComponentViewModel
{
    public string RequiredProperty { get; set; } = string.Empty;
    public string? OptionalProperty { get; set; }
}

// Enum for component types/variations
public enum ComponentType
{
    Type1,
    Type2
}
```

### **Step 2: Create Extensions**

```csharp
// Extensions/YourComponentExtensions.cs

public static class YourComponentExtensions
{
    // Main transformation method
    public static List<YourComponentViewModel> BuildComponent(this YourComponentConfig config)
    {
        // 1. Validate
        // 2. Transform config → viewmodels
        // 3. Apply defaults
        // 4. Return list
    }

    // Fluent API methods
    public static YourComponentConfig WithFeature(this YourComponentConfig config, ...) { ... }

    // Test data generator
    public static YourComponentConfig CreateDefaultTestConfig() { ... }
}
```

### **Step 3: Create Partial Views**

```cshtml
@* Views/Shared/Components/YourComponent/_Type1.cshtml *@

@model FormReporting.Models.ViewModels.Components.YourComponentViewModel

@* Pure Velzon HTML markup only *@
<div class="velzon-component-class">
    @Model.RequiredProperty
    @if (!string.IsNullOrEmpty(Model.OptionalProperty))
    {
        <span>@Model.OptionalProperty</span>
    }
</div>
```

### **Step 4: Create Test View**

```cshtml
@* Views/Shared/Components/YourComponent/_TestComponent.cshtml *@

@using FormReporting.Extensions

@{
    var config = YourComponentExtensions.CreateDefaultTestConfig();
    var items = config.BuildComponent();
}

@foreach (var item in items)
{
    <partial name="~/Views/Shared/Components/YourComponent/_Type1.cshtml" model="item" />
}
```

### **Step 5: Document in Reference**

Add section to this document explaining:
- Component purpose
- Available types/variations
- Usage examples
- KTDA-specific integration

---

## 🔍 Troubleshooting

### **Problem: Cards not showing**
**Solution:** Check that ViewBag values are set in controller

### **Problem: Icons not visible**
**Solution:** Verify icon class exists in Velzon (Remix icons: `ri-*`, BoxIcons: `bx bx-*`)

### **Problem: Wrong colors**
**Solution:** Ensure ColorThemes list matches number of cards, or set to null for defaults

### **Problem: Trends not showing**
**Solution:** Both TrendPercentages and TrendDirections must be provided

### **Problem: Background colors not working**
**Solution:** Set `card.HasBackgroundColor = true` after calling `BuildStatsRow()`

### **Problem: Extension method not found**
**Solution:** Add `@using FormReporting.Extensions` at top of view

---

## 📖 Summary

**What You Write (View):**
```cshtml
var config = new StatsRowConfig { Titles, Values, Icons };
var cards = config.BuildStatsRow();

@foreach (var card in cards)
{
    <partial name="_TileBoxCard" model="card" />
}
```

**What You Get:**
- Professional Velzon dashboard cards
- Consistent styling across 12 modules
- Automatic color themes and defaults
- Type-safe configuration
- Zero business logic in views
- Rapid development (5 lines vs 50+)
- Easy testing with built-in test data
- Seamless integration with KTDA multi-tenant system

**The Golden Rule:**
> **Views describe WHAT to show, Extensions determine HOW to show it, Partials handle rendering.**

---

## 🎯 Next Steps

1. **Use StatCards everywhere** - Dashboard, Index pages for all 77 tables
2. **Create DataTable component** - Follow this exact pattern for reusable tables
3. **Create Tab component** - For User Profile, Form Builder, Metric Setup pages
4. **Create Form component** - For all CRUD operations across 12 modules
5. **Create Wizard component** - For workflows (User Setup WF-2.35, Metric Setup WF-3.19)

Each component follows the **same three-layer pattern**:
- **ViewModels** = Configuration
- **Extensions** = Transformation
- **Partials** = Rendering

This architecture will accelerate development of all 77 tables across 12 modules significantly.
