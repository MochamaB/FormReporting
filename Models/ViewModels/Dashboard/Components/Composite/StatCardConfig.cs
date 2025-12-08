using FormReporting.Models.ViewModels.Dashboard.Components.Atomic;

namespace FormReporting.Models.ViewModels.Dashboard.Components.Composite
{
    /// <summary>
    /// Individual stat card component
    /// Displays a metric with icon, counter, and optional trend indicator
    /// </summary>
    public class StatCardConfig
    {
        /// <summary>
        /// Unique identifier for the card
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Metric label/title
        /// </summary>
        public LabelConfig Label { get; set; } = LabelConfig.MetricLabel("Metric");

        /// <summary>
        /// Counter configuration
        /// </summary>
        public CounterConfig Counter { get; set; } = new CounterConfig();

        /// <summary>
        /// Icon configuration
        /// </summary>
        public IconConfig Icon { get; set; } = new IconConfig();

        /// <summary>
        /// Trend indicator (optional)
        /// </summary>
        public TrendIndicatorConfig? Trend { get; set; }

        /// <summary>
        /// Card variant: Standard, Horizontal, Compact, IconLeft
        /// </summary>
        public StatCardVariant Variant { get; set; } = StatCardVariant.Standard;

        /// <summary>
        /// Enable card hover animation
        /// </summary>
        public bool Animate { get; set; } = true;

        /// <summary>
        /// Click action URL (makes card clickable)
        /// </summary>
        public string? LinkUrl { get; set; }

        /// <summary>
        /// Additional CSS classes
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Helper: Create standard stat card
        /// </summary>
        public static StatCardConfig Standard(
            string label,
            decimal value,
            string iconClass,
            string colorTheme = "primary",
            TrendIndicatorConfig? trend = null)
        {
            return new StatCardConfig
            {
                Label = LabelConfig.MetricLabel(label),
                Counter = CounterConfig.Number(value),
                Icon = IconConfig.Remix(iconClass, colorTheme),
                Trend = trend,
                Variant = StatCardVariant.Standard
            };
        }

        /// <summary>
        /// Helper: Create currency stat card
        /// </summary>
        public static StatCardConfig Currency(
            string label,
            decimal amount,
            string iconClass,
            string colorTheme = "success",
            TrendIndicatorConfig? trend = null)
        {
            return new StatCardConfig
            {
                Label = LabelConfig.MetricLabel(label),
                Counter = CounterConfig.Currency(amount),
                Icon = IconConfig.Remix(iconClass, colorTheme),
                Trend = trend
            };
        }

        /// <summary>
        /// Helper: Create percentage stat card
        /// </summary>
        public static StatCardConfig Percentage(
            string label,
            decimal percentage,
            string iconClass,
            string colorTheme = "info",
            TrendIndicatorConfig? trend = null)
        {
            return new StatCardConfig
            {
                Label = LabelConfig.MetricLabel(label),
                Counter = CounterConfig.Percentage(percentage),
                Icon = IconConfig.Remix(iconClass, colorTheme),
                Trend = trend
            };
        }
    }

    /// <summary>
    /// Stat card layout variants
    /// </summary>
    public enum StatCardVariant
    {
        /// <summary>Vertical layout with icon top-right</summary>
        Standard,
        /// <summary>Horizontal layout</summary>
        Horizontal,
        /// <summary>Compact minimal version</summary>
        Compact,
        /// <summary>Icon on left side</summary>
        IconLeft
    }
}
