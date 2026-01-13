using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FormReporting.Models.ViewModels.Dashboard.Components.Composite;
using FormReporting.Services.Dashboard.Common;
using FormReporting.Services.Dashboard.FormStatistics;
using FormReporting.Services.Identity;

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
        /// Single dashboard page - handles both overall and per-template views
        /// GET /Dashboard/FormStatistics?templateId=X (optional template parameter)
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> Index(int? templateId, DateTime? startDate, 
            DateTime? endDate, int? tenantId, string groupBy = "Daily")
        {
            // Apply scope filtering
            var accessibleTenantIds = await _scopeService.GetAccessibleTenantIdsAsync(User);
            if (tenantId.HasValue && !accessibleTenantIds.Contains(tenantId.Value))
            {
                return Forbid();
            }

            // Determine dashboard mode based on context
            DashboardMode mode;
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                mode = DashboardMode.Embedded; // AJAX calls are embedded
            }
            else
            {
                mode = DashboardMode.FullPage;
            }

            // Build dashboard config
            var dashboardConfig = await _dashboardBuilder.BuildDashboardAsync(
                templateId, startDate, endDate, tenantId, mode);

            // Don't show breadcrumbs in full page mode (layout already has them)
            if (mode == DashboardMode.FullPage)
            {
                dashboardConfig.ShowBreadcrumbs = false;
            }

            // Pass parameters to ViewBag for JavaScript initialization
            ViewBag.TemplateId = templateId;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.TenantId = tenantId;
            ViewBag.GroupBy = groupBy;
            ViewBag.IsEmbedded = (mode == DashboardMode.Embedded);

            // Always return the same view, mode controls rendering
            return View("~/Views/Dashboard/FormStatistics/Index.cshtml", dashboardConfig);
        }

        /// <summary>
        /// Legacy PerTemplate endpoint - builds embedded dashboard directly
        /// Kept for backward compatibility
        /// </summary>
        [HttpGet("PerTemplate")]
        public async Task<IActionResult> PerTemplate(int templateId, DateTime? startDate, 
            DateTime? endDate, int? tenantId, string groupBy = "Daily")
        {
            // Apply scope filtering
            var accessibleTenantIds = await _scopeService.GetAccessibleTenantIdsAsync(User);
            if (tenantId.HasValue && !accessibleTenantIds.Contains(tenantId.Value))
            {
                return Forbid();
            }

            // Build embedded dashboard config
            var dashboardConfig = await _dashboardBuilder.BuildDashboardAsync(
                templateId, startDate, endDate, tenantId, DashboardMode.Embedded);

            // Pass parameters to ViewBag for JavaScript initialization
            ViewBag.TemplateId = templateId;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.TenantId = tenantId;
            ViewBag.GroupBy = groupBy;
            ViewBag.IsEmbedded = true;

            // Return the same view, but in embedded mode
            return View("~/Views/Dashboard/FormStatistics/Index.cshtml", dashboardConfig);
        }

        /// <summary>
        /// Get quick stats section (can be used for refresh)
        /// GET /Dashboard/FormStatistics/GetQuickStats
        /// </summary>
        [HttpGet("GetQuickStats")]
        public async Task<IActionResult> GetQuickStats(int? templateId, DateTime? startDate, 
            DateTime? endDate, int? tenantId, int? regionId, int? submitterId, string status)
        {
            try
            {
                _logger.LogInformation("GetQuickStats called: templateId={TemplateId}, startDate={StartDate}, endDate={EndDate}, tenantId={TenantId}",
                    templateId, startDate, endDate, tenantId);

                var statCards = await _dashboardBuilder.BuildQuickStatsAsync(
                    templateId, startDate, endDate, tenantId, User);

                // Wrap in StatCardGroupConfig using the FourColumns helper
                var groupConfig = FormReporting.Models.ViewModels.Dashboard.Components.Composite.StatCardGroupConfig.FourColumns(statCards.ToArray());

                return PartialView("~/Views/Shared/Components/Dashboard/Composite/_StatCardGroup.cshtml", 
                    groupConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading quick stats for template {TemplateId}", templateId);
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Get trend chart data (AJAX)
        /// GET /Dashboard/FormStatistics/GetTrendChart
        /// </summary>
        [HttpGet("GetTrendChart")]
        public async Task<IActionResult> GetTrendChart(int? templateId, DateTime? startDate, 
            DateTime? endDate, int? tenantId, int? regionId, int? submitterId, string status, string groupBy = "Daily")
        {
            var chartConfig = await _dashboardBuilder.BuildTrendChartAsync(
                templateId, startDate, endDate, tenantId, groupBy, User);

            return Json(chartConfig);
        }

        /// <summary>
        /// Get status breakdown chart data (AJAX)
        /// GET /Dashboard/FormStatistics/GetStatusChart
        /// </summary>
        [HttpGet("GetStatusChart")]
        public async Task<IActionResult> GetStatusChart(int? templateId, DateTime? startDate, 
            DateTime? endDate, int? tenantId, int? regionId, int? submitterId, string status)
        {
            var chartConfig = await _dashboardBuilder.BuildStatusChartAsync(
                templateId, startDate, endDate, tenantId, User);

            return Json(chartConfig);
        }

        /// <summary>
        /// Get tenants for a specific region (AJAX for cascading filters)
        /// GET /Dashboard/FormStatistics/GetTenantsByRegion
        /// </summary>
        [HttpGet("GetTenantsByRegion")]
        public async Task<IActionResult> GetTenantsByRegion(int? regionId = null)
        {
            try
            {
                // Get filter service to get tenant options
                var filterService = HttpContext.RequestServices
                    .GetRequiredService<FormReporting.Services.Dashboard.IFilterService>();
                
                var tenantOptions = await filterService.GetTenantOptionsAsync(User, regionId);
                
                return Json(tenantOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tenants for region {RegionId}", regionId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get recent submissions table (AJAX)
        /// GET /Dashboard/FormStatistics/GetRecentSubmissions
        /// </summary>
        [HttpGet("GetRecentSubmissions")]
        public async Task<IActionResult> GetRecentSubmissions(int? templateId, 
            int count = 10, int? tenantId = null, int? regionId = null, int? submitterId = null, string status = null)
        {
            try
            {
                var dataTableConfig = await _dashboardBuilder.BuildRecentSubmissionsTableAsync(
                    templateId, count, tenantId, User);

                return PartialView("~/Views/Shared/Components/Dashboard/Composite/_DataTable.cshtml", 
                    dataTableConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recent submissions for template {TemplateId}", templateId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get tenant comparison table (AJAX)
        /// GET /Dashboard/FormStatistics/GetTenantComparison
        /// </summary>
        [HttpGet("GetTenantComparison")]
        public async Task<IActionResult> GetTenantComparison(int templateId, 
            DateTime? startDate = null, DateTime? endDate = null, int? regionId = null, int? submitterId = null, string status = null)
        {
            try
            {
                // Check if user has multi-tenant access
                var accessibleTenantIds = await _scopeService.GetAccessibleTenantIdsAsync(User);
                if (accessibleTenantIds.Count <= 1)
                {
                    return Json(new { error = "Multi-tenant access required" });
                }

                var dataTableConfig = await _dashboardBuilder.BuildTenantComparisonTableAsync(
                    templateId, startDate, endDate);

                return PartialView("~/Views/Shared/Components/Dashboard/Composite/_DataTable.cshtml", 
                    dataTableConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tenant comparison for template {TemplateId}", templateId);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
