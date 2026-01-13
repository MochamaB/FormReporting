namespace FormReporting.Models.ViewModels.Dashboard.Components.Composite
{
    /// <summary>
    /// TIER 2 COMPOSITE: Dashboard section definition
    /// Represents one section of a dashboard (chart, table, stat cards, etc.)
    /// </summary>
    public class DashboardSection
    {
        /// <summary>
        /// Section identifier (used for AJAX loading)
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Section title (optional)
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Display order (lower numbers first)
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        /// Component to render (ChartCardConfig, DataTableConfig, etc.)
        /// </summary>
        public object? Component { get; set; }

        /// <summary>
        /// Type of component
        /// </summary>
        public SectionComponentType ComponentType { get; set; }

        /// <summary>
        /// Bootstrap column width class (e.g., "col-lg-8", "col-md-6")
        /// </summary>
        public string ColumnWidth { get; set; } = "col-12";

        /// <summary>
        /// How this section should be loaded
        /// </summary>
        public SectionLoadMethod LoadMethod { get; set; } = SectionLoadMethod.Server;

        /// <summary>
        /// AJAX URL for progressive loading (if LoadMethod = Ajax)
        /// </summary>
        public string? AjaxUrl { get; set; }

        /// <summary>
        /// Auto-refresh interval in seconds (optional)
        /// </summary>
        public int? RefreshInterval { get; set; }

        /// <summary>
        /// Additional metadata (for backward compatibility with DashboardSectionConfig)
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Helper: Create chart section
        /// </summary>
        public static DashboardSection Chart(string id, ChartCardConfig? chartConfig, string columnWidth = "col-lg-8", SectionLoadMethod loadMethod = SectionLoadMethod.Server)
        {
            return new DashboardSection
            {
                Id = id,
                Component = chartConfig,
                ComponentType = SectionComponentType.Chart,
                ColumnWidth = columnWidth,
                LoadMethod = loadMethod
            };
        }

        /// <summary>
        /// Helper: Create table section
        /// </summary>
        public static DashboardSection Table(string id, DataTableConfig? tableConfig, string columnWidth = "col-12", SectionLoadMethod loadMethod = SectionLoadMethod.Server)
        {
            return new DashboardSection
            {
                Id = id,
                Component = tableConfig,
                ComponentType = SectionComponentType.Table,
                ColumnWidth = columnWidth,
                LoadMethod = loadMethod
            };
        }

        /// <summary>
        /// Helper: Create stat cards section
        /// </summary>
        public static DashboardSection StatCards(string id, StatCardGroupConfig? statCards, string columnWidth = "col-12")
        {
            return new DashboardSection
            {
                Id = id,
                Component = statCards,
                ComponentType = SectionComponentType.StatCards,
                ColumnWidth = columnWidth,
                LoadMethod = SectionLoadMethod.Server
            };
        }

        /// <summary>
        /// Helper: Create AJAX-loaded chart section
        /// </summary>
        public static DashboardSection AjaxChart(string id, string ajaxUrl, string columnWidth = "col-lg-8")
        {
            return new DashboardSection
            {
                Id = id,
                ComponentType = SectionComponentType.Chart,
                ColumnWidth = columnWidth,
                LoadMethod = SectionLoadMethod.Ajax,
                AjaxUrl = ajaxUrl
            };
        }

        /// <summary>
        /// Helper: Create AJAX-loaded table section
        /// </summary>
        public static DashboardSection AjaxTable(string id, string ajaxUrl, string columnWidth = "col-12")
        {
            return new DashboardSection
            {
                Id = id,
                ComponentType = SectionComponentType.Table,
                ColumnWidth = columnWidth,
                LoadMethod = SectionLoadMethod.Ajax,
                AjaxUrl = ajaxUrl
            };
        }
    }

    /// <summary>
    /// Type of component in a dashboard section
    /// </summary>
    public enum SectionComponentType
    {
        Chart,
        Table,
        StatCards,
        FilterPanel,
        Custom
    }

    /// <summary>
    /// How a section should be loaded
    /// </summary>
    public enum SectionLoadMethod
    {
        /// <summary>
        /// Rendered on server with data
        /// </summary>
        Server,

        /// <summary>
        /// Loaded via AJAX after page load
        /// </summary>
        Ajax,

        /// <summary>
        /// Placeholder only, manual JavaScript loading
        /// </summary>
        Manual
    }
}
