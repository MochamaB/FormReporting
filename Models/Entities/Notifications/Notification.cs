using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Notifications
{
    /// <summary>
    /// Central notification inbox for all notification types
    /// </summary>
    [Table("Notifications")]
    public class Notification
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long NotificationId { get; set; }

        /// <summary>
        /// Notification type/category
        /// </summary>
        [Required]
        [StringLength(50)]
        public string NotificationType { get; set; } = string.Empty;

        /// <summary>
        /// Notification title/subject
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Notification message body
        /// </summary>
        [Required]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Priority level: Low, Normal, High, Urgent
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = "Normal";

        /// <summary>
        /// Source entity type (e.g., FormSubmission, Ticket, Alert)
        /// </summary>
        [StringLength(100)]
        public string? SourceEntityType { get; set; }

        /// <summary>
        /// Source entity ID
        /// </summary>
        public long? SourceEntityId { get; set; }

        /// <summary>
        /// Action URL (where to navigate when clicked)
        /// </summary>
        [StringLength(1000)]
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Action button text
        /// </summary>
        [StringLength(100)]
        public string? ActionButtonText { get; set; }

        /// <summary>
        /// Additional data (JSON)
        /// </summary>
        public string? AdditionalData { get; set; }

        /// <summary>
        /// Template ID if generated from template
        /// </summary>
        public int? TemplateId { get; set; }

        /// <summary>
        /// User who triggered this notification
        /// </summary>
        public int? TriggeredBy { get; set; }

        /// <summary>
        /// When the notification was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When to send/display the notification
        /// </summary>
        public DateTime? ScheduledDate { get; set; }

        /// <summary>
        /// Expiry date (notification becomes invalid after this)
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Is this notification active?
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        /// <summary>
        /// Template used to generate this notification
        /// </summary>
        [ForeignKey(nameof(TemplateId))]
        public virtual NotificationTemplate? Template { get; set; }

        /// <summary>
        /// User who triggered this notification
        /// </summary>
        [ForeignKey(nameof(TriggeredBy))]
        public virtual User? Trigger { get; set; }

        /// <summary>
        /// Recipients of this notification
        /// </summary>
        public virtual ICollection<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();

        /// <summary>
        /// Delivery attempts for this notification
        /// </summary>
        public virtual ICollection<NotificationDelivery> Deliveries { get; set; } = new List<NotificationDelivery>();
    }
}
