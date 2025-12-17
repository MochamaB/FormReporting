using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Defines submission rules for a form template - WHEN and HOW submissions should occur.
    /// Separate from FormTemplateAssignment which defines WHO can submit.
    /// </summary>
    [Table("FormTemplateSubmissionRules")]
    public class FormTemplateSubmissionRule
    {
        [Key]
        public int SubmissionRuleId { get; set; }

        [Required]
        public int TemplateId { get; set; }

        // ===== RULE IDENTIFICATION =====
        /// <summary>
        /// Name for this rule (e.g., "Monthly Sales Report", "Annual Survey")
        /// </summary>
        [Required]
        [StringLength(100)]
        public string RuleName { get; set; } = string.Empty;

        /// <summary>
        /// Description of the submission rule
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        // ===== FREQUENCY CONFIGURATION =====
        /// <summary>
        /// Submission frequency: Daily, Weekly, Monthly, Quarterly, Annually, Once
        /// NULL = no submission schedule (access-only)
        /// </summary>
        [StringLength(20)]
        public string? Frequency { get; set; }

        /// <summary>
        /// Day component based on frequency:
        /// - Daily: Not used (NULL)
        /// - Weekly: 0-6 (Sunday=0 to Saturday=6)
        /// - Monthly: 1-31, -1 = last day of month
        /// - Quarterly: 1-31, -1 = last day of quarter
        /// - Annually: 1-31, -1 = last day of month
        /// - Once: Not used (use SpecificDueDate instead)
        /// </summary>
        public int? DueDay { get; set; }

        /// <summary>
        /// Month for Annual frequency (1-12)
        /// </summary>
        public int? DueMonth { get; set; }

        /// <summary>
        /// Time of day when submission is due
        /// </summary>
        public TimeSpan? DueTime { get; set; }

        /// <summary>
        /// Specific due date/time for "Once" frequency (one-time submissions)
        /// </summary>
        public DateTime? SpecificDueDate { get; set; }

        /// <summary>
        /// JSON configuration for complex rules (future use: SemiMonthly, BiWeekly, Custom)
        /// </summary>
        public string? RuleConfig { get; set; }

        // ===== GRACE PERIOD & LATE SUBMISSION =====
        /// <summary>
        /// Days after due date to still accept submissions without penalty
        /// </summary>
        public int GracePeriodDays { get; set; } = 0;

        /// <summary>
        /// Whether to allow submissions after grace period (flagged as late)
        /// </summary>
        public bool AllowLateSubmission { get; set; } = true;

        // ===== REMINDERS =====
        /// <summary>
        /// Days before due date to send reminders (comma-separated for multiple)
        /// Example: "7,3,1" = reminders at 7 days, 3 days, and 1 day before due
        /// </summary>
        [StringLength(50)]
        public string? ReminderDaysBefore { get; set; }

        // ===== STATUS =====
        /// <summary>
        /// Status: Active, Suspended, Archived
        /// </summary>
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        // ===== METADATA =====
        [Required]
        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        // ===== NAVIGATION PROPERTIES =====
        [ForeignKey(nameof(TemplateId))]
        public virtual FormTemplate Template { get; set; } = null!;

        [ForeignKey(nameof(CreatedBy))]
        public virtual User CreatedByUser { get; set; } = null!;

        [ForeignKey(nameof(ModifiedBy))]
        public virtual User? ModifiedByUser { get; set; }
    }
}
