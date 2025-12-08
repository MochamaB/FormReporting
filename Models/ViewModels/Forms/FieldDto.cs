using FormReporting.Models.Common;

namespace FormReporting.Models.ViewModels.Forms
{
    /// <summary>
    /// Field data transfer object for Form Builder
    /// Represents a field/question in the form
    /// </summary>
    public class FieldDto
    {
        /// <summary>
        /// Field ID (database primary key)
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Parent section ID
        /// </summary>
        public int SectionId { get; set; }

        /// <summary>
        /// Field code (auto-generated, e.g., "SEC1_001")
        /// </summary>
        public string ItemCode { get; set; } = string.Empty;

        /// <summary>
        /// Field name/question text
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// Optional field description
        /// </summary>
        public string? ItemDescription { get; set; }

        /// <summary>
        /// Data type (Text, Number, Date, Dropdown, etc.)
        /// </summary>
        public FormFieldType DataType { get; set; } = FormFieldType.Text;

        /// <summary>
        /// Data type as string name (for display purposes)
        /// </summary>
        public string DataTypeName => DataType.ToString();

        /// <summary>
        /// Is this field required?
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Display order within section
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Placeholder text for input
        /// </summary>
        public string? PlaceholderText { get; set; }

        /// <summary>
        /// Help text shown as tooltip/guidance
        /// </summary>
        public string? HelpText { get; set; }

        /// <summary>
        /// Prefix text (e.g., "KES" before input)
        /// </summary>
        public string? PrefixText { get; set; }

        /// <summary>
        /// Suffix text (e.g., "%" after input)
        /// </summary>
        public string? SuffixText { get; set; }

        /// <summary>
        /// Default value
        /// </summary>
        public string? DefaultValue { get; set; }

        // ========================================================================
        // FIELD-SPECIFIC CONFIGURATION PROPERTIES
        // These are loaded from FormItemConfiguration key-value store
        // ========================================================================

        /// <summary>
        /// Minimum numeric value (for Number, Decimal, Currency, Percentage fields)
        /// </summary>
        public decimal? MinValue { get; set; }

        /// <summary>
        /// Maximum numeric value (for Number, Decimal, Currency, Percentage fields)
        /// </summary>
        public decimal? MaxValue { get; set; }

        /// <summary>
        /// Step/increment value (for Number, Decimal fields)
        /// Example: 0.5, 1, 10
        /// </summary>
        public decimal? Step { get; set; }

        /// <summary>
        /// Number of decimal places allowed (for Decimal, Currency, Percentage fields)
        /// </summary>
        public int? DecimalPlaces { get; set; }

        /// <summary>
        /// Allow negative numbers (for Number, Decimal fields)
        /// </summary>
        public bool? AllowNegative { get; set; }

        /// <summary>
        /// Minimum text length (for Text, TextArea, Email, Phone, URL fields)
        /// </summary>
        public int? MinLength { get; set; }

        /// <summary>
        /// Maximum text length (for Text, TextArea, Email, Phone, URL fields)
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Input mask pattern (for Text, Phone fields)
        /// Example: "(###) ###-####"
        /// </summary>
        public string? InputMask { get; set; }

        /// <summary>
        /// Text transform (None, Uppercase, Lowercase, Capitalize)
        /// </summary>
        public string? TextTransform { get; set; }

        /// <summary>
        /// Auto-trim whitespace from start/end
        /// </summary>
        public bool? AutoTrim { get; set; }

        /// <summary>
        /// Number of visible rows (for TextArea fields)
        /// Default: 3
        /// </summary>
        public int? Rows { get; set; }

        // ========================================================================
        // DATE/TIME FIELD CONFIGURATION PROPERTIES
        // ========================================================================

        /// <summary>
        /// Minimum date allowed (for Date, DateTime fields)
        /// Format: yyyy-MM-dd
        /// </summary>
        public string? MinDate { get; set; }

        /// <summary>
        /// Maximum date allowed (for Date, DateTime fields)
        /// Format: yyyy-MM-dd
        /// </summary>
        public string? MaxDate { get; set; }

