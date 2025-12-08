namespace FormReporting.Models.ViewModels.Dashboard.Components.Composite
{
    /// <summary>
    /// TIER 2 COMPOSITE: Filter panel
    /// Panel with multiple filter inputs (search, dropdowns, date ranges, etc.)
    /// </summary>
    public class FilterPanelConfig
    {
        /// <summary>
        /// Panel identifier
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Panel title
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Filter inputs
        /// </summary>
        public List<FilterInput> Filters { get; set; } = new List<FilterInput>();

        /// <summary>
        /// Show in card wrapper
        /// </summary>
        public bool ShowCard { get; set; } = true;

        /// <summary>
        /// Show "Apply Filters" button
        /// </summary>
        public bool ShowApplyButton { get; set; } = true;

        /// <summary>
        /// Show "Clear Filters" button
        /// </summary>
        public bool ShowClearButton { get; set; } = true;

        /// <summary>
        /// Card CSS classes
        /// </summary>
        public string? CardClass { get; set; }

        /// <summary>
        /// Filter form action URL
        /// </summary>
        public string? ActionUrl { get; set; }

        /// <summary>
        /// HTTP method: GET or POST
        /// </summary>
        public string Method { get; set; } = "GET";

        /// <summary>
        /// Collapsible panel
        /// </summary>
        public bool Collapsible { get; set; } = false;

        /// <summary>
        /// Initially collapsed (if collapsible)
        /// </summary>
        public bool InitiallyCollapsed { get; set; } = false;
    }

    /// <summary>
    /// Filter input definition
    /// </summary>
    public class FilterInput
    {
        /// <summary>
        /// Input label
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Input name (form field name)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Input type: text, select, date, daterange, number, checkbox
        /// </summary>
        public FilterInputType Type { get; set; } = FilterInputType.Text;

        /// <summary>
        /// Input placeholder
        /// </summary>
        public string? Placeholder { get; set; }

        /// <summary>
        /// Current value
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Options (for select inputs)
        /// </summary>
        public List<SelectOption>? Options { get; set; }

        /// <summary>
        /// Column width (e.g., "col-md-3", "col-lg-4")
        /// </summary>
        public string ColumnClass { get; set; } = "col-md-4";

        /// <summary>
        /// Is this filter required?
        /// </summary>
        public bool Required { get; set; } = false;

        /// <summary>
        /// Helper: Create text search filter
        /// </summary>
        public static FilterInput Search(string label, string name, string? placeholder = null)
        {
            return new FilterInput
            {
                Label = label,
                Name = name,
                Type = FilterInputType.Text,
                Placeholder = placeholder ?? "Search...",
                ColumnClass = "col-md-6"
            };
        }

        /// <summary>
        /// Helper: Create dropdown/select filter
        /// </summary>
        public static FilterInput Dropdown(string label, string name, List<SelectOption> options, string? defaultValue = null)
        {
            return new FilterInput
            {
                Label = label,
                Name = name,
                Type = FilterInputType.Select,
                Options = options,
                Value = defaultValue,
                ColumnClass = "col-md-3"
            };
        }

        /// <summary>
        /// Helper: Create date range filter
        /// </summary>
        public static FilterInput DateRange(string label, string name)
        {
            return new FilterInput
            {
                Label = label,
                Name = name,
                Type = FilterInputType.DateRange,
                ColumnClass = "col-md-4"
            };
        }
    }

    /// <summary>
    /// Filter input type
    /// </summary>
    public enum FilterInputType
    {
        Text,
        Select,
        Date,
        DateRange,
        Number,
        Checkbox
    }

    /// <summary>
    /// Select option for dropdown filters
    /// </summary>
    public class SelectOption
    {
        /// <summary>
        /// Option value
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Option display text
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Is this option selected?
        /// </summary>
        public bool Selected { get; set; } = false;
    }
}
