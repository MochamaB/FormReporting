namespace FormReporting.Models.ViewModels.Dashboard.Components.Atomic
{
    /// <summary>
    /// Configuration for label/text atomic component
    /// Used for titles, captions, descriptions throughout dashboard components
    /// </summary>
    public class LabelConfig
    {
        /// <summary>
        /// Label text content
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// HTML tag: p, span, h1, h2, h3, h4, h5, h6, div
        /// </summary>
        public string Tag { get; set; } = "p";

        /// <summary>
        /// Size class: fs-1 through fs-6, or h1 through h6
        /// </summary>
        public string? Size { get; set; }

        /// <summary>
        /// Font weight: normal, medium, semibold, bold
        /// </summary>
        public string? Weight { get; set; }

        /// <summary>
        /// Color class: text-muted, text-primary, text-success, etc.
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Margin bottom class: mb-0, mb-1, mb-2, mb-3, mb-4, mb-5
        /// </summary>
        public string MarginBottom { get; set; } = "mb-0";

        /// <summary>
        /// Margin top class: mt-0, mt-1, mt-2, mt-3, mt-4, mt-5
        /// </summary>
        public string? MarginTop { get; set; }

        /// <summary>
        /// Text alignment: start, center, end
        /// </summary>
        public string? TextAlign { get; set; }

        /// <summary>
        /// Text transform: uppercase, lowercase, capitalize
        /// </summary>
        public string? TextTransform { get; set; }

        /// <summary>
        /// Line height class
        /// </summary>
        public string? LineHeight { get; set; }

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

            if (!string.IsNullOrEmpty(Size)) classes.Add(Size);

            if (!string.IsNullOrEmpty(Weight))
            {
                var weightClass = Weight switch
                {
                    "normal" => "fw-normal",
                    "medium" => "fw-medium",
                    "semibold" => "fw-semibold",
                    "bold" => "fw-bold",
                    _ => Weight.StartsWith("fw-") ? Weight : $"fw-{Weight}"
                };
                classes.Add(weightClass);
            }

            if (!string.IsNullOrEmpty(Color))
            {
                classes.Add(Color.StartsWith("text-") ? Color : $"text-{Color}");
            }

            if (!string.IsNullOrEmpty(MarginBottom)) classes.Add(MarginBottom);
            if (!string.IsNullOrEmpty(MarginTop)) classes.Add(MarginTop);

            if (!string.IsNullOrEmpty(TextAlign))
            {
                classes.Add($"text-{TextAlign}");
            }

            if (!string.IsNullOrEmpty(TextTransform))
            {
                classes.Add($"text-{TextTransform}");
            }

            if (!string.IsNullOrEmpty(LineHeight)) classes.Add(LineHeight);
            if (!string.IsNullOrEmpty(CssClass)) classes.Add(CssClass);

            return string.Join(" ", classes);
        }

        /// <summary>
        /// Helper: Create card title
        /// </summary>
        public static LabelConfig CardTitle(string text)
            => new LabelConfig
            {
                Text = text,
                Tag = "h4",
                CssClass = "card-title mb-0 flex-grow-1"
            };

        /// <summary>
        /// Helper: Create muted caption/subtitle
        /// </summary>
        public static LabelConfig Caption(string text)
            => new LabelConfig
            {
                Text = text,
                Tag = "p",
                Weight = "medium",
                Color = "text-muted",
                MarginBottom = "mb-0"
            };

        /// <summary>
        /// Helper: Create section heading
        /// </summary>
        public static LabelConfig Heading(string text, int level = 2)
            => new LabelConfig
            {
                Text = text,
                Tag = $"h{level}",
                Weight = "semibold"
            };

        /// <summary>
        /// Helper: Create small uppercase label
        /// </summary>
        public static LabelConfig SmallUppercase(string text)
            => new LabelConfig
            {
                Text = text,
                Tag = "span",
                Weight = "semibold",
                TextTransform = "uppercase",
                Size = "fs-12",
                Color = "text-muted"
            };

        /// <summary>
        /// Helper: Create metric label (for stat cards)
        /// </summary>
        public static LabelConfig MetricLabel(string text)
            => new LabelConfig
            {
                Text = text,
                Tag = "p",
                Weight = "medium",
                Color = "text-muted",
                MarginBottom = "mb-0"
            };
    }
}