        /// <summary>
        /// Minimum time allowed (for Time, DateTime fields)
        /// Format: HH:mm
        /// </summary>
        public string? MinTime { get; set; }

        /// <summary>
        /// Maximum time allowed (for Time, DateTime fields)
        /// Format: HH:mm
        /// </summary>
        public string? MaxTime { get; set; }

        /// <summary>
        /// Disable past dates (for Date, DateTime fields)
        /// </summary>
        public bool? DisablePastDates { get; set; }

        /// <summary>
        /// Disable future dates (for Date, DateTime fields)
        /// </summary>
        public bool? DisableFutureDates { get; set; }

        /// <summary>
        /// Default to today's date/current time
        /// </summary>
        public bool? DefaultToToday { get; set; }

        // ========================================================================
        // MEDIA FIELD CONFIGURATION PROPERTIES
        // ========================================================================

        // FileUpload Configuration
        public string? AllowedFileTypes { get; set; }
        public int? MaxFileSize { get; set; } // In MB
        public int? MinFileSize { get; set; } // In KB
        public int? MaxFiles { get; set; }
        public bool? AllowMultiple { get; set; }
        public bool? PreserveFileName { get; set; }

        // Image Configuration
        public string? AllowedImageTypes { get; set; }
        public int? ImageQuality { get; set; } // 10-100%
        public int? MaxWidth { get; set; } // In pixels
        public int? MaxHeight { get; set; } // In pixels
        public int? MinWidth { get; set; } // In pixels
        public int? MinHeight { get; set; } // In pixels
        public string? AspectRatio { get; set; } // e.g., "16:9", "1:1"
        public string? ThumbnailSize { get; set; } // e.g., "150x150"
        public bool? AllowCropping { get; set; }
        public bool? AutoResize { get; set; }

        // Signature Configuration
        public int? CanvasWidth { get; set; } // In pixels
        public int? CanvasHeight { get; set; } // In pixels
        public string? PenColor { get; set; } // Hex color
        public int? PenWidth { get; set; } // In pixels
        public string? BackgroundColor { get; set; } // Hex color
        public string? OutputFormat { get; set; } // PNG, JPEG, SVG, Base64
        public bool? ShowClearButton { get; set; }
        public bool? ShowUndoButton { get; set; }
        public bool? RequireFullName { get; set; }
        public bool? ShowDateStamp { get; set; }

        // ========================================================================
        // RATING FIELD CONFIGURATION
        // ========================================================================
        public int? RatingMax { get; set; } // 3, 5, 7, 10
        public string? RatingIcon { get; set; } // star, heart, thumb, circle, number
        public string? RatingActiveColor { get; set; } // Hex color
        public string? RatingInactiveColor { get; set; } // Hex color
        public string? RatingSize { get; set; } // sm, md, lg, xl
        public bool? AllowHalfRating { get; set; }
        public bool? ShowRatingValue { get; set; }
        public bool? ShowRatingLabels { get; set; }
        public bool? AllowClearRating { get; set; }

        // ========================================================================
        // SLIDER FIELD CONFIGURATION
        // ========================================================================
        public decimal? SliderMin { get; set; }
        public decimal? SliderMax { get; set; }
        public decimal? SliderStep { get; set; }
        public decimal? SliderDefault { get; set; }
        public string? SliderUnit { get; set; } // %, kg, km, etc.
        public string? SliderPrefix { get; set; } // $, £, etc.
        public string? SliderTrackColor { get; set; } // Hex color
        public bool? ShowSliderValue { get; set; }
        public bool? ShowSliderTicks { get; set; }
        public bool? ShowMinMaxLabels { get; set; }
        public bool? ShowSliderInput { get; set; }

