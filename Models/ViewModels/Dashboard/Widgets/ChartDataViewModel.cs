namespace FormReporting.Models.ViewModels.Dashboard.Widgets
{
    /// <summary>
    /// Data view model for chart widgets (BarChart, LineChart, PieChart, DoughnutChart)
    /// Compatible with ApexCharts configuration
    /// </summary>
    public class ChartDataViewModel
    {
        /// <summary>
        /// Labels for the X-axis or pie slices
        /// </summary>
        public List<string> Labels { get; set; } = new();

        /// <summary>
        /// Data series for the chart
        /// </summary>
        public List<ChartDatasetViewModel> Datasets { get; set; } = new();

        /// <summary>
        /// Chart-specific options (passed to ApexCharts)
        /// </summary>
        public ChartOptionsViewModel Options { get; set; } = new();

        /// <summary>
        /// Chart height in pixels (default 350)
        /// </summary>
        public int Height { get; set; } = 350;

        /// <summary>
        /// Whether to show the legend
        /// </summary>
        public bool ShowLegend { get; set; } = true;

        /// <summary>
        /// Whether to show data labels on the chart
        /// </summary>
        public bool ShowDataLabels { get; set; } = false;

        /// <summary>
        /// Whether the chart should be stacked (for bar/line charts)
        /// </summary>
        public bool Stacked { get; set; } = false;

        /// <summary>
        /// Whether the chart is horizontal (for bar charts)
        /// </summary>
        public bool Horizontal { get; set; } = false;
    }

    /// <summary>
    /// Single data series for a chart
    /// </summary>
    public class ChartDatasetViewModel
    {
        /// <summary>
        /// Name of the data series (shown in legend)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Data values for this series
        /// </summary>
        public List<decimal> Data { get; set; } = new();

        /// <summary>
        /// Color for this series (hex or CSS color)
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Chart type for this series (for mixed charts)
        /// Options: "bar", "line", "area"
        /// </summary>
        public string? Type { get; set; }
    }

    /// <summary>
    /// Chart configuration options
    /// </summary>
    public class ChartOptionsViewModel
    {
        /// <summary>
        /// Custom colors array for the chart
        /// </summary>
        public List<string>? Colors { get; set; }

        /// <summary>
        /// X-axis configuration
        /// </summary>
        public ChartAxisViewModel? XAxis { get; set; }

        /// <summary>
        /// Y-axis configuration
        /// </summary>
        public ChartAxisViewModel? YAxis { get; set; }

        /// <summary>
        /// Tooltip configuration
        /// </summary>
        public ChartTooltipViewModel? Tooltip { get; set; }

        /// <summary>
        /// Format string for values (e.g., "{value}%", "${value}")
        /// </summary>
        public string? ValueFormat { get; set; }

        /// <summary>
        /// Whether to animate the chart
        /// </summary>
        public bool Animate { get; set; } = true;

        /// <summary>
        /// Stroke/line width for line charts
        /// </summary>
        public int StrokeWidth { get; set; } = 2;

        /// <summary>
        /// Whether to use smooth curves for line charts
        /// </summary>
        public bool SmoothCurve { get; set; } = true;

        /// <summary>
        /// Fill opacity for area charts (0-1)
        /// </summary>
        public decimal FillOpacity { get; set; } = 0.3m;

        /// <summary>
        /// Donut hole size percentage (for doughnut charts, 0-100)
        /// </summary>
        public int DonutSize { get; set; } = 65;
    }

    /// <summary>
    /// Chart axis configuration
    /// </summary>
    public class ChartAxisViewModel
    {
        /// <summary>
        /// Axis title
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Minimum value for the axis
        /// </summary>
        public decimal? Min { get; set; }

        /// <summary>
        /// Maximum value for the axis
        /// </summary>
        public decimal? Max { get; set; }

        /// <summary>
        /// Number of decimal places for labels
        /// </summary>
        public int? DecimalPlaces { get; set; }

        /// <summary>
        /// Whether to show the axis line
        /// </summary>
        public bool ShowAxisLine { get; set; } = true;

        /// <summary>
        /// Whether to show grid lines
        /// </summary>
        public bool ShowGridLines { get; set; } = true;
    }

    /// <summary>
    /// Chart tooltip configuration
    /// </summary>
    public class ChartTooltipViewModel
    {
        /// <summary>
        /// Whether tooltips are enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether to show shared tooltip for all series
        /// </summary>
        public bool Shared { get; set; } = true;

        /// <summary>
        /// Value suffix (e.g., "%", " units")
        /// </summary>
        public string? ValueSuffix { get; set; }

        /// <summary>
        /// Value prefix (e.g., "$", "£")
        /// </summary>
        public string? ValuePrefix { get; set; }
    }
}
