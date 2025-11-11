using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Common;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Notifications
{
    /// <summary>
    /// Automated alert rules that create notifications when triggered
    /// </summary>
    [Table("AlertDefinitions")]
    public class AlertDefinition : BaseEntity, IActivatable
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AlertId { get; set; }

        /// <summary>
        /// Alert code (unique identifier)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string AlertCode { get; set; } = string.Empty;

        /// <summary>
        /// Alert name
        /// </summary>
        [Required]
        [StringLength(200)]
        public string AlertName { get; set; } = string.Empty;

        /// <summary>
        /// Alert description
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Alert type/category
        /// </summary>
        [Required]
        [StringLength(50)]
        public string AlertType { get; set; } = string.Empty;

        /// <summary>
        /// Trigger condition (JSON)
        /// Example: {"metric": "CPU_Usage", "operator": ">", "threshold": 90}
        /// </summary>
        [Required]
        public string TriggerCondition { get; set; } = string.Empty;

        /// <summary>
        /// Check frequency in minutes
        /// </summary>
        public int CheckFrequencyMinutes { get; set; } = 15;

        /// <summary>
        /// Severity level: Info, Warning, Error, Critical
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = "Warning";

        /// <summary>
        /// Notification template ID to use when alert triggers
        /// </summary>
        [Required]
        public int TemplateId { get; set; }

        /// <summary>
        /// Recipients configuration (JSON)
        /// Example: [{"type":"Role","id":1},{"type":"User","id":5}]
        /// </summary>
        [Required]
        public string Recipients { get; set; } = string.Empty;

        /// <summary>
        /// Channels to use (JSON array)
        /// Example: ["Email", "SMS", "InApp"]
        /// </summary>
        public string? Channels { get; set; }

        /// <summary>
        /// Cooldown period in minutes (prevent alert spam)
        /// </summary>
        public int CooldownMinutes { get; set; } = 60;

        /// <summary>
        /// Auto-resolve condition (JSON)
        /// </summary>
        public string? AutoResolveCondition { get; set; }

        /// <summary>
        /// Escalation rules (JSON)
        /// Example: {"after": 30, "escalateTo": [{"type":"Role","id":2}]}
        /// </summary>
        public string? EscalationRules { get; set; }

        /// <summary>
        /// Is this alert active?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When the alert was last triggered
        /// </summary>
        public DateTime? LastTriggeredDate { get; set; }

        /// <summary>
        /// When the alert was last checked
        /// </summary>
        public DateTime? LastCheckDate { get; set; }

        /// <summary>
        /// Number of times this alert has been triggered
        /// </summary>
        public int TriggerCount { get; set; } = 0;

        /// <summary>
        /// User who created this alert
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// User who last modified this alert
        /// </summary>
        public int? ModifiedBy { get; set; }

        // Navigation properties
        /// <summary>
        /// Notification template to use
        /// </summary>
        [ForeignKey(nameof(TemplateId))]
        public virtual NotificationTemplate Template { get; set; } = null!;

        /// <summary>
        /// User who created this alert
        /// </summary>
        [ForeignKey(nameof(CreatedBy))]
        public virtual User Creator { get; set; } = null!;

        /// <summary>
        /// User who last modified this alert
        /// </summary>
        [ForeignKey(nameof(ModifiedBy))]
        public virtual User? Modifier { get; set; }

        /// <summary>
        /// Alert trigger history
        /// </summary>
        public virtual ICollection<AlertHistory> AlertHistories { get; set; } = new List<AlertHistory>();
    }
}
