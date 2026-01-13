using FormReporting.Models.ViewModels.Dashboard.Components.Composite;
using FormReporting.Models.ViewModels.Dashboard.FormStatistics;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Services.Dashboard.Common;
using FormReporting.Services.Forms;
using FormReporting.Services.Dashboard;
using System.Security.Claims;

namespace FormReporting.Services.Dashboard.FormStatistics
{
    /// <summary>
    /// Implementation of Form Statistics Dashboard builder
    /// Orchestrates domain services and transforms data into UI-ready configurations
    /// </summary>
    public class FormStatisticsDashboardBuilder : IFormStatisticsDashboardBuilder
    {
        private readonly IFormSubmissionStatisticsService _statisticsService;
        private readonly StatCardBuilder _statCardBuilder;
        private readonly ChartBuilder _chartBuilder;
        private readonly TableBuilder _tableBuilder;
        private readonly IFilterService _filterService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FormStatisticsDashboardBuilder(
            IFormSubmissionStatisticsService statisticsService,
            StatCardBuilder statCardBuilder,
            ChartBuilder chartBuilder,
            TableBuilder tableBuilder,
            IFilterService filterService,
            IHttpContextAccessor httpContextAccessor)
        {
            _statisticsService = statisticsService;
            _statCardBuilder = statCardBuilder;
            _chartBuilder = chartBuilder;
            _tableBuilder = tableBuilder;
            _filterService = filterService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DashboardConfig> BuildDashboardAsync(
            int? templateId, 
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            int? tenantId = null,
            DashboardMode mode = DashboardMode.FullPage)
        {
            var dashboardTitle = templateId.HasValue 
                ? "Form Submission Statistics - Template" 
                : "Form Submission Statistics - All Templates";
                
            DashboardConfig dashboard;
            if (mode == DashboardMode.Widget)
            {
                dashboard = DashboardConfig.Widget($"form-stats-{templateId ?? 0}");
            }
            else if (mode == DashboardMode.Embedded)
            {
                // Embedded mode: use progressive loading like widget but without widget styling
                dashboard = new DashboardConfig
                {
                    Id = $"form-stats-{templateId ?? 0}",
                    Title = dashboardTitle,
                    Mode = DashboardMode.Embedded,
                    ShowBreadcrumbs = false,
                    LoadingStrategy = LoadingStrategy.Progressive
                };
            }
            else
            {
                dashboard = DashboardConfig.FullPage(dashboardTitle);
            }

            // Add filter panel (collapsible for widget/embedded mode)
            var filterPanel = await BuildFilterPanelAsync(templateId, startDate, endDate, tenantId, mode);
            if (mode == DashboardMode.Widget || mode == DashboardMode.Embedded)
            {
                filterPanel.Collapsible = true;
                filterPanel.InitiallyCollapsed = mode == DashboardMode.Widget;
            }
            dashboard.FilterPanel = filterPanel;

            // Get current user for scope filtering
            var currentUser = _httpContextAccessor.HttpContext?.User;
            if (currentUser == null)
            {
                throw new InvalidOperationException("Current user not available");
            }

            // Show stats for both single template and overall view using the same methods
            var quickStats = await BuildQuickStatsAsync(templateId, startDate, endDate, tenantId, currentUser);
            dashboard.QuickStats = StatCardGroupConfig.FourColumns(quickStats.ToArray());

            // Add sections based on loading strategy
            if (dashboard.LoadingStrategy == LoadingStrategy.ServerSide)
            {
                // Server-side: Build all components with data
                var trendChart = await BuildTrendChartAsync(templateId, startDate, endDate, tenantId, "Daily", currentUser);
                var statusChart = await BuildStatusChartAsync(templateId, startDate, endDate, tenantId, currentUser);
                var recentTable = await BuildRecentSubmissionsTableAsync(templateId, 10, tenantId, currentUser);

                dashboard.Sections.Add(DashboardSection.Chart("trend-chart", trendChart, "col-lg-8", SectionLoadMethod.Server));
                dashboard.Sections.Add(DashboardSection.Chart("status-chart", statusChart, "col-lg-4", SectionLoadMethod.Server));
                dashboard.Sections.Add(DashboardSection.Table("recent-submissions", recentTable, "col-12", SectionLoadMethod.Server));
            }
            else
            {
                // Progressive: Use AJAX loading
                dashboard.Sections.Add(DashboardSection.AjaxChart("trend-chart", "/Dashboard/FormStatistics/GetTrendChart", "col-lg-8"));
                dashboard.Sections.Add(DashboardSection.AjaxChart("status-chart", "/Dashboard/FormStatistics/GetStatusChart", "col-lg-4"));
                dashboard.Sections.Add(DashboardSection.AjaxTable("recent-submissions", "/Dashboard/FormStatistics/GetRecentSubmissions", "col-12"));
            }

            return dashboard;
        }

        private List<int> GetAccessibleTemplateIdsAsync(ClaimsPrincipal currentUser)
        {
            // ✅ Get template IDs from claims (zero database queries!)
            var templateAccessClaims = currentUser.FindAll("TemplateAccess");
            
            var accessibleTemplateIds = templateAccessClaims
                .Where(c => int.TryParse(c.Value, out _))
                .Select(c => int.Parse(c.Value))
                .ToList();
            
            // TEMPORARY: Log template access from claims for debugging
            System.Diagnostics.Debug.WriteLine($"=== DASHBOARD: TEMPLATES FROM CLAIMS ===");
            System.Diagnostics.Debug.WriteLine($"User: {currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value}");
            System.Diagnostics.Debug.WriteLine($"Template Count from Claims: {accessibleTemplateIds.Count}");
            System.Diagnostics.Debug.WriteLine($"Template IDs: {string.Join(", ", accessibleTemplateIds)}");
            System.Diagnostics.Debug.WriteLine("==========================================");
            
            return accessibleTemplateIds;
        }

        public async Task<List<StatCardConfig>> BuildQuickStatsAsync(
            int? templateId,  // Make nullable to support overall view
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            int? tenantId = null,
            ClaimsPrincipal currentUser = null)  // Add user for scope filtering
        {
            if (templateId.HasValue)
            {
                // Single template - use existing service methods
                var total = await _statisticsService.GetTotalSubmissionsAsync(templateId.Value, startDate, endDate, tenantId);
                var statusBreakdown = await _statisticsService.GetSubmissionsByStatusAsync(templateId.Value, startDate, endDate, tenantId);

                // Calculate process-focused metrics
                var completedCount = statusBreakdown
                    .Where(kvp => kvp.Key != "Draft")
                    .Sum(kvp => kvp.Value);
                var draftCount = statusBreakdown.GetValueOrDefault("Draft", 0);
                var pendingCount = statusBreakdown.GetValueOrDefault("Submitted", 0) + 
                                  statusBreakdown.GetValueOrDefault("InApproval", 0);

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
                        title: "Completed",
                        value: completedCount.ToString(),
                        icon: "ri-checkbox-circle-line",
                        iconColor: "success",
                        subText: "Submitted, approved & rejected"
                    ),
                    _statCardBuilder.BuildStatCard(
                        title: "Draft",
                        value: draftCount.ToString(),
                        icon: "ri-file-edit-line",
                        iconColor: "warning",
                        subText: "Work in progress"
                    ),
                    _statCardBuilder.BuildStatCard(
                        title: "Pending",
                        value: pendingCount.ToString(),
                        icon: "ri-time-line",
                        iconColor: "info",
                        subText: "Awaiting approval/action"
                    )
                };

                return statCards;
            }
            else
            {
                // Overall view - aggregate across accessible templates
                if (currentUser == null)
                    throw new ArgumentException("CurrentUser required for overall view");

                var accessibleTemplateIds = GetAccessibleTemplateIdsAsync(currentUser);
                
                if (!accessibleTemplateIds.Any())
                {
                    // Return empty stats if no accessible templates
                    return new List<StatCardConfig>
                    {
                        _statCardBuilder.BuildStatCard("Total Submissions", "0", "ri-file-list-3-line", "primary", "No accessible templates"),
                        _statCardBuilder.BuildStatCard("Completed", "0", "ri-checkbox-circle-line", "success", "No accessible templates"),
                        _statCardBuilder.BuildStatCard("Draft", "0", "ri-file-edit-line", "warning", "No accessible templates"),
                        _statCardBuilder.BuildStatCard("Pending", "0", "ri-time-line", "info", "No accessible templates")
                    };
                }

                // Use scope-aware service methods for multiple templates
                var statusBreakdown = await _statisticsService.GetSubmissionsByStatusAsync(accessibleTemplateIds, startDate, endDate, tenantId, currentUser);
                
                // Calculate total submissions
                var total = statusBreakdown.Values.Sum();
                
                // Calculate process-focused metrics
                var completedCount = statusBreakdown
                    .Where(kvp => kvp.Key != "Draft")
                    .Sum(kvp => kvp.Value);
                var draftCount = statusBreakdown.GetValueOrDefault("Draft", 0);
                var pendingCount = statusBreakdown.GetValueOrDefault("Submitted", 0) + 
                                  statusBreakdown.GetValueOrDefault("InApproval", 0);

                var statCards = new List<StatCardConfig>
                {
                    _statCardBuilder.BuildStatCard(
                        title: "Total Submissions",
                        value: total.ToString(),
                        icon: "ri-file-list-3-line",
                        iconColor: "primary",
                        subText: "Across all accessible templates"
                    ),
                    _statCardBuilder.BuildStatCard(
                        title: "Completed",
                        value: completedCount.ToString(),
                        icon: "ri-checkbox-circle-line",
                        iconColor: "success",
                        subText: "Submitted, approved & rejected"
                    ),
                    _statCardBuilder.BuildStatCard(
                        title: "Draft",
                        value: draftCount.ToString(),
                        icon: "ri-file-edit-line",
                        iconColor: "warning",
                        subText: "Work in progress"
                    ),
                    _statCardBuilder.BuildStatCard(
                        title: "Pending",
                        value: pendingCount.ToString(),
                        icon: "ri-time-line",
                        iconColor: "info",
                        subText: "Awaiting approval/action"
                    )
                };

                return statCards;
            }
        }

