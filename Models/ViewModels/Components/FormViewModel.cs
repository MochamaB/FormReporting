using FormReporting.Models.Common;

namespace FormReporting.Models.ViewModels.Components
{
    // ========== CONFIGURATION CLASSES (INPUT) ==========

    /// <summary>
    /// Main configuration object for form rendering
    /// Used in controllers to define form structure
    /// Extensions transform this into FormViewModel
    /// </summary>
    public class FormConfig
    {
        /// <summary>
        /// Unique identifier for this form instance
        /// </summary>
        public string FormId { get; set; } = $"form_{Guid.NewGuid():N}";

        /// <summary>
        /// Form title displayed in header
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Optional form description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Sections within the form
        /// </summary>
        public List<FormSectionConfig> Sections { get; set; } = new();

        /// <summary>
        /// URL for form submission
        /// </summary>
        public string? SubmitUrl { get; set; }

        /// <summary>
        /// URL for saving draft
        /// </summary>
        public string? SaveDraftUrl { get; set; }

        /// <summary>
        /// Enable automatic draft saving
        /// </summary>
        public bool EnableAutoSave { get; set; } = false;

        /// <summary>
        /// Auto-save interval in seconds
        /// </summary>
        public int AutoSaveIntervalSeconds { get; set; } = 30;

        /// <summary>
        /// Show progress bar at top of form
        /// </summary>
        public bool ShowProgressBar { get; set; } = true;

        /// <summary>
        /// Additional CSS classes for form container
        /// </summary>
        public string CssClass { get; set; } = string.Empty;

        /// <summary>
        /// Submit button text
        /// </summary>
        public string SubmitButtonText { get; set; } = "Submit Form";

        /// <summary>
        /// Save draft button text
        /// </summary>
        public string SaveDraftButtonText { get; set; } = "Save as Draft";

        /// <summary>
        /// Show reset button
        /// </summary>
        public bool ShowResetButton { get; set; } = true;

        /// <summary>
        /// Show cancel button
        /// </summary>
        public bool ShowCancelButton { get; set; } = true;

        // ========== WIZARD MODE PROPERTIES ==========

        /// <summary>
        /// Render mode for the form (SinglePage or Wizard)
        /// </summary>
        public FormRenderMode RenderMode { get; set; } = FormRenderMode.SinglePage;

        /// <summary>
        /// Show step numbers in wizard mode
        /// </summary>
        public bool ShowStepNumbers { get; set; } = true;

        /// <summary>
        /// Allow users to click on steps to navigate (skip ahead)
        /// </summary>
        public bool AllowStepSkipping { get; set; } = false;

        /// <summary>
        /// Validate current step before allowing navigation to next step
        /// </summary>
        public bool ValidateOnStepChange { get; set; } = true;

        /// <summary>
        /// Previous button text in wizard mode
        /// </summary>
        public string PreviousButtonText { get; set; } = "Previous";

        /// <summary>
        /// Next button text in wizard mode
        /// </summary>
        public string NextButtonText { get; set; } = "Next";
    }

    /// <summary>
    /// Section configuration
    /// Represents a group of related fields
    /// </summary>
    public class FormSectionConfig
    {
        /// <summary>
        /// Database section ID
        /// </summary>
        public int SectionId { get; set; }

        /// <summary>
        /// Section name/title
        /// </summary>
        public string SectionName { get; set; } = string.Empty;

        /// <summary>
        /// Optional section description
        /// </summary>
        public string? SectionDescription { get; set; }

        /// <summary>
        /// Remix icon class (e.g., "ri-computer-line")
        /// </summary>
        public string? IconClass { get; set; }

        /// <summary>
        /// Display order (lower = first)
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Can section be collapsed
        /// </summary>
        public bool IsCollapsible { get; set; } = true;

        /// <summary>
        /// Start in collapsed state
        /// </summary>
        public bool IsCollapsedByDefault { get; set; } = false;

        /// <summary>
        /// Fields within this section
        /// </summary>
        public List<FormFieldConfig> Fields { get; set; } = new();
    }

    /// <summary>
    /// Individual field configuration
    /// </summary>
    public class FormFieldConfig
    {
        /// <summary>
        /// Database item ID (as string for flexibility)
        /// </summary>
        public string FieldId { get; set; } = string.Empty;

        /// <summary>
        /// Field label/question text
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// Optional field description/help text shown above field
        /// </summary>
        public string? FieldDescription { get; set; }

