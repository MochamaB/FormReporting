namespace FormReporting.Models.ViewModels.Dashboard.Components.Atomic
{
    /// <summary>
    /// Configuration for divider/separator atomic component
    /// Creates horizontal lines to separate sections
    /// </summary>
    public class DividerConfig
    {
        /// <summary>
        /// Border style: solid, dashed, dotted
        /// </summary>
        public string Style { get; set; } = "solid";

        /// <summary>
        /// Border position: top, bottom, start, end
        /// </summary>
        public string Position { get; set; } = "top";

        /// <summary>
        /// Color class (e.g., "border-primary", "border-muted")
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Margin top class: my-0 through my-5
        /// </summary>
        public string? MarginY { get; set; }

        /// <summary>
        /// Margin top class: mt-0 through mt-5
        /// </summary>
        public string? MarginTop { get; set; }

        /// <summary>
        /// Margin bottom class: mb-0 through mb-5
        /// </summary>
        public string? MarginBottom { get; set; }

        /// <summary>
        /// Opacity level: 10, 25, 50, 75, 100
        /// </summary>
        public int? Opacity { get; set; }

        /// <summary>
        /// Additional CSS classes
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Build complete CSS class string
        /// </summary>
        public string GetClasses()
        {
            var classes = new List<string>();

            // Border position and style
            var positionClass = Position switch
            {
                "top" => "border-top",
                "bottom" => "border-bottom",
                "start" => "border-start",
                "end" => "border-end",
                _ => "border-top"
            };
            classes.Add(positionClass);

            // Border style
            if (Style == "dashed")
            {
                classes.Add($"{positionClass}-dashed");
            }
            else if (Style == "dotted")
            {
                classes.Add($"{positionClass}-dotted");
            }

            // Color
            if (!string.IsNullOrEmpty(Color))
            {
                classes.Add(Color.StartsWith("border-") ? Color : $"border-{Color}");
            }

            // Margins
            if (!string.IsNullOrEmpty(MarginY)) classes.Add(MarginY);
            if (!string.IsNullOrEmpty(MarginTop)) classes.Add(MarginTop);
            if (!string.IsNullOrEmpty(MarginBottom)) classes.Add(MarginBottom);

            // Opacity
            if (Opacity.HasValue)
            {
                classes.Add($"opacity-{Opacity.Value}");
            }

            if (!string.IsNullOrEmpty(CssClass)) classes.Add(CssClass);

            return string.Join(" ", classes);
        }

        /// <summary>
        /// Helper: Create simple top divider
        /// </summary>
        public static DividerConfig Top(string? marginY = "my-3")
            => new DividerConfig
            {
                Position = "top",
                Style = "solid",
                MarginY = marginY
            };

        /// <summary>
        /// Helper: Create dashed divider
        /// </summary>
        public static DividerConfig Dashed(string? marginY = "my-3")
            => new DividerConfig
            {
                Position = "top",
                Style = "dashed",
                MarginY = marginY
            };

        /// <summary>
        /// Helper: Create subtle divider with opacity
        /// </summary>
        public static DividerConfig Subtle(string? marginY = "my-3")
            => new DividerConfig
            {
                Position = "top",
                Style = "solid",
                MarginY = marginY,
                Opacity = 25
            };

        /// <summary>
        /// Helper: Create card footer divider
        /// </summary>
        public static DividerConfig CardFooter()
            => new DividerConfig
            {
                Position = "top",
                Style = "dashed",
                MarginTop = "mt-3",
                MarginBottom = "mb-0"
            };
    }
}
