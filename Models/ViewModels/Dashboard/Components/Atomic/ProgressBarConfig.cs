namespace FormReporting.Models.ViewModels.Dashboard.Components.Atomic
{
    /// <summary>
    /// Configuration for progress bar atomic component
    /// Displays progress indicators with optional labels and animations
    /// </summary>
    public class ProgressBarConfig
    {
        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public decimal Percentage { get; set; }

        /// <summary>
        /// Color theme: primary, success, danger, warning, info, secondary
        /// </summary>
        public string ColorTheme { get; set; } = "primary";

        /// <summary>
        /// Height size: xs, sm, md, lg
        /// </summary>
        public string Height { get; set; } = "md";

        /// <summary>
        /// Show striped pattern
        /// </summary>
        public bool Striped { get; set; } = false;

        /// <summary>
        /// Animate striped pattern
        /// </summary>
        public bool Animated { get; set; } = false;

        /// <summary>
        /// Show label inside progress bar
        /// </summary>
        public bool ShowLabel { get; set; } = false;

        /// <summary>
        /// Custom label text (if null, shows percentage)
        /// </summary>
        public string? LabelText { get; set; }

        /// <summary>
        /// Progress bar style: Default, Gradient, Custom
        /// </summary>
        public ProgressBarStyle Style { get; set; } = ProgressBarStyle.Default;

        /// <summary>
        /// Soft/Subtle color variant
        /// </summary>
        public bool Soft { get; set; } = false;

        /// <summary>
        /// Additional CSS classes for the progress container
        /// </summary>
        public string? ContainerClass { get; set; }

        /// <summary>
        /// Additional CSS classes for the progress bar
        /// </summary>
        public string? BarClass { get; set; }

        /// <summary>
        /// Get height class for Bootstrap progress bar
        /// </summary>
        public string GetHeightClass()
        {
            return Height switch
            {
                "xs" => "progress-xs",
                "sm" => "progress-sm",
                "lg" => "progress-lg",
                _ => "" // md is default
            };
        }

        /// <summary>
        /// Helper: Create simple progress bar
        /// </summary>
        public static ProgressBarConfig Simple(decimal percentage, string colorTheme = "primary")
            => new ProgressBarConfig
            {
                Percentage = percentage,
                ColorTheme = colorTheme
            };

        /// <summary>
        /// Helper: Create striped animated progress bar
        /// </summary>
        public static ProgressBarConfig StripedAnimated(decimal percentage, string colorTheme = "primary")
            => new ProgressBarConfig
            {
                Percentage = percentage,
                ColorTheme = colorTheme,
                Striped = true,
                Animated = true
            };

        /// <summary>
        /// Helper: Create progress bar with label
        /// </summary>
        public static ProgressBarConfig WithLabel(decimal percentage, string colorTheme = "primary", string? labelText = null)
            => new ProgressBarConfig
            {
                Percentage = percentage,
                ColorTheme = colorTheme,
                ShowLabel = true,
                LabelText = labelText
            };

        /// <summary>
        /// Helper: Create soft/subtle progress bar
        /// </summary>
        public static ProgressBarConfig SoftColors(decimal percentage, string colorTheme = "primary")
            => new ProgressBarConfig
            {
                Percentage = percentage,
                ColorTheme = colorTheme,
                Soft = true
            };
    }

    /// <summary>
    /// Progress bar visual style
    /// </summary>
    public enum ProgressBarStyle
    {
        Default,
        Gradient,
        Custom
    }
}
