using FormReporting.Models.ViewModels.Components;

namespace FormReporting.Extensions;

/// <summary>
/// Extension methods for building standard CRUD forms
/// Handles all transformation logic from Config → ViewModel
/// </summary>
public static class SimpleFormExtensions
{
    #region Main Transformation Method

    /// <summary>
    /// Transforms SimpleFormConfig into SimpleFormViewModel ready for rendering
    /// </summary>
    public static SimpleFormViewModel BuildForm(this SimpleFormConfig config)
    {
        // 1. Validate config
        if (string.IsNullOrEmpty(config.FormAction))
            throw new ArgumentException("FormAction is required", nameof(config.FormAction));

        // 2. Sort fields by DisplayOrder
        var orderedFields = config.Fields.OrderBy(f => f.DisplayOrder).ToList();

        // 3. Transform to ViewModels
        var viewModel = new SimpleFormViewModel
        {
            FormId = config.FormId,
            FormAction = config.FormAction,
            FormMethod = config.FormMethod,
            SubmitButtonText = config.SubmitButtonText,
            CancelButtonUrl = config.CancelButtonUrl,
            ShowCancelButton = config.ShowCancelButton,
            ShowResetButton = config.ShowResetButton,
            StyleType = config.StyleType,
            CardTitle = config.CardTitle,
            CardSubtitle = config.CardSubtitle,
            WrapInCard = config.WrapInCard,
            Fields = orderedFields.Select(f => new SimpleFormFieldViewModel
            {
                PropertyName = f.PropertyName,
                Label = f.Label,
                FieldType = f.FieldType,
                Value = f.Value?.ToString() ?? string.Empty,
                IsRequired = f.IsRequired,
                PlaceholderText = f.PlaceholderText,
                HelpText = f.HelpText,
                Options = f.Options ?? new List<SelectOption>(),
                Rows = f.Rows ?? 3,
                Min = f.Min,
                Max = f.Max,
                Step = f.Step,
                ColumnClass = f.ColumnClass,
                InputId = $"{config.FormId}_{f.PropertyName}",
                InputName = f.PropertyName,
                // Generate CSS classes based on style type
                FormGroupClass = GetFormGroupClass(config.StyleType, f.FieldType),
                LabelClass = GetLabelClass(config.StyleType, f.IsRequired),
                InputClass = GetInputClass(config.StyleType, f.FieldType)
            }).ToList()
        };

        return viewModel;
    }

    #endregion

    #region Fluent API Methods

