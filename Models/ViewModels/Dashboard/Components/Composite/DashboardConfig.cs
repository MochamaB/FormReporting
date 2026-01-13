namespace FormReporting.Models.ViewModels.Dashboard.Components.Composite
{
    /// <summary>
    /// TIER 2 COMPOSITE: Complete dashboard configuration
    /// Unified model for rendering any dashboard (full page or widget)
    /// </summary>
    public class DashboardConfig
    {
        /// <summary>
        /// Dashboard identifier
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Dashboard title (optional)
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Filter panel configuration (optional)
        /// </summary>
        public FilterPanelConfig? FilterPanel { get; set; }

        /// <summary>
        /// Quick stats/KPI cards displayed at top (optional)
        /// </summary>
        public StatCardGroupConfig? QuickStats { get; set; }

        /// <summary>
        /// Dashboard sections (charts, tables, etc.)
        /// </summary>
        public List<DashboardSection> Sections { get; set; } = new List<DashboardSection>();

        /// <summary>
        /// Dashboard display mode
        /// </summary>
        public DashboardMode Mode { get; set; } = DashboardMode.FullPage;

        /// <summary>
        /// Loading strategy for dashboard sections
        /// </summary>
        public LoadingStrategy LoadingStrategy { get; set; } = LoadingStrategy.ServerSide;

        /// <summary>
        /// Show breadcrumbs (for full page mode)
        /// </summary>
        public bool ShowBreadcrumbs { get; set; } = true;

        /// <summary>
        /// Custom CSS classes for dashboard container
        /// </summary>
        public string? ContainerClass { get; set; }

        /// <summary>
        /// Helper: Create full page dashboard
        /// </summary>
        public static DashboardConfig FullPage(string title)
        {
            return new DashboardConfig
            {
                Title = title,
                Mode = DashboardMode.FullPage,
                ShowBreadcrumbs = true,
                LoadingStrategy = LoadingStrategy.ServerSide
            };
        }

        /// <summary>
        /// Helper: Create widget/embedded dashboard
        /// </summary>
        public static DashboardConfig Widget(string id)
        {
            return new DashboardConfig
            {
                Id = id,
                Mode = DashboardMode.Widget,
                ShowBreadcrumbs = false,
                LoadingStrategy = LoadingStrategy.Progressive
            };
        }
    }

    /// <summary>
    /// Dashboard display mode
    /// </summary>
    public enum DashboardMode
    {
        /// <summary>
        /// Full page dashboard with all features
        /// </summary>
        FullPage,

        /// <summary>
        /// Embedded widget (compact, no breadcrumbs)
        /// </summary>
        Widget,

        /// <summary>
        /// Embedded in tab or panel
        /// </summary>
        Embedded
    }

    /// <summary>
    /// Dashboard loading strategy
    /// </summary>
    public enum LoadingStrategy
    {
        /// <summary>
        /// All sections rendered on server
        /// </summary>
        ServerSide,

        /// <summary>
        /// Sections loaded progressively via AJAX
        /// </summary>
        Progressive,

        /// <summary>
        /// Critical sections server-side, others AJAX
        /// </summary>
        Hybrid
    }
}
