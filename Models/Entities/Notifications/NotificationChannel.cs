using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Common;

namespace FormReporting.Models.Entities.Notifications
{
    /// <summary>
    /// Configuration for notification channels (Email, SMS, Push, InApp)
    /// </summary>
    [Table("NotificationChannels")]
    public class NotificationChannel : BaseEntity
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ChannelId { get; set; }

        /// <summary>
        /// Channel type: Email, SMS, Push, InApp
        /// </summary>
        [Required]
        [StringLength(20)]
        public string ChannelType { get; set; } = string.Empty;

        /// <summary>
        /// Display name for the channel
        /// </summary>
        [Required]
        [StringLength(100)]
        public string ChannelName { get; set; } = string.Empty;

        /// <summary>
        /// Channel description
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Is this channel enabled system-wide?
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Channel configuration settings (JSON)
        /// Example: {"smtpServer": "smtp.gmail.com", "port": 587}
        /// </summary>
        public string? Configuration { get; set; }

        /// <summary>
        /// Provider name (e.g., SendGrid, Twilio, Firebase)
        /// </summary>
        [StringLength(100)]
        public string? Provider { get; set; }

        /// <summary>
        /// API key or credentials (encrypted)
        /// </summary>
        public string? Credentials { get; set; }

        /// <summary>
        /// Maximum retry attempts for failed deliveries
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Retry delay in minutes
        /// </summary>
        public int RetryDelayMinutes { get; set; } = 5;

        /// <summary>
        /// Priority level (1=Highest, 5=Lowest)
        /// </summary>
        public int Priority { get; set; } = 3;

        /// <summary>
        /// Daily send limit (0 = unlimited)
        /// </summary>
        public int DailySendLimit { get; set; } = 0;

        /// <summary>
        /// Current daily send count
        /// </summary>
        public int DailySendCount { get; set; } = 0;

        /// <summary>
        /// Date when daily count was last reset
        /// </summary>
        public DateTime? LastResetDate { get; set; }

        // Navigation properties
        /// <summary>
        /// Notification deliveries using this channel
        /// </summary>
        public virtual ICollection<NotificationDelivery> Deliveries { get; set; } = new List<NotificationDelivery>();

        /// <summary>
        /// User preferences for this channel
        /// </summary>
        public virtual ICollection<UserNotificationPreference> UserPreferences { get; set; } = new List<UserNotificationPreference>();
    }
}
