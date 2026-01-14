# Universal Dashboard System - Structure & Implementation

## File Structure

```
FormReporting/
│
├── Models/
│   └── ViewModels/
│       └── Dashboard/
│           ├── DashboardViewModel.cs             [core - complete dashboard]
│           ├── WidgetViewModel.cs                [core - single widget with status]
│           ├── DashboardFilterViewModel.cs       [core - filter state]
│           ├── DashboardContextViewModel.cs      [core - context parameters]
│           ├── DashboardLayoutViewModel.cs       [core - layout config]
│           ├── WidgetPositionViewModel.cs        [core - widget positioning]
│           ├── ContextSelectorViewModel.cs       [core - dropdown options]
            ├── Widgets
│                      ├── StatCardDataViewModel.cs    [data - for StatCard]
│                      ├── ChartDataViewModel.cs       [data - for Bar/Line/Pie/Doughnut]
│                      ├── GaugeDataViewModel.cs       [data - for Gauge/ProgressBar]
│                      ├── TableDataViewModel.cs       [data - for DataTable]
│                      ├── ListDataViewModel.cs        [data - for List]
│                      └── SparklineDataViewModel.cs   [data - for Sparkline]
│
├── Models/
│   └── Common/
│       └── Enums.cs                          (add WidgetType, WidgetSize, LayoutMode enums)
│
├── Services/
│   └── Dashboard/
│       ├── IDashboardService.cs
│       ├── DashboardService.cs
│       ├── IWidgetDataProvider.cs
│       ├── DashboardRegistry.cs
│       ├── WidgetGroupRegistry.cs                [NEW - widget groups for embedding]
│       └── Providers/
│           ├── FormWidgetProvider.cs
│           ├── HardwareWidgetProvider.cs
│           ├── SoftwareWidgetProvider.cs
│           ├── TicketWidgetProvider.cs
│           ├── FinancialWidgetProvider.cs
│           ├── MetricWidgetProvider.cs
│           └── OrganizationalWidgetProvider.cs
│
├── Controllers/
│   └── DashboardController.cs
│
├── ViewComponents/                               [NEW - for embedding widgets]
│   └── WidgetViewComponent.cs
│
├── Views/
│   └── Dashboard/
│       ├── Index.cshtml                      (dashboard hub/selector)
│       ├── Render.cshtml                     (generic dashboard renderer)
│       └── Partials/
│           ├── _DashboardLayout.cshtml       [NEW - grid layout container]
│           ├── _ContextSelector.cshtml       [NEW - entity dropdown]
│           ├── _FilterBar.cshtml             [NEW - filter controls]
│           └── Widgets/
│               ├── _WidgetContainer.cshtml
│               ├── _WidgetGroup.cshtml       [NEW - render widget groups]
│               ├── _StatCard.cshtml
│               ├── _BarChart.cshtml
│               ├── _LineChart.cshtml
│               ├── _PieChart.cshtml
│               ├── _DoughnutChart.cshtml
│               ├── _Gauge.cshtml
│               ├── _ProgressBar.cshtml
│               ├── _DataTable.cshtml
│               ├── _List.cshtml
│               └── _Sparkline.cshtml
│
├── Views/
│   └── Shared/
│       └── Components/
│           └── Widget/                       [NEW - ViewComponent views]
│               └── Default.cshtml
│
└── wwwroot/
    └── assets/
        ├── js/
        │   └── pages/
        │       └── dashboard/
        │           ├── dashboard.js              [main orchestrator]
        │           ├── widget-renderer.js        [renders widgets, handles states]
        │           ├── context-selector.js       [dropdown handling]
        │           └── apexcharts-config.js      [ApexCharts default options]
        └── css/
            └── dashboard/
                ├── dashboard.css                 [grid layout styles]
                └── widget-states.css             [loading/empty/error state styles]
```

---

## ViewModels Description

### Core ViewModels

