using FormReporting.Models.ViewModels.Forms;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service interface for Form Builder operations
    /// Handles loading and validating template data for the builder UI
    /// </summary>
    public interface IFormBuilderService
    {
        /// <summary>
        /// Load complete template data for form builder
        /// Includes sections, fields, validations, options
        /// </summary>
        /// <param name="templateId">Template ID to load</param>
        /// <returns>Complete builder view model</returns>
        Task<FormBuilderViewModel?> LoadForBuilderAsync(int templateId);

        /// <summary>
        /// Get available field types for the palette
        /// </summary>
        /// <returns>List of field type definitions</returns>
        List<FieldTypeDto> GetAvailableFieldTypes();

        /// <summary>
        /// Validate template structure before allowing progression
        /// </summary>
        /// <param name="templateId">Template to validate</param>
        /// <returns>Validation result with errors if any</returns>
        Task<(bool IsValid, List<string> Errors)> ValidateTemplateStructureAsync(int templateId);

        /// <summary>
        /// Generate unique field code for a section
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="fieldName">Optional field name for context</param>
        /// <returns>Generated code (e.g., "SEC1_001")</returns>
        Task<string> GenerateFieldCodeAsync(int sectionId, string? fieldName = null);

        /// <summary>
        /// Add a new section to a template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="dto">Section creation data</param>
        /// <returns>Created section DTO</returns>
        Task<SectionDto?> AddSectionAsync(int templateId, CreateSectionDto dto);

        /// <summary>
        /// Get section by ID for editing
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <returns>Section DTO or null if not found</returns>
        Task<SectionDto?> GetSectionByIdAsync(int sectionId);

        /// <summary>
        /// Update section properties
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="dto">Updated section data</param>
        /// <returns>True if successful</returns>
        Task<bool> UpdateSectionAsync(int sectionId, UpdateSectionDto dto);

        /// <summary>
        /// Delete a section and all its fields
        /// </summary>
        /// <param name="sectionId">Section ID to delete</param>
        /// <returns>True if successful</returns>
        Task<bool> DeleteSectionAsync(int sectionId);

        /// <summary>
        /// Duplicate a section with all its fields
        /// </summary>
        /// <param name="sectionId">Section ID to duplicate</param>
        /// <returns>Duplicated section DTO or null if failed</returns>
        Task<SectionDto?> DuplicateSectionAsync(int sectionId);

        /// <summary>
        /// Update display order of sections after drag-drop reordering
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="sections">List of section IDs with new display orders</param>
        /// <returns>True if successful</returns>
        Task<bool> ReorderSectionsAsync(int templateId, List<SectionOrderDto> sections);

        /// <summary>
        /// Add a new field to a section
        /// </summary>
        /// <param name="dto">Field creation data</param>
        /// <returns>Created field DTO or null if failed</returns>
        Task<FieldDto?> AddFieldAsync(CreateFieldDto dto);

        /// <summary>
        /// Get field by ID for editing
        /// </summary>
        /// <param name="fieldId">Field ID</param>
        /// <returns>Field DTO or null if not found</returns>
        Task<FieldDto?> GetFieldByIdAsync(int fieldId);

        /// <summary>
        /// Update field properties
        /// </summary>
        /// <param name="fieldId">Field ID</param>
        /// <param name="dto">Updated field data</param>
        /// <returns>True if successful</returns>
        Task<bool> UpdateFieldAsync(int fieldId, UpdateFieldDto dto);

        /// <summary>
        /// Update field type with smart option handling
        /// Auto-creates default options when changing to selection fields (Dropdown, Radio, etc.)
        /// Preserves existing options when changing types (data preservation)
        /// </summary>
        /// <param name="fieldId">Field ID</param>
        /// <param name="newType">New field type (e.g., "Dropdown", "Text", etc.)</param>
        /// <returns>True if successful</returns>
        Task<bool> UpdateFieldTypeAsync(int fieldId, string newType);

        /// <summary>
        /// Move a field to a different section (cross-section drag)
        /// </summary>
        /// <param name="fieldId">Field ID to move</param>
        /// <param name="targetSectionId">Target section ID</param>
        /// <returns>True if successful</returns>
        Task<bool> MoveFieldToSectionAsync(int fieldId, int targetSectionId);

        /// <summary>
        /// Delete a field
        /// </summary>
        /// <param name="fieldId">Field ID to delete</param>
        /// <returns>True if successful</returns>
        Task<bool> DeleteFieldAsync(int fieldId);

        /// <summary>
        /// Duplicate a field with all its settings
        /// </summary>
        /// <param name="fieldId">Field ID to duplicate</param>
        /// <returns>Duplicated field DTO or null if failed</returns>
        Task<FieldDto?> DuplicateFieldAsync(int fieldId);

        /// <summary>
        /// Update display order of fields after drag-drop reordering
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="fields">List of field IDs with new display orders</param>
        /// <returns>True if successful</returns>
        Task<bool> ReorderFieldsAsync(int sectionId, List<FieldOrderDto> fields);

        // ========================================================================
        // OPTIONS MANAGEMENT
        // ========================================================================

        /// <summary>
        /// Add a new option to a field
        /// Auto-generates option value from label if not provided
        /// Ensures unique option values within the field
        /// </summary>
        /// <param name="fieldId">Field ID to add option to</param>
        /// <param name="dto">Option data (OptionId should be 0 for new)</param>
        /// <returns>Created option with new OptionId, or null if failed</returns>
        Task<FieldOptionDto?> AddOptionAsync(int fieldId, FieldOptionDto dto);

        /// <summary>
        /// Update an existing option
        /// Validates unique option values within the field
        /// </summary>
        /// <param name="optionId">Option ID to update</param>
        /// <param name="dto">Updated option data</param>
        /// <returns>True if successful</returns>
        Task<bool> UpdateOptionAsync(int optionId, FieldOptionDto dto);

        /// <summary>
        /// Delete an option
        /// Enforces minimum 2 options rule for selection fields
        /// </summary>
        /// <param name="optionId">Option ID to delete</param>
        /// <returns>True if successful</returns>
        /// <exception cref="InvalidOperationException">If deleting would leave less than 2 options</exception>
        Task<bool> DeleteOptionAsync(int optionId);

        /// <summary>
        /// Reorder options within a field via drag-drop
        /// Updates display order for all provided options
        /// </summary>
        /// <param name="fieldId">Field ID containing the options</param>
        /// <param name="updates">List of option IDs with new display orders</param>
        /// <returns>True if successful</returns>
        Task<bool> ReorderOptionsAsync(int fieldId, List<ReorderOptionDto> updates);

        /// <summary>
        /// Set an option as default (pre-selected)
        /// For single-select fields (Dropdown, Radio): unsets all other defaults
        /// For multi-select fields (Checkbox, MultiSelect): toggles this option independently
        /// </summary>
        /// <param name="optionId">Option ID to set as default</param>
        /// <param name="fieldId">Parent field ID (to determine single vs multi-select)</param>
        /// <returns>True if successful</returns>
        Task<bool> SetDefaultOptionAsync(int optionId, int fieldId);

        // ========================================================================
        // VALIDATION MANAGEMENT
        // ========================================================================

        /// <summary>
        /// Add a validation rule to a field
        /// </summary>
        /// <param name="fieldId">Field ID to add validation to</param>
        /// <param name="dto">Validation rule data</param>
        /// <returns>Created validation rule DTO</returns>
        Task<ValidationRuleDto?> AddValidationAsync(int fieldId, CreateValidationDto dto);

        /// <summary>
        /// Update an existing validation rule
        /// </summary>
        /// <param name="validationId">Validation ID to update</param>
        /// <param name="dto">Updated validation data</param>
        /// <returns>True if successful</returns>
        Task<bool> UpdateValidationAsync(int validationId, UpdateValidationDto dto);

        /// <summary>
        /// Delete a validation rule
        /// </summary>
        /// <param name="validationId">Validation ID to delete</param>
        /// <returns>True if successful</returns>
        Task<bool> DeleteValidationAsync(int validationId);

        /// <summary>
        /// Reorder validation rules within a field
        /// </summary>
        /// <param name="fieldId">Field ID containing the validations</param>
        /// <param name="updates">List of validation IDs with new orders</param>
        /// <returns>True if successful</returns>
        Task<bool> ReorderValidationsAsync(int fieldId, List<ReorderValidationDto> updates);

        /// <summary>
        /// Get all validation rules for a field
        /// </summary>
        /// <param name="fieldId">Field ID</param>
        /// <returns>List of validation rules</returns>
        Task<List<ValidationRuleDto>> GetValidationsForFieldAsync(int fieldId);
    }

    /// <summary>
    /// DTO for section reordering
    /// </summary>
    public class SectionOrderDto
    {
        public int SectionId { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// DTO for field reordering
    /// </summary>
    public class FieldOrderDto
    {
        public int ItemId { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// DTO for option reordering (within a field)
    /// </summary>
    public class ReorderOptionDto
    {
        public int OptionId { get; set; }
        public int DisplayOrder { get; set; }
    }
}
