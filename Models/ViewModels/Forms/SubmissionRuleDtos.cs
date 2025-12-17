using System.ComponentModel.DataAnnotations;

namespace FormReporting.Models.ViewModels.Forms
{
    /// <summary>
    /// DTO for creating a new submission rule
    /// </summary>
    public class SubmissionRuleCreateDto
    {
        [Required]
        public int TemplateId { get; set; }

        [Required]
        [StringLength(100)]
        public string RuleName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Frequency Configuration
        [StringLength(20)]
        public string? Frequency { get; set; }

        public int? DueDay { get; set; }

        public int? DueMonth { get; set; }

        public TimeSpan? DueTime { get; set; }

        public DateTime? SpecificDueDate { get; set; }

        // Grace Period & Late Submission
        public int GracePeriodDays { get; set; } = 0;

        public bool AllowLateSubmission { get; set; } = true;

        // Reminders
        [StringLength(50)]
        public string? ReminderDaysBefore { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing submission rule
    /// </summary>
    public class SubmissionRuleUpdateDto
    {
        [Required]
        public int SubmissionRuleId { get; set; }

        [Required]
        [StringLength(100)]
        public string RuleName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Frequency Configuration
        [StringLength(20)]
        public string? Frequency { get; set; }

        public int? DueDay { get; set; }

        public int? DueMonth { get; set; }

        public TimeSpan? DueTime { get; set; }

        public DateTime? SpecificDueDate { get; set; }

        // Grace Period & Late Submission
        public int GracePeriodDays { get; set; } = 0;

        public bool AllowLateSubmission { get; set; } = true;

        // Reminders
        [StringLength(50)]
        public string? ReminderDaysBefore { get; set; }

        // Status
        [StringLength(20)]
        public string Status { get; set; } = "Active";
    }

    /// <summary>
    /// DTO for listing submission rules (summary view)
    /// </summary>
    public class SubmissionRuleListDto
    {
        public int SubmissionRuleId { get; set; }
        public int TemplateId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Frequency { get; set; }
        public string? FrequencyDisplay { get; set; }
        public string? NextDueDateDisplay { get; set; }
        public DateTime? NextDueDate { get; set; }
        public int GracePeriodDays { get; set; }
        public bool AllowLateSubmission { get; set; }
        public string? ReminderDaysBefore { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// DTO for detailed submission rule view
    /// </summary>
    public class SubmissionRuleDetailDto
    {
        public int SubmissionRuleId { get; set; }
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;

        // Rule Identification
        public string RuleName { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Frequency Configuration
        public string? Frequency { get; set; }
        public string? FrequencyDisplay { get; set; }
        public int? DueDay { get; set; }
        public string? DueDayDisplay { get; set; }
        public int? DueMonth { get; set; }
        public string? DueMonthDisplay { get; set; }
        public TimeSpan? DueTime { get; set; }
        public string? DueTimeDisplay { get; set; }
        public DateTime? SpecificDueDate { get; set; }
        public string? SpecificDueDateDisplay { get; set; }

        // Calculated Next Due Date
        public DateTime? NextDueDate { get; set; }
        public string? NextDueDateDisplay { get; set; }

        // Grace Period & Late Submission
        public int GracePeriodDays { get; set; }
        public bool AllowLateSubmission { get; set; }

        // Reminders
        public string? ReminderDaysBefore { get; set; }
        public List<int>? ReminderDays { get; set; }

        // Status
        public string Status { get; set; } = string.Empty;

        // Metadata
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public string? ModifiedByName { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// DTO for submission rule dropdown/select options
    /// </summary>
    public class SubmissionRuleSelectDto
    {
        public int SubmissionRuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string? Frequency { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }
}