| File | Purpose |
|------|---------|
| DashboardViewModel | Complete dashboard: key, title, icon, layout, widgets, filters, context |
| WidgetViewModel | Single widget: key, type, title, size, position, status, data, errorMessage |
| DashboardFilterViewModel | Filter state: date range, tenant, region, category |
| DashboardContextViewModel | Context state: contextType, contextId, contextLabel |
| DashboardLayoutViewModel | Layout config: columns, rowGap, columnGap, layoutMode |
| WidgetPositionViewModel | Widget position: row, column, colSpan, rowSpan, order |
| ContextSelectorViewModel | Dropdown options: items list, selected value, placeholder |

### Widget Data ViewModels (6 total for 10 widget types)

| File | Used By | Properties |
|------|---------|------------|
| StatCardDataViewModel | StatCard | value, label, icon, iconColor, trend, trendDirection, trendLabel |
| ChartDataViewModel | BarChart, LineChart, PieChart, DoughnutChart | labels[], datasets[], chartOptions |
| GaugeDataViewModel | Gauge, ProgressBar | value, minValue, maxValue, thresholds (green/yellow/red), label |
| TableDataViewModel | DataTable | columns[], rows[], totalCount, pageSize, currentPage |
| ListDataViewModel | List | items[] (each: icon, text, subText, link, badge, badgeColor) |
| SparklineDataViewModel | Sparkline | values[], color, height |

### Widget Status Handling

Each WidgetViewModel includes Status property to handle different states:

| Status | Widget Displays |
|--------|-----------------|
| Loading | Skeleton/shimmer placeholder animation |
| Empty | "No data available" message with icon |
| Error | Error message with optional retry button |
| Success | Actual widget content with real data |

---

## Enums to Add (in Enums.cs)

| Enum | Values |
|------|--------|
| WidgetType | StatCard, BarChart, LineChart, PieChart, DoughnutChart, Gauge, ProgressBar, DataTable, List, Sparkline |
| WidgetSize | XSmall (2 cols), Small (3 cols), Medium (4 cols), Large (6 cols), XLarge (8 cols), Full (12 cols) |
| LayoutMode | Auto, Fixed, Mixed |
| ContextType | None, FormTemplate, Tenant, Region, HardwareCategory, TicketCategory |
| WidgetStatus | Loading, Empty, Error, Success |

---

## Services Description

| File | Purpose |
|------|---------|
| IDashboardService | Interface for orchestrator |
| DashboardService | Gets dashboard config, coordinates providers, assembles response |
| IWidgetDataProvider | Interface all providers implement |
| DashboardRegistry | Configuration of all dashboards, their widgets, layout, and context settings |
| WidgetGroupRegistry | Configuration of widget groups for embedding in other views |
| FormWidgetProvider | Queries form entities, calculates scores, returns widget data |
| HardwareWidgetProvider | Queries hardware entities, returns asset/maintenance data |
| SoftwareWidgetProvider | Queries software/license entities |
| TicketWidgetProvider | Queries ticket entities |
| FinancialWidgetProvider | Queries budget/expense entities |
| MetricWidgetProvider | Queries metric definitions and values |
| OrganizationalWidgetProvider | Queries tenant/user/department counts |

---

## View Components Description

| File | Purpose |
|------|---------|
| WidgetViewComponent | Renders a single widget anywhere in the application |

---

## Controller Actions

| Action | Route | Purpose |
|--------|-------|---------|
| Index | GET /Dashboard | Dashboard hub showing available dashboards |
| Render | GET /Dashboard/Render?key={key} | Renders any dashboard by key |
| Render (with context) | GET /Dashboard/Render?key={key}&contextType={type}&contextId={id} | Renders dashboard filtered to specific entity |
| GetWidgetData | GET /api/dashboard/widget?key={key} | API: Returns single widget data |
| GetWidgetData (with context) | GET /api/dashboard/widget?key={key}&contextType={type}&contextId={id} | API: Returns widget data for specific entity |
| GetContextOptions | GET /api/dashboard/context-options?type={type} | API: Returns dropdown options for context selector |
| RefreshDashboard | GET /api/dashboard/refresh?key={key} | API: Returns all widgets for refresh |

