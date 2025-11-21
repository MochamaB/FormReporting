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
        /// Update display order of sections after drag-drop reordering
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="sections">List of section IDs with new display orders</param>
        /// <returns>True if successful</returns>
        Task<bool> ReorderSectionsAsync(int templateId, List<SectionOrderDto> sections);
    }

    /// <summary>
    /// DTO for section reordering
    /// </summary>
    public class SectionOrderDto
    {
        public int SectionId { get; set; }
        public int DisplayOrder { get; set; }
    }
}
