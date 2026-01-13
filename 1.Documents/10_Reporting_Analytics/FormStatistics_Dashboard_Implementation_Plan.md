# IMPLEMENTATION PLAN: FORMS SUBMISSION STATISTICS DASHBOARD

**Document Version:** 1.0  
**Date:** January 9, 2026  
**Project:** FormReporting - Dashboard Implementation  

---

## **ARCHITECTURAL DECISIONS SUMMARY**

✅ **Dashboard Type:** Forms Submission Statistics Dashboard  
✅ **Chart Library:** ApexCharts (already in `/wwwroot/lib/apexcharts`)  
✅ **Loading Strategy:** Progressive AJAX Loading  
✅ **Folder Structure:** Feature-Based (Option A)  
✅ **ViewModel Strategy:** Hierarchical with Config Objects  
✅ **Dashboard Approach:** Config-Driven  
✅ **Caching:** None  
✅ **Filter Persistence:** URL Query String Only  
✅ **Integration:** Embedded in Form Submissions page + reusable for other views  

---

## **IMPLEMENTATION PHASES**

---

### **PHASE 1: FOUNDATION SETUP (Infrastructure)**

#### **Step 1.1: Add ApexCharts to Shared Layout Files**

**Files to modify:**
- `Views/Shared/_Styles.cshtml` - Add ApexCharts CSS
- `Views/Shared/_Scripts.cshtml` - Add ApexCharts JS

**What to add:**
- ApexCharts library references from `wwwroot/lib/apexcharts`
- Dashboard component JavaScript module

**Code additions:**

**_Styles.cshtml:**
```html
<!-- ApexCharts CSS -->
<link href="~/lib/apexcharts/dist/apexcharts.css" rel="stylesheet" type="text/css" />
```

**_Scripts.cshtml:**
```html
<!-- ApexCharts Library -->
<script src="~/lib/apexcharts/dist/apexcharts.min.js"></script>

<!-- Dashboard Components -->
<script src="~/js/dashboard/form-statistics.js"></script>
```

---

#### **Step 1.2: Create Folder Structure**

**New Folders to Create:**
```
Controllers/
  Dashboard/
    FormStatistics/

Services/
  Dashboard/
    FormStatistics/
    Common/

Models/ViewModels/
  Dashboard/
    FormStatistics/
    Common/

Views/
  Dashboard/
    FormStatistics/
    
wwwroot/js/
  dashboard/
```

---

### **PHASE 2: VIEWMODELS & CONFIG OBJECTS (Data Contracts)**

#### **Step 2.1: Create Common Config Classes**

**Location:** `Models/ViewModels/Dashboard/Common/`

**Files to create:**

**A. `StatCardConfig.cs`**
```csharp
namespace FormReporting.Models.ViewModels.Dashboard.Common
{
    public class StatCardConfig
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string IconColor { get; set; } = "primary"; // primary, success, warning, danger, info
        public string TrendValue { get; set; } = string.Empty; // "+12%"
        public string TrendDirection { get; set; } = "neutral"; // up, down, neutral
        public string SubText { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
        public string LinkText { get; set; } = string.Empty;
        public bool ShowSpinner { get; set; } = false; // Loading state
    }
}
```

**B. `ChartConfig.cs`**
```csharp
namespace FormReporting.Models.ViewModels.Dashboard.Common
{
    public class ChartConfig
    {
        public string ChartId { get; set; } = string.Empty; // Unique DOM ID
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = "line"; // line, bar, pie, donut, area
        public List<string> Labels { get; set; } = new List<string>(); // X-axis labels
        public List<ChartDataset> Datasets { get; set; } = new List<ChartDataset>(); // Series data
        public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>(); // ApexCharts options
        public int Height { get; set; } = 350; // Height in pixels
        public bool ShowLegend { get; set; } = true;
        public bool ShowExport { get; set; } = true;
    }

    public class ChartDataset
    {
        public string Name { get; set; } = string.Empty;
        public List<decimal> Data { get; set; } = new List<decimal>();
        public string Color { get; set; } = string.Empty;
    }
}
```

**C. `TableConfig.cs`**
```csharp
namespace FormReporting.Models.ViewModels.Dashboard.Common
{
    public class TableConfig
    {
        public string Title { get; set; } = string.Empty;
        public List<TableColumn> Columns { get; set; } = new List<TableColumn>();
        public List<Dictionary<string, object>> Rows { get; set; } = new List<Dictionary<string, object>>();
        public bool IsSortable { get; set; } = true;
        public bool IsSearchable { get; set; } = true;
        public bool IsPaginated { get; set; } = true;
        public int PageSize { get; set; } = 10;
        public bool ShowExport { get; set; } = true;
    }

    public class TableColumn
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool IsSortable { get; set; } = true;
        public string Align { get; set; } = "left"; // left, center, right
        public string Format { get; set; } = "text"; // text, number, date, badge, link
    }
}
```

**D. `DashboardSectionConfig.cs`**
```csharp
namespace FormReporting.Models.ViewModels.Dashboard.Common
{
    public class DashboardSectionConfig
    {
        public string SectionId { get; set; } = string.Empty;
        public string SectionType { get; set; } = string.Empty; // StatCards, Chart, Table, etc.
        public string Title { get; set; } = string.Empty;
        public string Component { get; set; } = string.Empty; // Partial view name
        public string Width { get; set; } = "col-12"; // Bootstrap column class
        public int Order { get; set; } = 0;
        public string LoadMethod { get; set; } = "Server"; // Server, Ajax
        public int? RefreshInterval { get; set; } // Milliseconds, null = no auto-refresh
        public string AjaxUrl { get; set; } = string.Empty;
    }
}
```

---

#### **Step 2.2: Create Form Statistics ViewModels**

**Location:** `Models/ViewModels/Dashboard/FormStatistics/`

**Files to create:**

