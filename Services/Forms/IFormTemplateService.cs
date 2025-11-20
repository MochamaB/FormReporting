using FormReporting.Models.Entities.Forms;
using FormReporting.Models.ViewModels.Forms;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service interface for FormTemplate operations
    /// Provides methods for template code generation, validation, and progress tracking
    /// </summary>
    public interface IFormTemplateService
    {
        /// <summary>
        /// Generate a unique template code from template name
        /// Ensures uniqueness by adding numeric suffix if needed
        /// </summary>
        /// <param name="templateName">Template name to generate code from</param>
        /// <param name="excludeTemplateId">Template ID to exclude from uniqueness check (for edits)</param>
        /// <returns>Unique template code</returns>
        Task<string> GenerateUniqueTemplateCodeAsync(string templateName, int? excludeTemplateId = null);

        /// <summary>
        /// Check if template code already exists
        /// </summary>
        /// <param name="templateCode">Template code to check</param>
        /// <param name="excludeTemplateId">Template ID to exclude from check (for edits)</param>
        /// <returns>True if code exists, false otherwise</returns>
        Task<bool> TemplateCodeExistsAsync(string templateCode, int? excludeTemplateId = null);

        /// <summary>
        /// Generate template code from template name (without uniqueness check)
        /// </summary>
        /// <param name="templateName">Template name</param>
        /// <returns>Generated template code</returns>
        string GenerateTemplateCode(string templateName);

        /// <summary>
        /// Validate template code format
        /// </summary>
        /// <param name="templateCode">Template code to validate</param>
        /// <returns>True if valid format, false otherwise</returns>
        bool IsValidTemplateCodeFormat(string templateCode);

        /// <summary>
        /// Load template with all related entities for editing/resume
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <returns>FormTemplate with related data, or null if not found</returns>
        Task<FormTemplate?> LoadTemplateForEditingAsync(int templateId);

        /// <summary>
        /// Analyze template progress and determine current step for resume functionality
        /// </summary>
        /// <param name="template">FormTemplate to analyze</param>
        /// <returns>Resume information with current step and completion status</returns>
        FormBuilderResumeInfo AnalyzeTemplateProgress(FormTemplate template);

        /// <summary>
        /// Create a new version from a published template for editing
        /// Copies all data (sections, items, assignments) and increments version number
        /// </summary>
        /// <param name="publishedTemplateId">ID of published template to version</param>
        /// <param name="userId">User creating the new version</param>
        /// <returns>New template version as Draft</returns>
        Task<FormTemplate> CreateNewVersionAsync(int publishedTemplateId, int userId);

        /// <summary>
        /// Check if template can have a new version created (must be published)
        /// </summary>
        /// <param name="template">Template to check</param>
        /// <returns>True if can create version, false otherwise</returns>
        bool CanCreateVersion(FormTemplate template);
    }
}
