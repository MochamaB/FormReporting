using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Organizational;

namespace FormReporting.Models.Entities.Metrics
{
    /// <summary>
    /// Logs for Hangfire jobs and automated metric checks
    /// </summary>
    [Table("SystemMetricLogs")]
    public class SystemMetricLog
    {
        [Key]
        public long LogId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        public int MetricId { get; set; }

        [Required]
        public DateTime CheckDate { get; set; }

        [StringLength(20)]
        public string? Status { get; set; } // Success, Failed, Warning

        [Column(TypeName = "decimal(18,4)")]
        public decimal? NumericValue { get; set; }

        public string? TextValue { get; set; }

        public string? Details { get; set; } // JSON with additional info

        [StringLength(100)]
        public string? JobName { get; set; }

        public int? ExecutionDuration { get; set; } // milliseconds

        public string? ErrorMessage { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant Tenant { get; set; } = null!;

        [ForeignKey(nameof(MetricId))]
        public virtual MetricDefinition MetricDefinition { get; set; } = null!;
    }
}