**A. `FormStatisticsDashboardViewModel.cs`**
```csharp
using FormReporting.Models.ViewModels.Dashboard.Common;

namespace FormReporting.Models.ViewModels.Dashboard.FormStatistics
{
    public class FormStatisticsDashboardViewModel
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? TenantId { get; set; }
        
        // Filters
        public FormStatisticsFilterViewModel Filters { get; set; } = new FormStatisticsFilterViewModel();
        
        // Quick Stats (Server-Side Loaded)
        public List<StatCardConfig> QuickStats { get; set; } = new List<StatCardConfig>();
        
        // Charts (AJAX Loaded)
        public ChartConfig? SubmissionTrendChart { get; set; }
        public ChartConfig? StatusBreakdownChart { get; set; }
        
        // Tables (AJAX Loaded)
        public TableConfig? RecentSubmissionsTable { get; set; }
        public TableConfig? TenantComparisonTable { get; set; }
        
        // Display options
        public bool ShowTenantComparison { get; set; } = false;
    }
}
```

**B. `FormStatisticsFilterViewModel.cs`**
```csharp
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FormReporting.Models.ViewModels.Dashboard.FormStatistics
{
    public class FormStatisticsFilterViewModel
    {
        public int? TemplateId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? TenantId { get; set; }
        public string GroupBy { get; set; } = "Daily"; // Daily, Weekly, Monthly
        
        // Dropdown options
        public List<SelectListItem> TemplateOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> TenantOptions { get; set; } = new List<SelectListItem>();
    }
}
```

---

### **PHASE 3: BUILDER SERVICE LAYER (Business Logic → UI Transformation)**

#### **Step 3.1: Create Common Builder Helpers**

**Location:** `Services/Dashboard/Common/`

**Files to create:**

**A. `IComponentBuilder.cs`** (Interface)
```csharp
using FormReporting.Models.ViewModels.Dashboard.Common;

namespace FormReporting.Services.Dashboard.Common
{
    public interface IComponentBuilder
    {
        StatCardConfig BuildStatCard(string title, string value, string icon, 
            string iconColor, string trendValue = "", string trendDirection = "neutral", 
            string subText = "", string linkUrl = "", string linkText = "");
    }
}
```

**B. `StatCardBuilder.cs`**
```csharp
using FormReporting.Models.ViewModels.Dashboard.Common;

namespace FormReporting.Services.Dashboard.Common
{
    public class StatCardBuilder
    {
        public StatCardConfig BuildStatCard(string title, string value, string icon, 
            string iconColor, string trendValue = "", string trendDirection = "neutral", 
            string subText = "", string linkUrl = "", string linkText = "")
        {
            return new StatCardConfig
            {
                Title = title,
                Value = value,
                Icon = icon,
                IconColor = iconColor,
                TrendValue = trendValue,
                TrendDirection = trendDirection,
                SubText = subText,
                LinkUrl = linkUrl,
                LinkText = linkText
            };
        }
        
        public string DetermineTrendDirection(decimal currentValue, decimal previousValue)
        {
            if (currentValue > previousValue) return "up";
            if (currentValue < previousValue) return "down";
            return "neutral";
        }
        
        public string CalculateTrendPercentage(decimal currentValue, decimal previousValue)
        {
            if (previousValue == 0) return "N/A";
            var change = ((currentValue - previousValue) / previousValue) * 100;
            return $"{(change >= 0 ? "+" : "")}{change:F1}%";
        }
    }
}
```

**C. `ChartBuilder.cs`**
```csharp
using FormReporting.Models.ViewModels.Dashboard.Common;

namespace FormReporting.Services.Dashboard.Common
{
    public class ChartBuilder
    {
        public ChartConfig BuildLineChart(string chartId, string title, List<string> labels, 
            List<ChartDataset> datasets, int height = 350)
        {
            return new ChartConfig
            {
                ChartId = chartId,
                Title = title,
                Type = "line",
                Labels = labels,
                Datasets = datasets,
                Height = height,
                ShowLegend = true,
                ShowExport = true
            };
        }
        
        public ChartConfig BuildPieChart(string chartId, string title, List<string> labels, 
            List<decimal> data, int height = 350)
        {
            return new ChartConfig
            {
                ChartId = chartId,
                Title = title,
                Type = "pie",
                Labels = labels,
                Datasets = new List<ChartDataset>
                {
                    new ChartDataset { Name = "Count", Data = data }
                },
                Height = height,
                ShowLegend = true,
                ShowExport = true
            };
        }
    }
}
```

**D. `TableBuilder.cs`**
```csharp
using FormReporting.Models.ViewModels.Dashboard.Common;

namespace FormReporting.Services.Dashboard.Common
{
    public class TableBuilder
    {
        public TableConfig BuildTable(string title, List<TableColumn> columns, 
            List<Dictionary<string, object>> rows, bool paginated = true, int pageSize = 10)
        {
            return new TableConfig
            {
                Title = title,
                Columns = columns,
                Rows = rows,
                IsSortable = true,
                IsSearchable = true,
                IsPaginated = paginated,
                PageSize = pageSize,
                ShowExport = true
            };
        }
    }
}
```

---

#### **Step 3.2: Create Form Statistics Builder Service**

**Location:** `Services/Dashboard/FormStatistics/`

**Files to create:**

**A. `IFormStatisticsDashboardBuilder.cs`** (Interface)
```csharp
using FormReporting.Models.ViewModels.Dashboard.Common;
using FormReporting.Models.ViewModels.Dashboard.FormStatistics;

namespace FormReporting.Services.Dashboard.FormStatistics
{
    public interface IFormStatisticsDashboardBuilder
    {
        Task<FormStatisticsDashboardViewModel> BuildDashboardAsync(int templateId, 
            DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null);
        
        Task<List<StatCardConfig>> BuildQuickStatsAsync(int templateId, 
            DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null);
        
        Task<ChartConfig> BuildTrendChartAsync(int templateId, 
            DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null, 
            string groupBy = "Daily");
        
        Task<ChartConfig> BuildStatusChartAsync(int templateId, 
            DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null);
        
        Task<TableConfig> BuildRecentSubmissionsTableAsync(int templateId, 
            int count = 10, int? tenantId = null);
        
        Task<TableConfig> BuildTenantComparisonTableAsync(int templateId, 
            DateTime? startDate = null, DateTime? endDate = null);
    }
}
```

