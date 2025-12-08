using FormReporting.Models.ViewModels.Dashboard.Components.Atomic;

namespace FormReporting.Models.ViewModels.Dashboard.Components.Composite
{
    /// <summary>
    /// Trend indicator - combines badge with trend arrow and comparison text
    /// Used in stat cards and metric displays
    /// </summary>
    public class TrendIndicatorConfig
    {
        /// <summary>
        /// Badge configuration (contains trend arrow and percentage)
        /// </summary>
        public BadgeConfig Badge { get; set; } = new BadgeConfig();

        /// <summary>
        /// Comparison text (e.g., "vs. previous month", "from last week")
        /// </summary>
        public string ComparisonText { get; set; } = "vs. previous month";

        /// <summary>
        /// Show the trend indicator
        /// </summary>
        public bool Show { get; set; } = true;

        /// <summary>
        /// Helper: Create from percentage and direction
        /// </summary>
        public static TrendIndicatorConfig FromPercentage(
            decimal percentage,
            bool isIncrease,
            bool isPositive = true,
            string comparisonText = "vs. previous month")
        {
            // Determine if this is good or bad based on context
            var isGoodTrend = isIncrease == isPositive;

            var badge = isGoodTrend
                ? BadgeConfig.Success($"{percentage:0.##}%", "ri-arrow-up-line")
                : BadgeConfig.Danger($"{percentage:0.##}%", "ri-arrow-down-line");

            // If decrease and positive context, it's good (use success colors)
            if (!isIncrease && isPositive)
            {
                badge = BadgeConfig.Success($"{percentage:0.##}%", "ri-arrow-down-line");
            }

            return new TrendIndicatorConfig
            {
                Badge = badge,
                ComparisonText = comparisonText
            };
        }

        /// <summary>
        /// Helper: Create neutral trend
        /// </summary>
        public static TrendIndicatorConfig Neutral(string text = "No change", string comparisonText = "vs. previous month")
        {
            return new TrendIndicatorConfig
            {
                Badge = new BadgeConfig
                {
                    Text = text,
                    IconClass = "ri-subtract-line",
                    ColorTheme = "secondary",
                    Variant = BadgeVariant.Subtle
                },
                ComparisonText = comparisonText
            };
        }
    }
}
