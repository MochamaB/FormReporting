using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Notifications
{
    /// <summary>
    /// Multi-channel delivery tracking with retry logic
    /// </summary>
    [Table("NotificationDelivery")]
    public class NotificationDelivery
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long DeliveryId { get; set; }

        /// <summary>
        /// Notification ID
        /// </summary>
        [Required]
        public long NotificationId { get; set; }

        /// <summary>
        /// Recipient user ID
        /// </summary>
        [Required]
        public int RecipientUserId { get; set; }

        /// <summary>
        /// Delivery channel ID
        /// </summary>
        [Required]
        public int ChannelId { get; set; }

        /// <summary>
        /// Delivery status: Pending, Sent, Delivered, Failed, Bounced
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Recipient address (email, phone number, device token)
        /// </summary>
        [Required]
        [StringLength(500)]
        public string RecipientAddress { get; set; } = string.Empty;

        /// <summary>
        /// When the delivery was attempted
        /// </summary>
        public DateTime? SentDate { get; set; }

        /// <summary>
        /// When the delivery was confirmed
        /// </summary>
        public DateTime? DeliveredDate { get; set; }

        /// <summary>
        /// Number of retry attempts
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// When to retry next
        /// </summary>
        public DateTime? NextRetryDate { get; set; }

        /// <summary>
        /// Error message if delivery failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// External provider message ID (for tracking)
        /// </summary>
        [StringLength(200)]
        public string? ExternalMessageId { get; set; }

        /// <summary>
        /// Provider response (JSON)
        /// </summary>
        public string? ProviderResponse { get; set; }

        /// <summary>
        /// Delivery cost (if applicable)
        /// </summary>
        [Column(TypeName = "decimal(10,4)")]
        public decimal? DeliveryCost { get; set; }

        /// <summary>
        /// When the delivery record was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the delivery record was last updated
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        /// <summary>
        /// The notification being delivered
        /// </summary>
        [ForeignKey(nameof(NotificationId))]
        public virtual Notification Notification { get; set; } = null!;

        /// <summary>
        /// The recipient user
        /// </summary>
        [ForeignKey(nameof(RecipientUserId))]
        public virtual User RecipientUser { get; set; } = null!;

        /// <summary>
        /// The delivery channel
        /// </summary>
        [ForeignKey(nameof(ChannelId))]
        public virtual NotificationChannel Channel { get; set; } = null!;
    }
}