**B. `FormStatisticsDashboardBuilder.cs`** (Implementation)
```csharp
using FormReporting.Models.ViewModels.Dashboard.Common;
using FormReporting.Models.ViewModels.Dashboard.FormStatistics;
using FormReporting.Services.Dashboard.Common;
using FormReporting.Services.Forms;

namespace FormReporting.Services.Dashboard.FormStatistics
{
    public class FormStatisticsDashboardBuilder : IFormStatisticsDashboardBuilder
    {
        private readonly IFormSubmissionStatisticsService _statisticsService;
        private readonly StatCardBuilder _statCardBuilder;
        private readonly ChartBuilder _chartBuilder;
        private readonly TableBuilder _tableBuilder;

        public FormStatisticsDashboardBuilder(
            IFormSubmissionStatisticsService statisticsService,
            StatCardBuilder statCardBuilder,
            ChartBuilder chartBuilder,
            TableBuilder tableBuilder)
        {
            _statisticsService = statisticsService;
            _statCardBuilder = statCardBuilder;
            _chartBuilder = chartBuilder;
            _tableBuilder = tableBuilder;
        }

        public async Task<FormStatisticsDashboardViewModel> BuildDashboardAsync(
            int templateId, DateTime? startDate = null, DateTime? endDate = null, 
            int? tenantId = null)
        {
            var viewModel = new FormStatisticsDashboardViewModel
            {
                TemplateId = templateId,
                StartDate = startDate,
                EndDate = endDate,
                TenantId = tenantId,
                QuickStats = await BuildQuickStatsAsync(templateId, startDate, endDate, tenantId)
            };

            return viewModel;
        }

        public async Task<List<StatCardConfig>> BuildQuickStatsAsync(
            int templateId, DateTime? startDate = null, DateTime? endDate = null, 
            int? tenantId = null)
        {
            var total = await _statisticsService.GetTotalSubmissionsAsync(templateId, startDate, endDate, tenantId);
            var statusBreakdown = await _statisticsService.GetSubmissionsByStatusAsync(templateId, startDate, endDate, tenantId);
            var onTimeStats = await _statisticsService.GetOnTimeStatisticsAsync(templateId, startDate, endDate, tenantId);
            var avgCompletionTime = await _statisticsService.GetAverageCompletionTimeAsync(templateId, startDate, endDate, tenantId);

            var statCards = new List<StatCardConfig>
            {
                _statCardBuilder.BuildStatCard(
                    title: "Total Submissions",
                    value: total.ToString(),
                    icon: "ri-file-list-3-line",
                    iconColor: "primary",
                    subText: "All submissions"
                ),
                _statCardBuilder.BuildStatCard(
                    title: "Submitted",
                    value: statusBreakdown.GetValueOrDefault("Submitted", 0).ToString(),
                    icon: "ri-checkbox-circle-line",
                    iconColor: "success",
                    subText: "Completed submissions"
                ),
                _statCardBuilder.BuildStatCard(
                    title: "On-Time Rate",
                    value: $"{onTimeStats.OnTimePercentage:F1}%",
                    icon: "ri-time-line",
                    iconColor: onTimeStats.OnTimePercentage >= 80 ? "success" : "warning",
                    subText: $"{onTimeStats.OnTimeCount} of {onTimeStats.TotalSubmissions} on time"
                ),
                _statCardBuilder.BuildStatCard(
                    title: "Avg Completion Time",
                    value: avgCompletionTime.HasValue ? $"{avgCompletionTime.Value:F1} hrs" : "N/A",
                    icon: "ri-hourglass-line",
                    iconColor: "info",
                    subText: "Time to complete"
                )
            };

            return statCards;
        }

        public async Task<ChartConfig> BuildTrendChartAsync(
            int templateId, DateTime? startDate = null, DateTime? endDate = null, 
            int? tenantId = null, string groupBy = "Daily")
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;
            
            var trendData = await _statisticsService.GetSubmissionTrendsAsync(
                templateId, start, end, groupBy, tenantId);

            var labels = trendData.Select(t => t.Label).ToList();
            var data = trendData.Select(t => (decimal)t.Count).ToList();

            var dataset = new ChartDataset
            {
                Name = "Submissions",
                Data = data,
                Color = "#405189"
            };

            return _chartBuilder.BuildLineChart(
                chartId: "submission-trend-chart",
                title: "Submission Trends",
                labels: labels,
                datasets: new List<ChartDataset> { dataset }
            );
        }

        public async Task<ChartConfig> BuildStatusChartAsync(
            int templateId, DateTime? startDate = null, DateTime? endDate = null, 
            int? tenantId = null)
        {
            var statusBreakdown = await _statisticsService.GetSubmissionsByStatusAsync(
                templateId, startDate, endDate, tenantId);

            var labels = statusBreakdown.Keys.ToList();
            var data = statusBreakdown.Values.Select(v => (decimal)v).ToList();

            return _chartBuilder.BuildPieChart(
                chartId: "status-breakdown-chart",
                title: "Status Breakdown",
                labels: labels,
                data: data,
                height: 300
            );
        }

        public async Task<TableConfig> BuildRecentSubmissionsTableAsync(
            int templateId, int count = 10, int? tenantId = null)
        {
            var submissions = await _statisticsService.GetRecentSubmissionsAsync(
                templateId, count, tenantId);

            var columns = new List<TableColumn>
            {
                new TableColumn { Key = "submissionId", Label = "ID", Align = "center" },
                new TableColumn { Key = "status", Label = "Status", Format = "badge" },
                new TableColumn { Key = "submittedBy", Label = "Submitted By" },
                new TableColumn { Key = "tenantName", Label = "Tenant" },
                new TableColumn { Key = "submittedDate", Label = "Submitted Date", Format = "date" }
            };

            var rows = submissions.Select(s => new Dictionary<string, object>
            {
                ["submissionId"] = s.SubmissionId,
                ["status"] = s.Status,
                ["submittedBy"] = s.SubmittedBy,
                ["tenantName"] = s.TenantName,
                ["submittedDate"] = s.SubmittedDate ?? DateTime.MinValue
            }).ToList();

            return _tableBuilder.BuildTable(
                title: "Recent Submissions",
                columns: columns,
                rows: rows,
                paginated: false
            );
        }

        public async Task<TableConfig> BuildTenantComparisonTableAsync(
            int templateId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var tenantStats = await _statisticsService.GetTenantComparisonAsync(
                templateId, startDate, endDate);

            var columns = new List<TableColumn>
            {
                new TableColumn { Key = "tenantName", Label = "Tenant" },
                new TableColumn { Key = "totalSubmissions", Label = "Total", Align = "center" },
                new TableColumn { Key = "submittedCount", Label = "Submitted", Align = "center" },
                new TableColumn { Key = "draftCount", Label = "Draft", Align = "center" },
                new TableColumn { Key = "approvedCount", Label = "Approved", Align = "center" }
            };

            var rows = tenantStats.Select(t => new Dictionary<string, object>
            {
                ["tenantName"] = t.TenantName,
                ["totalSubmissions"] = t.TotalSubmissions,
                ["submittedCount"] = t.SubmittedCount,
                ["draftCount"] = t.DraftCount,
                ["approvedCount"] = t.ApprovedCount
            }).ToList();

            return _tableBuilder.BuildTable(
                title: "Tenant Comparison",
                columns: columns,
                rows: rows
            );
        }
    }
}
```

