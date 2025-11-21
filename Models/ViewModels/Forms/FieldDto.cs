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
        public string? ItemCode { get; set; } // Optional - will be auto-generated if not provided
        public FormFieldType DataType { get; set; } = FormFieldType.Text;
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; } // Optional - will be auto-calculated if 0
        public string? PlaceholderText { get; set; }
        public string? HelpText { get; set; }
        public string? DefaultValue { get; set; }
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
    }
}
