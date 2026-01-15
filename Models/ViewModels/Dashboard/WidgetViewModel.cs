using FormReporting.Models.Common;

namespace FormReporting.Models.ViewModels.Dashboard
{
    /// <summary>
    /// Single widget view model with type, status, position, and data
    /// </summary>
    public class WidgetViewModel
    {
        /// <summary>
        /// Unique identifier for the widget (e.g., "total-submissions", "completion-rate")
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Type of widget to render (StatCard, BarChart, etc.)
        /// </summary>
        public WidgetType Type { get; set; }

        /// <summary>
        /// Display title for the widget
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Optional subtitle or description
        /// </summary>
        public string? Subtitle { get; set; }

        /// <summary>
        /// Widget size in the grid (determines column span)
        /// </summary>
        public WidgetSize Size { get; set; } = WidgetSize.Medium;

        /// <summary>
        /// Position configuration for fixed/mixed layouts
        /// </summary>
        public WidgetPositionViewModel? Position { get; set; }

        /// <summary>
        /// Current loading/data state of the widget
        /// </summary>
        public WidgetStatus Status { get; set; } = WidgetStatus.Loading;

        /// <summary>
        /// Widget data (type depends on WidgetType)
        /// Cast to appropriate data ViewModel based on Type:
        /// - StatCard → StatCardDataViewModel
        /// - BarChart/LineChart/PieChart/DoughnutChart → ChartDataViewModel
        /// - Gauge/ProgressBar → GaugeDataViewModel
        /// - DataTable → TableDataViewModel
        /// - List → ListDataViewModel
        /// - Sparkline → SparklineDataViewModel
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Error message when Status is Error
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Optional link for widget drill-down navigation
        /// </summary>
        public string? DrillDownUrl { get; set; }

        /// <summary>
        /// CSS class for custom styling
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Whether the widget supports refresh
        /// </summary>
        public bool CanRefresh { get; set; } = true;

        /// <summary>
        /// Display order for auto layout mode
        /// </summary>
        public int Order { get; set; }
    }
}