---

### **PHASE 4: DASHBOARD CONFIGURATION (Config-Driven Rendering)**

#### **Step 4.1: Create Dashboard Definition**

**Location:** `Models/ViewModels/Dashboard/FormStatistics/`

**File to create:** `FormStatisticsDashboardDefinition.cs`

```csharp
using FormReporting.Models.ViewModels.Dashboard.Common;

namespace FormReporting.Models.ViewModels.Dashboard.FormStatistics
{
    public class FormStatisticsDashboardDefinition
    {
        public string DashboardId => "form-statistics";
        public string Title => "Form Submission Statistics";
        
        public List<DashboardSectionConfig> GetSections()
        {
            return new List<DashboardSectionConfig>
            {
                // Row 1: Quick Stats
                new DashboardSectionConfig
                {
                    SectionId = "quick-stats",
                    SectionType = "StatCards",
                    Title = "Quick Statistics",
                    Component = "_StatCardGroup",
                    Width = "col-12",
                    Order = 1,
                    LoadMethod = "Server",
                    RefreshInterval = null
                },
                
                // Row 2: Trend Chart
                new DashboardSectionConfig
                {
                    SectionId = "trend-chart",
                    SectionType = "Chart",
                    Title = "Submission Trends",
                    Component = "_TrendChartPartial",
                    Width = "col-lg-8",
                    Order = 2,
                    LoadMethod = "Ajax",
                    AjaxUrl = "/Dashboard/FormStatistics/GetTrendChart",
                    RefreshInterval = null
                },
                
                // Row 2: Status Chart
                new DashboardSectionConfig
                {
                    SectionId = "status-chart",
                    SectionType = "Chart",
                    Title = "Status Breakdown",
                    Component = "_StatusChartPartial",
                    Width = "col-lg-4",
                    Order = 3,
                    LoadMethod = "Ajax",
                    AjaxUrl = "/Dashboard/FormStatistics/GetStatusChart",
                    RefreshInterval = null
                },
                
                // Row 3: Recent Submissions Table
                new DashboardSectionConfig
                {
                    SectionId = "recent-submissions",
                    SectionType = "Table",
                    Title = "Recent Submissions",
                    Component = "_DataTable",
                    Width = "col-12",
                    Order = 4,
                    LoadMethod = "Ajax",
                    AjaxUrl = "/Dashboard/FormStatistics/GetRecentSubmissions",
                    RefreshInterval = null
                },
                
                // Row 4: Tenant Comparison (Optional)
                new DashboardSectionConfig
                {
                    SectionId = "tenant-comparison",
                    SectionType = "Table",
                    Title = "Tenant Comparison",
                    Component = "_DataTable",
                    Width = "col-12",
                    Order = 5,
                    LoadMethod = "Ajax",
                    AjaxUrl = "/Dashboard/FormStatistics/GetTenantComparison",
                    RefreshInterval = null
                }
            };
        }
    }
}
```

---

### **PHASE 5: CONTROLLER LAYER (API Endpoints)**

#### **Step 5.1: Create Main Dashboard Controller**

**Location:** `Controllers/Dashboard/FormStatistics/`

