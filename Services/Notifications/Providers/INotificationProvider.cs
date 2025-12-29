using FormReporting.Models.Entities.Notifications;

namespace FormReporting.Services.Notifications.Providers
{
    /// <summary>
    /// Base interface for all notification delivery providers
    /// Implementations: EmailProvider, InAppProvider, SmsProvider, PushProvider, etc.
    /// </summary>
    public interface INotificationProvider
    {
        /// <summary>
        /// Channel type this provider handles (Email, InApp, SMS, Push, etc.)
        /// </summary>
        string ChannelType { get; }

        /// <summary>
        /// Send notification via this channel
        /// </summary>
        /// <param name="delivery">Delivery record with recipient and message details</param>
        /// <param name="notification">Full notification with content</param>
        /// <param name="channel">Channel configuration (SMTP settings, API keys, etc.)</param>
        /// <returns>True if sent successfully, false otherwise</returns>
        Task<bool> SendAsync(
            NotificationDelivery delivery,
            Notification notification,
            NotificationChannel channel
        );

        /// <summary>
        /// Validate channel configuration (called when admin configures channel)
        /// </summary>
        /// <param name="configuration">JSON configuration string</param>
        /// <returns>Validation result with errors if any</returns>
        Task<(bool isValid, List<string> errors)> ValidateConfigurationAsync(string configuration);

        /// <summary>
        /// Test channel connectivity (called from admin UI)
        /// </summary>
        /// <param name="channel">Channel to test</param>
        /// <returns>Test result with details</returns>
        Task<(bool success, string message)> TestConnectionAsync(NotificationChannel channel);
    }
}
