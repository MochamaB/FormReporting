using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Notifications
{
    /// <summary>
    /// Per-user notification channel and frequency preferences
    /// </summary>
    [Table("UserNotificationPreferences")]
    public class UserNotificationPreference
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PreferenceId { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Notification channel ID
        /// </summary>
        [Required]
        public int ChannelId { get; set; }

        /// <summary>
        /// Notification type/category
        /// </summary>
        [StringLength(50)]
        public string? NotificationType { get; set; }

        /// <summary>
        /// Is this channel enabled for this user?
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Delivery frequency: Immediate, Hourly, Daily, Weekly
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Frequency { get; set; } = "Immediate";

        /// <summary>
        /// Quiet hours start time (e.g., 22:00)
        /// </summary>
        public TimeSpan? QuietHoursStart { get; set; }

        /// <summary>
        /// Quiet hours end time (e.g., 08:00)
        /// </summary>
        public TimeSpan? QuietHoursEnd { get; set; }

        /// <summary>
        /// Minimum priority to receive: Low, Normal, High, Urgent
        /// </summary>
        [StringLength(20)]
        public string MinimumPriority { get; set; } = "Low";

        /// <summary>
        /// Custom delivery address (overrides user's default)
        /// </summary>
        [StringLength(500)]
        public string? CustomAddress { get; set; }

        /// <summary>
        /// When the preference was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the preference was last modified
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        /// <summary>
        /// The user
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// The notification channel
        /// </summary>
        [ForeignKey(nameof(ChannelId))]
        public virtual NotificationChannel Channel { get; set; } = null!;
    }
}