---

## MVP Implementation Order

### Phase 1: Foundation

**Goal:** Core infrastructure for any dashboard

| Step | Task | Files |
|------|------|-------|
| 1.1 | Add WidgetType, WidgetSize, LayoutMode, ContextType enums | Enums.cs |
| 1.2 | Create DashboardViewModel | DashboardViewModel.cs |
| 1.3 | Create WidgetViewModel | WidgetViewModel.cs |
| 1.4 | Create DashboardLayoutViewModel | DashboardLayoutViewModel.cs |
| 1.5 | Create WidgetPositionViewModel | WidgetPositionViewModel.cs |
| 1.6 | Create DashboardFilterViewModel | DashboardFilterViewModel.cs |
| 1.7 | Create data ViewModels (StatCard, Chart, Gauge, Table, List, Sparkline) | Data ViewModels |
| 1.8 | Create IWidgetDataProvider interface | IWidgetDataProvider.cs |
| 1.9 | Create IDashboardService interface | IDashboardService.cs |
| 1.10 | Create DashboardRegistry with empty structure | DashboardRegistry.cs |
| 1.11 | Create DashboardService (basic orchestration) | DashboardService.cs |
| 1.12 | Create DashboardController with Render action | DashboardController.cs |
| 1.13 | Register services in Program.cs | Program.cs |
| 1.14 | Create _DashboardLayout partial (grid container) | _DashboardLayout.cshtml |
| 1.15 | Create _WidgetContainer partial | _WidgetContainer.cshtml |
| 1.16 | Create _StatCard partial | _StatCard.cshtml |
| 1.17 | Create _DataTable partial | _DataTable.cshtml |
| 1.18 | Create Render.cshtml view | Render.cshtml |
| 1.19 | Create dashboard.css (grid styles) | dashboard.css |
| 1.20 | Create widget-states.css (loading/empty/error styles) | widget-states.css |

**Deliverable:** Empty dashboard renders with grid layout and placeholder widgets (with loading states)

---

### Phase 2: Form Statistics Dashboard

**Goal:** First working dashboard with real data

| Step | Task | Files |
|------|------|-------|
| 2.1 | Create FormWidgetProvider | FormWidgetProvider.cs |
| 2.2 | Implement GetTemplateCount method | FormWidgetProvider.cs |
| 2.3 | Implement GetSubmissionCount method | FormWidgetProvider.cs |
| 2.4 | Implement GetCompletionRate method | FormWidgetProvider.cs |
| 2.5 | Implement GetSubmissionsByTenant method | FormWidgetProvider.cs |
| 2.6 | Implement GetSubmissionsByStatus method | FormWidgetProvider.cs |
| 2.7 | Implement GetRecentSubmissions method | FormWidgetProvider.cs |
| 2.8 | Add form-statistics dashboard to registry | DashboardRegistry.cs |
| 2.9 | Create _BarChart partial (with ApexCharts) | _BarChart.cshtml |
| 2.10 | Create _PieChart partial (with ApexCharts) | _PieChart.cshtml |
| 2.11 | Create apexcharts-config.js (default options) | apexcharts-config.js |
| 2.12 | Register FormWidgetProvider in Program.cs | Program.cs |

**Deliverable:** Form Statistics dashboard with stat cards, charts, and table

---

### Phase 3: Form Scoring Dashboard

**Goal:** Scoring functionality with gauges

| Step | Task | Files |
|------|------|-------|
| 3.1 | Implement score calculation logic | FormWidgetProvider.cs |
| 3.2 | Implement GetOverallScore method | FormWidgetProvider.cs |
| 3.3 | Implement GetScoreBySection method | FormWidgetProvider.cs |
| 3.4 | Implement GetScoreTrend method | FormWidgetProvider.cs |
| 3.5 | Implement GetTenantScoreComparison method | FormWidgetProvider.cs |
| 3.6 | Implement GetLowScoringQuestions method | FormWidgetProvider.cs |
| 3.7 | Add form-scoring dashboard to registry | DashboardRegistry.cs |
| 3.8 | Create _Gauge partial | _Gauge.cshtml |
| 3.9 | Create _LineChart partial | _LineChart.cshtml |