        // ========================================================================
        // CURRENCY FIELD CONFIGURATION
        // ========================================================================
        public string? CurrencyCode { get; set; } // KES, USD, EUR, etc.
        public string? CurrencySymbol { get; set; } // KSh, $, €, etc.
        public string? CurrencyPosition { get; set; } // prefix, suffix
        public int? CurrencyDecimals { get; set; } // 0, 2, 3, 4
        public string? ThousandSeparator { get; set; } // , . or space
        public string? DecimalSeparator { get; set; } // . or ,
        public decimal? CurrencyMin { get; set; }
        public decimal? CurrencyMax { get; set; }
        public bool? AllowNegativeCurrency { get; set; }

        // ========================================================================
        // PERCENTAGE FIELD CONFIGURATION
        // ========================================================================
        public decimal? PercentageMin { get; set; }
        public decimal? PercentageMax { get; set; }
        public int? PercentageDecimals { get; set; }
        public decimal? PercentageStep { get; set; }
        public bool? ShowPercentSymbol { get; set; }
        public bool? AllowOverHundred { get; set; }
        public bool? ShowAsSlider { get; set; }
        public bool? ShowProgressBar { get; set; }

        // ========================================================================
        // EMAIL FIELD CONFIGURATION
        // ========================================================================
        public bool? AllowMultipleEmails { get; set; }
        public string? AllowedEmailDomains { get; set; }
        public string? BlockedEmailDomains { get; set; }

        // ========================================================================
        // PHONE FIELD CONFIGURATION
        // ========================================================================
        public string? DefaultCountryCode { get; set; } // +254, +1, etc.
        public string? PhoneFormat { get; set; } // international, national, e164
        public bool? ShowCountrySelector { get; set; }
        public bool? ValidatePhoneFormat { get; set; }

        // ========================================================================
        // URL FIELD CONFIGURATION
        // ========================================================================
        public bool? AllowHttp { get; set; }
        public bool? AllowHttps { get; set; }
        public bool? AllowFtp { get; set; }
        public string? AllowedProtocols { get; set; }
        public string? AllowedUrlDomains { get; set; }
        public bool? RequireHttps { get; set; }
        public bool? ShowUrlPreview { get; set; }

        /// <summary>
        /// Conditional logic JSON
        /// </summary>
        public string? ConditionalLogic { get; set; }

        /// <summary>
        /// Number of validation rules
        /// </summary>
        public int ValidationCount { get; set; }

        /// <summary>
        /// Number of options (for dropdowns, etc.)
        /// </summary>
        public int OptionCount { get; set; }

        /// <summary>
        /// List of options (for Dropdown, Radio, Checkbox, MultiSelect)
        /// </summary>
        public List<FieldOptionDto> Options { get; set; } = new();

        /// <summary>
        /// Has conditional logic configured?
        /// </summary>
        public bool HasConditionalLogic => !string.IsNullOrEmpty(ConditionalLogic);

        /// <summary>
        /// Has validation rules?
        /// </summary>
        public bool HasValidation => ValidationCount > 0;
    }

    /// <summary>
    /// DTO for creating a new field
    /// </summary>
    public class CreateFieldDto
    {
        public int SectionId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemDescription { get; set; } // Optional description
        public string? ItemCode { get; set; } // Optional - will be auto-generated if not provided

        // Accept DataType as string from JSON, then convert to enum
        public string DataType { get; set; } = "Text";

        // Computed property to get the enum value
        public FormFieldType DataTypeEnum
        {
            get
            {
                if (Enum.TryParse<FormFieldType>(DataType, ignoreCase: true, out var result))
                    return result;
                return FormFieldType.Text; // Default fallback
            }
        }

        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; } // Optional - will be auto-calculated if 0
        public string? PlaceholderText { get; set; }
        public string? HelpText { get; set; }
        public string? DefaultValue { get; set; }
        
