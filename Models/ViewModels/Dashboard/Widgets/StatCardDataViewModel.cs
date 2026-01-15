namespace FormReporting.Models.ViewModels.Dashboard.Widgets
{
    /// <summary>
    /// Data view model for StatCard widgets
    /// Displays a single metric value with optional trend indicator
    /// </summary>
    public class StatCardDataViewModel
    {
        /// <summary>
        /// The main value to display (e.g., "1,234", "85%", "$50,000")
        /// </summary>
        public string Value { get; set; } = "—";

        /// <summary>
        /// Label describing what the value represents
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Icon class for the stat card (e.g., "ri-file-list-3-line")
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Background color class for the icon (e.g., "primary", "success", "warning", "danger", "info")
        /// </summary>
        public string IconColor { get; set; } = "primary";

        /// <summary>
        /// Trend percentage value (e.g., 12.5 for +12.5%, -5.2 for -5.2%)
        /// </summary>
        public decimal? TrendValue { get; set; }

        /// <summary>
        /// Trend direction: "up", "down", or "neutral"
        /// </summary>
        public string? TrendDirection { get; set; }

        /// <summary>
        /// Label for the trend (e.g., "vs last month", "since yesterday")
        /// </summary>
        public string? TrendLabel { get; set; }

        /// <summary>
        /// Whether an upward trend is positive (green) or negative (red)
        /// Default true: up = good (green), down = bad (red)
        /// Set to false for metrics like "errors" where down is good
        /// </summary>
        public bool UpIsGood { get; set; } = true;

        /// <summary>
        /// Optional secondary value (e.g., "of 1,500 total")
        /// </summary>
        public string? SecondaryValue { get; set; }

        /// <summary>
        /// Optional footer text
        /// </summary>
        public string? FooterText { get; set; }

        /// <summary>
        /// Gets the CSS class for trend color based on direction and UpIsGood setting
        /// </summary>
        public string TrendColorClass
        {
            get
            {
                if (string.IsNullOrEmpty(TrendDirection) || TrendDirection == "neutral")
                    return "text-muted";

                var isUp = TrendDirection == "up";
                var isPositive = UpIsGood ? isUp : !isUp;

                return isPositive ? "text-success" : "text-danger";
            }
        }

        /// <summary>
        /// Gets the trend icon class based on direction
        /// </summary>
        public string TrendIconClass
        {
            get
            {
                return TrendDirection switch
                {
                    "up" => "ri-arrow-up-line",
                    "down" => "ri-arrow-down-line",
                    _ => "ri-subtract-line"
                };
            }
        }
    }
}
