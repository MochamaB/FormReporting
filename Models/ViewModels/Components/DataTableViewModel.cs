using Microsoft.AspNetCore.Html;

namespace FormReporting.Models.ViewModels.Components
{
    /// <summary>
    /// Configuration object for creating data tables
    /// Used in views to define table structure, filters, pagination, etc.
    /// Extensions transform this into DataTableViewModel
    /// </summary>
    public class DataTableConfig
    {
        /// <summary>
        /// Unique identifier for this table (used for HTML id attributes)
        /// </summary>
        public string TableId { get; set; } = $"dataTable_{Guid.NewGuid():N}";

        /// <summary>
        /// Column headers (e.g., ["Name", "Email", "Role", "Status", "Actions"])
        /// </summary>
        public List<string> Columns { get; set; } = new();

        /// <summary>
        /// Data source - list of entities to display
        /// Will be transformed into rows by extension methods
        /// </summary>
        public object? DataSource { get; set; }

        /// <summary>
        /// Function to render table rows from data source
        /// Signature: Func<object?, IHtmlContent>
        /// </summary>
        public Func<object?, IHtmlContent>? TableContentRenderer { get; set; }

        // ========== SEARCH ==========

        /// <summary>
        /// Enable search box in header
        /// </summary>
        public bool EnableSearch { get; set; } = true;

        /// <summary>
        /// Search box configuration
        /// </summary>
        public SearchBoxConfig? SearchBox { get; set; }

        // ========== FILTERS ==========

        /// <summary>
        /// Simple dropdown filters (e.g., Status: All/Active/Inactive)
        /// </summary>
        public List<FilterDropdownConfig>? FilterDropdowns { get; set; }

        /// <summary>
        /// Advanced select filters (collapsible, e.g., Region, Department)
        /// </summary>
        public List<FilterSelectConfig>? FilterSelects { get; set; }

        // ========== PAGINATION ==========

        /// <summary>
        /// Enable pagination footer
        /// </summary>
        public bool ShowPagination { get; set; } = true;

        /// <summary>
        /// Current page number (1-indexed)
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; } = 1;

        /// <summary>
        /// Total number of records across all pages
        /// </summary>
        public int TotalRecords { get; set; } = 0;

        /// <summary>
        /// Records per page
        /// </summary>
        public int PageSize { get; set; } = 10;

        // ========== ACTIONS ==========

        /// <summary>
        /// Header actions (e.g., Create button, Export button)
        /// </summary>
        public List<HeaderActionConfig>? HeaderActions { get; set; }

        /// <summary>
        /// Quick create button (shorthand for common case)
        /// </summary>
        public string? CreateButtonText { get; set; }

        /// <summary>
        /// URL for create button
        /// </summary>
        public string? CreateButtonUrl { get; set; }

        // ========== BULK ACTIONS ==========

        /// <summary>
        /// Enable row selection checkboxes for bulk actions
        /// </summary>
        public bool EnableBulkActions { get; set; } = false;

        /// <summary>
        /// Bulk action buttons (e.g., "Delete Selected", "Export Selected")
        /// </summary>
        public List<BulkActionConfig>? BulkActions { get; set; }

        // ========== SORTING ==========

        /// <summary>
        /// Enable client-side sorting
        /// </summary>
        public bool EnableSorting { get; set; } = true;

        /// <summary>
        /// Columns excluded from sorting (by index, e.g., Actions column)
        /// </summary>
        public List<int>? NonSortableColumns { get; set; }

        // ========== STYLING ==========

        /// <summary>
        /// Enable hover effect on rows
        /// </summary>
        public bool EnableHover { get; set; } = true;

        /// <summary>
        /// Enable striped rows
        /// </summary>
        public bool EnableStriped { get; set; } = false;

        /// <summary>
        /// Enable bordered table
        /// </summary>
        public bool EnableBordered { get; set; } = false;

        /// <summary>
        /// Table size: "sm" for compact, null for default
        /// </summary>
        public string? TableSize { get; set; }

        // ========== VIEW TOGGLE (Table/Card) ==========

        /// <summary>
        /// Enable toggle between table and card views
        /// </summary>
        public bool EnableViewToggle { get; set; } = false;

        /// <summary>
        /// Default view mode: "table" or "card"
        /// </summary>
        public string DefaultView { get; set; } = "table";

