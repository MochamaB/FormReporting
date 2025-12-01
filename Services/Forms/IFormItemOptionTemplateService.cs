using FormReporting.Models.Entities.Forms;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service interface for FormItemOptionTemplate operations
    /// Provides methods for retrieving and managing option templates
    /// </summary>
    public interface IFormItemOptionTemplateService
    {
        /// <summary>
        /// Get all active templates ordered by display order
        /// </summary>
        /// <returns>List of active templates with their items</returns>
        Task<List<FormItemOptionTemplate>> GetActiveTemplatesAsync();

        /// <summary>
        /// Get templates filtered by category
        /// </summary>
        /// <param name="category">Category filter</param>
        /// <returns>List of templates in the specified category</returns>
        Task<List<FormItemOptionTemplate>> GetTemplatesByCategoryAsync(string category);

        /// <summary>
        /// Get templates filtered by field type
        /// </summary>
        /// <param name="fieldType">Field type (Radio, Dropdown, etc.)</param>
        /// <returns>List of templates applicable to the field type</returns>
        Task<List<FormItemOptionTemplate>> GetTemplatesByFieldTypeAsync(string fieldType);

        /// <summary>
        /// Get template by ID with items
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <returns>Template if found, null otherwise</returns>
        Task<FormItemOptionTemplate?> GetByIdWithItemsAsync(int templateId);

        /// <summary>
        /// Get template by code with items
        /// </summary>
        /// <param name="templateCode">Template code</param>
        /// <returns>Template if found, null otherwise</returns>
        Task<FormItemOptionTemplate?> GetByCodeWithItemsAsync(string templateCode);

        /// <summary>
        /// Get all templates (including inactive) with items
        /// </summary>
        /// <returns>List of all templates</returns>
        Task<List<FormItemOptionTemplate>> GetAllTemplatesWithItemsAsync();

        /// <summary>
        /// Get all unique categories
        /// </summary>
        /// <returns>List of distinct categories</returns>
        Task<List<string>> GetCategoriesAsync();

        /// <summary>
        /// Get templates for dropdown/select list
        /// </summary>
        /// <param name="fieldType">Optional field type filter</param>
        /// <returns>Anonymous objects for dropdown binding</returns>
        Task<List<object>> GetTemplateSelectListAsync(string? fieldType = null);

        /// <summary>
        /// Increment usage count for a template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        Task IncrementUsageCountAsync(int templateId);

        /// <summary>
        /// Check if template code exists
        /// </summary>
        /// <param name="templateCode">Template code</param>
        /// <param name="excludeTemplateId">Template ID to exclude from check (for edit)</param>
        /// <returns>True if code exists, false otherwise</returns>
        Task<bool> TemplateCodeExistsAsync(string templateCode, int? excludeTemplateId = null);

        /// <summary>
        /// Get templates with pagination and filtering
        /// </summary>
        /// <param name="search">Search term</param>
        /// <param name="category">Category filter</param>
        /// <param name="status">Status filter (active/inactive)</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Tuple of (templates, totalCount)</returns>
        Task<(List<FormItemOptionTemplate> templates, int totalCount)> GetTemplatesPagedAsync(
            string? search = null,
            string? category = null,
            string? status = null,
            int page = 1,
            int pageSize = 10);

        /// <summary>
        /// Create a new template
        /// </summary>
        /// <param name="template">Template to create</param>
        Task<FormItemOptionTemplate> CreateTemplateAsync(FormItemOptionTemplate template);

        /// <summary>
        /// Update an existing template
        /// </summary>
        /// <param name="template">Template to update</param>
        Task UpdateTemplateAsync(FormItemOptionTemplate template);

        /// <summary>
        /// Delete a template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        Task<bool> DeleteTemplateAsync(int templateId);
    }
}
