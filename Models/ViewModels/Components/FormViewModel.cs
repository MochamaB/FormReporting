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

        // ========== SUBMISSION CONTEXT PROPERTIES ==========

        /// <summary>
        /// Submission ID (null for lazy submission - will be created on first save)
        /// </summary>
        public int? SubmissionId { get; set; }

        /// <summary>
        /// Current section index for wizard resume
        /// </summary>
        public int CurrentSectionIndex { get; set; }

        /// <summary>
        /// URL for auto-save API endpoint
        /// </summary>
        public string? AutoSaveUrl { get; set; }

        /// <summary>
        /// Auto-save interval in milliseconds
        /// </summary>
        public int AutoSaveIntervalMs { get; set; } = 30000;

        /// <summary>
        /// Is the form in read-only mode (for viewing submitted forms)
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Cancel URL (where to redirect on cancel)
        /// </summary>
        public string? CancelUrl { get; set; }

        /// <summary>
        /// Template ID (for auto-save context)
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// Tenant ID (optional, for multi-tenant submissions)
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Reporting period for the submission
        /// </summary>
        public string? ReportingPeriod { get; set; }
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
        
        /// <summary>
        /// Display order within section (used for numbering in form builder)
        /// </summary>
        public int DisplayOrder { get; set; }
        
        /// <summary>
        /// Whether to show field number prefix in label (for form builder preview)
        /// </summary>
        public bool ShowFieldNumber { get; set; } = false;

        // ========================================================================
        // DATE/TIME FIELD PROPERTIES (for rendering min/max attributes)
        // ========================================================================

        /// <summary>
        /// Minimum date allowed (format: yyyy-MM-dd)
        /// </summary>
        public string? MinDate { get; set; }

        /// <summary>
        /// Maximum date allowed (format: yyyy-MM-dd)
        /// </summary>
        public string? MaxDate { get; set; }

        /// <summary>
        /// Minimum time allowed (format: HH:mm)
        /// </summary>
        public string? MinTime { get; set; }

        /// <summary>
        /// Maximum time allowed (format: HH:mm)
        /// </summary>
        public string? MaxTime { get; set; }

        // ========================================================================
        // TEXT FIELD PROPERTIES
        // ========================================================================

        /// <summary>
        /// Number of visible rows (for TextArea fields)
        /// </summary>
        public int? Rows { get; set; }

        /// <summary>
        /// Maximum character length
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Minimum character length
        /// </summary>
        public int? MinLength { get; set; }

        // ========================================================================
        // NUMERIC FIELD PROPERTIES
        // ========================================================================

        /// <summary>
        /// Minimum numeric value allowed
        /// </summary>
        public decimal? MinValue { get; set; }

        /// <summary>
        /// Maximum numeric value allowed
        /// </summary>
        public decimal? MaxValue { get; set; }

        /// <summary>
        /// Step increment for numeric inputs
        /// </summary>
        public decimal? Step { get; set; }

        /// <summary>
        /// Number of decimal places to display
        /// </summary>
        public int? DecimalPlaces { get; set; }

        // ========================================================================
        // RATING FIELD PROPERTIES
        // ========================================================================

        /// <summary>
        /// Maximum rating value (default 5 for star ratings)
        /// </summary>
        public int RatingMax { get; set; } = 5;

        /// <summary>
        /// Icon to use for rating (e.g., "star", "heart", "circle")
        /// </summary>
        public string RatingIcon { get; set; } = "star";

        /// <summary>
        /// Allow half ratings (e.g., 3.5 stars)
        /// </summary>
        public bool AllowHalfRating { get; set; } = false;

        // ========================================================================
        // SLIDER FIELD PROPERTIES
        // ========================================================================

        /// <summary>
        /// Minimum slider value
        /// </summary>
        public decimal SliderMin { get; set; } = 0;

        /// <summary>
        /// Maximum slider value
        /// </summary>
        public decimal SliderMax { get; set; } = 100;

        /// <summary>
        /// Slider step increment
        /// </summary>
        public decimal SliderStep { get; set; } = 1;

        /// <summary>
        /// Show tick marks on slider
        /// </summary>
        public bool ShowSliderTicks { get; set; } = false;

        /// <summary>
        /// Show value label while dragging
        /// </summary>
        public bool ShowSliderValue { get; set; } = true;

        // ========================================================================
        // FILE UPLOAD PROPERTIES
        // ========================================================================

        /// <summary>
        /// Allowed file extensions (e.g., ".pdf,.doc,.docx")
        /// </summary>
        public string? AllowedFileTypes { get; set; }

        /// <summary>
        /// Maximum file size in bytes
        /// </summary>
        public long? MaxFileSize { get; set; }

        /// <summary>
        /// Maximum file size formatted for display (e.g., "5 MB")
        /// </summary>
        public string? MaxFileSizeDisplay { get; set; }

        /// <summary>
        /// Allow multiple file uploads
        /// </summary>
        public bool AllowMultipleFiles { get; set; } = false;

        /// <summary>
        /// Maximum number of files allowed
        /// </summary>
        public int? MaxFiles { get; set; }

        // ========================================================================
        // IMAGE FIELD PROPERTIES
        // ========================================================================

        /// <summary>
        /// Allowed image types (e.g., ".jpg,.jpeg,.png,.gif")
        /// </summary>
        public string? AllowedImageTypes { get; set; }

        /// <summary>
        /// Maximum image width in pixels
        /// </summary>
        public int? MaxImageWidth { get; set; }

        /// <summary>
        /// Maximum image height in pixels
        /// </summary>
        public int? MaxImageHeight { get; set; }

        /// <summary>
        /// Enable image cropping
        /// </summary>
        public bool EnableImageCrop { get; set; } = false;

        /// <summary>
        /// Required aspect ratio for cropping (e.g., "16:9", "1:1")
        /// </summary>
        public string? ImageAspectRatio { get; set; }

        // ========================================================================
        // SIGNATURE FIELD PROPERTIES
        // ========================================================================

        /// <summary>
        /// Signature canvas width in pixels
        /// </summary>
        public int SignatureWidth { get; set; } = 400;

        /// <summary>
        /// Signature canvas height in pixels
        /// </summary>
        public int SignatureHeight { get; set; } = 150;

        /// <summary>
        /// Signature pen color
        /// </summary>
        public string SignaturePenColor { get; set; } = "#000000";

        /// <summary>
        /// Signature pen width
        /// </summary>
        public int SignaturePenWidth { get; set; } = 2;

        // ========================================================================
        // DROPDOWN/SELECT PROPERTIES
        // ========================================================================

        /// <summary>
        /// Enable search/filter in dropdown
        /// </summary>
        public bool EnableSearch { get; set; } = false;

        /// <summary>
        /// Placeholder text for dropdown
        /// </summary>
        public string? DropdownPlaceholder { get; set; }

        /// <summary>
        /// Allow clearing selection
        /// </summary>
        public bool AllowClear { get; set; } = false;

        /// <summary>
        /// Load options dynamically via API
        /// </summary>
        public bool IsDynamicOptions { get; set; } = false;

        /// <summary>
        /// API endpoint for dynamic options
        /// </summary>
        public string? OptionsApiUrl { get; set; }

        /// <summary>
        /// Parent field ID for cascading dropdowns
        /// </summary>
        public string? CascadeParentFieldId { get; set; }

        // ========================================================================
        // MULTISELECT PROPERTIES
        // ========================================================================

        /// <summary>
        /// Minimum selections required
        /// </summary>
        public int? MinSelections { get; set; }

        /// <summary>
        /// Maximum selections allowed
        /// </summary>
        public int? MaxSelections { get; set; }

        // ========================================================================
        // PHONE FIELD PROPERTIES
        // ========================================================================

        /// <summary>
        /// Default country code for phone field
        /// </summary>
        public string? DefaultCountryCode { get; set; }

        /// <summary>
        /// Phone number format mask (e.g., "(###) ###-####")
        /// </summary>
        public string? PhoneFormat { get; set; }

        // ========================================================================
        // CURRENCY FIELD PROPERTIES
        // ========================================================================

        /// <summary>
        /// Currency code (e.g., "USD", "KES", "EUR")
        /// </summary>
        public string CurrencyCode { get; set; } = "KES";

        /// <summary>
        /// Currency symbol (e.g., "$", "KES", "€")
        /// </summary>
        public string? CurrencySymbol { get; set; }

        /// <summary>
        /// Show currency symbol as prefix or suffix
        /// </summary>
        public bool CurrencySymbolAsPrefix { get; set; } = true;

        // ========================================================================
        // ADVANCED DISPLAY PROPERTIES
        // ========================================================================

        /// <summary>
        /// Custom input mask pattern (e.g., "99/99/9999" for dates)
        /// </summary>
        public string? InputMask { get; set; }

        /// <summary>
        /// Auto-capitalize input: none, words, sentences, characters
        /// </summary>
        public string? AutoCapitalize { get; set; }

        /// <summary>
        /// Autocomplete attribute value
        /// </summary>
        public string? AutoComplete { get; set; }

        /// <summary>
        /// Spellcheck enabled
        /// </summary>
        public bool? SpellCheck { get; set; }

        /// <summary>
        /// Show character counter for text fields
        /// </summary>
        public bool ShowCharacterCount { get; set; } = false;

        /// <summary>
        /// Custom tooltip text
        /// </summary>
        public string? Tooltip { get; set; }

        /// <summary>
        /// Icon class to display with field (e.g., "ri-user-line")
        /// </summary>
        public string? IconClass { get; set; }

        /// <summary>
        /// Position of icon: prefix or suffix
        /// </summary>
        public string IconPosition { get; set; } = "prefix";

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
