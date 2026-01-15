using FormReporting.Models.Common;

namespace FormReporting.Models.ViewModels.Dashboard
{
    /// <summary>
    /// Layout configuration for dashboard grid
    /// </summary>
    public class DashboardLayoutViewModel
    {
        /// <summary>
        /// Number of columns in the grid (default 12 for Bootstrap compatibility)
        /// </summary>
        public int Columns { get; set; } = 12;

        /// <summary>
        /// Gap between rows in pixels or CSS unit
        /// </summary>
        public string RowGap { get; set; } = "1rem";

        /// <summary>
        /// Gap between columns in pixels or CSS unit
        /// </summary>
        public string ColumnGap { get; set; } = "1rem";

        /// <summary>
        /// Layout mode determining widget placement behavior
        /// </summary>
        public LayoutMode Mode { get; set; } = LayoutMode.Auto;

        /// <summary>
        /// Minimum row height for grid items
        /// </summary>
        public string MinRowHeight { get; set; } = "100px";

        /// <summary>
        /// Whether to use CSS Grid (true) or Bootstrap grid (false)
        /// </summary>
        public bool UseCssGrid { get; set; } = false;

        /// <summary>
        /// Responsive breakpoint for collapsing to single column (in pixels)
        /// </summary>
        public int MobileBreakpoint { get; set; } = 768;

        /// <summary>
        /// Responsive breakpoint for medium screens (in pixels)
        /// </summary>
        public int TabletBreakpoint { get; set; } = 992;
    }
}
