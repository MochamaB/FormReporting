using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;
using FormReporting.Models.Entities.Notifications;

namespace FormReporting.Models.Entities.Reporting
{
    [Table("ReportSchedules")]
    public class ReportSchedule
    {
        [Key]
        public int ScheduleId { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        [StringLength(200)]
        public string ScheduleName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ScheduleType { get; set; } = string.Empty;

        public byte? DayOfWeek { get; set; }

        public byte? DayOfMonth { get; set; }

        public TimeSpan? ExecutionTime { get; set; }

        [StringLength(50)]
        public string Timezone { get; set; } = "East Africa Time";

        [Required]
        [StringLength(20)]
        public string OutputFormat { get; set; } = string.Empty;

        public bool IncludeCharts { get; set; } = true;

        [StringLength(20)]
        public string PageOrientation { get; set; } = "Portrait";

        [Required]
        public int NotificationTemplateId { get; set; }

        [Required]
        public string Recipients { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime? LastRunDate { get; set; }

        public DateTime? NextRunDate { get; set; }

        [StringLength(20)]
        public string? LastRunStatus { get; set; }

        public string? LastRunError { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ReportId))]
        public virtual ReportDefinition Report { get; set; } = null!;

        [ForeignKey(nameof(NotificationTemplateId))]
        public virtual NotificationTemplate NotificationTemplate { get; set; } = null!;

        [ForeignKey(nameof(CreatedBy))]
        public virtual User Creator { get; set; } = null!;

        public virtual ICollection<ReportExecutionLog> ExecutionLogs { get; set; } = new List<ReportExecutionLog>();
    }
}