        /// <summary>
        /// Field type (from FormFieldType enum)
        /// </summary>
        public FormFieldType FieldType { get; set; }

        /// <summary>
        /// Is this field required
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Default value (if any)
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Placeholder text shown in empty field
        /// </summary>
        public string? PlaceholderText { get; set; }

        /// <summary>
        /// Help text shown below field
        /// </summary>
        public string? HelpText { get; set; }

        /// <summary>
        /// Text shown before input (e.g., "KES", "$")
        /// </summary>
        public string? PrefixText { get; set; }

        /// <summary>
        /// Text shown after input (e.g., "%", "kg")
        /// </summary>
        public string? SuffixText { get; set; }

        /// <summary>
        /// Display order within section
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Options for dropdown/radio/checkbox fields
        /// </summary>
        public List<FormFieldOption> Options { get; set; } = new();

        /// <summary>
        /// Validation rules for this field
        /// </summary>
        public List<FormFieldValidation> Validations { get; set; } = new();

        /// <summary>
        /// Conditional logic for show/hide
        /// </summary>
        public ConditionalLogic? ConditionalLogic { get; set; }

        /// <summary>
        /// Layout type: Single, Matrix, Grid, Inline
        /// </summary>
        public FieldLayoutType LayoutType { get; set; } = FieldLayoutType.Single;

        /// <summary>
        /// Matrix group ID (fields with same ID render as matrix)
        /// </summary>
        public int? MatrixGroupId { get; set; }

        /// <summary>
        /// Row label in matrix layout
        /// </summary>
        public string? MatrixRowLabel { get; set; }

        /// <summary>
        /// Current value (for edit mode/draft restoration)
        /// </summary>
        public object? CurrentValue { get; set; }

        /// <summary>
        /// Is field read-only
        /// </summary>
        public bool IsReadOnly { get; set; } = false;

        /// <summary>
        /// Is field disabled
        /// </summary>
        public bool IsDisabled { get; set; } = false;

        /// <summary>
        /// Custom CSS classes for field wrapper
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Column width in Bootstrap grid (1-12)
        /// </summary>
        public int ColumnWidth { get; set; } = 12;
    }

    /// <summary>
    /// Field option for dropdown, radio, checkbox, multiselect
    /// </summary>
    public class FormFieldOption
    {
        /// <summary>
        /// Option value (submitted to server)
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Option display label
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Is this the default selected option
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Is this option currently selected
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Parent option ID (for cascading dropdowns)
        /// </summary>
        public int? ParentOptionId { get; set; }

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Validation rule configuration
    /// </summary>
    public class FormFieldValidation
    {
        /// <summary>
        /// Validation type (Required, Email, MinLength, MaxLength, etc.)
        /// </summary>
        public string ValidationType { get; set; } = string.Empty;

        /// <summary>
        /// Minimum numeric value
        /// </summary>
        public decimal? MinValue { get; set; }

        /// <summary>
        /// Maximum numeric value
        /// </summary>
        public decimal? MaxValue { get; set; }

        /// <summary>
        /// Minimum string length
        /// </summary>
        public int? MinLength { get; set; }

        /// <summary>
        /// Maximum string length
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Regex pattern for validation
        /// </summary>
        public string? RegexPattern { get; set; }

        /// <summary>
        /// Custom validation expression (JavaScript)
        /// </summary>
        public string? CustomExpression { get; set; }

        /// <summary>
        /// Error message to display
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Severity: Error, Warning, Info
        /// </summary>
        public string Severity { get; set; } = "Error";

        /// <summary>
        /// Validation order (for multiple validations)
        /// </summary>
        public int ValidationOrder { get; set; }
    }

    /// <summary>
    /// Conditional logic configuration
    /// </summary>
    public class ConditionalLogic
    {
        /// <summary>
        /// Action to take: show, hide, enable, disable
        /// </summary>
        public string Action { get; set; } = "show";

        /// <summary>
        /// Logic operator: all (AND), any (OR)
        /// </summary>
        public string Logic { get; set; } = "all";

        /// <summary>
        /// Rules to evaluate
        /// </summary>
        public List<ConditionalRule> Rules { get; set; } = new();
    }

    /// <summary>
    /// Individual conditional rule
    /// </summary>
    public class ConditionalRule
    {
        /// <summary>
        /// Target field ID to watch
        /// </summary>
        public string TargetFieldId { get; set; } = string.Empty;

