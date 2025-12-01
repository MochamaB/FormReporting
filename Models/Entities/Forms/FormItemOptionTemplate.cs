using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Organizational;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Pre-defined option sets (templates) for quick-filling dropdown/radio/checkbox fields
    /// Examples: "Agree-Disagree (5-point)", "Yes-No", "Satisfaction Scale"
    /// </summary>
    [Table("FormItemOptionTemplates")]
    public class FormItemOptionTemplate
    {
        [Key]
        public int TemplateId { get; set; }

        // Identity
        [Required]
        [StringLength(100)]
        public string TemplateName { get; set; } = string.Empty; // "Satisfaction Scale (5-point)"

        [Required]
        [StringLength(50)]
        public string TemplateCode { get; set; } = string.Empty; // "SATISFACTION_5PT"

        // Categorization
        [StringLength(50)]
        public string? Category { get; set; } // "Rating", "Binary", "Frequency", "Agreement"

        [StringLength(50)]
        public string? SubCategory { get; set; } // "Likert", "Sentiment", "Yes/No"

        // Metadata
        [StringLength(500)]
        public string? Description { get; set; }

        public int UsageCount { get; set; } = 0; // Track how many times this template has been used

        public int DisplayOrder { get; set; } = 0; // Order in template picker

        // Applicability
        [StringLength(200)]
        public string? ApplicableFieldTypes { get; set; } // JSON array: ["Radio", "Dropdown", "Rating"]

        [StringLength(500)]
        public string? RecommendedFor { get; set; } // "Customer satisfaction surveys, service quality assessments"

        // Scoring (Optional - for assessment forms)
        public bool HasScoring { get; set; } = false; // Does this template include score values?

        [StringLength(30)]
        public string? ScoringType { get; set; } // "Linear", "Custom", "Weighted"

        // Ownership
        public bool IsSystemTemplate { get; set; } = true; // System-provided or organization-specific?

        public int? TenantId { get; set; } // NULL = global, otherwise tenant-specific custom template

        // Audit
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        // Navigation properties
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant? Tenant { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual User? Creator { get; set; }

        [ForeignKey(nameof(ModifiedBy))]
        public virtual User? Modifier { get; set; }

        public virtual ICollection<FormItemOptionTemplateItem> Items { get; set; } = new List<FormItemOptionTemplateItem>();
    }
}