**File to create:** `FormStatisticsDashboardController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FormReporting.Services.Dashboard.FormStatistics;
using FormReporting.Services.Identity;
using System.Security.Claims;

namespace FormReporting.Controllers.Dashboard.FormStatistics
{
    [Authorize]
    [Route("Dashboard/FormStatistics")]
    public class FormStatisticsDashboardController : Controller
    {
        private readonly IFormStatisticsDashboardBuilder _dashboardBuilder;
        private readonly IScopeService _scopeService;
        private readonly ILogger<FormStatisticsDashboardController> _logger;

        public FormStatisticsDashboardController(
            IFormStatisticsDashboardBuilder dashboardBuilder,
            IScopeService scopeService,
            ILogger<FormStatisticsDashboardController> logger)
        {
            _dashboardBuilder = dashboardBuilder;
            _scopeService = scopeService;
            _logger = logger;
        }

        /// <summary>
        /// Main dashboard page - loads skeleton with server-side quick stats
        /// GET /Dashboard/FormStatistics
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> Index(int? templateId, DateTime? startDate, 
            DateTime? endDate, int? tenantId)
        {
            if (!templateId.HasValue)
            {
                return View("SelectTemplate");
            }

            // Apply scope filtering
            var accessibleTenantIds = await _scopeService.GetAccessibleTenantIdsAsync(User);
            if (tenantId.HasValue && !accessibleTenantIds.Contains(tenantId.Value))
            {
                return Forbid();
            }

            var viewModel = await _dashboardBuilder.BuildDashboardAsync(
                templateId.Value, startDate, endDate, tenantId);

            return View(viewModel);
        }

        /// <summary>
        /// Get quick stats section (can be used for refresh)
        /// GET /Dashboard/FormStatistics/GetQuickStats
        /// </summary>
        [HttpGet("GetQuickStats")]
        public async Task<IActionResult> GetQuickStats(int templateId, DateTime? startDate, 
            DateTime? endDate, int? tenantId)
        {
            var statCards = await _dashboardBuilder.BuildQuickStatsAsync(
                templateId, startDate, endDate, tenantId);

            return PartialView("~/Views/Shared/Components/Dashboard/Composite/_StatCardGroup.cshtml", 
                statCards);
        }

        /// <summary>
        /// Get trend chart data (AJAX)
        /// GET /Dashboard/FormStatistics/GetTrendChart
        /// </summary>
        [HttpGet("GetTrendChart")]
        public async Task<IActionResult> GetTrendChart(int templateId, DateTime? startDate, 
            DateTime? endDate, int? tenantId, string groupBy = "Daily")
        {
            var chartConfig = await _dashboardBuilder.BuildTrendChartAsync(
                templateId, startDate, endDate, tenantId, groupBy);

            return Json(chartConfig);
        }

        /// <summary>
        /// Get status breakdown chart data (AJAX)
        /// GET /Dashboard/FormStatistics/GetStatusChart
        /// </summary>
        [HttpGet("GetStatusChart")]
        public async Task<IActionResult> GetStatusChart(int templateId, DateTime? startDate, 
            DateTime? endDate, int? tenantId)
        {
            var chartConfig = await _dashboardBuilder.BuildStatusChartAsync(
                templateId, startDate, endDate, tenantId);

            return Json(chartConfig);
        }

        /// <summary>
        /// Get recent submissions table (AJAX)
        /// GET /Dashboard/FormStatistics/GetRecentSubmissions
        /// </summary>
        [HttpGet("GetRecentSubmissions")]
        public async Task<IActionResult> GetRecentSubmissions(int templateId, 
            int count = 10, int? tenantId = null)
        {
            var tableConfig = await _dashboardBuilder.BuildRecentSubmissionsTableAsync(
                templateId, count, tenantId);

            return PartialView("~/Views/Shared/Components/Dashboard/Composite/_DataTable.cshtml", 
                tableConfig);
        }

        /// <summary>
        /// Get tenant comparison table (AJAX)
        /// GET /Dashboard/FormStatistics/GetTenantComparison
        /// </summary>
        [HttpGet("GetTenantComparison")]
        public async Task<IActionResult> GetTenantComparison(int templateId, 
            DateTime? startDate = null, DateTime? endDate = null)
        {
            // Check if user has multi-tenant access
            var accessibleTenantIds = await _scopeService.GetAccessibleTenantIdsAsync(User);
            if (accessibleTenantIds.Count <= 1)
            {
                return Json(new { error = "Multi-tenant access required" });
            }

            var tableConfig = await _dashboardBuilder.BuildTenantComparisonTableAsync(
                templateId, startDate, endDate);

            return PartialView("~/Views/Shared/Components/Dashboard/Composite/_DataTable.cshtml", 
                tableConfig);
        }
    }
}
```

---

### **PHASE 6: VIEW LAYER (Progressive AJAX UI)**

#### **Step 6.1: Create Main Dashboard View**

**Location:** `Views/Dashboard/FormStatistics/`

**File to create:** `Index.cshtml`

```cshtml
@model FormReporting.Models.ViewModels.Dashboard.FormStatistics.FormStatisticsDashboardViewModel

@{
    ViewData["Title"] = "Form Submission Statistics";
}

<div class="row">
    <div class="col-12">
        <div class="page-title-box d-sm-flex align-items-center justify-content-between">
            <h4 class="mb-sm-0">@ViewData["Title"]</h4>
            <div class="page-title-right">
                <ol class="breadcrumb m-0">
                    <li class="breadcrumb-item"><a href="/">Dashboard</a></li>
                    <li class="breadcrumb-item active">Form Statistics</li>
                </ol>
            </div>
        </div>
    </div>
</div>

<!-- Filters Section -->
<div class="row mb-3">
    <div class="col-12">
        @await Html.PartialAsync("_FormStatisticsFilters", Model.Filters)
    </div>
</div>

<!-- Quick Stats Section (Server-Side) -->
<div class="row mb-3" id="quick-stats-section">
    @await Html.PartialAsync("~/Views/Shared/Components/Dashboard/Composite/_StatCardGroup.cshtml", 
        Model.QuickStats)
</div>

<!-- Charts Row (AJAX Loading) -->
<div class="row mb-3">
    <!-- Trend Chart -->
    <div class="col-lg-8">
        <div id="trend-chart-section" 
             data-section="trend-chart" 
             data-template-id="@Model.TemplateId">
            @await Html.PartialAsync("~/Views/Shared/Components/Dashboard/Skeletons/_ChartSkeleton.cshtml")
        </div>
    </div>
    
    <!-- Status Chart -->
    <div class="col-lg-4">
        <div id="status-chart-section" 
             data-section="status-chart"
             data-template-id="@Model.TemplateId">
            @await Html.PartialAsync("~/Views/Shared/Components/Dashboard/Skeletons/_ChartSkeleton.cshtml")
        </div>
    </div>
</div>

<!-- Recent Submissions Table (AJAX Loading) -->
<div class="row mb-3">
    <div class="col-12">
        <div id="recent-submissions-section" 
             data-section="recent-submissions"
             data-template-id="@Model.TemplateId">
            @await Html.PartialAsync("~/Views/Shared/Components/Dashboard/Skeletons/_TableSkeleton.cshtml")
        </div>
    </div>
</div>

<!-- Tenant Comparison (AJAX Loading, Conditional) -->
@if (Model.ShowTenantComparison)
{
    <div class="row">
        <div class="col-12">
            <div id="tenant-comparison-section" 
                 data-section="tenant-comparison"
                 data-template-id="@Model.TemplateId">
                @await Html.PartialAsync("~/Views/Shared/Components/Dashboard/Skeletons/_TableSkeleton.cshtml")
            </div>
        </div>
    </div>
}

@section Scripts {
    <script>
        // Initialize dashboard with progressive loading
        $(document).ready(function() {
            FormStatisticsDashboard.init({
                templateId: @Model.TemplateId,
                startDate: '@Model.StartDate?.ToString("yyyy-MM-dd")',
                endDate: '@Model.EndDate?.ToString("yyyy-MM-dd")',
                tenantId: @(Model.TenantId?.ToString() ?? "null"),
                groupBy: 'Daily'
            });
        });
    </script>
}
```

