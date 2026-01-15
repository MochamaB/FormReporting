using FormReporting.Models.Common;
using FormReporting.Models.ViewModels.Dashboard;
using FormReporting.Services.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FormReporting.Controllers
{
    /// <summary>
    /// Controller for dashboard rendering and widget data API
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard hub showing available dashboards
        /// GET /Dashboard
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            var dashboards = _dashboardService.GetAvailableDashboards().ToList();
            return View(dashboards);
        }

        /// <summary>
        /// Renders a specific dashboard
        /// GET /Dashboard/Render?key={key}
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Render(
            string key,
            [FromQuery] ContextType? contextType = null,
            [FromQuery] int? contextId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? tenantId = null,
            [FromQuery] int? regionId = null,
            [FromQuery] string? datePreset = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogWarning("Dashboard key is required");
                return RedirectToAction(nameof(Index));
            }

            if (!_dashboardService.DashboardExists(key))
            {
                _logger.LogWarning("Dashboard not found: {Key}", key);
                return NotFound($"Dashboard '{key}' not found");
            }

            // Build filters from query parameters
            var filters = new DashboardFilterViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                TenantId = tenantId,
                RegionId = regionId,
                DatePreset = datePreset
            };

            // Apply date preset if specified
            if (!string.IsNullOrEmpty(datePreset))
            {
                filters = filters.WithDatePreset(datePreset);
            }

            var dashboard = await _dashboardService.GetDashboardAsync(
                key,
                filters,
                contextType ?? ContextType.None,
                contextId);

            if (dashboard == null)
            {
                return NotFound($"Dashboard '{key}' could not be loaded");
            }

            return View(dashboard);
        }

        /// <summary>
        /// API: Get single widget data
        /// GET /api/dashboard/widget?key={key}
        /// </summary>
        [HttpGet]
        [Route("api/dashboard/widget")]
        public async Task<IActionResult> GetWidgetData(
            [FromQuery] string key,
            [FromQuery] ContextType? contextType = null,
            [FromQuery] int? contextId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                return BadRequest("Widget key is required");
            }

            var filters = new DashboardFilterViewModel
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var widget = await _dashboardService.GetWidgetAsync(
                key,
                filters,
                contextType ?? ContextType.None,
                contextId);

            if (widget == null)
            {
                return NotFound($"Widget '{key}' not found");
            }

            return Json(widget);
        }

        /// <summary>
        /// API: Get context selector options
        /// GET /api/dashboard/context-options?type={type}
        /// </summary>
        [HttpGet]
        [Route("api/dashboard/context-options")]
        public async Task<IActionResult> GetContextOptions([FromQuery] ContextType type)
        {
            var options = await _dashboardService.GetContextOptionsAsync(type);
            return Json(options);
        }

        /// <summary>
        /// API: Refresh all dashboard widgets
        /// GET /api/dashboard/refresh?key={key}
        /// </summary>
        [HttpGet]
        [Route("api/dashboard/refresh")]
        public async Task<IActionResult> RefreshDashboard(
            [FromQuery] string key,
            [FromQuery] ContextType? contextType = null,
            [FromQuery] int? contextId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                return BadRequest("Dashboard key is required");
            }

            var filters = new DashboardFilterViewModel
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var widgets = await _dashboardService.RefreshDashboardWidgetsAsync(
                key,
                filters,
                contextType ?? ContextType.None,
                contextId);

            return Json(widgets);
        }
    }
}
