namespace FormReporting.Models.ViewModels.Dashboard.Components.Atomic
{
    /// <summary>
    /// Configuration for animated counter atomic component
    /// Displays numbers with optional animation, formatting, and affixes
    /// </summary>
    public class CounterConfig
    {
        /// <summary>
        /// Target value for counter animation
        /// </summary>
        public decimal TargetValue { get; set; }

        /// <summary>
        /// Display value (used if animation is disabled or for formatted display)
        /// </summary>
        public string? DisplayValue { get; set; }

        /// <summary>
        /// Enable counter animation
        /// </summary>
        public bool Animate { get; set; } = true;

        /// <summary>
        /// Animation duration in milliseconds
        /// </summary>
        public int AnimationDuration { get; set; } = 2000;

        /// <summary>
        /// Number of decimal places to display
        /// </summary>
        public int DecimalPlaces { get; set; } = 0;

        /// <summary>
        /// Prefix text (e.g., "$", "Ksh ")
        /// </summary>
        public string? Prefix { get; set; }

        /// <summary>
        /// Suffix text (e.g., "k", "M", "%", " devices")
        /// </summary>
        public string? Suffix { get; set; }

        /// <summary>
        /// Use thousands separator (1,234 vs 1234)
        /// </summary>
        public bool UseThousandsSeparator { get; set; } = true;

        /// <summary>
        /// Font size class (h1, h2, h3, h4, h5, h6, or fs-1 through fs-6)
        /// </summary>
        public string FontSize { get; set; } = "h2";

        /// <summary>
        /// Font weight: normal, medium, semibold, bold
        /// </summary>
        public string FontWeight { get; set; } = "semibold";

        /// <summary>
        /// Font family: secondary (ff-secondary) or default
        /// </summary>
        public bool UseSecondaryFont { get; set; } = true;

        /// <summary>
        /// Text color class (e.g., "text-primary", "text-success")
        /// </summary>
        public string? ColorClass { get; set; }

        /// <summary>
        /// Additional CSS classes
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Helper: Create currency counter (with Ksh prefix)
        /// </summary>
        public static CounterConfig Currency(decimal amount, bool inMillions = false)
            => new CounterConfig
            {
                TargetValue = inMillions ? amount : amount,
                Prefix = "Ksh ",
                Suffix = inMillions ? "M" : null,
                DecimalPlaces = inMillions ? 2 : 0,
                UseThousandsSeparator = true
            };

        /// <summary>
        /// Helper: Create percentage counter
        /// </summary>
        public static CounterConfig Percentage(decimal percentage, int decimals = 1)
            => new CounterConfig
            {
                TargetValue = percentage,
                Suffix = "%",
                DecimalPlaces = decimals,
                UseThousandsSeparator = false
            };

        /// <summary>
        /// Helper: Create simple number counter
        /// </summary>
        public static CounterConfig Number(decimal number, string? suffix = null)
            => new CounterConfig
            {
                TargetValue = number,
                Suffix = suffix,
                UseThousandsSeparator = true
            };

        /// <summary>
        /// Helper: Create abbreviated counter (e.g., 28.05k, 1.2M)
        /// </summary>
        public static CounterConfig Abbreviated(decimal number)
        {
            if (number >= 1_000_000)
            {
                return new CounterConfig
                {
                    TargetValue = number / 1_000_000,
                    Suffix = "M",
                    DecimalPlaces = 2,
                    UseThousandsSeparator = false
                };
            }
            else if (number >= 1_000)
            {
                return new CounterConfig
                {
                    TargetValue = number / 1_000,
                    Suffix = "k",
                    DecimalPlaces = 2,
                    UseThousandsSeparator = false
                };
            }
            else
            {
                return new CounterConfig
                {
                    TargetValue = number,
                    UseThousandsSeparator = true
                };
            }
        }
    }
}