    /// <summary>
    /// Adds a text input field
    /// </summary>
    public static SimpleFormConfig WithTextField(this SimpleFormConfig config,
        string propertyName,
        string label,
        object? value = null,
        bool isRequired = false,
        string? placeholder = null,
        string? helpText = null,
        string columnClass = "col-12")
    {
        config.Fields.Add(new SimpleFormFieldConfig
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = SimpleFieldType.Text,
            Value = value,
            IsRequired = isRequired,
            PlaceholderText = placeholder,
            HelpText = helpText,
            ColumnClass = columnClass,
            DisplayOrder = config.Fields.Count + 1
        });
        return config;
    }

    /// <summary>
    /// Adds a textarea field
    /// </summary>
    public static SimpleFormConfig WithTextAreaField(this SimpleFormConfig config,
        string propertyName,
        string label,
        object? value = null,
        int rows = 3,
        bool isRequired = false,
        string? placeholder = null,
        string? helpText = null,
        string columnClass = "col-12")
    {
        config.Fields.Add(new SimpleFormFieldConfig
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = SimpleFieldType.TextArea,
            Value = value,
            Rows = rows,
            IsRequired = isRequired,
            PlaceholderText = placeholder,
            HelpText = helpText,
            ColumnClass = columnClass,
            DisplayOrder = config.Fields.Count + 1
        });
        return config;
    }

    /// <summary>
    /// Adds an email input field
    /// </summary>
    public static SimpleFormConfig WithEmailField(this SimpleFormConfig config,
        string propertyName,
        string label,
        object? value = null,
        bool isRequired = false,
        string? placeholder = null,
        string? helpText = null,
        string columnClass = "col-12")
    {
        config.Fields.Add(new SimpleFormFieldConfig
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = SimpleFieldType.Email,
            Value = value,
            IsRequired = isRequired,
            PlaceholderText = placeholder,
            HelpText = helpText,
            ColumnClass = columnClass,
            DisplayOrder = config.Fields.Count + 1
        });
        return config;
    }

    /// <summary>
    /// Adds a password input field
    /// </summary>
    public static SimpleFormConfig WithPasswordField(this SimpleFormConfig config,
        string propertyName,
        string label,
        object? value = null,
        bool isRequired = false,
        string? placeholder = null,
        string? helpText = null,
        string columnClass = "col-12")
    {
        config.Fields.Add(new SimpleFormFieldConfig
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = SimpleFieldType.Password,
            Value = value,
            IsRequired = isRequired,
            PlaceholderText = placeholder,
            HelpText = helpText,
            ColumnClass = columnClass,
            DisplayOrder = config.Fields.Count + 1
        });
        return config;
    }

    /// <summary>
    /// Adds a number input field
    /// </summary>
    public static SimpleFormConfig WithNumberField(this SimpleFormConfig config,
        string propertyName,
        string label,
        object? value = null,
        decimal? min = null,
        decimal? max = null,
        decimal? step = null,
        bool isRequired = false,
        string? placeholder = null,
        string? helpText = null,
        string columnClass = "col-12")
    {
        config.Fields.Add(new SimpleFormFieldConfig
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = SimpleFieldType.Number,
            Value = value,
            Min = min,
            Max = max,
            Step = step,
            IsRequired = isRequired,
            PlaceholderText = placeholder,
            HelpText = helpText,
            ColumnClass = columnClass,
            DisplayOrder = config.Fields.Count + 1
        });
        return config;
    }

    /// <summary>
    /// Adds a date input field
    /// </summary>
    public static SimpleFormConfig WithDateField(this SimpleFormConfig config,
        string propertyName,
        string label,
        object? value = null,
        bool isRequired = false,
        string? helpText = null,
        string columnClass = "col-12")
    {
        config.Fields.Add(new SimpleFormFieldConfig
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = SimpleFieldType.Date,
            Value = value,
            IsRequired = isRequired,
            HelpText = helpText,
            ColumnClass = columnClass,
            DisplayOrder = config.Fields.Count + 1
        });
        return config;
    }

    /// <summary>
    /// Adds a datetime input field
    /// </summary>
    public static SimpleFormConfig WithDateTimeField(this SimpleFormConfig config,
        string propertyName,
        string label,
        object? value = null,
        bool isRequired = false,
        string? helpText = null,
        string columnClass = "col-12")
    {
        config.Fields.Add(new SimpleFormFieldConfig
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = SimpleFieldType.DateTime,
            Value = value,
            IsRequired = isRequired,
            HelpText = helpText,
            ColumnClass = columnClass,
            DisplayOrder = config.Fields.Count + 1
        });
        return config;
    }

    /// <summary>
    /// Adds a select/dropdown field
    /// </summary>
    public static SimpleFormConfig WithSelectField(this SimpleFormConfig config,
        string propertyName,
        string label,
        List<SelectOption> options,
        object? value = null,
        bool isRequired = false,
        string? helpText = null,
        string columnClass = "col-12")
    {
        // Set selected option based on value
        if (value != null)
        {
            var valueStr = value.ToString();
            var selectedOption = options.FirstOrDefault(o => o.Value == valueStr);
            if (selectedOption != null)
                selectedOption.IsSelected = true;
        }

        config.Fields.Add(new SimpleFormFieldConfig
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = SimpleFieldType.Select,
            Value = value,
            Options = options,
            IsRequired = isRequired,
            HelpText = helpText,
            ColumnClass = columnClass,
            DisplayOrder = config.Fields.Count + 1
        });
        return config;
    }

    /// <summary>
    /// Adds a time field
    /// </summary>
    public static SimpleFormConfig WithTimeField(this SimpleFormConfig config,
        string propertyName,
        string label,
        TimeSpan? value = null,
        bool isRequired = false,
        string? helpText = null,
        string columnClass = "col-12")
    {
        // Format TimeSpan for HTML5 time input (HH:mm format)
        string? timeValue = value?.ToString(@"hh\:mm");
        
        config.Fields.Add(new SimpleFormFieldConfig
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = SimpleFieldType.Time,
            Value = timeValue,
            IsRequired = isRequired,
            HelpText = helpText,
            ColumnClass = columnClass,
            DisplayOrder = config.Fields.Count + 1
        });
        return config;
    }

    /// <summary>
    /// Adds a checkbox field
    /// </summary>
    public static SimpleFormConfig WithCheckboxField(this SimpleFormConfig config,
        string propertyName,
        string label,
        bool isChecked = false,
        string? helpText = null,
        string columnClass = "col-12")
    {
        config.Fields.Add(new SimpleFormFieldConfig
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = SimpleFieldType.Checkbox,
            Value = isChecked,
            HelpText = helpText,
            ColumnClass = columnClass,
            DisplayOrder = config.Fields.Count + 1
        });
        return config;
    }

    /// <summary>
    /// Adds a file upload field
    /// </summary>
    public static SimpleFormConfig WithFileField(this SimpleFormConfig config,
        string propertyName,
        string label,
        bool isRequired = false,
        string? helpText = null,
        string columnClass = "col-12")
    {
        config.Fields.Add(new SimpleFormFieldConfig
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = SimpleFieldType.File,
            IsRequired = isRequired,
            HelpText = helpText,
            ColumnClass = columnClass,
            DisplayOrder = config.Fields.Count + 1
        });
        return config;
    }

    /// <summary>
    /// Adds a hidden field
    /// </summary>
    public static SimpleFormConfig WithHiddenField(this SimpleFormConfig config,
        string propertyName,
        object? value = null)
    {
        config.Fields.Add(new SimpleFormFieldConfig
        {
            PropertyName = propertyName,
            Label = string.Empty,
            FieldType = SimpleFieldType.Hidden,
            Value = value,
            DisplayOrder = config.Fields.Count + 1
        });
        return config;
    }

    /// <summary>
    /// Adds an icon picker field with live preview and modal selection
    /// </summary>
    public static SimpleFormConfig WithIconPickerField(this SimpleFormConfig config,
        string propertyName,
        string label,
        object? value = null,
        bool isRequired = false,
        string? placeholder = null,
        string? helpText = null,
        string columnClass = "col-12")
    {
        config.Fields.Add(new SimpleFormFieldConfig
        {
            PropertyName = propertyName,
            Label = label,
            FieldType = SimpleFieldType.IconPicker,
            Value = value,
            IsRequired = isRequired,
            PlaceholderText = placeholder ?? "e.g., ri-file-list-line",
            HelpText = helpText ?? "Click the button to browse available icons",
            ColumnClass = columnClass,
            DisplayOrder = config.Fields.Count + 1
        });
        return config;
    }

    #endregion

    #region Helper Methods for CSS Classes

    private static string GetFormGroupClass(FormStyleType styleType, SimpleFieldType fieldType)
    {
        return styleType switch
        {
            FormStyleType.FloatingLabels when fieldType != SimpleFieldType.Checkbox
                && fieldType != SimpleFieldType.Radio
                && fieldType != SimpleFieldType.File => "form-floating mb-3",
            FormStyleType.Horizontal => "row mb-3",
            FormStyleType.Inline => "col-auto",
            _ => "mb-3"
        };
    }

    private static string GetLabelClass(FormStyleType styleType, bool isRequired)
    {
        var requiredClass = isRequired ? "required" : "";

        return styleType switch
        {
            FormStyleType.FloatingLabels => requiredClass,  // No form-label for floating
            FormStyleType.Horizontal => $"col-md-3 col-form-label {requiredClass}".Trim(),
            FormStyleType.Inline => $"visually-hidden {requiredClass}".Trim(),
            _ => $"form-label {requiredClass}".Trim()
        };
    }

    private static string GetInputClass(FormStyleType styleType, SimpleFieldType fieldType)
    {
        return fieldType switch
        {
            SimpleFieldType.Checkbox => "form-check-input",
            SimpleFieldType.Select => "form-select",
            SimpleFieldType.File => "form-control",
            _ => "form-control"
        };
    }

    #endregion

    #region Test Data Generator

    /// <summary>
    /// Creates a test form configuration with sample data for all field types
    /// </summary>
    public static SimpleFormConfig CreateTestFormConfig(FormStyleType styleType = FormStyleType.Standard)
    {
        var config = new SimpleFormConfig
        {
            FormId = "testForm",
            FormAction = "/Test/Submit",
            CardTitle = "Test Form - All Field Types",
            CardSubtitle = $"Testing standard form components with {styleType} style",
            StyleType = styleType,
            CancelButtonUrl = "/Test/Index",
            ShowResetButton = true
        };

        return config
            .WithTextField("Name", "Full Name", "John Doe", isRequired: true, placeholder: "Enter full name", columnClass: "col-md-6")
            .WithTextField("Code", "Code", "TEST-001", isRequired: true, placeholder: "Enter code", columnClass: "col-md-6")
            .WithEmailField("Email", "Email Address", "john.doe@ktda.com", isRequired: true, placeholder: "name@example.com", columnClass: "col-md-6")
            .WithPasswordField("Password", "Password", isRequired: true, placeholder: "Enter password", columnClass: "col-md-6")
            .WithTextAreaField("Description", "Description", "Sample description text", rows: 4, placeholder: "Enter description")
            .WithNumberField("DisplayOrder", "Display Order", 1, min: 1, max: 100, step: 1, isRequired: true, columnClass: "col-md-4")
            .WithDateField("CreatedDate", "Created Date", DateTime.Now.ToString("yyyy-MM-dd"), isRequired: true, columnClass: "col-md-4")
            .WithDateTimeField("LastModified", "Last Modified", DateTime.Now.ToString("yyyy-MM-ddTHH:mm"), columnClass: "col-md-4")
            .WithSelectField("Category", "Category", new List<SelectOption>
            {
                new() { Value = "", Text = "-- Select Category --" },
                new() { Value = "1", Text = "Category 1" },
                new() { Value = "2", Text = "Category 2", IsSelected = true },
                new() { Value = "3", Text = "Category 3" }
            }, isRequired: true, columnClass: "col-md-6")
            .WithSelectField("Status", "Status", new List<SelectOption>
            {
                new() { Value = "active", Text = "Active", IsSelected = true },
                new() { Value = "inactive", Text = "Inactive" }
            }, columnClass: "col-md-6")
            .WithCheckboxField("IsActive", "Is Active", true, helpText: "Check to activate this item")
            .WithCheckboxField("SendNotification", "Send Email Notification", false)
            .WithFileField("Attachment", "Attachment", helpText: "Maximum file size: 5MB");
    }

    #endregion
}
