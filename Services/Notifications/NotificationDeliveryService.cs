using FormReporting.Data;
using FormReporting.Models.Entities.Notifications;
using FormReporting.Services.Notifications.Providers;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Services.Notifications
{
    /// <summary>
    /// Service for orchestrating notification delivery across multiple channels
    /// Uses provider factory pattern to support any channel type dynamically
    /// </summary>
    public class NotificationDeliveryService : INotificationDeliveryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEnumerable<INotificationProvider> _providers;
        private readonly ILogger<NotificationDeliveryService> _logger;

        public NotificationDeliveryService(
            ApplicationDbContext context,
            IEnumerable<INotificationProvider> providers,
            ILogger<NotificationDeliveryService> logger)
        {
            _context = context;
            _providers = providers;
            _logger = logger;
        }

        /// <summary>
        /// Send notification to all recipients via all configured channels
        /// </summary>
        public async Task<DeliveryResult> SendNotificationAsync(long notificationId)
        {
            var result = new DeliveryResult
            {
                NotificationId = notificationId
            };

            try
            {
                // Get all pending deliveries for this notification
                var deliveries = await _context.NotificationDeliveries
                    .Where(d => d.NotificationId == notificationId)
                    .Where(d => d.Status == "Pending")
                    .Include(d => d.Channel)
                    .Include(d => d.Notification)
                        .ThenInclude(n => n.Template)
                    .ToListAsync();

                result.TotalDeliveries = deliveries.Count;

                if (deliveries.Count == 0)
                {
                    _logger.LogInformation("No pending deliveries for notification {NotificationId}", notificationId);
                    return result;
                }

                // Group deliveries by channel for efficiency
                var deliveryGroups = deliveries.GroupBy(d => d.Channel.ChannelType);

                foreach (var group in deliveryGroups)
                {
                    var channelType = group.Key;
                    var channelDeliveries = group.ToList();

                    _logger.LogInformation(
                        "Processing {Count} deliveries for channel {ChannelType}",
                        channelDeliveries.Count,
                        channelType
                    );

                    foreach (var delivery in channelDeliveries)
                    {
                        try
                        {
                            var success = await SendDeliveryAsync(delivery.DeliveryId);

                            if (success)
                            {
                                result.SuccessfulDeliveries++;
                            }
                            else
                            {
                                result.FailedDeliveries++;
                                result.Errors.Add($"{channelType} delivery failed for {delivery.RecipientAddress}");
                            }
                        }
                        catch (Exception ex)
                        {
                            result.FailedDeliveries++;
                            result.Errors.Add($"{channelType}: {ex.Message}");

                            _logger.LogError(
                                ex,
                                "Exception during delivery {DeliveryId}",
                                delivery.DeliveryId
                            );
                        }
                    }
                }

                _logger.LogInformation(
                    "Notification {NotificationId} delivery complete: {Success}/{Total} succeeded",
                    notificationId,
                    result.SuccessfulDeliveries,
                    result.TotalDeliveries
                );

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification {NotificationId}", notificationId);
                result.Errors.Add($"Critical error: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Send a specific delivery via its channel
        /// </summary>
        public async Task<bool> SendDeliveryAsync(long deliveryId)
        {
            try
            {
                // Load delivery with all related data
                var delivery = await _context.NotificationDeliveries
                    .Include(d => d.Channel)
                    .Include(d => d.Notification)
                        .ThenInclude(n => n.Template)
                    .FirstOrDefaultAsync(d => d.DeliveryId == deliveryId);

                if (delivery == null)
                {
                    _logger.LogWarning("Delivery {DeliveryId} not found", deliveryId);
                    return false;
                }

                // Skip if already sent
                if (delivery.Status == "Sent" || delivery.Status == "Delivered")
                {
                    _logger.LogInformation("Delivery {DeliveryId} already sent", deliveryId);
                    return true;
                }

                // Get appropriate provider for this channel
                var provider = GetProviderForChannel(delivery.Channel.ChannelType);

                if (provider == null)
                {
                    _logger.LogError(
                        "No provider found for channel type {ChannelType}",
                        delivery.Channel.ChannelType
                    );

                    delivery.Status = "Failed";
                    delivery.ErrorMessage = $"No provider available for {delivery.Channel.ChannelType}";
                    delivery.ModifiedDate = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    return false;
                }

                // Send via provider
                var success = await provider.SendAsync(delivery, delivery.Notification, delivery.Channel);

                // Save updated delivery status
                await _context.SaveChangesAsync();

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending delivery {DeliveryId}", deliveryId);

                // Try to update delivery status
                try
                {
                    var delivery = await _context.NotificationDeliveries.FindAsync(deliveryId);
                    if (delivery != null)
                    {
                        delivery.Status = "Failed";
                        delivery.ErrorMessage = ex.Message;
                        delivery.ModifiedDate = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception saveEx)
                {
                    _logger.LogError(saveEx, "Failed to update delivery status for {DeliveryId}", deliveryId);
                }

                return false;
            }
        }

        /// <summary>
        /// Retry failed deliveries for a notification
        /// </summary>
        public async Task<int> RetryFailedDeliveriesAsync(long notificationId)
        {
            var failedDeliveries = await _context.NotificationDeliveries
                .Where(d => d.NotificationId == notificationId)
                .Where(d => d.Status == "Failed")
                .Where(d => d.RetryCount < d.Channel.MaxRetries)
                .Include(d => d.Channel)
                .ToListAsync();

            int retriedCount = 0;

            foreach (var delivery in failedDeliveries)
            {
                // Update retry count and reset status
                delivery.RetryCount++;
                delivery.Status = "Pending";
                delivery.NextRetryDate = DateTime.UtcNow.AddMinutes(
                    delivery.Channel.RetryDelayMinutes * delivery.RetryCount
                );
                delivery.ModifiedDate = DateTime.UtcNow;

                // Try to send again
                var success = await SendDeliveryAsync(delivery.DeliveryId);

                if (success || delivery.RetryCount >= delivery.Channel.MaxRetries)
                {
                    delivery.NextRetryDate = null;
                }

                retriedCount++;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Retried {Count} failed deliveries for notification {NotificationId}",
                retriedCount,
                notificationId
            );

            return retriedCount;
        }

        /// <summary>
        /// Get delivery status summary for a notification
        /// </summary>
        public async Task<DeliveryStatusSummary> GetDeliveryStatusAsync(long notificationId)
        {
            var deliveries = await _context.NotificationDeliveries
                .Where(d => d.NotificationId == notificationId)
                .Include(d => d.Channel)
                .ToListAsync();

            var summary = new DeliveryStatusSummary
            {
                NotificationId = notificationId,
                TotalRecipients = deliveries.Select(d => d.RecipientUserId).Distinct().Count(),
                PendingCount = deliveries.Count(d => d.Status == "Pending"),
                SentCount = deliveries.Count(d => d.Status == "Sent"),
                DeliveredCount = deliveries.Count(d => d.Status == "Delivered"),
                FailedCount = deliveries.Count(d => d.Status == "Failed" || d.Status == "Bounced")
            };

            // Group by channel
            summary.ByChannel = deliveries
                .GroupBy(d => d.Channel.ChannelType)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                );

            return summary;
        }

        // ========================================================================
        // PROVIDER FACTORY (Dynamic - supports any channel)
        // ========================================================================

        /// <summary>
        /// Get provider for a channel type (dynamic factory pattern)
        /// </summary>
        private INotificationProvider? GetProviderForChannel(string channelType)
        {
            // Find provider that handles this channel type
            var provider = _providers.FirstOrDefault(p => p.ChannelType.Equals(channelType, StringComparison.OrdinalIgnoreCase));

            if (provider == null)
            {
                _logger.LogWarning(
                    "No provider registered for channel type: {ChannelType}. Available providers: {Providers}",
                    channelType,
                    string.Join(", ", _providers.Select(p => p.ChannelType))
                );
            }

            return provider;
        }

        /// <summary>
        /// Get all registered providers (for admin UI)
        /// </summary>
        public IEnumerable<string> GetRegisteredChannelTypes()
        {
            return _providers.Select(p => p.ChannelType);
        }

        /// <summary>
        /// Test channel configuration (for admin UI)
        /// </summary>
        public async Task<(bool success, string message)> TestChannelAsync(int channelId)
        {
            var channel = await _context.NotificationChannels.FindAsync(channelId);

            if (channel == null)
            {
                return (false, "Channel not found");
            }

            var provider = GetProviderForChannel(channel.ChannelType);

            if (provider == null)
            {
                return (false, $"No provider available for {channel.ChannelType}");
            }

            return await provider.TestConnectionAsync(channel);
        }
    }
}