**Deliverable:** Form Scoring dashboard with gauges, trends, comparisons

---

### Phase 4: Dashboard Hub, Filters & Context

**Goal:** Navigation, filtering, and single-entity context support

| Step | Task | Files |
|------|------|-------|
| 4.1 | Create Index.cshtml (dashboard selector) | Index.cshtml |
| 4.2 | Create DashboardContextViewModel | DashboardContextViewModel.cs |
| 4.3 | Create ContextSelectorViewModel | ContextSelectorViewModel.cs |
| 4.4 | Create _FilterBar partial | _FilterBar.cshtml |
| 4.5 | Create _ContextSelector partial (dropdown) | _ContextSelector.cshtml |
| 4.6 | Add filter handling to DashboardService | DashboardService.cs |
| 4.7 | Add context handling to DashboardService | DashboardService.cs |
| 4.8 | Add GetContextOptions API action | DashboardController.cs |
| 4.9 | Update providers to accept filters and context | All providers |
| 4.10 | Create dashboard.js for interactions | dashboard.js |
| 4.11 | Create context-selector.js (dropdown logic) | context-selector.js |
| 4.12 | Implement widget refresh AJAX | widget-renderer.js |
| 4.13 | Add GetWidgetData API action | DashboardController.cs |

**Deliverable:** Dashboard hub with filters, context dropdown (single form selection), and AJAX refresh

---

### Phase 5: Widget Embedding

**Goal:** Reuse widgets in other views (not just dashboards)

| Step | Task | Files |
|------|------|-------|
| 5.1 | Create WidgetGroupRegistry | WidgetGroupRegistry.cs |
| 5.2 | Create WidgetViewComponent | WidgetViewComponent.cs |
| 5.3 | Create ViewComponent view | Views/Shared/Components/Widget/Default.cshtml |
| 5.4 | Create _WidgetGroup partial | _WidgetGroup.cshtml |
| 5.5 | Add GetSingleWidgetAsync to DashboardService | DashboardService.cs |
| 5.6 | Add GetWidgetGroupAsync to DashboardService | DashboardService.cs |
| 5.7 | Define form-summary-cards widget group | WidgetGroupRegistry.cs |
| 5.8 | Define form-detail-stats widget group | WidgetGroupRegistry.cs |

**Deliverable:** Widgets can be embedded in any view using ViewComponent or WidgetGroup partial

---

### Phase 6: Additional Providers (As Needed)

**Goal:** Expand to other domains

| Step | Task | Files |
|------|------|-------|
| 6.1 | Create HardwareWidgetProvider | HardwareWidgetProvider.cs |
| 6.2 | Add hardware-inventory dashboard | DashboardRegistry.cs |
| 6.3 | Create TicketWidgetProvider | TicketWidgetProvider.cs |
| 6.4 | Add ticket-overview dashboard | DashboardRegistry.cs |
| 6.5 | Create FinancialWidgetProvider | FinancialWidgetProvider.cs |
| 6.6 | Add financial-summary dashboard | DashboardRegistry.cs |
| 6.7 | Create OrganizationalWidgetProvider | OrganizationalWidgetProvider.cs |
| 6.8 | Add executive-summary dashboard | DashboardRegistry.cs |

**Deliverable:** Full suite of domain dashboards

---

### Phase 7: Enhancements (Optional)

**Goal:** Polish and advanced features

| Step | Task |
|------|------|
| 7.1 | Auto-refresh timer for widgets |
| 7.2 | Dashboard export to PDF |
| 7.3 | Widget drill-down navigation |
| 7.4 | User dashboard preferences |
| 7.5 | Additional widget types (Map, Sparkline) |

---

## Dependencies