---

#### **Step 6.2: Create Filter Partial View**

**Location:** `Views/Dashboard/FormStatistics/`

**File to create:** `_FormStatisticsFilters.cshtml`

```cshtml
@model FormReporting.Models.ViewModels.Dashboard.FormStatistics.FormStatisticsFilterViewModel

<div class="card">
    <div class="card-body">
        <form id="dashboard-filters" method="get" action="/Dashboard/FormStatistics">
            <div class="row g-3 align-items-end">
                <!-- Template Selector -->
                <div class="col-md-3">
                    <label class="form-label">Template</label>
                    <select name="templateId" class="form-select" required>
                        @foreach (var option in Model.TemplateOptions)
                        {
                            <option value="@option.Value" selected="@option.Selected">
                                @option.Text
                            </option>
                        }
                    </select>
                </div>
                
                <!-- Start Date -->
                <div class="col-md-2">
                    <label class="form-label">Start Date</label>
                    <input type="date" name="startDate" class="form-control" 
                           value="@Model.StartDate?.ToString("yyyy-MM-dd")" />
                </div>
                
                <!-- End Date -->
                <div class="col-md-2">
                    <label class="form-label">End Date</label>
                    <input type="date" name="endDate" class="form-control" 
                           value="@Model.EndDate?.ToString("yyyy-MM-dd")" />
                </div>
                
                <!-- Tenant Filter (if applicable) -->
                @if (Model.TenantOptions.Any())
                {
                    <div class="col-md-2">
                        <label class="form-label">Tenant</label>
                        <select name="tenantId" class="form-select">
                            <option value="">All Tenants</option>
                            @foreach (var option in Model.TenantOptions)
                            {
                                <option value="@option.Value" selected="@option.Selected">
                                    @option.Text
                                </option>
                            }
                        </select>
                    </div>
                }
                
                <!-- Grouping -->
                <div class="col-md-2">
                    <label class="form-label">Group By</label>
                    <select name="groupBy" class="form-select">
                        <option value="Daily" selected="@(Model.GroupBy == "Daily")">Daily</option>
                        <option value="Weekly" selected="@(Model.GroupBy == "Weekly")">Weekly</option>
                        <option value="Monthly" selected="@(Model.GroupBy == "Monthly")">Monthly</option>
                    </select>
                </div>
                
                <!-- Action Buttons -->
                <div class="col-md-1">
                    <button type="submit" class="btn btn-primary w-100">
                        <i class="ri-search-line"></i> Apply
                    </button>
                </div>
            </div>
        </form>
    </div>
</div>
```

---

#### **Step 6.3: Create Chart Partial Views**

**Location:** `Views/Dashboard/FormStatistics/`

**Files to create:**

**A. `_TrendChartPartial.cshtml`**
```cshtml
<div class="card">
    <div class="card-header">
        <h5 class="card-title mb-0">Submission Trends</h5>
    </div>
    <div class="card-body">
        <div id="submission-trend-chart" style="min-height: 350px;"></div>
    </div>
</div>
```

**B. `_StatusChartPartial.cshtml`**
```cshtml
<div class="card">
    <div class="card-header">
        <h5 class="card-title mb-0">Status Breakdown</h5>
    </div>
    <div class="card-body">
        <div id="status-breakdown-chart" style="min-height: 300px;"></div>
    </div>
</div>
```

---

### **PHASE 7: JAVASCRIPT (Progressive AJAX Loading)**

#### **Step 7.1: Create Dashboard JavaScript Module**

**Location:** `wwwroot/js/dashboard/form-statistics.js`

**File to create:** `form-statistics.js`