        private async Task<FilterPanelConfig> BuildFilterPanelAsync(
            int? templateId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? tenantId = null,
            DashboardMode mode = DashboardMode.FullPage)
        {
            // Get current user
            var currentUser = _httpContextAccessor.HttpContext?.User;
            if (currentUser == null)
            {
                throw new InvalidOperationException("Current user not available");
            }

            // Use FilterService to build standard filters
            var filterPanel = await _filterService.BuildStandardFiltersAsync(
                currentUser, 
                templateId, 
                mode.ToString());

            // Set initial values for date filters if provided
            if (startDate.HasValue)
            {
                var startDateFilter = filterPanel.AdvancedFilters?.FirstOrDefault(f => f.Name == "startDate");
                if (startDateFilter != null)
                {
                    startDateFilter.Value = startDate.Value.ToString("yyyy-MM-dd");
                }
            }

            if (endDate.HasValue)
            {
                var endDateFilter = filterPanel.AdvancedFilters?.FirstOrDefault(f => f.Name == "endDate");
                if (endDateFilter != null)
                {
                    endDateFilter.Value = endDate.Value.ToString("yyyy-MM-dd");
                }
            }

            return filterPanel;
        }

        public async Task<ChartCardConfig> BuildTrendChartAsync(
            int? templateId,  // Make nullable to support overall view
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            int? tenantId = null, 
            string groupBy = "Daily",
            ClaimsPrincipal currentUser = null)  // Add user for scope filtering
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;
            
