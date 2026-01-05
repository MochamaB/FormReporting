namespace FormReporting.Models.ViewModels.Components;

/// <summary>
/// Configuration object for building standard CRUD forms
/// This is what developers create in their views
/// </summary>
public class SimpleFormConfig
{
    // Basic form metadata
    public string FormId { get; set; } = "simpleForm";
    public string FormAction { get; set; } = string.Empty;
    public string FormMethod { get; set; } = "POST";
    public string SubmitButtonText { get; set; } = "Save";
    public string CancelButtonUrl { get; set; } = string.Empty;

    // Form style variations
    public FormStyleType StyleType { get; set; } = FormStyleType.Standard;

    // Fields collection
    public List<SimpleFormFieldConfig> Fields { get; set; } = new();

    // Optional features
    public bool ShowCancelButton { get; set; } = true;
    public bool ShowResetButton { get; set; } = false;
    public string? CardTitle { get; set; }
    public string? CardSubtitle { get; set; }
    public bool WrapInCard { get; set; } = true;
}

/// <summary>
/// Configuration for individual form field
/// </summary>
public class SimpleFormFieldConfig
{
    // Basic properties
    public string PropertyName { get; set; } = string.Empty;  // "Name", "Email"
    public string Label { get; set; } = string.Empty;
    public SimpleFieldType FieldType { get; set; }

    // Value binding
    public object? Value { get; set; }

    // Validation
    public bool IsRequired { get; set; } = false;
    public string? PlaceholderText { get; set; }
    public string? HelpText { get; set; }

    // For dropdowns/select
    public List<SelectOption>? Options { get; set; }

    // For textareas
    public int? Rows { get; set; }

    // For number inputs
    public decimal? Min { get; set; }
    public decimal? Max { get; set; }
    public decimal? Step { get; set; }

    // Layout
    public string ColumnClass { get; set; } = "col-12";  // "col-md-6" for two columns
    public int DisplayOrder { get; set; }
}

/// <summary>
/// ViewModel produced by extensions and consumed by partials
/// </summary>
public class SimpleFormViewModel
{
    public string FormId { get; set; } = string.Empty;
    public string FormAction { get; set; } = string.Empty;
    public string FormMethod { get; set; } = string.Empty;
    public string SubmitButtonText { get; set; } = string.Empty;
    public string CancelButtonUrl { get; set; } = string.Empty;
    public bool ShowCancelButton { get; set; }
    public bool ShowResetButton { get; set; }
    public FormStyleType StyleType { get; set; }
    public string? CardTitle { get; set; }
    public string? CardSubtitle { get; set; }
    public bool WrapInCard { get; set; }
    public List<SimpleFormFieldViewModel> Fields { get; set; } = new();
}

/// <summary>
/// ViewModel for individual field (transformed from config)
/// </summary>
public class SimpleFormFieldViewModel
{
    public string PropertyName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public SimpleFieldType FieldType { get; set; }
    public string Value { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? PlaceholderText { get; set; }
    public string? HelpText { get; set; }
    public List<SelectOption> Options { get; set; } = new();
    public int Rows { get; set; } = 3;
    public decimal? Min { get; set; }
    public decimal? Max { get; set; }
    public decimal? Step { get; set; }
    public string ColumnClass { get; set; } = "col-12";

    // Generated properties
    public string InputId { get; set; } = string.Empty;
    public string InputName { get; set; } = string.Empty;
    public string FormGroupClass { get; set; } = string.Empty;
    public string LabelClass { get; set; } = string.Empty;
    public string InputClass { get; set; } = string.Empty;
}

/// <summary>
/// Option for dropdown/select fields
/// </summary>
public class SelectOption
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsSelected { get; set; } = false;
}

/// <summary>
/// Form style variations
/// </summary>
public enum FormStyleType
{
    Standard,           // Normal labels above inputs
    FloatingLabels,     // Bootstrap floating labels
    Horizontal,         // Label left, input right (2-column layout)
    Inline              // All fields in one row
}

/// <summary>
/// Simple field types for CRUD forms
/// </summary>
public enum SimpleFieldType
{
    Text,              // <input type="text">
    TextArea,          // <textarea>
    Email,             // <input type="email">
    Password,          // <input type="password">
    Number,            // <input type="number">
    Date,              // <input type="date">
    DateTime,          // <input type="datetime-local">
    Time,              // <input type="time">
    Checkbox,          // <input type="checkbox">
    Select,            // <select>
    Radio,             // <input type="radio">
    Hidden,            // <input type="hidden">
    File               // <input type="file">
}