        /// <summary>
        /// Option template ID to use for selection fields (Dropdown, Radio, Checkbox, MultiSelect)
        /// If null or 0, default options (Option 1, 2, 3) will be created
        /// </summary>
        public int? OptionTemplateId { get; set; }
    }

    /// <summary>
    /// DTO for updating field type only (inline quick edit)
    /// </summary>
    public class UpdateFieldTypeDto
    {
        public string DataType { get; set; } = "Text";
    }

    /// <summary>
    /// DTO for moving a field to a different section (cross-section drag)
    /// </summary>
    public class MoveFieldToSectionDto
    {
        public int SectionId { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing field
    /// </summary>
    public class UpdateFieldDto
    {
        public string ItemName { get; set; } = string.Empty;
        public string? ItemDescription { get; set; }
        public bool IsRequired { get; set; }
        public string? PlaceholderText { get; set; }
        public string? HelpText { get; set; }
        public string? PrefixText { get; set; }
        public string? SuffixText { get; set; }
        public string? DefaultValue { get; set; }

        // Field-Specific Configuration Properties (Number/Text)
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal? Step { get; set; }
        public int? DecimalPlaces { get; set; }
        public bool? AllowNegative { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? InputMask { get; set; }
        public string? TextTransform { get; set; }
        public bool? AutoTrim { get; set; }
        public int? Rows { get; set; }

        // Date/Time Configuration Properties
        public string? MinDate { get; set; }
        public string? MaxDate { get; set; }
        public string? MinTime { get; set; }
        public string? MaxTime { get; set; }
        public bool? DisablePastDates { get; set; }
        public bool? DisableFutureDates { get; set; }
        public bool? DefaultToToday { get; set; }

        // FileUpload Configuration Properties
        public string? AllowedFileTypes { get; set; }
        public int? MaxFileSize { get; set; } // In MB
        public int? MinFileSize { get; set; } // In KB
        public int? MaxFiles { get; set; }
        public bool? AllowMultiple { get; set; }
        public bool? PreserveFileName { get; set; }

        // Image Configuration Properties
        public string? AllowedImageTypes { get; set; }
        public int? ImageQuality { get; set; } // 10-100%
        public int? MaxWidth { get; set; } // In pixels
        public int? MaxHeight { get; set; } // In pixels
        public int? MinWidth { get; set; } // In pixels
        public int? MinHeight { get; set; } // In pixels
        public string? AspectRatio { get; set; } // e.g., "16:9", "1:1"
        public string? ThumbnailSize { get; set; } // e.g., "150x150"
        public bool? AllowCropping { get; set; }
        public bool? AutoResize { get; set; }

        // Signature Configuration Properties
        public int? CanvasWidth { get; set; } // In pixels
        public int? CanvasHeight { get; set; } // In pixels
        public string? PenColor { get; set; } // Hex color
        public int? PenWidth { get; set; } // In pixels
        public string? BackgroundColor { get; set; } // Hex color
        public string? OutputFormat { get; set; } // PNG, JPEG, SVG, Base64
        public bool? ShowClearButton { get; set; }
        public bool? ShowUndoButton { get; set; }
        public bool? RequireFullName { get; set; }
        public bool? ShowDateStamp { get; set; }

        // Rating Configuration Properties
        public int? RatingMax { get; set; }
        public string? RatingIcon { get; set; }
        public string? RatingActiveColor { get; set; }
        public string? RatingInactiveColor { get; set; }
        public string? RatingSize { get; set; }
        public bool? AllowHalfRating { get; set; }
        public bool? ShowRatingValue { get; set; }
        public bool? ShowRatingLabels { get; set; }
        public bool? AllowClearRating { get; set; }

        // Slider Configuration Properties
        public decimal? SliderMin { get; set; }
        public decimal? SliderMax { get; set; }
        public decimal? SliderStep { get; set; }
        public decimal? SliderDefault { get; set; }
        public string? SliderUnit { get; set; }
        public string? SliderPrefix { get; set; }
        public string? SliderTrackColor { get; set; }
        public bool? ShowSliderValue { get; set; }
        public bool? ShowSliderTicks { get; set; }
        public bool? ShowMinMaxLabels { get; set; }
        public bool? ShowSliderInput { get; set; }

        // Currency Configuration Properties
        public string? CurrencyCode { get; set; }
        public string? CurrencySymbol { get; set; }
        public string? CurrencyPosition { get; set; }
        public int? CurrencyDecimals { get; set; }
        public string? ThousandSeparator { get; set; }
        public string? DecimalSeparator { get; set; }
        public decimal? CurrencyMin { get; set; }
        public decimal? CurrencyMax { get; set; }
        public bool? AllowNegativeCurrency { get; set; }

        // Percentage Configuration Properties
        public decimal? PercentageMin { get; set; }
        public decimal? PercentageMax { get; set; }
        public int? PercentageDecimals { get; set; }
        public decimal? PercentageStep { get; set; }
        public bool? ShowPercentSymbol { get; set; }
        public bool? AllowOverHundred { get; set; }
        public bool? ShowAsSlider { get; set; }
        public bool? ShowProgressBar { get; set; }

        // Email Configuration Properties
        public bool? AllowMultipleEmails { get; set; }
        public string? AllowedEmailDomains { get; set; }
        public string? BlockedEmailDomains { get; set; }

        // Phone Configuration Properties
        public string? DefaultCountryCode { get; set; }
        public string? PhoneFormat { get; set; }
        public bool? ShowCountrySelector { get; set; }
        public bool? ValidatePhoneFormat { get; set; }

        // URL Configuration Properties
        public bool? AllowHttp { get; set; }
        public bool? AllowHttps { get; set; }
        public bool? AllowFtp { get; set; }
        public string? AllowedProtocols { get; set; }
        public string? AllowedUrlDomains { get; set; }
        public bool? RequireHttps { get; set; }
        public bool? ShowUrlPreview { get; set; }
    }

    /// <summary>
    /// DTO for field options (used by Dropdown, Radio, Checkbox, MultiSelect)
    /// </summary>
    public class FieldOptionDto
    {
        public int OptionId { get; set; }
        public string OptionLabel { get; set; } = string.Empty;
        public string OptionValue { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ========================================================================
    // VALIDATION DTOs
    // ========================================================================

    /// <summary>
    /// DTO for field validation rules
    /// </summary>
    public class ValidationRuleDto
    {
        public int ValidationId { get; set; }
        public string ValidationType { get; set; } = string.Empty;
        
        // Inline parameters
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? RegexPattern { get; set; }
        public string? CustomExpression { get; set; }
        
        public int ValidationOrder { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string Severity { get; set; } = "Error";
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Display-friendly summary of the validation rule
        /// </summary>
        public string ValidationSummary => GetValidationSummary();
        
        private string GetValidationSummary()
        {
            return ValidationType switch
            {
                "Required" => "Required field",
                "MinLength" => $"Min {MinLength} characters",
                "MaxLength" => $"Max {MaxLength} characters",
                "Email" => "Valid email format",
                "Phone" => "Valid phone number",
                "URL" => "Valid URL format",
                "MinValue" => $"Min value: {MinValue}",
                "MaxValue" => $"Max value: {MaxValue}",
                "Range" => $"Range: {MinValue} - {MaxValue}",
                "Pattern" => $"Pattern: {RegexPattern}",
                "Integer" => "Must be an integer",
                "Decimal" => "Must be a decimal number",
                "Date" => "Valid date format",
                "Number" => "Must be a number",
                _ => ValidationType
            };
        }
    }

    /// <summary>
    /// DTO for creating a new validation rule
    /// </summary>
    public class CreateValidationDto
    {
        public string ValidationType { get; set; } = string.Empty;
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? RegexPattern { get; set; }
        public string? CustomExpression { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string Severity { get; set; } = "Error";
    }

    /// <summary>
    /// DTO for updating an existing validation rule
    /// </summary>
    public class UpdateValidationDto
    {
        public string ValidationType { get; set; } = string.Empty;
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? RegexPattern { get; set; }
        public string? CustomExpression { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string Severity { get; set; } = "Error";
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for reordering validation rules
    /// </summary>
    public class ReorderValidationDto
    {
        public int ValidationId { get; set; }
        public int ValidationOrder { get; set; }
    }
}
