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

        // Field-Specific Configuration Properties
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
