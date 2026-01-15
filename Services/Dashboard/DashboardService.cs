using FormReporting.Models.Common;
using FormReporting.Models.ViewModels.Dashboard;

namespace FormReporting.Services.Dashboard
{
    /// <summary>
    /// Dashboard orchestration service
    /// Coordinates dashboard configuration, widget data providers, and response assembly
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly IEnumerable<IWidgetDataProvider> _providers;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IEnumerable<IWidgetDataProvider> providers,
            ILogger<DashboardService> logger)
        {
            _providers = providers;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<DashboardViewModel?> GetDashboardAsync(
            string dashboardKey,
            DashboardFilterViewModel? filters = null,
            ContextType contextType = ContextType.None,
            int? contextId = null)
        {
            var config = DashboardRegistry.GetDashboard(dashboardKey);
            if (config == null)
            {
                _logger.LogWarning("Dashboard not found: {DashboardKey}", dashboardKey);
                return null;
            }

            // Create view model from config
            var dashboard = config.ToViewModel();
            dashboard.Filters = filters;
            dashboard.ContextType = contextType != ContextType.None ? contextType : config.ContextType;
            dashboard.ContextId = contextId;

            // Load widget data
            foreach (var widget in dashboard.Widgets)
            {
                await PopulateWidgetDataAsync(widget, filters, dashboard.ContextType, contextId);
            }

            dashboard.LastRefreshed = DateTime.UtcNow;

            return dashboard;
        }

        /// <inheritdoc />
        public async Task<WidgetViewModel?> GetWidgetAsync(
            string widgetKey,
            DashboardFilterViewModel? filters = null,
            ContextType contextType = ContextType.None,
            int? contextId = null)
        {
            // Find widget config in any dashboard
            var widgetConfig = FindWidgetConfig(widgetKey);
            if (widgetConfig == null)
            {
                _logger.LogWarning("Widget not found: {WidgetKey}", widgetKey);
                return null;
            }

            var widget = widgetConfig.ToViewModel();
            await PopulateWidgetDataAsync(widget, filters, contextType, contextId);

            return widget;
        }

        /// <inheritdoc />
        public IEnumerable<DashboardViewModel> GetAvailableDashboards()
        {
            return DashboardRegistry.GetAllDashboards()
                .Select(config => new DashboardViewModel
                {
                    Key = config.Key,
                    Title = config.Title,
                    Description = config.Description,
                    Icon = config.Icon,
                    ContextType = config.ContextType,
                    HasContextSelector = config.HasContextSelector,
                    HasFilterBar = config.HasFilterBar
                });
        }

        /// <inheritdoc />
        public DashboardViewModel? GetDashboardConfig(string dashboardKey)
        {
            var config = DashboardRegistry.GetDashboard(dashboardKey);
            return config?.ToViewModel();
        }

        /// <inheritdoc />
        public bool DashboardExists(string dashboardKey)
        {
            return DashboardRegistry.Exists(dashboardKey);
        }

        /// <inheritdoc />
        public async Task<List<ContextOptionViewModel>> GetContextOptionsAsync(ContextType contextType)
        {
            // This will be implemented in Phase 4
            // For now, return empty list
            await Task.CompletedTask;

            return contextType switch
            {
                ContextType.FormTemplate => new List<ContextOptionViewModel>(),
                ContextType.Tenant => new List<ContextOptionViewModel>(),
                ContextType.Region => new List<ContextOptionViewModel>(),
                _ => new List<ContextOptionViewModel>()
            };
        }

        /// <inheritdoc />
        public async Task<List<WidgetViewModel>> RefreshDashboardWidgetsAsync(
            string dashboardKey,
            DashboardFilterViewModel? filters = null,
            ContextType contextType = ContextType.None,
            int? contextId = null)
        {
            var dashboard = await GetDashboardAsync(dashboardKey, filters, contextType, contextId);
            return dashboard?.Widgets ?? new List<WidgetViewModel>();
        }

        /// <summary>
        /// Populates widget data from the appropriate provider
        /// </summary>
        private async Task PopulateWidgetDataAsync(
            WidgetViewModel widget,
            DashboardFilterViewModel? filters,
            ContextType contextType,
            int? contextId)
        {
            try
            {
                // Find provider that can handle this widget
                var provider = _providers.FirstOrDefault(p => p.CanHandle(widget.Key));

                if (provider == null)
                {
                    _logger.LogDebug("No provider found for widget: {WidgetKey}", widget.Key);
                    widget.Status = WidgetStatus.Empty;
                    return;
                }

                // Get data from provider
                var data = await provider.GetWidgetDataAsync(widget.Key, filters, contextType, contextId);

                if (data == null)
                {
                    widget.Status = WidgetStatus.Empty;
                }
                else
                {
                    widget.Data = data;
                    widget.Status = WidgetStatus.Success;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading widget data: {WidgetKey}", widget.Key);
                widget.Status = WidgetStatus.Error;
                widget.ErrorMessage = "Failed to load widget data";
            }
        }

        /// <summary>
        /// Finds a widget configuration across all dashboards
        /// </summary>
        private WidgetConfig? FindWidgetConfig(string widgetKey)
        {
            foreach (var dashboard in DashboardRegistry.GetAllDashboards())
            {
                var widget = dashboard.Widgets.FirstOrDefault(
                    w => w.Key.Equals(widgetKey, StringComparison.OrdinalIgnoreCase));

                if (widget != null)
                    return widget;
            }

            return null;
        }
    }
}
