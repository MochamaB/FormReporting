using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Tracks user behavior and form performance analytics
    /// </summary>
    [Table("FormAnalytics")]
    public class FormAnalytics
    {
        [Key]
        public long AnalyticId { get; set; }

        [Required]
        public int TemplateId { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? SubmissionId { get; set; } // NULL if form abandoned before creating submission

        [Required]
        [StringLength(50)]
        public string EventType { get; set; } = string.Empty; // 'FormOpened', 'SectionStarted', 'SectionCompleted', 'FieldFilled', 'FormAbandoned', 'FormSubmitted', 'FormSaved'

        public string? EventData { get; set; } // JSON with context: {"sectionId": 5, "timeSpentSeconds": 45}

        public DateTime EventDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? SessionId { get; set; } // Browser session to track single user journey

        // Navigation properties
        [ForeignKey(nameof(TemplateId))]
        public virtual FormTemplate Template { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(SubmissionId))]
        public virtual FormTemplateSubmission? Submission { get; set; }
    }
}
