namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service interface for FormTemplate operations
    /// Provides methods for template code generation and validation
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
    }
}
