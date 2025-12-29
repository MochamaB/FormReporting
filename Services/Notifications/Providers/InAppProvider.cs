using FormReporting.Models.Entities.Notifications;
using System.Text.Json;

namespace FormReporting.Services.Notifications.Providers
{
    /// <summary>
    /// In-App notification provider
    /// Stores in database and broadcasts via SignalR (Phase 4)
    /// Configuration can specify SignalR hub path and other settings
    /// </summary>
    public class InAppProvider : INotificationProvider
    {
        private readonly ILogger<InAppProvider> _logger;
        // TODO: Inject IHubContext<NotificationHub> in Phase 4

        public string ChannelType => "InApp";

        public InAppProvider(ILogger<InAppProvider> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// "Send" in-app notification (actually just mark as delivered since it's stored in DB)
        /// </summary>
        public async Task<bool> SendAsync(
            NotificationDelivery delivery,
            Notification notification,
            NotificationChannel channel)
        {
            try
            {
                // Parse configuration
                var config = ParseInAppConfiguration(channel.Configuration);

                // InApp notifications are already in the database (NotificationRecipient table)
                // We just mark the delivery as successful
                delivery.Status = "Delivered";
                delivery.SentDate = DateTime.UtcNow;
                delivery.DeliveredDate = DateTime.UtcNow;
                delivery.ModifiedDate = DateTime.UtcNow;

                // TODO: Phase 4 - Broadcast via SignalR
                // if (config?.UseSignalR == true)
                // {
                //     await BroadcastViaSignalRAsync(notification, delivery.RecipientUserId);
                // }

                _logger.LogInformation(
                    "InApp notification delivered for user {UserId}, notification {NotificationId}",
                    delivery.RecipientUserId,
                    notification.NotificationId
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to deliver InApp notification to user {UserId}",
                    delivery.RecipientUserId
                );

                delivery.Status = "Failed";
                delivery.ErrorMessage = ex.Message;
                delivery.ModifiedDate = DateTime.UtcNow;

                return false;
            }
        }

        /// <summary>
        /// Validate InApp configuration
        /// </summary>
        public async Task<(bool isValid, List<string> errors)> ValidateConfigurationAsync(string configuration)
        {
            var errors = new List<string>();

            try
            {
                if (string.IsNullOrWhiteSpace(configuration))
                {
                    // InApp can work without configuration
                    return (true, errors);
                }

                var config = JsonSerializer.Deserialize<InAppConfiguration>(configuration);

                if (config == null)
                {
                    errors.Add("Invalid JSON configuration");
                    return (false, errors);
                }

                // Validate HubPath if SignalR is enabled
                if (config.UseSignalR && string.IsNullOrWhiteSpace(config.HubPath))
                {
                    errors.Add("Hub Path is required when SignalR is enabled");
                }

                return (errors.Count == 0, errors);
            }
            catch (JsonException ex)
            {
                errors.Add($"Invalid JSON: {ex.Message}");
                return (false, errors);
            }
        }

        /// <summary>
        /// Test InApp "connection" (always succeeds since it's DB-based)
        /// </summary>
        public async Task<(bool success, string message)> TestConnectionAsync(NotificationChannel channel)
        {
            try
            {
                var config = ParseInAppConfiguration(channel.Configuration);

                if (config?.UseSignalR == true)
                {
                    // TODO: Phase 4 - Test SignalR hub availability
                    return (true, $"InApp notifications enabled. SignalR hub: {config.HubPath}");
                }

                return (true, "InApp notifications enabled (database-based)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InApp connection test failed");
                return (false, $"Test failed: {ex.Message}");
            }
        }

        // ========================================================================
        // HELPER METHODS
        // ========================================================================

        /// <summary>
        /// Parse InApp configuration from JSON
        /// </summary>
        private InAppConfiguration? ParseInAppConfiguration(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new InAppConfiguration(); // Default config

            try
            {
                return JsonSerializer.Deserialize<InAppConfiguration>(json);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse InApp configuration: {Json}", json);
                return new InAppConfiguration();
            }
        }

        /// <summary>
        /// Broadcast notification via SignalR (Phase 4 implementation)
        /// </summary>
        private async Task BroadcastViaSignalRAsync(Notification notification, int userId)
        {
            // TODO: Phase 4 - Implement SignalR broadcasting
            // await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", new
            // {
            //     notificationId = notification.NotificationId,
            //     title = notification.Title,
            //     message = notification.Template.PushTemplate,
            //     priority = notification.Priority,
            //     actionUrl = notification.ActionUrl,
            //     createdDate = notification.CreatedDate
            // });

            await Task.CompletedTask;
        }

        // ========================================================================
        // CONFIGURATION MODEL
        // ========================================================================

        /// <summary>
        /// InApp configuration model (matches JSON in NotificationChannel.Configuration)
        /// </summary>
        private class InAppConfiguration
        {
            public bool UseSignalR { get; set; } = true;
            public string HubPath { get; set; } = "/notificationHub";
            public string StorageType { get; set; } = "Database";
        }
    }
}
