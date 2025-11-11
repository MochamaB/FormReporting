using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Notifications
{
    /// <summary>
    /// Alert trigger log with acknowledge/resolve workflow
    /// </summary>
    [Table("AlertHistory")]
    public class AlertHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long HistoryId { get; set; }

        [Required]
        public int AlertId { get; set; }

        public DateTime TriggeredDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Triggered";

        public string? TriggerDetails { get; set; }

        [StringLength(20)]
        public string? Severity { get; set; }

        public long? NotificationId { get; set; }

        public int? AcknowledgedBy { get; set; }

        public DateTime? AcknowledgedDate { get; set; }

        [StringLength(1000)]
        public string? AcknowledgmentNotes { get; set; }

        public int? ResolvedBy { get; set; }

        public DateTime? ResolvedDate { get; set; }

        [StringLength(1000)]
        public string? ResolutionNotes { get; set; }

        public int? TimeToAcknowledgeMinutes { get; set; }

        public int? TimeToResolveMinutes { get; set; }

        public bool IsEscalated { get; set; } = false;

        public DateTime? EscalatedDate { get; set; }

        public string? EscalationDetails { get; set; }

        // Navigation properties
        [ForeignKey(nameof(AlertId))]
        public virtual AlertDefinition Alert { get; set; } = null!;

        [ForeignKey(nameof(NotificationId))]
        public virtual Notification? Notification { get; set; }

        [ForeignKey(nameof(AcknowledgedBy))]
        public virtual User? Acknowledger { get; set; }

        [ForeignKey(nameof(ResolvedBy))]
        public virtual User? Resolver { get; set; }
    }
}
