using FormReporting.Models.Common;

namespace FormReporting.Models.ViewModels.Forms
{
    /// <summary>
    /// Main view model for the Form Builder interface
    /// Contains template data and all sections/fields for editing
    /// </summary>
    public class FormBuilderViewModel
    {
        /// <summary>
        /// Template ID being edited
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// Template name
        /// </summary>
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// Template code
        /// </summary>
        public string TemplateCode { get; set; } = string.Empty;

        /// <summary>
        /// Template version
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Current publish status (Draft, Published)
        /// </summary>
        public string PublishStatus { get; set; } = "Draft";

        /// <summary>
        /// Template category ID
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Category name for display
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>
        /// Template description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// List of sections in this template
        /// </summary>
        public List<SectionDto> Sections { get; set; } = new();

        /// <summary>
        /// Available field types for the palette
        /// </summary>
        public List<FieldTypeDto> AvailableFieldTypes { get; set; } = new();

        /// <summary>
        /// Total number of fields across all sections
        /// </summary>
        public int TotalFields => Sections.Sum(s => s.FieldCount);

        /// <summary>
        /// Is this template editable? (only drafts can be edited)
        /// </summary>
        public bool IsEditable => PublishStatus == "Draft";
    }

    /// <summary>
    /// Field type definition for palette
    /// </summary>
    public class FieldTypeDto
    {
        public FormFieldType FieldType { get; set; }                // Enum value (Text, Number, etc.)
        public string DisplayName { get; set; } = string.Empty;     // "Text Input"
        public string Icon { get; set; } = string.Empty;            // "ri-text"
        public string Description { get; set; } = string.Empty;     // "Single-line text input"
        public string Category { get; set; } = string.Empty;        // "Basic", "Choice", "Advanced"
    }
}
