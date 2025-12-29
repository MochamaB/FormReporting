using FormReporting.Models.Entities.Notifications;

namespace FormReporting.Services.Notifications
{
    /// <summary>
    /// Service for orchestrating notification delivery across multiple channels
    /// </summary>
    public interface INotificationDeliveryService
    {
        /// <summary>
        /// Send notification to all recipients via all configured channels
        /// </summary>
        /// <param name="notificationId">Notification to send</param>
        /// <returns>Summary of delivery results</returns>
        Task<DeliveryResult> SendNotificationAsync(long notificationId);

        /// <summary>
        /// Send a specific delivery via its channel
        /// </summary>
        /// <param name="deliveryId">Delivery record to process</param>
        /// <returns>True if sent successfully</returns>
        Task<bool> SendDeliveryAsync(long deliveryId);

        /// <summary>
        /// Retry failed deliveries for a notification
        /// </summary>
        /// <param name="notificationId">Notification to retry</param>
        /// <returns>Number of deliveries retried</returns>
        Task<int> RetryFailedDeliveriesAsync(long notificationId);

        /// <summary>
        /// Get delivery status summary for a notification
        /// </summary>
        /// <param name="notificationId">Notification to check</param>
        /// <returns>Delivery statistics</returns>
        Task<DeliveryStatusSummary> GetDeliveryStatusAsync(long notificationId);
    }

    /// <summary>
    /// Result of sending a notification
    /// </summary>
    public class DeliveryResult
    {
        public long NotificationId { get; set; }
        public int TotalDeliveries { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool AllSucceeded => FailedDeliveries == 0;
    }

    /// <summary>
    /// Delivery status summary
    /// </summary>
    public class DeliveryStatusSummary
    {
        public long NotificationId { get; set; }
        public int TotalRecipients { get; set; }
        public int PendingCount { get; set; }
        public int SentCount { get; set; }
        public int DeliveredCount { get; set; }
        public int FailedCount { get; set; }
        public Dictionary<string, int> ByChannel { get; set; } = new();
    }
}
