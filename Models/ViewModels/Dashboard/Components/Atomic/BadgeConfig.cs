namespace FormReporting.Models.ViewModels.Dashboard.Components.Atomic
{
    /// <summary>
    /// Configuration for badge atomic component
    /// Used for status indicators, trend labels, metrics, etc.
    /// </summary>
    public class BadgeConfig
    {
        /// <summary>
        /// Badge text content
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Icon class (optional) - e.g., "ri-arrow-up-line", "bx bx-check"
        /// </summary>
        public string? IconClass { get; set; }

        /// <summary>
        /// Icon position: Left or Right
        /// </summary>
        public IconPosition IconPosition { get; set; } = IconPosition.Left;

        /// <summary>
        /// Color theme: primary, success, danger, warning, info, secondary, dark, light
        /// </summary>
        public string ColorTheme { get; set; } = "primary";

        /// <summary>
        /// Badge variant: Solid, Subtle, Outline
        /// </summary>
        public BadgeVariant Variant { get; set; } = BadgeVariant.Solid;

        /// <summary>
        /// Badge size: sm, md (default), lg
        /// </summary>
        public string Size { get; set; } = "md";

        /// <summary>
        /// Pill shape (rounded)
        /// </summary>
        public bool Pill { get; set; } = false;

        /// <summary>
        /// Additional CSS classes
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Helper: Create success badge with up arrow
        /// </summary>
        public static BadgeConfig Success(string text, string? icon = "ri-arrow-up-line")
            => new BadgeConfig
            {
                Text = text,
                IconClass = icon,
                ColorTheme = "success",
                Variant = BadgeVariant.Subtle
            };

        /// <summary>
        /// Helper: Create danger badge with down arrow
        /// </summary>
        public static BadgeConfig Danger(string text, string? icon = "ri-arrow-down-line")
            => new BadgeConfig
            {
                Text = text,
                IconClass = icon,
                ColorTheme = "danger",
                Variant = BadgeVariant.Subtle
            };

        /// <summary>
        /// Helper: Create warning badge
        /// </summary>
        public static BadgeConfig Warning(string text, string? icon = null)
            => new BadgeConfig
            {
                Text = text,
                IconClass = icon,
                ColorTheme = "warning",
                Variant = BadgeVariant.Subtle
            };

        /// <summary>
        /// Helper: Create info badge
        /// </summary>
        public static BadgeConfig Info(string text, string? icon = null)
            => new BadgeConfig
            {
                Text = text,
                IconClass = icon,
                ColorTheme = "info",
                Variant = BadgeVariant.Subtle
            };

        /// <summary>
        /// Helper: Create status badge (solid, pill)
        /// </summary>
        public static BadgeConfig Status(string text, string colorTheme = "success")
            => new BadgeConfig
            {
                Text = text,
                ColorTheme = colorTheme,
                Variant = BadgeVariant.Solid,
                Pill = true,
                Size = "sm"
            };
    }

    /// <summary>
    /// Badge visual variant
    /// </summary>
    public enum BadgeVariant
    {
        /// <summary>Solid background with white text (bg-primary text-white)</summary>
        Solid,
        /// <summary>Subtle background with colored text (bg-primary-subtle text-primary)</summary>
        Subtle,
        /// <summary>Outline border with colored text (border-primary text-primary)</summary>
        Outline
    }

    /// <summary>
    /// Icon position within badge
    /// </summary>
    public enum IconPosition
    {
        Left,
        Right
    }
}
