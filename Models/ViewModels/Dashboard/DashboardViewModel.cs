using FormReporting.Models.Common;

namespace FormReporting.Models.ViewModels.Dashboard
{
    /// <summary>
    /// Complete dashboard view model containing all widgets, layout, filters, and context
    /// </summary>
    public class DashboardViewModel
    {
        /// <summary>
        /// Unique identifier for the dashboard (e.g., "form-statistics", "form-scoring")
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Display title for the dashboard
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the dashboard
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Icon class for the dashboard (e.g., "ri-bar-chart-box-line")
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Layout configuration for the dashboard grid
        /// </summary>
        public DashboardLayoutViewModel Layout { get; set; } = new();

        /// <summary>
        /// Collection of widgets to display on the dashboard
        /// </summary>
        public List<WidgetViewModel> Widgets { get; set; } = new();

        /// <summary>
        /// Current filter state (date range, tenant, region, etc.)
        /// </summary>
        public DashboardFilterViewModel? Filters { get; set; }

        /// <summary>
        /// Context type for entity-specific filtering (None = show all data)
        /// </summary>
        public ContextType ContextType { get; set; } = ContextType.None;

        /// <summary>
        /// Context entity ID when filtering to a specific entity
        /// </summary>
        public int? ContextId { get; set; }

        /// <summary>
        /// Display label for the current context (e.g., form template name)
        /// </summary>
        public string? ContextLabel { get; set; }

        /// <summary>
        /// Whether the dashboard supports context selection dropdown
        /// </summary>
        public bool HasContextSelector { get; set; }

        /// <summary>
        /// Whether the dashboard has a filter bar
        /// </summary>
        public bool HasFilterBar { get; set; }

        /// <summary>
        /// Timestamp when dashboard data was last refreshed
        /// </summary>
        public DateTime? LastRefreshed { get; set; }
    }
}
