namespace FormReporting.Models.ViewModels.Dashboard.Components.Atomic
{
    /// <summary>
    /// Configuration for trend arrow atomic component
    /// Displays directional arrow icons for trends (up/down/neutral)
    /// </summary>
    public class TrendArrowConfig
    {
        /// <summary>
        /// Trend direction: Up, Down, Neutral
        /// </summary>
        public TrendDirection Direction { get; set; } = TrendDirection.Neutral;

        /// <summary>
        /// Icon library to use
        /// </summary>
        public IconLibrary Library { get; set; } = IconLibrary.RemixIcon;

        /// <summary>
        /// Icon size class (e.g., "fs-6", "fs-5", etc.)
        /// </summary>
        public string? SizeClass { get; set; }

        /// <summary>
        /// Additional CSS classes
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Alignment class (e.g., "align-middle", "align-top")
        /// </summary>
        public string Alignment { get; set; } = "align-middle";

        /// <summary>
        /// Get the icon class based on direction and library
        /// </summary>
        public string GetIconClass()
        {
            return Library switch
            {
                IconLibrary.RemixIcon => Direction switch
                {
                    TrendDirection.Up => "ri-arrow-up-line",
                    TrendDirection.Down => "ri-arrow-down-line",
                    TrendDirection.Neutral => "ri-subtract-line",
                    _ => "ri-subtract-line"
                },
                IconLibrary.Boxicons => Direction switch
                {
                    TrendDirection.Up => "bx bx-up-arrow-alt",
                    TrendDirection.Down => "bx bx-down-arrow-alt",
                    TrendDirection.Neutral => "bx bx-minus",
                    _ => "bx bx-minus"
                },
                IconLibrary.Feather => Direction switch
                {
                    TrendDirection.Up => "arrow-up",
                    TrendDirection.Down => "arrow-down",
                    TrendDirection.Neutral => "minus",
                    _ => "minus"
                },
                _ => "ri-subtract-line"
            };
        }

        /// <summary>
        /// Helper: Create up arrow
        /// </summary>
        public static TrendArrowConfig Up(IconLibrary library = IconLibrary.RemixIcon)
            => new TrendArrowConfig
            {
                Direction = TrendDirection.Up,
                Library = library
            };

        /// <summary>
        /// Helper: Create down arrow
        /// </summary>
        public static TrendArrowConfig Down(IconLibrary library = IconLibrary.RemixIcon)
            => new TrendArrowConfig
            {
                Direction = TrendDirection.Down,
                Library = library
            };

        /// <summary>
        /// Helper: Create neutral arrow
        /// </summary>
        public static TrendArrowConfig Neutral(IconLibrary library = IconLibrary.RemixIcon)
            => new TrendArrowConfig
            {
                Direction = TrendDirection.Neutral,
                Library = library
            };
    }

    /// <summary>
    /// Trend direction enum
    /// </summary>
    public enum TrendDirection
    {
        Up,
        Down,
        Neutral
    }
}
