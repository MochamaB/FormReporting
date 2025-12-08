using FormReporting.Models.ViewModels.Dashboard.Components.Atomic;

namespace FormReporting.Models.ViewModels.Dashboard.Components.Composite
{
    /// <summary>
    /// TIER 2 COMPOSITE: Data table
    /// Table with pagination, search, and sorting capabilities
    /// </summary>
    public class DataTableConfig
    {
        /// <summary>
        /// Table identifier
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Table title
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Column definitions
        /// </summary>
        public List<TableColumn> Columns { get; set; } = new List<TableColumn>();

        /// <summary>
        /// Table rows (list of objects - will be rendered dynamically)
        /// </summary>
        public List<Dictionary<string, object>> Rows { get; set; } = new List<Dictionary<string, object>>();

        /// <summary>
        /// Show search box
        /// </summary>
        public bool ShowSearch { get; set; } = true;

        /// <summary>
        /// Show pagination
        /// </summary>
        public bool ShowPagination { get; set; } = true;

        /// <summary>
        /// Items per page
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Total items count (for pagination)
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Current page (1-based)
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// Table responsive class
        /// </summary>
        public string ResponsiveClass { get; set; } = "table-responsive";

        /// <summary>
        /// Table CSS classes (e.g., "table-striped table-hover")
        /// </summary>
        public string TableClass { get; set; } = "table table-nowrap align-middle";

        /// <summary>
        /// Card wrapper CSS classes
        /// </summary>
        public string? CardClass { get; set; }

        /// <summary>
        /// Show table in card wrapper
        /// </summary>
        public bool ShowCard { get; set; } = true;

        /// <summary>
        /// Actions column (optional - buttons/links for each row)
        /// </summary>
        public List<TableAction>? Actions { get; set; }
    }

    /// <summary>
    /// Table column definition
    /// </summary>
    public class TableColumn
    {
        /// <summary>
        /// Column header text
        /// </summary>
        public string Header { get; set; } = string.Empty;

        /// <summary>
        /// Property/field name in data object
        /// </summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Column width (optional)
        /// </summary>
        public string? Width { get; set; }

        /// <summary>
        /// Is this column sortable?
        /// </summary>
        public bool Sortable { get; set; } = false;

        /// <summary>
        /// Column alignment: start, center, end
        /// </summary>
        public string Alignment { get; set; } = "start";

        /// <summary>
        /// Render as badge (for status columns)
        /// </summary>
        public bool RenderAsBadge { get; set; } = false;

        /// <summary>
        /// Badge color mapping (value => color)
        /// </summary>
        public Dictionary<string, string>? BadgeColorMap { get; set; }

        /// <summary>
        /// Custom render function name (JavaScript function)
        /// </summary>
        public string? CustomRenderFunction { get; set; }
    }

    /// <summary>
    /// Table row action definition
    /// </summary>
    public class TableAction
    {
        /// <summary>
        /// Action label/text
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Icon class
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Action URL pattern (use {id} placeholder)
        /// </summary>
        public string? UrlPattern { get; set; }

        /// <summary>
        /// JavaScript onclick handler
        /// </summary>
        public string? OnClick { get; set; }

        /// <summary>
        /// Button CSS class
        /// </summary>
        public string ButtonClass { get; set; } = "btn btn-sm btn-soft-primary";
    }
}
