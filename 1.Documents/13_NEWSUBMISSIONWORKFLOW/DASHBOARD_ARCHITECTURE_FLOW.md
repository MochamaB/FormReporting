# Universal Dashboard System - Architecture & Flow

## Overview

A widget-based, configuration-driven dashboard system that supports any domain (Forms, Hardware, Software, Tickets, Financials) without adding new database models. Uses existing entities with service-layer aggregation.

---

## Core Principles

- **No New Models** - Leverages existing entities only
- **Configuration-Driven** - Dashboards defined through registry, not hardcoded views
- **Widget-Based** - Self-contained display units that can be reused across dashboards
- **Provider Pattern** - Domain-specific services supply widget data
- **Scope-Aware** - All data respects user's tenant/region access level

---

## Component Architecture

### 1. Widget Types

Standardized visualization components available system-wide:

| Type | Purpose | Use Case |
|------|---------|----------|
| StatCard | Single KPI value | Total submissions, open tickets |
| BarChart | Comparisons | Submissions by tenant, assets by category |
| LineChart | Trends over time | Monthly submissions, score progression |
| PieChart | Distribution | Status breakdown, category split |
| DoughnutChart | Distribution with center | Budget allocation |
| Gauge | Score with thresholds | Form scores, compliance rates |
| ProgressBar | Linear progress | Budget utilization |
| DataTable | Tabular records | Recent submissions, pending items |
| List | Simple item list | Alerts, recent activity |
| Sparkline | Mini inline trend | Compact trend indicators |

**Chart Library:** ApexCharts (for Bar, Line, Pie, Doughnut charts)

---

### 2. Widget Data ViewModels

Widgets are grouped by data structure - 6 ViewModels serve all 10 widget types:

| Data ViewModel | Widget Types | Key Properties |
|----------------|--------------|----------------|
| StatCardDataViewModel | StatCard | value, trend, trendDirection, icon, iconColor |
| ChartDataViewModel | BarChart, LineChart, PieChart, DoughnutChart | labels[], datasets[], chartOptions |
| GaugeDataViewModel | Gauge, ProgressBar | value, min, max, thresholds |
| TableDataViewModel | DataTable | columns[], rows[], pagination |
| ListDataViewModel | List | items[] with icon, text, link, badge |
| SparklineDataViewModel | Sparkline | values[], color |

---

### 3. Widget States

Each widget handles four states for robust UX:

| State | Trigger | Display |
|-------|---------|---------|
| Loading | Data being fetched | Skeleton/shimmer animation |
| Empty | No results returned | "No data available" message |
| Error | Provider failed | Error message + retry button |
| Success | Data exists | Full widget content |

**Benefits:**
- Prevents blank/broken UI
- Immediate user feedback
- Graceful error handling
- Testable without real data

---

### 4. Dashboard Registry

Central configuration that defines all available dashboards. Each dashboard has:

- **Key** - Unique identifier (e.g., `form-statistics`, `hardware-inventory`)
- **Title** - Display name
- **Icon** - Remix icon class
- **Description** - Brief purpose
- **Widgets** - Collection of widget configurations
- **Filters** - Available filter options
- **RequiredScope** - Minimum access level needed

The registry lives in code (DashboardRegistry class) and returns configurations on request.

---

### 5. Widget Configuration

Each widget within a dashboard is defined by:

| Property | Description |
|----------|-------------|
| WidgetKey | Unique identifier within dashboard |
| WidgetType | Enum value (StatCard, BarChart, etc.) |
| Title | Display title |
| DataProviderKey | Which provider supplies data (forms, hardware) |
| DataMethod | Method name on provider to call |
| Parameters | Additional query parameters |
| Size | Grid width (Small=3, Medium=6, Large=12 columns) |
| Position | Row and column for placement |
| RefreshInterval | Auto-refresh seconds (0=disabled) |
| ThresholdConfig | For gauges - green/yellow/red boundaries |

---

### 6. Widget Data Providers

Domain-specific services implementing a common interface. Each provider:

- Registers with a domain key (forms, hardware, tickets)
- Implements methods that return standardized widget data
- Queries existing entities and aggregates results
- Applies filters (date range, tenant, region)
- Respects user scope for data isolation

**Provider Domains:**

| Provider | Domain | Data Sources |
|----------|--------|--------------|
| FormWidgetProvider | Forms | FormTemplate, FormTemplateSubmission, FormTemplateResponse, FormItemOption |
| HardwareWidgetProvider | Hardware | TenantHardware, HardwareItem, HardwareMaintenanceLog |
| SoftwareWidgetProvider | Software | SoftwareProduct, SoftwareLicense, TenantSoftwareInstallation |
| TicketWidgetProvider | Tickets | Ticket, TicketComment |
| FinancialWidgetProvider | Finance | TenantBudget, TenantExpense |
| MetricWidgetProvider | Metrics | MetricDefinition, TenantMetric |
| OrganizationalWidgetProvider | Organization | Tenant, User, Department |

---

### 7. Dashboard Service (Orchestrator)

Central service that coordinates everything:

