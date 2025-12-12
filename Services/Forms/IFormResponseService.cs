using FormReporting.Models.Entities.Forms;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service interface for handling form responses (saving, validating, submitting)
    /// </summary>
    public interface IFormResponseService
    {
        /// <summary>
        /// Save or update responses for a submission
        /// </summary>
        /// <param name="submissionId">Submission ID</param>
        /// <param name="responses">Dictionary of ItemId to value</param>
        /// <returns>Number of responses saved</returns>
        Task<int> SaveResponsesAsync(int submissionId, Dictionary<int, string?> responses);

        /// <summary>
        /// Save draft with auto-save (updates LastSavedDate and CurrentSection)
        /// </summary>
        /// <param name="submissionId">Submission ID (0 for new)</param>
        /// <param name="templateId">Template ID (required if submissionId is 0)</param>
        /// <param name="userId">User saving the draft</param>
        /// <param name="tenantId">Optional tenant ID</param>
        /// <param name="reportingPeriod">Reporting period</param>
        /// <param name="responses">Dictionary of ItemId to value</param>
        /// <param name="currentSection">Current wizard section index</param>
        /// <returns>Auto-save result with submission ID and timestamp</returns>
        Task<AutoSaveResult> SaveDraftAsync(
            int submissionId,
            int templateId,
            int userId,
            int? tenantId,
            DateTime reportingPeriod,
            Dictionary<int, string?> responses,
            int currentSection);

        /// <summary>
        /// Finalize and submit a draft submission
        /// </summary>
        /// <param name="submissionId">Submission ID</param>
        /// <param name="userId">User submitting</param>
        /// <returns>Submit result with validation errors if any</returns>
        Task<SubmitResult> SubmitAsync(int submissionId, int userId);

        /// <summary>
        /// Validate all responses for a submission
        /// </summary>
        /// <param name="submissionId">Submission ID</param>
        /// <returns>Validation result with errors by field</returns>
        Task<ValidationResult> ValidateResponsesAsync(int submissionId);

        /// <summary>
        /// Delete a draft submission
        /// </summary>
        /// <param name="submissionId">Submission ID</param>
        /// <param name="userId">User requesting deletion (must own the draft)</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteDraftAsync(int submissionId, int userId);

        /// <summary>
        /// Get responses for a submission as a dictionary
        /// </summary>
        /// <param name="submissionId">Submission ID</param>
        /// <returns>Dictionary of ItemId to response value</returns>
        Task<Dictionary<int, object?>> GetResponsesAsync(int submissionId);
    }

    /// <summary>
    /// Result of auto-save operation
    /// </summary>
    public class AutoSaveResult
    {
        public bool Success { get; set; }
        public int SubmissionId { get; set; }
        public DateTime SavedAt { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Result of submit operation
    /// </summary>
    public class SubmitResult
    {
        public bool Success { get; set; }
        public int SubmissionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public Dictionary<string, List<string>> ValidationErrors { get; set; } = new();
    }

    /// <summary>
    /// Result of validation operation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; } = new();
        public int TotalFields { get; set; }
        public int ValidFields { get; set; }
        public int InvalidFields { get; set; }
    }
}