| Phase | Depends On |
|-------|------------|
| Phase 1 | None (foundation) |
| Phase 2 | Phase 1 |
| Phase 3 | Phase 2 (extends FormWidgetProvider) |
| Phase 4 | Phase 2 or 3 |
| Phase 5 | Phase 1 (can run parallel to 2/3/4) |
| Phase 6 | Phase 1 (can run parallel to 2/3/4/5) |
| Phase 7 | Phase 4 |

---

## Service Registration Order

Add to Program.cs in this order:

1. IDashboardService → DashboardService
2. IWidgetDataProvider implementations:
   - FormWidgetProvider
   - HardwareWidgetProvider
   - SoftwareWidgetProvider
   - TicketWidgetProvider
   - FinancialWidgetProvider
   - MetricWidgetProvider
   - OrganizationalWidgetProvider

---

## Testing Checkpoints

| After Phase | Verify |
|-------------|--------|
| Phase 1 | Dashboard renders with grid layout and placeholder widgets |
| Phase 2 | Form statistics shows real data from database |
| Phase 3 | Form scores calculate correctly |
| Phase 4 | Filters work, context dropdown works, selecting form filters all widgets |
| Phase 5 | Widgets render correctly when embedded in other views |
| Phase 6 | All domain dashboards render |

---

## Notes

- No database migrations required
- All data comes from existing entities
- Scope filtering uses existing ScopeService
- **ApexCharts** library needed for charts (add via CDN or npm) - chosen for better defaults and easier configuration
- Follow existing service registration pattern in Program.cs
- Follow existing partial view patterns in Views/Shared/Components

---

## Widget States & Default Handling

Each widget partial handles four states for robust UX:

### State Definitions

| State | When Triggered | What Partial Shows |
|-------|----------------|-------------------|
| Loading | Initial render, data being fetched | Skeleton/shimmer animation matching widget shape |
| Empty | Provider returned data but results are empty | "No data available" icon + message |
| Error | Provider threw exception or API failed | Error message + retry button |
| Success | Data exists and is valid | Full widget with real data |

### Default Placeholder Examples

| Widget Type | Loading State | Empty State |
|-------------|---------------|-------------|
| StatCard | Gray box with pulse animation | "—" value with muted label |
| BarChart | Gray bar placeholders | "No data to display" centered |
| LineChart | Gray line placeholder | Empty chart area with message |
| PieChart | Gray circle outline | "No data" in center |
| Gauge | Gray gauge arc | Needle at 0, "No score" label |
| DataTable | 5 skeleton rows | "No records found" single row |
| List | 3 skeleton list items | "No items" message |

### Benefits of Default States

- Prevents broken/blank UI when data unavailable
- User sees immediate feedback while loading
- Graceful error handling without page crash
- Easier testing without real data

---

## Layout Configuration Notes

- Grid uses CSS Grid or Bootstrap 12-column system
- Widget sizes map to column spans (Small=3, Medium=4, Large=6, Full=12)
- LayoutMode determines widget placement:
  - Auto: Widgets flow by Order property
  - Fixed: Widgets placed at exact Row/Column
  - Mixed: Some fixed, others auto-fill
- Responsive breakpoints collapse widgets on smaller screens

---

## Context Usage Examples

| Dashboard | Context Type | Dropdown Shows | Result |
|-----------|--------------|----------------|--------|
| Form Statistics | FormTemplate | All published templates | Stats for ONE form |
| Form Statistics | None | No dropdown | Stats for ALL forms |
| Hardware Inventory | Tenant | All accessible tenants | Assets for ONE tenant |
| Ticket Overview | TicketCategory | All ticket categories | Tickets in ONE category |

---

## Widget Embedding Examples

| Use Case | Method | Where |
|----------|--------|-------|
| Show submission count on form detail page | WidgetViewComponent | FormTemplates/Details.cshtml |
| Show 4 stat cards on form index page | WidgetGroup partial | FormTemplates/Index.cshtml |
| Show mini chart in sidebar | WidgetViewComponent | _Sidebar.cshtml |
| Show tenant stats on tenant detail | WidgetGroup partial | Tenants/Details.cshtml |