- Receives dashboard key from controller
- Looks up configuration from registry
- Validates user access/scope
- For each widget, locates appropriate provider
- Calls provider method with filters
- Assembles complete DashboardViewModel
- Returns to controller for rendering

---

### 8. Layout Configuration

Dashboard and widget positioning use a 12-column grid system.

**Dashboard Layout Properties:**

| Property | Description |
|----------|-------------|
| Columns | Grid columns (default: 12) |
| RowGap | Vertical spacing between rows |
| ColumnGap | Horizontal spacing between columns |
| Breakpoints | Responsive breakpoints (lg, md, sm) |

**Widget Size Definitions:**

| Size | Columns | Typical Use |
|------|---------|-------------|
| XSmall | 2 | Mini sparklines |
| Small | 3 | Stat cards (4 per row) |
| Medium | 4 | Charts (3 per row) |
| Large | 6 | Large charts (2 per row) |
| XLarge | 8 | Wide tables |
| Full | 12 | Full-width tables, large visualizations |

**Widget Position Properties:**

| Property | Description |
|----------|-------------|
| Row | Grid row number (1-based, auto if not set) |
| Column | Starting column (1-12, auto if not set) |
| ColSpan | Number of columns to span (same as Size) |
| RowSpan | Number of rows to span (default: 1) |
| Order | Display order for auto-layout |

**Layout Modes:**

| Mode | Description |
|------|-------------|
| Auto | Widgets flow left-to-right, top-to-bottom by Order |
| Fixed | Widgets placed at exact Row/Column positions |
| Mixed | Some widgets fixed, others auto-fill remaining space |

---

### 9. Filter System

Common filters applied across all widgets in a dashboard:

