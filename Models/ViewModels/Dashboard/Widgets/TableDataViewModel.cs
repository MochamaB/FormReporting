namespace FormReporting.Models.ViewModels.Dashboard.Widgets
{
    /// <summary>
    /// Data view model for DataTable widgets
    /// Displays tabular data with optional pagination
    /// </summary>
    public class TableDataViewModel
    {
        /// <summary>
        /// Column definitions for the table
        /// </summary>
        public List<TableColumnViewModel> Columns { get; set; } = new();

        /// <summary>
        /// Row data for the table
        /// </summary>
        public List<TableRowViewModel> Rows { get; set; } = new();

        /// <summary>
        /// Total number of records (for pagination)
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Number of records per page
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// Whether to show pagination controls
        /// </summary>
        public bool ShowPagination { get; set; } = false;

        /// <summary>
        /// Whether to show a search box
        /// </summary>
        public bool ShowSearch { get; set; } = false;

        /// <summary>
        /// Whether the table is sortable
        /// </summary>
        public bool Sortable { get; set; } = false;

        /// <summary>
        /// Whether to use striped rows
        /// </summary>
        public bool Striped { get; set; } = true;

        /// <summary>
        /// Whether to use hover effect on rows
        /// </summary>
        public bool Hover { get; set; } = true;

        /// <summary>
        /// Whether to use compact/small table style
        /// </summary>
        public bool Compact { get; set; } = false;

        /// <summary>
        /// Optional footer row for totals/summaries
        /// </summary>
        public TableRowViewModel? FooterRow { get; set; }

        /// <summary>
        /// Empty state message when no rows
        /// </summary>
        public string EmptyMessage { get; set; } = "No records found";

        /// <summary>
        /// Gets the total number of pages
        /// </summary>
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 1;

        /// <summary>
        /// Gets whether there are any rows
        /// </summary>
        public bool HasRows => Rows.Count > 0;
    }

    /// <summary>
    /// Table column definition
    /// </summary>
    public class TableColumnViewModel
    {
        /// <summary>
        /// Column identifier/key (maps to row cell keys)
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Column header text
        /// </summary>
        public string Header { get; set; } = string.Empty;

        /// <summary>
        /// Column width (CSS value, e.g., "100px", "20%", "auto")
        /// </summary>
        public string? Width { get; set; }

        /// <summary>
        /// Text alignment: "left", "center", "right"
        /// </summary>
        public string Align { get; set; } = "left";

        /// <summary>
        /// Whether the column is sortable
        /// </summary>
        public bool Sortable { get; set; } = false;

        /// <summary>
        /// CSS class for the column
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Whether to hide this column on mobile
        /// </summary>
        public bool HideOnMobile { get; set; } = false;

        /// <summary>
        /// Data type for formatting: "text", "number", "date", "currency", "badge"
        /// </summary>
        public string DataType { get; set; } = "text";
    }

    /// <summary>
    /// Table row data
    /// </summary>
    public class TableRowViewModel
    {
        /// <summary>
        /// Unique identifier for the row
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Cell values keyed by column key
        /// </summary>
        public Dictionary<string, TableCellViewModel> Cells { get; set; } = new();

        /// <summary>
        /// CSS class for the row
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Optional click URL for the row
        /// </summary>
        public string? ClickUrl { get; set; }

        /// <summary>
        /// Whether the row is clickable
        /// </summary>
        public bool IsClickable => !string.IsNullOrEmpty(ClickUrl);
    }

    /// <summary>
    /// Table cell data
    /// </summary>
    public class TableCellViewModel
    {
        /// <summary>
        /// Display value for the cell
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Raw value for sorting/filtering
        /// </summary>
        public object? RawValue { get; set; }

        /// <summary>
        /// Badge color for badge-type cells (e.g., "success", "warning", "danger")
        /// </summary>
        public string? BadgeColor { get; set; }

        /// <summary>
        /// Icon class to show before the value
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Icon color class
        /// </summary>
        public string? IconColor { get; set; }

        /// <summary>
        /// Tooltip text
        /// </summary>
        public string? Tooltip { get; set; }

        /// <summary>
        /// CSS class for the cell
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Creates a simple text cell
        /// </summary>
        public static TableCellViewModel Text(string value) => new() { Value = value, RawValue = value };

        /// <summary>
        /// Creates a number cell
        /// </summary>
        public static TableCellViewModel Number(decimal value, string? format = null) =>
            new() { Value = format != null ? value.ToString(format) : value.ToString("N0"), RawValue = value };

        /// <summary>
        /// Creates a badge cell
        /// </summary>
        public static TableCellViewModel Badge(string value, string color) =>
            new() { Value = value, BadgeColor = color, RawValue = value };

        /// <summary>
        /// Creates a date cell
        /// </summary>
        public static TableCellViewModel Date(DateTime? date, string format = "MMM dd, yyyy") =>
            new() { Value = date?.ToString(format) ?? "—", RawValue = date };
    }
}
