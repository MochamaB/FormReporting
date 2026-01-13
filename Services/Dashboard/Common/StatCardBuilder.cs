using FormReporting.Models.ViewModels.Dashboard.Components.Composite;
using FormReporting.Models.ViewModels.Dashboard.Components.Atomic;

namespace FormReporting.Services.Dashboard.Common
{
    /// <summary>
    /// Builder service for creating stat card configurations
    /// Handles formatting, trend calculations, and color logic
    /// </summary>
    public class StatCardBuilder
    {
        public StatCardConfig BuildStatCard(
            string title, 
            string value, 
            string icon, 
            string iconColor, 
            string trendValue = "", 
            string trendDirection = "neutral", 
            string subText = "", 
            string linkUrl = "", 
            string linkText = "")
        {
            // Parse value to determine if it's numeric or text
            CounterConfig counter;
            if (decimal.TryParse(value.Replace(",", "").Replace("%", "").Replace(" hrs", "").Trim(), out decimal numericValue))
            {
                // Numeric value - check if it has special formatting
                if (value.Contains("%"))
                {
                    counter = new CounterConfig
                    {
                        TargetValue = numericValue,
                        Suffix = "%",
                        DecimalPlaces = 1,
                        Animate = false, // Disable animation until CountUp.js is added
                        UseThousandsSeparator = false
                    };
                }
                else if (value.Contains("hrs"))
                {
                    counter = new CounterConfig
                    {
                        TargetValue = numericValue,
                        Suffix = " hrs",
                        DecimalPlaces = 1,
                        Animate = false, // Disable animation until CountUp.js is added
                        UseThousandsSeparator = false
                    };
                }
                else
                {
                    counter = new CounterConfig
                    {
                        TargetValue = numericValue,
                        Animate = false, // Disable animation until CountUp.js is added
                        UseThousandsSeparator = true,
                        DecimalPlaces = 0
                    };
                }
            }
            else
            {
                // Non-numeric value (like "N/A") - use DisplayValue
                counter = new CounterConfig
                {
                    TargetValue = 0,
                    DisplayValue = value,
                    Animate = false
                };
            }

            var statCard = new StatCardConfig
            {
                Label = LabelConfig.MetricLabel(title),
                Counter = counter,
                Icon = IconConfig.Remix(icon, iconColor),
                LinkUrl = string.IsNullOrEmpty(linkUrl) ? null : linkUrl
            };

            // Add trend if provided
            if (!string.IsNullOrEmpty(trendValue) && trendDirection != "neutral")
            {
                // Parse trend percentage from trendValue (e.g., "+12.5%" or "-5.3%")
                var percentageText = trendValue.Replace("+", "").Replace("%", "").Trim();
                if (decimal.TryParse(percentageText, out decimal percentage))
                {
                    var isIncrease = trendDirection == "up";
                    statCard.Trend = TrendIndicatorConfig.FromPercentage(
                        Math.Abs(percentage), 
                        isIncrease, 
                        isPositive: true,
                        comparisonText: string.IsNullOrEmpty(subText) ? "vs. previous month" : subText
                    );
                }
            }

            return statCard;
        }
        
        /// <summary>
        /// Determines trend direction based on current vs previous value
        /// </summary>
        public string DetermineTrendDirection(decimal currentValue, decimal previousValue)
        {
            if (currentValue > previousValue) return "up";
            if (currentValue < previousValue) return "down";
            return "neutral";
        }
        
        /// <summary>
        /// Calculates percentage change between current and previous value
        /// </summary>
        public string CalculateTrendPercentage(decimal currentValue, decimal previousValue)
        {
            if (previousValue == 0) return "N/A";
            
            var change = ((currentValue - previousValue) / previousValue) * 100;
            var sign = change >= 0 ? "+" : "";
            
            return $"{sign}{change:F1}%";
        }
        
        /// <summary>
        /// Determines icon color based on threshold and direction
        /// </summary>
        public string DetermineIconColor(decimal value, decimal goodThreshold, decimal warningThreshold, bool higherIsBetter = true)
        {
            if (higherIsBetter)
            {
                if (value >= goodThreshold) return "success";
                if (value >= warningThreshold) return "warning";
                return "danger";
            }
            else
            {
                if (value <= goodThreshold) return "success";
                if (value <= warningThreshold) return "warning";
                return "danger";
            }
        }
    }
}