| Filter | Description |
|--------|-------------|
| DateRange | Period selection (This Month, Last Quarter, YTD, Custom) |
| TenantId | Specific tenant (within user's scope) |
| RegionId | Specific region (for regional+ scope users) |
| Category | Domain-specific (form category, hardware category) |

Filters are captured from UI, passed to DashboardService, then forwarded to each provider.

---

### 10. Context Parameters

Context parameters allow dashboards to show data for a specific entity rather than aggregates.

**How Context Works:**

| Parameter | Description | Example |
|-----------|-------------|---------|
| ContextType | Entity type being filtered | "FormTemplate", "Tenant", "HardwareCategory" |
| ContextId | Specific entity ID | 5 (TemplateId), 12 (TenantId) |
| ContextLabel | Display name for UI | "Monthly ICT Report" |

**Context-Aware Dashboard Flow:**

1. Dashboard renders with optional context selector (dropdown)
2. User selects specific entity (e.g., a form template)
3. Context passed to all widget providers
4. Providers filter data by context entity
5. Widgets show data for that specific entity only

**Context Selector Types:**

| Type | Use Case |
|------|----------|
| Dropdown | Select one entity (form template, tenant) |
| SearchSelect | Search and select for large lists |
| None | Always show aggregate (no selector) |

**Example: Form Statistics with Context**

- No context: Shows stats across ALL forms
- With TemplateId context: Shows stats for ONE specific form
- Dropdown lists published templates user can access
- Selecting a template reloads dashboard with that context

---

### 11. Embedding Widgets in Other Views

Widgets can be rendered as standalone partials in any view, not just dashboards.

**Use Cases:**

| Scenario | Example |
|----------|---------|
| Detail Page Stats | Show submission count on FormTemplate detail page |
| Sidebar Widgets | Quick stats in sidebar navigation |
| Card Footers | Mini chart in a card component |
| Modal Content | Stats popup on hover/click |

**Embedding Methods:**

**Method 1: Widget Component (View Component)**

Renders a single widget anywhere by calling DashboardService directly from a ViewComponent.

- Input: WidgetKey, optional filters, optional context
- Output: Rendered widget HTML
- Use: `@await Component.InvokeAsync("Widget", new { widgetKey = "form-submission-count", templateId = Model.TemplateId })`

**Method 2: Widget Group Partial**

Renders a predefined group of widgets as a partial.

- Groups defined in registry (e.g., "form-summary-cards" = 4 stat cards)
- Use: `@await Html.PartialAsync("_WidgetGroup", new { groupKey = "form-summary-cards", context = Model.TemplateId })`

**Method 3: Inline Widget Partial**

Pass widget data directly to partial (no service call).

- Data prepared in controller action
- Use: `@await Html.PartialAsync("Dashboard/Widgets/_StatCard", statCardViewModel)`

**Widget Group Registry:**

| Group Key | Widgets | Use In |
|-----------|---------|--------|
| form-summary-cards | 4 stat cards (templates, submissions, rate, pending) | Form index page |
| form-detail-stats | 3 stat cards (submissions, completion, score) | Form detail page |
| tenant-overview | 4 stat cards (users, assets, tickets, budget) | Tenant detail page |
| ticket-quick-stats | 3 stat cards (open, overdue, resolved today) | Ticket list header |

---

## Data Flow

### Initial Dashboard Load

```
User Request: /Dashboard/Render?key=form-statistics
        │
        ▼
DashboardController.Render("form-statistics")
        │
        ▼
DashboardService.GetDashboardAsync("form-statistics", filters)
        │
        ├──► Validate user has required scope
        │
        ├──► Get dashboard config from registry
        │
        └──► For each widget in config:
                │
                ├──► Find provider by DataProviderKey
                │
                └──► Call provider.GetWidgetDataAsync(config, filters)
                        │
                        └──► Provider queries entities, aggregates, returns data
        │
        ▼
Returns DashboardViewModel (title, widgets with data)
        │
        ▼
View renders using widget partials based on WidgetType
```

### Single Widget Refresh (AJAX)

```
JavaScript: /api/dashboard/widget?key=submission-count&filters=...
        │
        ▼
DashboardController.GetWidgetData(widgetKey, filters)
        │
        ▼
DashboardService.GetWidgetAsync(widgetKey, filters)
        │
        ▼
Provider.GetWidgetDataAsync(config, filters)
        │
        ▼
Returns JSON with widget data
        │
        ▼
JavaScript updates specific widget in DOM
```

### Contextual Dashboard Load (Single Form Example)

```
User Request: /Dashboard/Render?key=form-statistics&contextType=FormTemplate&contextId=5
        │
        ▼
DashboardController.Render("form-statistics", context)
        │
        ▼
DashboardService.GetDashboardAsync("form-statistics", filters, context)
        │
        ├──► Get dashboard config from registry
        │
        ├──► Get context selector options (list of templates user can access)
        │
        └──► For each widget:
                │
                └──► Provider.GetWidgetDataAsync(config, filters, context)
                        │
                        └──► Provider adds WHERE TemplateId = 5 to all queries
        │
        ▼
Returns DashboardViewModel with:
  - Context selector populated (dropdown with templates)
  - Selected context highlighted
  - Widget data filtered to single template
        │
        ▼
View renders with context dropdown and filtered widgets
```

### Embedded Widget Load (View Component)

```
Any View calls: @await Component.InvokeAsync("Widget", params)
        │
        ▼
WidgetViewComponent.InvokeAsync(widgetKey, context)
        │
        ▼
DashboardService.GetSingleWidgetAsync(widgetKey, context)
        │
        ▼
Provider.GetWidgetDataAsync(config, context)
        │
        ▼
Returns WidgetViewModel
        │
        ▼
ViewComponent renders appropriate widget partial
        │
        ▼
Returns HTML to parent view
```

---

## View Rendering Flow

### Dashboard Layout

1. **_DashboardLayout.cshtml** - Creates responsive grid container
2. Iterates through widgets in ViewModel
3. For each widget, renders **_WidgetContainer.cshtml** (wrapper with title, refresh button)
4. Inside container, switches on WidgetType to render appropriate partial

### Widget Partial Selection

| WidgetType | Partial View |
|------------|--------------|
| StatCard | _StatCard.cshtml |
| BarChart | _BarChart.cshtml |
| LineChart | _LineChart.cshtml |
| PieChart | _PieChart.cshtml |
| Gauge | _Gauge.cshtml |
| DataTable | _DataTable.cshtml |
| List | _List.cshtml |

Each partial expects standardized data structure from WidgetViewModel.Data property.

---

## Form Scoring Calculation

For form scoring dashboards, FormWidgetProvider calculates scores using existing fields:

1. Query FormTemplateSubmissions for target template
2. Get FormTemplateResponses for each submission
3. For selection fields (Dropdown, Radio, Checkbox):
   - Match ResponseValue to FormItemOption.OptionValue
   - Get ScoreValue and ScoreWeight from matched option
4. Calculate: Sum(ScoreValue × ScoreWeight) for all responses
5. Calculate percentage: (Achieved / MaxPossible) × 100
6. Return aggregated scores for widgets (gauge, trends, comparisons)

---

## Scope Integration

All providers use ScopeService to filter data:

| User Scope | Data Access |
|------------|-------------|
| GLOBAL | All tenants |
| REGIONAL | Tenants in user's region(s) |
| TENANT | User's primary tenant only |
| DEPARTMENT | User's department only |
| INDIVIDUAL | User's own submissions only |

Providers call `ScopeService.GetAccessibleTenantIdsAsync()` and filter queries accordingly.

---

## Dashboard Examples

### Form Statistics Dashboard
- Total templates count
- Published templates count
- Submissions this month
- Completion rate percentage
- Submissions by tenant (bar chart)
- Submissions by status (pie chart)
- Submission trends (line chart)
- Recent submissions (data table)

### Form Scoring Dashboard
- Overall form score (gauge)
- Score by section (bar chart)
- Score trend over time (line chart)
- Tenant comparison (horizontal bar)
- Low-scoring questions (data table)

### Executive Summary Dashboard
- Total tenants
- Active users
- Open tickets
- Budget utilization
- Compliance score (gauge)
- Regional performance (bar chart)
- Critical alerts (list)

---

## Key Benefits

| Aspect | Benefit |
|--------|---------|
| Single View | One Render.cshtml handles all dashboards |
| Pluggable | New domain = new provider class |
| Consistent | All dashboards share same UX patterns |
| Maintainable | Widget partials reused everywhere |
| Extensible | Add dashboard via registry entry |
| Performant | Individual widget refresh via AJAX |
