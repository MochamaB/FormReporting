namespace FormReporting.Models.ViewModels.Dashboard.Widgets
{
    /// <summary>
    /// Data view model for Gauge and ProgressBar widgets
    /// Displays a value within a min/max range with optional thresholds
    /// </summary>
    public class GaugeDataViewModel
    {
        /// <summary>
        /// Current value to display
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Minimum value for the gauge (default 0)
        /// </summary>
        public decimal MinValue { get; set; } = 0;

        /// <summary>
        /// Maximum value for the gauge (default 100)
        /// </summary>
        public decimal MaxValue { get; set; } = 100;

        /// <summary>
        /// Display label for the value (e.g., "Score", "Completion")
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Unit suffix for the value (e.g., "%", "pts")
        /// </summary>
        public string? Unit { get; set; }

        /// <summary>
        /// Threshold configuration for color coding
        /// </summary>
        public GaugeThresholdsViewModel Thresholds { get; set; } = new();

        /// <summary>
        /// Height of the gauge/progress bar in pixels
        /// </summary>
        public int Height { get; set; } = 200;

        /// <summary>
        /// Whether to show the value label
        /// </summary>
        public bool ShowValue { get; set; } = true;

        /// <summary>
        /// Whether to animate the gauge
        /// </summary>
        public bool Animate { get; set; } = true;

        /// <summary>
        /// Gets the percentage value (0-100)
        /// </summary>
        public decimal Percentage
        {
            get
            {
                if (MaxValue == MinValue) return 0;
                var pct = (Value - MinValue) / (MaxValue - MinValue) * 100;
                return Math.Max(0, Math.Min(100, pct));
            }
        }

        /// <summary>
        /// Gets the color class based on current value and thresholds
        /// </summary>
        public string ColorClass
        {
            get
            {
                if (Value >= Thresholds.GreenMin)
                    return "success";
                if (Value >= Thresholds.YellowMin)
                    return "warning";
                return "danger";
            }
        }

        /// <summary>
        /// Gets the formatted display value
        /// </summary>
        public string DisplayValue => $"{Value:N0}{Unit}";
    }

    /// <summary>
    /// Threshold configuration for gauge color coding
    /// </summary>
    public class GaugeThresholdsViewModel
    {
        /// <summary>
        /// Minimum value for green/success zone (default 70)
        /// </summary>
        public decimal GreenMin { get; set; } = 70;

        /// <summary>
        /// Minimum value for yellow/warning zone (default 40)
        /// Values below this are red/danger
        /// </summary>
        public decimal YellowMin { get; set; } = 40;

        /// <summary>
        /// Whether higher values are better (true) or lower values are better (false)
        /// Affects color assignment
        /// </summary>
        public bool HigherIsBetter { get; set; } = true;

        /// <summary>
        /// Creates default thresholds for percentage-based gauges
        /// </summary>
        public static GaugeThresholdsViewModel DefaultPercentage() => new()
        {
            GreenMin = 70,
            YellowMin = 40,
            HigherIsBetter = true
        };

        /// <summary>
        /// Creates thresholds where lower is better (e.g., error rates)
        /// </summary>
        public static GaugeThresholdsViewModel LowerIsBetter() => new()
        {
            GreenMin = 0,
            YellowMin = 30,
            HigherIsBetter = false
        };
    }
}
