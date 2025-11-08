using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Metrics;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Tracks auto-population of metrics from form submissions
    /// </summary>
    [Table("MetricPopulationLog")]
    public class MetricPopulationLog
    {
        [Key]
        public long LogId { get; set; }

        [Required]
        public int SubmissionId { get; set; }

        [Required]
        public int MetricId { get; set; }

        [Required]
        public int MappingId { get; set; }

        [Required]
        public int SourceItemId { get; set; }

        // Source and calculated values
        public string? SourceValue { get; set; } // Raw value from form response

        [Column(TypeName = "decimal(18,4)")]
        public decimal? CalculatedValue { get; set; } // Final calculated value for metric

        public string? CalculationFormula { get; set; } // Audit trail of calculation used

        // Processing metadata
        public DateTime PopulatedDate { get; set; } = DateTime.UtcNow;

        public int? PopulatedBy { get; set; } // NULL if system/automated, UserId if manual override

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty; // Success, Failed, Skipped, Pending

        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        public int? ProcessingTimeMs { get; set; } // Performance tracking

        // Navigation properties
        [ForeignKey(nameof(SubmissionId))]
        public virtual FormTemplateSubmission Submission { get; set; } = null!;

        [ForeignKey(nameof(MetricId))]
        public virtual MetricDefinition Metric { get; set; } = null!;

        [ForeignKey(nameof(MappingId))]
        public virtual FormItemMetricMapping Mapping { get; set; } = null!;

        [ForeignKey(nameof(SourceItemId))]
        public virtual FormTemplateItem SourceItem { get; set; } = null!;

        [ForeignKey(nameof(PopulatedBy))]
        public virtual User? PopulatedByUser { get; set; }
    }
}
