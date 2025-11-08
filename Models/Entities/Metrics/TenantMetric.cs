using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Organizational;

namespace FormReporting.Models.Entities.Metrics
{
    /// <summary>
    /// Time-series data for metrics across all tenants with source tracking
    /// </summary>
    [Table("TenantMetrics")]
    public class TenantMetric
    {
        [Key]
        public long MetricValueId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        public int MetricId { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime ReportingPeriod { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? NumericValue { get; set; }

        public string? TextValue { get; set; }

        // Track source of metric value
        [StringLength(30)]
        public string? SourceType { get; set; } // UserInput, SystemCalculated, HangfireJob, ExternalAPI, Manual, Import

        public int? SourceReferenceId { get; set; } // SubmissionId if from form, LogId if from SystemMetricLogs

        public DateTime CapturedDate { get; set; } = DateTime.Now;

        public int? CapturedBy { get; set; }

        // Navigation properties
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant Tenant { get; set; } = null!;

        [ForeignKey(nameof(MetricId))]
        public virtual MetricDefinition MetricDefinition { get; set; } = null!;
    }
}
