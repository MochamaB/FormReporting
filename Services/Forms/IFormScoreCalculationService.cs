using FormReporting.Models.ViewModels.Forms;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service for calculating scores from form responses
    /// Handles field, section, and template score calculations with weighted averages
    /// </summary>
    public interface IFormScoreCalculationService
    {
        /// <summary>
        /// Calculate average score for a specific field across all submissions
        /// </summary>
        /// <param name="itemId">Field/Item ID</param>
        /// <param name="templateId">Optional template filter</param>
        /// <param name="startDate">Optional date range start</param>
        /// <param name="endDate">Optional date range end</param>
        /// <returns>Average weighted score for the field</returns>
        Task<decimal?> GetFieldAverageScoreAsync(int itemId, int? templateId = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Calculate average score for a specific field within a single submission
        /// </summary>
        /// <param name="submissionId">Submission ID</param>
        /// <param name="itemId">Field/Item ID</param>
        /// <returns>Weighted score for the field in this submission</returns>
        Task<decimal?> GetFieldScoreForSubmissionAsync(int submissionId, int itemId);

        /// <summary>
        /// Calculate section score using weighted average of field scores
        /// </summary>
        /// <param name="submissionId">Submission ID</param>
        /// <param name="sectionId">Section ID</param>
        /// <returns>Weighted average score for the section</returns>
        Task<decimal?> GetSectionScoreAsync(int submissionId, int sectionId);

        /// <summary>
        /// Calculate section average score across multiple submissions
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="templateId">Optional template filter</param>
        /// <param name="startDate">Optional date range start</param>
        /// <param name="endDate">Optional date range end</param>
        /// <returns>Average section score across submissions</returns>
        Task<decimal?> GetSectionAverageScoreAsync(int sectionId, int? templateId = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Calculate template overall score using weighted average of section scores
        /// </summary>
        /// <param name="submissionId">Submission ID</param>
        /// <returns>Weighted average overall score for the template submission</returns>
        Task<decimal?> GetTemplateOverallScoreAsync(int submissionId);

        /// <summary>
        /// Calculate template average score across multiple submissions
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="startDate">Optional date range start</param>
        /// <param name="endDate">Optional date range end</param>
        /// <returns>Average template score across submissions</returns>
        Task<decimal?> GetTemplateAverageScoreAsync(int templateId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get detailed score breakdown for a submission (all fields, sections, overall)
        /// </summary>
        /// <param name="submissionId">Submission ID</param>
        /// <returns>Complete score breakdown view model</returns>
        Task<SubmissionScoreBreakdownViewModel> GetSubmissionScoreBreakdownAsync(int submissionId);

        /// <summary>
        /// Get field performance statistics across submissions
        /// </summary>
        /// <param name="itemId">Field/Item ID</param>
        /// <param name="templateId">Optional template filter</param>
        /// <param name="startDate">Optional date range start</param>
        /// <param name="endDate">Optional date range end</param>
        /// <returns>Field performance statistics</returns>
        Task<FieldPerformanceViewModel> GetFieldPerformanceAsync(int itemId, int? templateId = null, DateTime? startDate = null, DateTime? endDate = null);
    }
}
