using FormReporting.Models.Common;
using FormReporting.Models.ViewModels.Dashboard;

namespace FormReporting.Services.Dashboard
{
    /// <summary>
    /// Service interface for dashboard orchestration
    /// Coordinates dashboard configuration, widget data providers, and response assembly
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Gets a complete dashboard with all widgets populated
        /// </summary>
        /// <param name="dashboardKey">The dashboard identifier (e.g., "form-statistics")</param>
        /// <param name="filters">Optional filters to apply to all widgets</param>
        /// <param name="contextType">Optional context type for entity filtering</param>
        /// <param name="contextId">Optional context entity ID</param>
        /// <returns>Complete dashboard view model with widget data</returns>
        Task<DashboardViewModel?> GetDashboardAsync(
            string dashboardKey,
            DashboardFilterViewModel? filters = null,
            ContextType contextType = ContextType.None,
            int? contextId = null);

        /// <summary>
        /// Gets data for a single widget
        /// </summary>
        /// <param name="widgetKey">The widget identifier</param>
        /// <param name="filters">Optional filters to apply</param>
        /// <param name="contextType">Optional context type for entity filtering</param>
        /// <param name="contextId">Optional context entity ID</param>
        /// <returns>Widget view model with data</returns>
        Task<WidgetViewModel?> GetWidgetAsync(
            string widgetKey,
            DashboardFilterViewModel? filters = null,
            ContextType contextType = ContextType.None,
            int? contextId = null);

        /// <summary>
        /// Gets all available dashboard definitions
        /// </summary>
        /// <returns>List of dashboard configurations (without widget data)</returns>
        IEnumerable<DashboardViewModel> GetAvailableDashboards();

        /// <summary>
        /// Gets dashboard configuration without loading widget data
        /// </summary>
        /// <param name="dashboardKey">The dashboard identifier</param>
        /// <returns>Dashboard configuration with widgets in loading state</returns>
        DashboardViewModel? GetDashboardConfig(string dashboardKey);

        /// <summary>
        /// Checks if a dashboard exists
        /// </summary>
        /// <param name="dashboardKey">The dashboard identifier</param>
        /// <returns>True if dashboard exists in registry</returns>
        bool DashboardExists(string dashboardKey);

        /// <summary>
        /// Gets context selector options for a context type
        /// </summary>
        /// <param name="contextType">The type of context to get options for</param>
        /// <returns>List of selectable options (id, label pairs)</returns>
        Task<List<ContextOptionViewModel>> GetContextOptionsAsync(ContextType contextType);

        /// <summary>
        /// Refreshes all widgets in a dashboard
        /// </summary>
        /// <param name="dashboardKey">The dashboard identifier</param>
        /// <param name="filters">Optional filters to apply</param>
        /// <param name="contextType">Optional context type</param>
        /// <param name="contextId">Optional context entity ID</param>
        /// <returns>List of refreshed widget view models</returns>
        Task<List<WidgetViewModel>> RefreshDashboardWidgetsAsync(
            string dashboardKey,
            DashboardFilterViewModel? filters = null,
            ContextType contextType = ContextType.None,
            int? contextId = null);
    }

    /// <summary>
    /// Option for context selector dropdown
    /// </summary>
    public class ContextOptionViewModel
    {
        /// <summary>
        /// Entity ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Display label
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Optional group/category for the option
        /// </summary>
        public string? Group { get; set; }

        /// <summary>
        /// Optional description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Optional icon class
        /// </summary>
        public string? Icon { get; set; }
    }
}