```javascript
/**
 * Form Statistics Dashboard - Progressive AJAX Loading
 */
const FormStatisticsDashboard = {
    config: {
        templateId: null,
        startDate: null,
        endDate: null,
        tenantId: null,
        groupBy: 'Daily'
    },

    /**
     * Initialize dashboard with progressive loading
     */
    init: function(options) {
        this.config = { ...this.config, ...options };
        
        // Load all AJAX sections
        this.loadTrendChart();
        this.loadStatusChart();
        this.loadRecentSubmissions();
        
        // Load tenant comparison if applicable
        if ($('#tenant-comparison-section').length > 0) {
            this.loadTenantComparison();
        }
    },

    /**
     * Load trend chart via AJAX
     */
    loadTrendChart: function() {
        const $container = $('#trend-chart-section');
        
        $.ajax({
            url: '/Dashboard/FormStatistics/GetTrendChart',
            type: 'GET',
            data: {
                templateId: this.config.templateId,
                startDate: this.config.startDate,
                endDate: this.config.endDate,
                tenantId: this.config.tenantId,
                groupBy: this.config.groupBy
            },
            success: function(chartConfig) {
                // Replace skeleton with chart container
                $container.html('<div class="card"><div class="card-header"><h5 class="card-title mb-0">' + 
                    chartConfig.title + '</h5></div><div class="card-body">' +
                    '<div id="' + chartConfig.chartId + '"></div></div></div>');
                
                // Render ApexChart
                FormStatisticsDashboard.renderLineChart(chartConfig);
            },
            error: function(xhr, status, error) {
                console.error('Error loading trend chart:', error);
                $container.html('<div class="alert alert-danger">Failed to load chart</div>');
            }
        });
    },

    /**
     * Load status chart via AJAX
     */
    loadStatusChart: function() {
        const $container = $('#status-chart-section');
        
        $.ajax({
            url: '/Dashboard/FormStatistics/GetStatusChart',
            type: 'GET',
            data: {
                templateId: this.config.templateId,
                startDate: this.config.startDate,
                endDate: this.config.endDate,
                tenantId: this.config.tenantId
            },
            success: function(chartConfig) {
                // Replace skeleton with chart container
                $container.html('<div class="card"><div class="card-header"><h5 class="card-title mb-0">' + 
                    chartConfig.title + '</h5></div><div class="card-body">' +
                    '<div id="' + chartConfig.chartId + '"></div></div></div>');
                
                // Render ApexChart
                FormStatisticsDashboard.renderPieChart(chartConfig);
            },
            error: function(xhr, status, error) {
                console.error('Error loading status chart:', error);
                $container.html('<div class="alert alert-danger">Failed to load chart</div>');
            }
        });
    },

    /**
     * Load recent submissions table via AJAX
     */
    loadRecentSubmissions: function() {
        const $container = $('#recent-submissions-section');
        
        $.ajax({
            url: '/Dashboard/FormStatistics/GetRecentSubmissions',
            type: 'GET',
            data: {
                templateId: this.config.templateId,
                count: 10,
                tenantId: this.config.tenantId
            },
            success: function(html) {
                $container.html(html);
            },
            error: function(xhr, status, error) {
                console.error('Error loading recent submissions:', error);
                $container.html('<div class="alert alert-danger">Failed to load submissions</div>');
            }
        });
    },

    /**
     * Load tenant comparison table via AJAX
     */
    loadTenantComparison: function() {
        const $container = $('#tenant-comparison-section');
        
        $.ajax({
            url: '/Dashboard/FormStatistics/GetTenantComparison',
            type: 'GET',
            data: {
                templateId: this.config.templateId,
                startDate: this.config.startDate,
                endDate: this.config.endDate
            },
            success: function(html) {
                $container.html(html);
            },
            error: function(xhr, status, error) {
                console.error('Error loading tenant comparison:', error);
                $container.html('<div class="alert alert-danger">Failed to load comparison</div>');
            }
        });
    },

    /**
     * Render line chart using ApexCharts
     */
    renderLineChart: function(chartConfig) {
        const options = {
            chart: {
                type: 'line',
                height: chartConfig.height,
                toolbar: {
                    show: chartConfig.showExport
                }
            },
            series: chartConfig.datasets.map(dataset => ({
                name: dataset.name,
                data: dataset.data
            })),
            xaxis: {
                categories: chartConfig.labels
            },
            colors: chartConfig.datasets.map(d => d.color || '#405189'),
            stroke: {
                curve: 'smooth',
                width: 2
            },
            legend: {
                show: chartConfig.showLegend
            }
        };

        const chart = new ApexCharts(document.querySelector('#' + chartConfig.chartId), options);
        chart.render();
    },

    /**
     * Render pie chart using ApexCharts
     */
    renderPieChart: function(chartConfig) {
        const options = {
            chart: {
                type: 'pie',
                height: chartConfig.height
            },
            series: chartConfig.datasets[0].data,
            labels: chartConfig.labels,
            legend: {
                show: chartConfig.showLegend,
                position: 'bottom'
            },
            responsive: [{
                breakpoint: 480,
                options: {
                    chart: {
                        width: 200
                    },
                    legend: {
                        position: 'bottom'
                    }
                }
            }]
        };

        const chart = new ApexCharts(document.querySelector('#' + chartConfig.chartId), options);
        chart.render();
    }
};
```

---

### **PHASE 8: INTEGRATION (Reusable Dashboard Component)**

#### **Step 8.1: Create Reusable Partial Widget**

**Location:** `Views/Shared/Dashboard/`

**File to create:** `_FormStatisticsWidget.cshtml`

```cshtml
@model dynamic

@{
    int templateId = Model.templateId;
    bool showFilters = Model.showFilters ?? true;
    bool showTenantComparison = Model.showTenantComparison ?? false;
    string containerId = Model.containerId ?? "form-statistics-widget";
}

<div id="@containerId" class="form-statistics-widget">
    @if (showFilters)
    {
        <!-- Include filters if enabled -->
        <div class="row mb-3">
            <div class="col-12">
                <!-- Filter component here -->
            </div>
        </div>
    }
    
    <!-- Quick Stats -->
    <div class="row mb-3" id="@(containerId)-quick-stats"></div>
    
    <!-- Charts -->
    <div class="row mb-3">
        <div class="col-lg-8">
            <div id="@(containerId)-trend-chart" data-template-id="@templateId"></div>
        </div>
        <div class="col-lg-4">
            <div id="@(containerId)-status-chart" data-template-id="@templateId"></div>
        </div>
    </div>
    
    <!-- Recent Submissions -->
    <div class="row">
        <div class="col-12">
            <div id="@(containerId)-recent-submissions" data-template-id="@templateId"></div>
        </div>
    </div>
</div>

<script>
    $(document).ready(function() {
        // Initialize widget with specific container ID
        FormStatisticsDashboard.init({
            templateId: @templateId,
            containerId: '@containerId'
        });
    });
</script>
```

---

#### **Step 8.2: Integration into Form Submissions Page**

**Location:** Modify existing view (e.g., `Views/Submissions/Index.cshtml` or similar)

**Add this section:**

```cshtml
@* Add at top or bottom of submissions list page *@

@if (Model.TemplateId.HasValue)
{
    <div class="row mb-4">
        <div class="col-12">
            <h4 class="card-title mb-3">
                <i class="ri-bar-chart-line me-2"></i>Submission Statistics
            </h4>
            @await Html.PartialAsync("~/Views/Shared/Dashboard/_FormStatisticsWidget.cshtml", 
                new { 
                    templateId = Model.TemplateId.Value, 
                    showFilters = false, 
                    showTenantComparison = false,
                    containerId = "submissions-page-stats"
                })
        </div>
    </div>
    
    <hr class="my-4" />
}

@* Existing submissions list below *@
```

---

### **PHASE 9: SERVICE REGISTRATION**

#### **Step 9.1: Register Services in Program.cs**

**Location:** `Program.cs`

**Add to existing service registration section:**

