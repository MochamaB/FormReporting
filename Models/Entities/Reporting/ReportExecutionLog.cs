using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Reporting
{
    [Table("ReportExecutionLog")]
    public class ReportExecutionLog
    {
        [Key]
        public long ExecutionId { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        public int ExecutedBy { get; set; }

        public DateTime ExecutionDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string ExecutionType { get; set; } = string.Empty;

        public int? ScheduleId { get; set; }

        public string? Parameters { get; set; }

        public string? Filters { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        public int? RowCount { get; set; }

        public string? ErrorMessage { get; set; }

        public int? ExecutionTimeMs { get; set; }

        public int? QueryExecutionTimeMs { get; set; }

        public int? RenderingTimeMs { get; set; }

        [StringLength(20)]
        public string? OutputFormat { get; set; }

        public int? OutputSizeKB { get; set; }

        [StringLength(1000)]
        public string? OutputPath { get; set; }

        [ForeignKey(nameof(ReportId))]
        public virtual ReportDefinition Report { get; set; } = null!;

        [ForeignKey(nameof(ExecutedBy))]
        public virtual User Executor { get; set; } = null!;

        [ForeignKey(nameof(ScheduleId))]
        public virtual ReportSchedule? Schedule { get; set; }
    }
}