        /// <summary>
        /// Function to render card view content
        /// Signature: Func<object?, IHtmlContent>
        /// </summary>
        public Func<object?, IHtmlContent>? CardContentRenderer { get; set; }
    }

    /// <summary>
    /// Search box configuration
    /// </summary>
    public class SearchBoxConfig
    {
        public string ParameterName { get; set; } = "search";
        public string PlaceholderText { get; set; } = "Search...";
        public string? CurrentValue { get; set; }
        public string ActionUrl { get; set; } = "";
        public bool ShowButton { get; set; } = true;
        public string InputId { get; set; } = $"searchBox_{Guid.NewGuid():N}";
        public Dictionary<string, string>? PreserveQueryParams { get; set; }
    }

    /// <summary>
    /// Simple dropdown filter configuration
    /// </summary>
    public class FilterDropdownConfig
    {
        public string Label { get; set; } = "Filter";
        public List<FilterOption> Options { get; set; } = new();
    }

    /// <summary>
    /// Advanced select filter configuration
    /// </summary>
    public class FilterSelectConfig
    {
        public string ParameterName { get; set; } = "filter";
        public string PlaceholderText { get; set; } = "Select...";
        public string ActionUrl { get; set; } = "";
        public string FormId { get; set; } = $"filterSelect_{Guid.NewGuid():N}";
        public List<FilterOption> Options { get; set; } = new();
        public Dictionary<string, string>? PreserveQueryParams { get; set; }
    }

    /// <summary>
    /// Filter option for dropdowns and selects
    /// </summary>
    public class FilterOption
    {
        public string Text { get; set; } = "";
        public string Value { get; set; } = "";
        public string? Url { get; set; }
        public bool IsActive { get; set; } = false;
        public bool IsSelected { get; set; } = false;
    }

    /// <summary>
    /// Header action configuration (e.g., Create, Export, Import buttons)
    /// </summary>
    public class HeaderActionConfig
    {
        public string Text { get; set; } = "";
        public string Url { get; set; } = "#";
        public string IconClass { get; set; } = "";
        public string ColorClass { get; set; } = "primary"; // primary, success, info, etc.
        public bool RequiresPermission { get; set; } = false;
        public string? PermissionName { get; set; }
    }

    /// <summary>
    /// Bulk action configuration (e.g., Delete Selected, Export Selected)
    /// </summary>
    public class BulkActionConfig
    {
        public string Text { get; set; } = "";
        public string ActionUrl { get; set; } = "#";
        public string IconClass { get; set; } = "";
        public string ColorClass { get; set; } = "primary";
        public bool RequiresConfirmation { get; set; } = true;
        public string? ConfirmationMessage { get; set; }
    }

    /// <summary>
    /// Final DataTable ViewModel (created by extensions, consumed by partials)
    /// </summary>
    public class DataTableViewModel
    {
        public string TableId { get; set; } = "";
        public List<string> Columns { get; set; } = new();
        public Func<object?, IHtmlContent> TableContent { get; set; } = _ => new HtmlString("");

        // Search
        public SearchBoxViewModel? SearchBox { get; set; }

        // Filters
        public List<FilterDropdownViewModel>? FilterDropdowns { get; set; }
        public List<FilterSelectViewModel>? FilterSelects { get; set; }

        // Pagination
        public bool ShowPagination { get; set; } = true;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalRecords { get; set; } = 0;
        public int PageSize { get; set; } = 10;

        // Actions
        public List<HeaderActionViewModel>? HeaderActions { get; set; }
        public string? CreateButtonText { get; set; }
        public string? CreateButtonUrl { get; set; }

        // Bulk Actions
        public bool EnableBulkActions { get; set; } = false;
        public List<BulkActionViewModel>? BulkActions { get; set; }

        // Sorting
        public bool EnableSorting { get; set; } = true;
        public List<int>? NonSortableColumns { get; set; }

        // Styling
        public string TableClasses { get; set; } = "table table-hover mb-0";

        // View Toggle
        public bool EnableViewToggle { get; set; } = false;
        public string DefaultView { get; set; } = "table";
        public Func<object?, IHtmlContent>? CardContent { get; set; }
    }

    /// <summary>
    /// Search box view model (for rendering)
    /// </summary>
    public class SearchBoxViewModel
    {
        public string ParameterName { get; set; } = "search";
        public string PlaceholderText { get; set; } = "Search...";
        public string? CurrentValue { get; set; }
        public string ActionUrl { get; set; } = "";
        public bool ShowButton { get; set; } = true;
        public string InputId { get; set; } = "";
        public Dictionary<string, string>? PreserveQueryParams { get; set; }
    }

    /// <summary>
    /// Filter dropdown view model (for rendering)
    /// </summary>
    public class FilterDropdownViewModel
    {
        public string Label { get; set; } = "Filter";
        public List<FilterOptionViewModel> Options { get; set; } = new();
    }

    /// <summary>
    /// Filter select view model (for rendering)
    /// </summary>
    public class FilterSelectViewModel
    {
        public string ParameterName { get; set; } = "filter";
        public string PlaceholderText { get; set; } = "Select...";
        public string ActionUrl { get; set; } = "";
        public string FormId { get; set; } = "";
        public List<FilterOptionViewModel> Options { get; set; } = new();
        public Dictionary<string, string>? PreserveQueryParams { get; set; }
    }

    /// <summary>
    /// Filter option view model (for rendering)
    /// </summary>
    public class FilterOptionViewModel
    {
        public string Text { get; set; } = "";
        public string Value { get; set; } = "";
        public string? Url { get; set; }
        public bool IsActive { get; set; } = false;
        public bool IsSelected { get; set; } = false;
    }

    /// <summary>
    /// Header action view model (for rendering)
    /// </summary>
    public class HeaderActionViewModel
    {
        public string Text { get; set; } = "";
        public string Url { get; set; } = "#";
        public string IconClass { get; set; } = "";
        public string ColorClass { get; set; } = "primary";
    }

    /// <summary>
    /// Bulk action view model (for rendering)
    /// </summary>
    public class BulkActionViewModel
    {
        public string Text { get; set; } = "";
        public string ActionUrl { get; set; } = "#";
        public string IconClass { get; set; } = "";
        public string ColorClass { get; set; } = "primary";
        public bool RequiresConfirmation { get; set; } = true;
        public string? ConfirmationMessage { get; set; }
    }

    // ========== ROW ACTIONS (View/Edit/Delete buttons) ==========

    /// <summary>
    /// Display style for row actions
    /// </summary>
    public enum RowActionDisplayStyle
    {
        /// <summary>
        /// Show actions as inline buttons side-by-side (recommended for 2-4 actions)
        /// </summary>
        Inline,

        /// <summary>
        /// Show actions in a dropdown menu (recommended for 4+ actions)
        /// </summary>
        Dropdown
    }

    /// <summary>
    /// Configuration for row action buttons (View/Edit/Delete)
    /// </summary>
    public class RowActionsConfig
    {
        /// <summary>
        /// How to display the actions (Inline buttons or Dropdown menu)
        /// </summary>
        public RowActionDisplayStyle DisplayStyle { get; set; } = RowActionDisplayStyle.Inline;

        /// <summary>
        /// List of actions to display
        /// </summary>
        public List<RowActionConfig> Actions { get; set; } = new();

        /// <summary>
        /// Text for dropdown button (only used when DisplayStyle = Dropdown)
        /// </summary>
        public string DropdownText { get; set; } = "Actions";

        /// <summary>
        /// Icon for dropdown button (only used when DisplayStyle = Dropdown)
        /// </summary>
        public string DropdownIconClass { get; set; } = "ri-more-2-fill";

        /// <summary>
        /// Size of buttons: "sm", "md", null for default
        /// </summary>
        public string? ButtonSize { get; set; } = "sm";

        /// <summary>
        /// Use soft colored buttons (btn-soft-primary) instead of solid buttons
        /// </summary>
        public bool UseSoftButtons { get; set; } = true;
    }

    /// <summary>
    /// Individual row action configuration
    /// </summary>
    public class RowActionConfig
    {
        /// <summary>
        /// Display text (e.g., "View", "Edit", "Delete")
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// Remix icon class (e.g., "ri-eye-line", "ri-pencil-line", "ri-delete-bin-line")
        /// </summary>
        public string IconClass { get; set; } = "";

        /// <summary>
        /// Bootstrap color class: primary, success, warning, danger, info, secondary
        /// </summary>
        public string ColorClass { get; set; } = "primary";

        /// <summary>
        /// URL template with {id} placeholder (e.g., "/users/{id}/edit", "/users/{id}/delete")
        /// {id} will be replaced with the row identifier
        /// </summary>
        public string UrlTemplate { get; set; } = "#";

        /// <summary>
        /// Requires confirmation dialog before action
        /// </summary>
        public bool RequiresConfirmation { get; set; } = false;

        /// <summary>
        /// Confirmation message (e.g., "Are you sure you want to delete this user?")
        /// </summary>
        public string? ConfirmationMessage { get; set; }

        /// <summary>
        /// Requires permission to view this action
        /// </summary>
        public bool RequiresPermission { get; set; } = false;

        /// <summary>
        /// Permission name required to view this action
        /// </summary>
        public string? PermissionName { get; set; }

        /// <summary>
        /// Show icon only (no text) when DisplayStyle = Inline
        /// </summary>
        public bool IconOnly { get; set; } = true;
    }

    /// <summary>
    /// ViewModel for rendering row actions (created by extensions)
    /// </summary>
    public class RowActionsViewModel
    {
        public RowActionDisplayStyle DisplayStyle { get; set; } = RowActionDisplayStyle.Inline;
        public List<RowActionViewModel> Actions { get; set; } = new();
        public string DropdownText { get; set; } = "Actions";
        public string DropdownIconClass { get; set; } = "ri-more-2-fill";
        public string ButtonSizeClass { get; set; } = "btn-sm";
        public bool UseSoftButtons { get; set; } = true;
        public string RowId { get; set; } = "";
    }

    /// <summary>
    /// Individual row action view model (for rendering)
    /// </summary>
    public class RowActionViewModel
    {
        public string Text { get; set; } = "";
        public string IconClass { get; set; } = "";
        public string ColorClass { get; set; } = "primary";
        public string Url { get; set; } = "#";
        public bool RequiresConfirmation { get; set; } = false;
        public string? ConfirmationMessage { get; set; }
        public bool IconOnly { get; set; } = true;
        public bool IsVisible { get; set; } = true;
    }
}
