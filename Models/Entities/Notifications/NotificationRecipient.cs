using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Notifications
{
    /// <summary>
    /// Tracks who receives each notification
    /// </summary>
    [Table("NotificationRecipients")]
    public class NotificationRecipient
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RecipientId { get; set; }

        /// <summary>
        /// Notification ID
        /// </summary>
        [Required]
        public long NotificationId { get; set; }

        /// <summary>
        /// User ID of the recipient
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Has the user read this notification?
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// When the notification was read
        /// </summary>
        public DateTime? ReadDate { get; set; }

        /// <summary>
        /// Has the user dismissed this notification?
        /// </summary>
        public bool IsDismissed { get; set; } = false;

        /// <summary>
        /// When the notification was dismissed
        /// </summary>
        public DateTime? DismissedDate { get; set; }

        /// <summary>
        /// Has the user acted on this notification?
        /// </summary>
        public bool IsActioned { get; set; } = false;

        /// <summary>
        /// When the user acted on the notification
        /// </summary>
        public DateTime? ActionedDate { get; set; }

        /// <summary>
        /// When the recipient record was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        /// <summary>
        /// The notification
        /// </summary>
        [ForeignKey(nameof(NotificationId))]
        public virtual Notification Notification { get; set; } = null!;

        /// <summary>
        /// The recipient user
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}
