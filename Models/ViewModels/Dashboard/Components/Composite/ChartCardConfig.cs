using FormReporting.Models.ViewModels.Dashboard.Components.Atomic;

namespace FormReporting.Models.ViewModels.Dashboard.Components.Composite
{
    /// <summary>
    /// TIER 2 COMPOSITE: Chart card
    /// Chart with header, footer, and controls (filters, export, etc.)
    /// </summary>
    public class ChartCardConfig
    {
        /// <summary>
        /// Unique chart identifier
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Chart title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Chart subtitle (optional)
        /// </summary>
        public string? Subtitle { get; set; }

        /// <summary>
        /// Chart type: area, line, bar, column, donut, pie, radialBar, etc.
        /// </summary>
        public string ChartType { get; set; } = "area";

        /// <summary>
        /// Chart height in pixels
        /// </summary>
        public int Height { get; set; } = 350;

        /// <summary>
        /// Series data (JSON serialized for ApexCharts)
        /// </summary>
        public string SeriesJson { get; set; } = "[]";

        /// <summary>
        /// Categories/Labels (JSON serialized)
        /// </summary>
        public string CategoriesJson { get; set; } = "[]";

        /// <summary>
        /// Full ApexCharts options (JSON serialized)
        /// </summary>
        public string? OptionsJson { get; set; }

        /// <summary>
        /// Color scheme array
        /// </summary>
        public string[] Colors { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Show header with title/controls
        /// </summary>
        public bool ShowHeader { get; set; } = true;

        /// <summary>
        /// Show footer
        /// </summary>
        public bool ShowFooter { get; set; } = false;

        /// <summary>
        /// Header dropdown filter (optional)
        /// </summary>
        public DropdownConfig? HeaderFilter { get; set; }

        /// <summary>
        /// Footer content (HTML or text)
        /// </summary>
        public string? FooterContent { get; set; }

        /// <summary>
        /// Card CSS classes
        /// </summary>
        public string? CardClass { get; set; }

        /// <summary>
        /// Show ApexCharts toolbar
        /// </summary>
        public bool ShowToolbar { get; set; } = false;

        /// <summary>
        /// Helper: Create area chart
        /// </summary>
        public static ChartCardConfig AreaChart(string title, string seriesJson, string categoriesJson, int height = 350)
        {
            return new ChartCardConfig
            {
                Title = title,
                ChartType = "area",
                SeriesJson = seriesJson,
                CategoriesJson = categoriesJson,
                Height = height,
                Colors = new[] { "#405189", "#0ab39c", "#f06548", "#f7b84b" }
            };
        }

        /// <summary>
        /// Helper: Create bar chart
        /// </summary>
        public static ChartCardConfig BarChart(string title, string seriesJson, string categoriesJson, int height = 350)
        {
            return new ChartCardConfig
            {
                Title = title,
                ChartType = "bar",
                SeriesJson = seriesJson,
                CategoriesJson = categoriesJson,
                Height = height,
                Colors = new[] { "#405189", "#0ab39c" }
            };
        }

        /// <summary>
        /// Helper: Create donut chart
        /// </summary>
        public static ChartCardConfig DonutChart(string title, string seriesJson, string[] labels, int height = 300)
        {
            return new ChartCardConfig
            {
                Title = title,
                ChartType = "donut",
                SeriesJson = seriesJson,
                CategoriesJson = System.Text.Json.JsonSerializer.Serialize(labels),
                Height = height,
                Colors = new[] { "#405189", "#0ab39c", "#f06548", "#f7b84b", "#299cdb" }
            };
        }
    }
}
