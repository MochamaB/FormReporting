namespace FormReporting.Models.ViewModels.Dashboard.Widgets
{
    /// <summary>
    /// Data view model for Sparkline widgets
    /// Displays a small inline chart showing trends
    /// </summary>
    public class SparklineDataViewModel
    {
        /// <summary>
        /// Data values for the sparkline
        /// </summary>
        public List<decimal> Values { get; set; } = new();

        /// <summary>
        /// Color for the sparkline (hex or CSS color)
        /// </summary>
        public string Color { get; set; } = "#405189";

        /// <summary>
        /// Height of the sparkline in pixels
        /// </summary>
        public int Height { get; set; } = 35;

        /// <summary>
        /// Width of the sparkline in pixels (0 = auto/100%)
        /// </summary>
        public int Width { get; set; } = 0;

        /// <summary>
        /// Type of sparkline: "line", "bar", "area"
        /// </summary>
        public string Type { get; set; } = "line";

        /// <summary>
        /// Stroke/line width for line sparklines
        /// </summary>
        public int StrokeWidth { get; set; } = 2;

        /// <summary>
        /// Fill opacity for area sparklines (0-1)
        /// </summary>
        public decimal FillOpacity { get; set; } = 0.3m;

        /// <summary>
        /// Whether to show markers/dots on line sparklines
        /// </summary>
        public bool ShowMarkers { get; set; } = false;

        /// <summary>
        /// Whether to use smooth curves
        /// </summary>
        public bool Smooth { get; set; } = true;

        /// <summary>
        /// Labels for each data point (optional, for tooltips)
        /// </summary>
        public List<string>? Labels { get; set; }

        /// <summary>
        /// Gets the minimum value in the dataset
        /// </summary>
        public decimal MinValue => Values.Count > 0 ? Values.Min() : 0;

        /// <summary>
        /// Gets the maximum value in the dataset
        /// </summary>
        public decimal MaxValue => Values.Count > 0 ? Values.Max() : 0;

        /// <summary>
        /// Gets the latest value
        /// </summary>
        public decimal LatestValue => Values.Count > 0 ? Values.Last() : 0;

        /// <summary>
        /// Gets the trend direction based on first and last values
        /// </summary>
        public string TrendDirection
        {
            get
            {
                if (Values.Count < 2) return "neutral";
                var first = Values.First();
                var last = Values.Last();
                if (last > first) return "up";
                if (last < first) return "down";
                return "neutral";
            }
        }

        /// <summary>
        /// Gets whether there is data to display
        /// </summary>
        public bool HasData => Values.Count > 0;
    }
}