            List<SubmissionTrendDataPoint> trendData;
            
            if (templateId.HasValue)
            {
                // Single template - use existing service method
                trendData = await _statisticsService.GetSubmissionTrendsAsync(
                    templateId.Value, start, end, groupBy, tenantId);
            }
            else
            {
                // Overall view - get accessible templates and use scope-aware method
                if (currentUser == null)
                    throw new ArgumentException("CurrentUser required for overall view");

                var accessibleTemplateIds = GetAccessibleTemplateIdsAsync(currentUser);
                
                if (!accessibleTemplateIds.Any())
                {
                    // Return empty trend data if no accessible templates
                    trendData = new List<SubmissionTrendDataPoint>();
                }
                else
                {
                    trendData = await _statisticsService.GetSubmissionTrendsAsync(
                        accessibleTemplateIds, start, end, groupBy, tenantId, currentUser);
                }
            }

            var labels = trendData.Select(t => t.Label).ToList();
            var data = trendData.Select(t => (decimal)t.Count).ToList();

            var dataset = new ChartDataset
            {
                Name = "Submissions",
                Data = data,
                Color = "#405189"
            };

            return _chartBuilder.BuildLineChart(
                title: templateId.HasValue ? "Submission Trends" : "Overall Submission Trends",
                labels: labels,
                datasets: new List<ChartDataset> { dataset }
            );
        }

        public async Task<ChartCardConfig> BuildStatusChartAsync(
            int? templateId,  // Make nullable to support overall view
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            int? tenantId = null,
            ClaimsPrincipal currentUser = null)  // Add user for scope filtering
        {
            Dictionary<string, int> statusBreakdown;
            
            if (templateId.HasValue)
            {
                // Single template - use existing service method
                statusBreakdown = await _statisticsService.GetSubmissionsByStatusAsync(
                    templateId.Value, startDate, endDate, tenantId);
            }
            else
            {
                // Overall view - get accessible templates and use scope-aware method
                if (currentUser == null)
                    throw new ArgumentException("CurrentUser required for overall view");

                var accessibleTemplateIds = GetAccessibleTemplateIdsAsync(currentUser);
                
                if (!accessibleTemplateIds.Any())
                {
                    // Return empty status breakdown if no accessible templates
                    statusBreakdown = new Dictionary<string, int>();
                }
                else
                {
                    statusBreakdown = await _statisticsService.GetSubmissionsByStatusAsync(
                        accessibleTemplateIds, startDate, endDate, tenantId, currentUser);
                }
            }

            var labels = statusBreakdown.Keys.ToList();
            var data = statusBreakdown.Values.Select(v => (decimal)v).ToList();

            return _chartBuilder.BuildPieChart(
                title: templateId.HasValue ? "Status Breakdown" : "Overall Status Breakdown",
                labels: labels,
                data: data,
                height: 300
            );
        }

        public async Task<DataTableConfig> BuildRecentSubmissionsTableAsync(
            int? templateId,  // Make nullable to support overall view
            int count = 10, 
            int? tenantId = null,
            ClaimsPrincipal currentUser = null)  // Add user for scope filtering
        {
            List<SubmissionSummaryViewModel> submissions;
            
            if (templateId.HasValue)
            {
                // Single template - use existing service method
                submissions = await _statisticsService.GetRecentSubmissionsAsync(
                    templateId.Value, count, tenantId, currentUser);
            }
            else
            {
                // Overall view - get accessible templates and use scope-aware method
                if (currentUser == null)
                    throw new ArgumentException("CurrentUser required for overall view");

                var accessibleTemplateIds = GetAccessibleTemplateIdsAsync(currentUser);
                
                if (!accessibleTemplateIds.Any())
                {
                    // Return empty submissions if no accessible templates
                    submissions = new List<SubmissionSummaryViewModel>();
                }
                else
                {
                    submissions = await _statisticsService.GetRecentSubmissionsAsync(
                        accessibleTemplateIds, count, tenantId, currentUser);
                }
            }

            var columns = new List<TableColumn>
            {
                _tableBuilder.CreateColumn("submissionId", "ID", "center"),
                _tableBuilder.CreateColumn("status", "Status", "center", true, true),
                _tableBuilder.CreateColumn("submittedBy", "Submitted By", "start"),
                _tableBuilder.CreateColumn("tenantName", "Tenant", "start"),
                _tableBuilder.CreateColumn("submittedDate", "Date", "start")
            };

            var rows = submissions.Select(s => new Dictionary<string, object>
            {
                ["submissionId"] = s.SubmissionId,
                ["status"] = s.Status,
                ["submittedBy"] = s.SubmittedBy,
                ["tenantName"] = s.TenantName,
                ["submittedDate"] = (object)(s.SubmittedDate ?? DateTime.MinValue)
            }).ToList();

            return _tableBuilder.BuildTable(
                title: templateId.HasValue ? "Recent Submissions" : "Recent Submissions (All Templates)",
                columns: columns,
                rows: rows,
                paginated: false
            );
        }

        public async Task<DataTableConfig> BuildTenantComparisonTableAsync(
            int templateId, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            var tenantStats = await _statisticsService.GetTenantComparisonAsync(
                templateId, startDate, endDate);

            var columns = new List<TableColumn>
            {
                _tableBuilder.CreateColumn("tenantName", "Tenant", "start"),
                _tableBuilder.CreateColumn("totalSubmissions", "Total", "center"),
                _tableBuilder.CreateColumn("submittedCount", "Submitted", "center"),
                _tableBuilder.CreateColumn("draftCount", "Draft", "center"),
                _tableBuilder.CreateColumn("approvedCount", "Approved", "center"),
                _tableBuilder.CreateColumn("rejectedCount", "Rejected", "center")
            };

            var rows = tenantStats.Select(t => new Dictionary<string, object>
            {
                ["tenantName"] = t.TenantName,
                ["totalSubmissions"] = t.TotalSubmissions,
                ["submittedCount"] = t.SubmittedCount,
                ["draftCount"] = t.DraftCount,
                ["approvedCount"] = t.ApprovedCount,
                ["rejectedCount"] = t.RejectedCount
            }).ToList();

            return _tableBuilder.BuildTable(
                title: "Tenant Comparison",
                columns: columns,
                rows: rows
            );
        }
    }
}
