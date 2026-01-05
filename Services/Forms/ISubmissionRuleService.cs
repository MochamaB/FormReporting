using FormReporting.Models.ViewModels.Forms;
using FormReporting.Models.Entities.Forms;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service interface for managing form template submission rules
    /// </summary>
    public interface ISubmissionRuleService
    {
        // ===== CRUD OPERATIONS =====
        
        /// <summary>
        /// Get submission rule by ID with full details
        /// </summary>
        Task<SubmissionRuleDetailDto?> GetByIdAsync(int ruleId);
        
        /// <summary>
        /// Get all submission rules for a specific template
        /// </summary>
        Task<List<SubmissionRuleListDto>> GetByTemplateIdAsync(int templateId);
        
        /// <summary>
        /// Create a new submission rule
        /// </summary>
        Task<SubmissionRuleDetailDto> CreateAsync(SubmissionRuleCreateDto dto);
        
        /// <summary>
        /// Update an existing submission rule
        /// </summary>
        Task<SubmissionRuleDetailDto> UpdateAsync(SubmissionRuleUpdateDto dto);
        
        /// <summary>
        /// Delete a submission rule
        /// </summary>
        Task<bool> DeleteAsync(int ruleId);
        
        // ===== DATE CALCULATION =====
        
        /// <summary>
        /// Calculate the next due date based on rule configuration
        /// </summary>
        DateTime? CalculateNextDueDate(FormTemplateSubmissionRule rule, DateTime? fromDate = null);
        
        /// <summary>
        /// Get human-readable frequency display text
        /// </summary>
        string GetFrequencyDisplayText(string? frequency, int? dueDay, int? dueMonth);
        
        /// <summary>
        /// Get human-readable due day display text based on frequency
        /// </summary>
        string? GetDueDayDisplayText(string? frequency, int? dueDay);
        
        /// <summary>
        /// Get human-readable due month display text
        /// </summary>
        string? GetDueMonthDisplayText(int? dueMonth);
        
        // ===== VALIDATION =====
        
        /// <summary>
        /// Validate submission rule configuration
        /// </summary>
        Task<bool> ValidateRuleAsync(SubmissionRuleCreateDto dto);
        
        /// <summary>
        /// Check if a template can have additional submission rules
        /// </summary>
        Task<bool> CanAddRuleToTemplateAsync(int templateId);
        
        // ===== SUBMISSION TIMING VALIDATION =====
        
        /// <summary>
        /// Validate if a submission can be made at the given time based on rules
        /// </summary>
        Task<SubmissionTimingValidationResult> ValidateSubmissionTimingAsync(int templateId, DateTime submissionDate);
        
        /// <summary>
        /// Get all rules that need reminders sent today
        /// </summary>
        Task<List<SubmissionRuleDetailDto>> GetRulesNeedingRemindersAsync(DateTime forDate);
    }
    
    /// <summary>
    /// Result of submission timing validation
    /// </summary>
    public class SubmissionTimingValidationResult
    {
        public bool CanSubmit { get; set; }
        public bool IsLate { get; set; }
        public bool IsWithinGracePeriod { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? GracePeriodEnd { get; set; }
        public string? Message { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
    }
}
