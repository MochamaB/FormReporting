namespace FormReporting.Models.ViewModels.Components
{
    // ============================================================================
    // ENUMS
    // ============================================================================

    /// <summary>
    /// Column types for editable table
    /// </summary>
    public enum EditableColumnType
    {
        Text = 1,
        Number = 2,
        Decimal = 3,
        Date = 4,
        Checkbox = 5,
        Select = 6,
        Color = 7,
        TextArea = 8,
        Hidden = 9,
        Email = 10,
        Url = 11
    }

    // ============================================================================
    // CONFIGURATION CLASSES (What users create)
    // ============================================================================

    /// <summary>
    /// Configuration object for creating editable tables
    /// User creates this to define WHAT to display
    /// Extensions handle HOW to transform this into EditableTableViewModel
    /// </summary>
    public class EditableTableConfig
    {
        /// <summary>
        /// Unique table ID
        /// </summary>
        public string TableId { get; set; } = "editable-table";

        /// <summary>
        /// Property name for item array in model binding (e.g., "Items")
        /// </summary>
        public string ItemIndexPropertyName { get; set; } = "Items";

        /// <summary>
        /// List of columns to display
        /// </summary>
        public List<EditableTableColumn> Columns { get; set; } = new();

        /// <summary>
        /// Button configuration
        /// </summary>
        public EditableTableButtons Buttons { get; set; } = new();

        /// <summary>
        /// Enable drag-and-drop row reordering
        /// </summary>
        public bool AllowReorder { get; set; } = false;

        /// <summary>
        /// Show auto-numbered rows
        /// </summary>
        public bool ShowRowNumbers { get; set; } = true;

        /// <summary>
        /// Enable multi-select checkboxes
        /// </summary>
        public bool AllowMultiSelect { get; set; } = false;

        /// <summary>
        /// Enable column sorting
        /// </summary>
        public bool AllowSorting { get; set; } = false;

        /// <summary>
        /// Message to show when table is empty
        /// </summary>
        public string EmptyMessage { get; set; } = "No items added yet.";

        /// <summary>
        /// Number of existing rows (from model data)
        /// </summary>
        public int InitialRowCount { get; set; } = 0;

        /// <summary>
        /// Additional CSS classes for table
        /// </summary>
        public string TableCssClass { get; set; } = string.Empty;

        /// <summary>
        /// Show table in card wrapper
        /// </summary>
        public bool WrapInCard { get; set; } = false;

        /// <summary>
        /// Card title (if WrapInCard is true)
        /// </summary>
        public string? CardTitle { get; set; }
    }

    /// <summary>
    /// Individual column configuration
    /// </summary>
    public class EditableTableColumn
    {
        /// <summary>
        /// Property name for model binding (e.g., "OptionLabel")
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// Column header text
        /// </summary>
        public string Header { get; set; } = string.Empty;

        /// <summary>
        /// Column type (Text, Number, Select, etc.)
        /// </summary>
        public EditableColumnType ColumnType { get; set; } = EditableColumnType.Text;

        /// <summary>
        /// Column width (e.g., "200px", "20%")
        /// </summary>
        public string? Width { get; set; }

        /// <summary>
        /// Is this field required?
        /// </summary>
        public bool IsRequired { get; set; } = false;

        /// <summary>
        /// Placeholder text for input
        /// </summary>
        public string? Placeholder { get; set; }

        /// <summary>
        /// Default value for new rows
        /// </summary>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// Options for select/dropdown columns
        /// </summary>
        public List<EditableTableSelectOption>? SelectOptions { get; set; }

        /// <summary>
        /// Additional CSS classes for input
        /// </summary>
        public string? InputCssClass { get; set; }

        /// <summary>
        /// HTML5 pattern attribute for validation
        /// </summary>
        public string? Pattern { get; set; }

        /// <summary>
        /// Minimum value (for number/decimal types)
        /// </summary>
        public decimal? Min { get; set; }

        /// <summary>
        /// Maximum value (for number/decimal types)
        /// </summary>
        public decimal? Max { get; set; }

        /// <summary>
        /// Step value (for number/decimal types)
        /// </summary>
        public decimal? Step { get; set; }

        /// <summary>
        /// Max length (for text types)
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Allow sorting on this column
        /// </summary>
        public bool Sortable { get; set; } = true;

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Help text shown below input
        /// </summary>
        public string? HelpText { get; set; }

        /// <summary>
        /// Conditional visibility - JavaScript expression (e.g., "HasScoring == true")
        /// </summary>
        public string? ConditionalDisplay { get; set; }
    }

    /// <summary>
    /// Select option for dropdown columns
    /// </summary>
    public class EditableTableSelectOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool Selected { get; set; } = false;
    }

    /// <summary>
    /// Button configuration for editable table
    /// </summary>
    public class EditableTableButtons
    {
        /// <summary>
        /// Show "Add Row" button
        /// </summary>
        public bool ShowAddButton { get; set; } = true;

        /// <summary>
        /// Show "Clear All" button
        /// </summary>
        public bool ShowClearAllButton { get; set; } = true;

        /// <summary>
        /// Show "Remove" button in each row
        /// </summary>
        public bool ShowRemoveButton { get; set; } = true;

        /// <summary>
        /// Show "Delete Selected" button (requires AllowMultiSelect)
        /// </summary>
        public bool ShowDeleteSelectedButton { get; set; } = false;

        /// <summary>
        /// Custom text for add button
        /// </summary>
        public string AddButtonText { get; set; } = "Add Row";

        /// <summary>
        /// Icon class for add button
        /// </summary>
        public string AddButtonIcon { get; set; } = "ri-add-line";

        /// <summary>
        /// Custom text for clear all button
        /// </summary>
        public string ClearAllButtonText { get; set; } = "Clear All";

        /// <summary>
        /// Icon class for clear all button
        /// </summary>
        public string ClearAllButtonIcon { get; set; } = "ri-delete-bin-line";
    }

    // ============================================================================
    // VIEW MODEL CLASSES (What partials receive for rendering)
    // ============================================================================

    /// <summary>
    /// Editable table view model - ready for rendering
    /// Created by EditableTableExtensions.BuildEditableTable()
    /// Contains all render-ready data, no logic needed in views
    /// </summary>
    public class EditableTableViewModel
    {
        /// <summary>
        /// Unique table identifier
        /// </summary>
        public string TableId { get; set; } = string.Empty;

        /// <summary>
        /// Property name for item indexing
        /// </summary>
        public string ItemIndexPropertyName { get; set; } = string.Empty;

        /// <summary>
        /// List of columns (render-ready)
        /// </summary>
        public List<EditableTableColumnViewModel> Columns { get; set; } = new();

        /// <summary>
        /// Button configuration
        /// </summary>
        public EditableTableButtons Buttons { get; set; } = new();

        /// <summary>
        /// Allow reordering
        /// </summary>
        public bool AllowReorder { get; set; }

        /// <summary>
        /// Show row numbers
        /// </summary>
        public bool ShowRowNumbers { get; set; }

        /// <summary>
        /// Allow multi-select
        /// </summary>
        public bool AllowMultiSelect { get; set; }

        /// <summary>
        /// Allow sorting
        /// </summary>
        public bool AllowSorting { get; set; }

        /// <summary>
        /// Empty message
        /// </summary>
        public string EmptyMessage { get; set; } = string.Empty;

        /// <summary>
        /// Initial row count
        /// </summary>
        public int InitialRowCount { get; set; }

        /// <summary>
        /// Table CSS classes
        /// </summary>
        public string TableCssClass { get; set; } = string.Empty;

        /// <summary>
        /// Wrap in card
        /// </summary>
        public bool WrapInCard { get; set; }

        /// <summary>
        /// Card title
        /// </summary>
        public string? CardTitle { get; set; }

        /// <summary>
        /// Configuration JSON for JavaScript
        /// </summary>
        public string ConfigJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// Column view model - pre-computed for rendering
    /// </summary>
    public class EditableTableColumnViewModel
    {
        public string PropertyName { get; set; } = string.Empty;
        public string Header { get; set; } = string.Empty;
        public string ColumnTypeString { get; set; } = string.Empty;
        public string? Width { get; set; }
        public bool IsRequired { get; set; }
        public string? Placeholder { get; set; }
        public object? DefaultValue { get; set; }
        public List<EditableTableSelectOption>? SelectOptions { get; set; }
        public string? InputCssClass { get; set; }
        public string? Pattern { get; set; }
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
        public decimal? Step { get; set; }
        public int? MaxLength { get; set; }
        public bool Sortable { get; set; }
        public string? HelpText { get; set; }
        public string? ConditionalDisplay { get; set; }

        // Computed properties
        public string RequiredAttribute => IsRequired ? "required" : string.Empty;
        public string WidthStyle => !string.IsNullOrEmpty(Width) ? $"width: {Width};" : string.Empty;
        public string InputTypeAttribute { get; set; } = "text";
        public string StepAttribute => Step.HasValue ? $"step=\"{Step}\"" : string.Empty;
        public string MinAttribute => Min.HasValue ? $"min=\"{Min}\"" : string.Empty;
        public string MaxAttribute => Max.HasValue ? $"max=\"{Max}\"" : string.Empty;
        public string MaxLengthAttribute => MaxLength.HasValue ? $"maxlength=\"{MaxLength}\"" : string.Empty;
        public string PatternAttribute => !string.IsNullOrEmpty(Pattern) ? $"pattern=\"{Pattern}\"" : string.Empty;
    }
}