        /// <summary>
        /// Comparison operator: equals, notEquals, contains, greaterThan, lessThan, isEmpty, isNotEmpty
        /// </summary>
        public string Operator { get; set; } = "equals";

        /// <summary>
        /// Value to compare against
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }

    // ========== VIEW MODELS (OUTPUT) ==========

    /// <summary>
    /// Final form view model (created by extensions, consumed by partials)
    /// </summary>
    public class FormViewModel
    {
        public string FormId { get; set; } = string.Empty;
        public int? FormTemplateId { get; set; }
        public int? ResponseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<FormSectionViewModel> Sections { get; set; } = new();
        public string? SubmitUrl { get; set; }
        public string? SaveDraftUrl { get; set; }
        public bool EnableAutoSave { get; set; }
        public int AutoSaveIntervalSeconds { get; set; }
        public bool ShowProgressBar { get; set; }
        public int TotalFields { get; set; }
        public int RequiredFields { get; set; }
        public string CssClass { get; set; } = string.Empty;
        public string SubmitButtonText { get; set; } = "Submit Form";
        public string SaveDraftButtonText { get; set; } = "Save as Draft";
        public bool AllowSaveDraft { get; set; } = true;
        public bool ShowResetButton { get; set; } = true;
        public bool ShowCancelButton { get; set; } = true;

        // Wizard mode properties
        public FormRenderMode RenderMode { get; set; } = FormRenderMode.SinglePage;
        public bool ShowStepNumbers { get; set; } = true;
        public bool AllowStepSkipping { get; set; } = false;
        public bool ValidateOnStepChange { get; set; } = true;
        public string PreviousButtonText { get; set; } = "Previous";
        public string NextButtonText { get; set; } = "Next";
        public int TotalSteps => Sections?.Count ?? 0;
    }

    /// <summary>
    /// Section view model (for rendering)
    /// </summary>
    public class FormSectionViewModel
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string? SectionDescription { get; set; }
        public string? IconClass { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsCollapsible { get; set; }
        public bool IsCollapsedByDefault { get; set; }
        public bool IsRequired { get; set; }
        public List<FormFieldViewModel> Fields { get; set; } = new();
        public int TotalFields => Fields?.Count ?? 0;
        public string CollapseId { get; set; } = string.Empty; // Bootstrap collapse target ID
        public List<List<FormFieldViewModel>> FieldRows { get; set; } = new(); // For multi-column layouts
    }

    /// <summary>
    /// Field view model (for rendering)
    /// </summary>
    public class FormFieldViewModel
    {
        public string FieldId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string? FieldDescription { get; set; }
        public FormFieldType FieldType { get; set; }
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public string? PlaceholderText { get; set; }
        public string? HelpText { get; set; }
        public string? PrefixText { get; set; }
        public string? SuffixText { get; set; }
        public List<FormFieldOption> Options { get; set; } = new();
        public List<FormFieldValidation> Validations { get; set; } = new();
        public ConditionalLogic? ConditionalLogic { get; set; }
        public FieldLayoutType LayoutType { get; set; } = FieldLayoutType.Single;
        public int? MatrixGroupId { get; set; }
        public string? MatrixRowLabel { get; set; }
        public object? CurrentValue { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsDisabled { get; set; }
        public string? CssClass { get; set; }
        public int ColumnWidth { get; set; } = 12;

        // Generated properties for HTML rendering
        public string InputName { get; set; } = string.Empty; // Form POST name
        public string InputId { get; set; } = string.Empty; // HTML id attribute
        public string ValidationDataAttribute { get; set; } = string.Empty; // JSON for client-side validation
        public string ConditionalLogicDataAttribute { get; set; } = string.Empty; // JSON for conditional logic
    }

    // ========== HELPER CLASSES ==========

    /// <summary>
    /// Form submission result
    /// </summary>
    public class FormSubmissionResult
    {
        public bool IsValid { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; } = new();
        public Dictionary<string, object?> SubmittedData { get; set; } = new();
        public int? SubmissionId { get; set; }
    }

    /// <summary>
    /// Matrix group (for rendering fields as table/grid)
    /// </summary>
    public class MatrixGroup
    {
        public int GroupId { get; set; }
        public List<FormFieldViewModel> Fields { get; set; } = new();
        public List<string> ColumnHeaders { get; set; } = new();
        public List<string> RowLabels { get; set; } = new();
    }
}