```csharp
// Dashboard builder services - Common
builder.Services.AddScoped<Services.Dashboard.Common.StatCardBuilder>();
builder.Services.AddScoped<Services.Dashboard.Common.ChartBuilder>();
builder.Services.AddScoped<Services.Dashboard.Common.TableBuilder>();

// Dashboard builder services - Form Statistics
builder.Services.AddScoped<Services.Dashboard.FormStatistics.IFormStatisticsDashboardBuilder, 
    Services.Dashboard.FormStatistics.FormStatisticsDashboardBuilder>();
```

---

### **PHASE 10: TESTING & VALIDATION**

#### **Step 10.1: Unit Tests**

**Create test files:**
- `Tests/Services/Dashboard/FormStatisticsDashboardBuilderTests.cs`
- Test builder methods
- Test data transformations
- Test threshold logic
- Mock domain services

#### **Step 10.2: Integration Tests**

**Test scenarios:**
- AJAX endpoints return correct data
- Filters work correctly
- Charts render with valid data
- Tables display correctly
- Scope filtering works

#### **Step 10.3: Performance Tests**

**Measure:**
- Page load time (target: <2 seconds)
- AJAX request times (target: <500ms each)
- Chart rendering time
- Large dataset handling (100+ submissions)

---

## **IMPLEMENTATION SEQUENCE**

### **Day 1: Foundation (6-8 hours)**
- [ ] Add ApexCharts references to _Styles.cshtml and _Scripts.cshtml
- [ ] Create folder structure
- [ ] Create all common config classes (StatCardConfig, ChartConfig, TableConfig, DashboardSectionConfig)
- [ ] Create Form Statistics ViewModels (FormStatisticsDashboardViewModel, FormStatisticsFilterViewModel)

### **Day 2: Builder Layer (6-8 hours)**
- [ ] Create common builder helpers (StatCardBuilder, ChartBuilder, TableBuilder)
- [ ] Create IFormStatisticsDashboardBuilder interface
- [ ] Implement FormStatisticsDashboardBuilder service
- [ ] Register all services in Program.cs

### **Day 3: Controller & Configuration (6-8 hours)**
- [ ] Create FormStatisticsDashboardDefinition
- [ ] Create FormStatisticsDashboardController with all endpoints
- [ ] Test controller endpoints with Postman/Swagger

### **Day 4: Views & UI (6-8 hours)**
- [ ] Create Index.cshtml main dashboard view
- [ ] Create _FormStatisticsFilters.cshtml
- [ ] Create chart partial views (_TrendChartPartial, _StatusChartPartial)
- [ ] Create JavaScript module (form-statistics.js)

### **Day 5: Integration & Testing (6-8 hours)**
- [ ] Create reusable widget (_FormStatisticsWidget.cshtml)
- [ ] Integrate into form submissions page
- [ ] Test all AJAX loading
- [ ] Test filters and interactions
- [ ] Performance testing
- [ ] Bug fixes and refinements

---

## **FILES CHECKLIST**

### **Modified Files (2)**
- [ ] `Views/Shared/_Styles.cshtml`
- [ ] `Views/Shared/_Scripts.cshtml`

### **New ViewModels (7 files)**
- [ ] `Models/ViewModels/Dashboard/Common/StatCardConfig.cs`
- [ ] `Models/ViewModels/Dashboard/Common/ChartConfig.cs`
- [ ] `Models/ViewModels/Dashboard/Common/TableConfig.cs`
- [ ] `Models/ViewModels/Dashboard/Common/DashboardSectionConfig.cs`
- [ ] `Models/ViewModels/Dashboard/FormStatistics/FormStatisticsDashboardViewModel.cs`
- [ ] `Models/ViewModels/Dashboard/FormStatistics/FormStatisticsFilterViewModel.cs`
- [ ] `Models/ViewModels/Dashboard/FormStatistics/FormStatisticsDashboardDefinition.cs`

### **New Services (6 files)**
- [ ] `Services/Dashboard/Common/IComponentBuilder.cs`
- [ ] `Services/Dashboard/Common/StatCardBuilder.cs`
- [ ] `Services/Dashboard/Common/ChartBuilder.cs`
- [ ] `Services/Dashboard/Common/TableBuilder.cs`
- [ ] `Services/Dashboard/FormStatistics/IFormStatisticsDashboardBuilder.cs`
- [ ] `Services/Dashboard/FormStatistics/FormStatisticsDashboardBuilder.cs`

### **New Controllers (1 file)**
- [ ] `Controllers/Dashboard/FormStatistics/FormStatisticsDashboardController.cs`

### **New Views (5 files)**
- [ ] `Views/Dashboard/FormStatistics/Index.cshtml`
- [ ] `Views/Dashboard/FormStatistics/_FormStatisticsFilters.cshtml`
- [ ] `Views/Dashboard/FormStatistics/_TrendChartPartial.cshtml`
- [ ] `Views/Dashboard/FormStatistics/_StatusChartPartial.cshtml`
- [ ] `Views/Shared/Dashboard/_FormStatisticsWidget.cshtml`

### **New JavaScript (1 file)**
- [ ] `wwwroot/js/dashboard/form-statistics.js`

---

## **TOTAL SCOPE**

- **New Files:** 23
- **Modified Files:** 2
- **Estimated Time:** 4-5 days (30-40 hours)
- **Complexity:** Medium-High

---

## **SUCCESS CRITERIA**

✅ Dashboard loads in <2 seconds  
✅ All AJAX sections load independently  
✅ Charts render correctly with ApexCharts  
✅ Filters work and update URL query string  
✅ Scope filtering applied correctly  
✅ Widget can be embedded in other pages  
✅ Responsive design works on mobile  
✅ No JavaScript errors in console  

---

## **NEXT STEPS AFTER COMPLETION**

1. **Document patterns learned** for future dashboards
2. **Extract common patterns** if duplication noticed
3. **Build second dashboard** (Form Score Analytics or My Dashboard)
4. **Consider config-driven approach** if patterns emerge
5. **Add caching** if performance needs improvement
6. **Add real-time updates** with SignalR if needed

---

**End of Implementation Plan**
