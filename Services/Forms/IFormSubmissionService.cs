using FormReporting.Models.Entities.Forms;
using FormReporting.Models.ViewModels.Components;
using FormReporting.Models.ViewModels.Forms;
using System.Security.Claims;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service interface for form submission operations
    /// Handles building ViewModels, creating submissions, and access control
    /// </summary>
    public interface IFormSubmissionService
    {
        /// <summary>
        /// Build a FormViewModel from a template for rendering
        /// If submissionId is provided, loads existing responses for resume/edit
        /// </summary>
        /// <param name="templateId">Template to render</param>
        /// <param name="submissionId">Optional existing submission for resume</param>
        /// <param name="readOnly">If true, renders in read-only mode</param>
        /// <returns>FormViewModel ready for rendering</returns>
        Task<FormViewModel> BuildFormViewModelAsync(int templateId, int? submissionId = null, bool readOnly = false);

        /// <summary>
        /// Create a new draft submission for a template
        /// </summary>
        /// <param name="templateId">Template to create submission for</param>
        /// <param name="userId">User creating the submission</param>
        /// <param name="tenantId">Optional tenant (for location-based forms)</param>
        /// <param name="reportingPeriod">Reporting period for the submission</param>
        /// <returns>Created submission entity</returns>
        Task<FormTemplateSubmission> CreateSubmissionAsync(int templateId, int userId, int? tenantId, DateTime reportingPeriod);

        /// <summary>
        /// Get an existing submission by ID
        /// </summary>
        /// <param name="submissionId">Submission ID</param>
        /// <returns>Submission entity with related data</returns>
        Task<FormTemplateSubmission?> GetSubmissionAsync(int submissionId);

        /// <summary>
        /// Get all submissions for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="status">Optional status filter</param>
        /// <returns>List of user's submissions</returns>
        Task<List<FormTemplateSubmission>> GetUserSubmissionsAsync(int userId, string? status = null);

        /// <summary>
        /// Get all submissions for a template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="status">Optional status filter</param>
        /// <returns>List of submissions for the template</returns>
        Task<List<FormTemplateSubmission>> GetTemplateSubmissionsAsync(int templateId, string? status = null);

        /// <summary>
        /// Check if user can access a template (basic check)
        /// Will be expanded for assignment-based access control later
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="templateId">Template ID</param>
        /// <returns>True if user can access</returns>
        Task<bool> CanUserAccessTemplateAsync(int userId, int templateId);

        /// <summary>
        /// Check if user owns a submission (for resume/edit)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="submissionId">Submission ID</param>
        /// <returns>True if user owns the submission</returns>
        Task<bool> UserOwnsSubmissionAsync(int userId, int submissionId);

        /// <summary>
        /// Get available templates for a user to fill
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of published templates user can access</returns>
        Task<List<FormTemplate>> GetAvailableTemplatesAsync(int userId);

        /// <summary>
        /// Check if user has an existing draft for a template/period combination
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="templateId">Template ID</param>
        /// <param name="tenantId">Optional tenant ID</param>
        /// <param name="reportingPeriod">Reporting period</param>
        /// <returns>Existing draft submission if found</returns>
        Task<FormTemplateSubmission?> GetExistingDraftAsync(int userId, int templateId, int? tenantId, DateTime reportingPeriod);

        /// <summary>
        /// Get tenants by their IDs
        /// </summary>
        /// <param name="tenantIds">List of tenant IDs</param>
        /// <returns>List of tenants</returns>
        Task<List<FormReporting.Models.Entities.Organizational.Tenant>> GetTenantsAsync(List<int> tenantIds);

        /// <summary>
        /// Get submission statistics for templates (for a specific user)
        /// Returns counts of total, submitted, and draft submissions per template
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="templateIds">List of template IDs to get stats for</param>
        /// <returns>Dictionary with templateId as key and stats object as value</returns>
        Task<Dictionary<int, TemplateSubmissionStats>> GetTemplateSubmissionStatsAsync(int userId, List<int> templateIds);

        // ========================================================================
        // SCOPE-AWARE METHODS (for viewing submissions across tenants)
        // ========================================================================

        /// <summary>
        /// Get paginated submissions for a template, filtered by user's scope
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="user">Current user's claims principal</param>
        /// <param name="filters">Filter criteria</param>
        /// <param name="page">Page number (1-indexed)</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>ViewModel with template info, submissions, and pagination</returns>
        Task<TemplateSubmissionsViewModel> GetScopedTemplateSubmissionsAsync(
            int templateId,
            ClaimsPrincipal user,
            SubmissionFilters? filters = null,
            int page = 1,
            int pageSize = 10);

        /// <summary>
        /// Get detailed submission information for offcanvas display
        /// Includes all responses grouped by section
        /// </summary>
        /// <param name="submissionId">Submission ID</param>
        /// <param name="user">Current user's claims principal (for access check)</param>
        /// <returns>Detailed submission ViewModel or null if not found/not accessible</returns>
        Task<SubmissionDetailViewModel?> GetSubmissionDetailAsync(int submissionId, ClaimsPrincipal user);

        /// <summary>
        /// Get scope-filtered submission statistics for templates
        /// Used for displaying counts on form cards based on user's scope
        /// </summary>
        /// <param name="user">Current user's claims principal</param>
        /// <param name="templateIds">List of template IDs</param>
        /// <returns>Dictionary with templateId as key and stats as value</returns>
        Task<Dictionary<int, TemplateSubmissionStats>> GetScopedTemplateStatsAsync(
            ClaimsPrincipal user,
            List<int> templateIds);

        /// <summary>
        /// Check if user can access a specific submission based on their scope
        /// </summary>
        /// <param name="submissionId">Submission ID</param>
        /// <param name="user">Current user's claims principal</param>
        /// <returns>True if user can access the submission</returns>
        Task<bool> CanUserAccessSubmissionAsync(int submissionId, ClaimsPrincipal user);

        /// <summary>
        /// Get submissions data for export (CSV/Excel)
        /// Returns all submissions matching filters without pagination
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="user">Current user's claims principal</param>
        /// <param name="filters">Filter criteria</param>
        /// <returns>Export data with headers and rows</returns>
        Task<SubmissionExportData> GetSubmissionsForExportAsync(
            int templateId,
            ClaimsPrincipal user,
            SubmissionFilters? filters = null);
    }

    /// <summary>
    /// Data structure for exporting submissions
    /// </summary>
    public class SubmissionExportData
    {
        public string TemplateName { get; set; } = string.Empty;
        public List<string> Headers { get; set; } = new();
        public List<List<string>> Rows { get; set; } = new();
    }

    /// <summary>
    /// Submission statistics for a template
    /// </summary>
    public class TemplateSubmissionStats
    {
        public int TotalResponses { get; set; }
        public int SubmittedCount { get; set; }
        public int DraftCount { get; set; }
    }
}
