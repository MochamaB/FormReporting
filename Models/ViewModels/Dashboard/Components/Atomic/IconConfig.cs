namespace FormReporting.Models.ViewModels.Dashboard.Components.Atomic
{
    /// <summary>
    /// Configuration for icon atomic component with avatar circle background
    /// Used in stat cards, headers, and visual indicators
    /// </summary>
    public class IconConfig
    {
        /// <summary>
        /// Icon class - e.g., "ri-user-line", "bx bx-dollar-circle", "users" (for feather)
        /// </summary>
        public string IconClass { get; set; } = string.Empty;

        /// <summary>
        /// Icon library: RemixIcon, Boxicons, Feather
        /// </summary>
        public IconLibrary Library { get; set; } = IconLibrary.RemixIcon;

        /// <summary>
        /// Color theme: primary, success, danger, warning, info, secondary, dark
        /// </summary>
        public string ColorTheme { get; set; } = "primary";

        /// <summary>
        /// Avatar size: xs, sm, md, lg, xl
        /// </summary>
        public string Size { get; set; } = "sm";

        /// <summary>
        /// Icon font size (fs-2, fs-3, fs-4, etc.)
        /// </summary>
        public string FontSize { get; set; } = "fs-2";

        /// <summary>
        /// Show icon (allows conditional rendering)
        /// </summary>
        public bool Show { get; set; } = true;

        /// <summary>
        /// Use subtle background (bg-{color}-subtle) instead of solid
        /// </summary>
        public bool Subtle { get; set; } = true;

        /// <summary>
        /// Shape: Circle or Square
        /// </summary>
        public IconShape Shape { get; set; } = IconShape.Circle;

        /// <summary>
        /// Additional CSS classes for the avatar container
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Helper: Create icon with RemixIcon library
        /// </summary>
        public static IconConfig Remix(string iconClass, string colorTheme = "primary", string size = "sm")
            => new IconConfig
            {
                IconClass = iconClass,
                Library = IconLibrary.RemixIcon,
                ColorTheme = colorTheme,
                Size = size
            };

        /// <summary>
        /// Helper: Create icon with Feather library
        /// </summary>
        public static IconConfig Feather(string iconName, string colorTheme = "primary", string size = "sm")
            => new IconConfig
            {
                IconClass = iconName,
                Library = IconLibrary.Feather,
                ColorTheme = colorTheme,
                Size = size
            };

        /// <summary>
        /// Helper: Create icon with Boxicons library
        /// </summary>
        public static IconConfig Boxicon(string iconClass, string colorTheme = "primary", string size = "sm")
            => new IconConfig
            {
                IconClass = iconClass,
                Library = IconLibrary.Boxicons,
                ColorTheme = colorTheme,
                Size = size
            };
    }

    /// <summary>
    /// Supported icon libraries
    /// </summary>
    public enum IconLibrary
    {
        RemixIcon,
        Boxicons,
        Feather
    }

    /// <summary>
    /// Icon avatar shape
    /// </summary>
    public enum IconShape
    {
        Circle,
        Square
    }
}
