using FormReporting.Models.Entities.Notifications;
using FormReporting.Models.ViewModels.Notifications;

namespace FormReporting.Services.Notifications
{
    /// <summary>
    /// Service for managing notifications
    /// </summary>
    public interface INotificationService
    {
        // ========================================================================
        // CREATE (Used by Forms/Workflows/Assignments modules)
        // ========================================================================

        /// <summary>
        /// Create a new notification from a template
        /// </summary>
        Task<Notification> CreateNotificationAsync(CreateNotificationDto dto);

        // ========================================================================
        // ALL NOTIFICATIONS PAGE (/Notifications/Index)
        // ========================================================================

        /// <summary>
        /// Get all notifications (Email + InApp) for DataTable view
        /// </summary>
        Task<PaginatedResult<NotificationListDto>> GetAllNotificationsAsync(
            int userId,
            string? searchTerm = null,
            string? categoryFilter = null,
            string? priorityFilter = null,
            string? statusFilter = null,
            int page = 1,
            int pageSize = 20
        );

        /// <summary>
        /// Get stats for All Notifications page (4 cards)
        /// </summary>
        Task<NotificationStatsDto> GetNotificationStatsAsync(int userId);

        // ========================================================================
        // EMAIL NOTIFICATIONS PAGE (/Notifications/Email)
        // ========================================================================

        /// <summary>
        /// Get email notifications for mailbox view
        /// </summary>
        Task<PaginatedResult<EmailNotificationDto>> GetEmailNotificationsAsync(
            int userId,
            string filter = "all",
            string? recipientType = null,
            string? category = null,
            int page = 1,
            int pageSize = 20
        );

        /// <summary>
        /// Get stats for Email Notifications page (left sidebar)
        /// </summary>
        Task<EmailStatsDto> GetEmailStatsAsync(int userId);

        // ========================================================================
        // INAPP NOTIFICATIONS PAGE (/Notifications/InApp)
        // ========================================================================

        /// <summary>
        /// Get in-app notifications for mailbox view
        /// </summary>
        Task<PaginatedResult<InAppNotificationDto>> GetInAppNotificationsAsync(
            int userId,
            string filter = "all",
            string? priorityFilter = null,
            int page = 1,
            int pageSize = 20
        );

        /// <summary>
        /// Get stats for InApp Notifications page (left sidebar)
        /// </summary>
        Task<InAppStatsDto> GetInAppStatsAsync(int userId);

        // ========================================================================
        // COMMON (Used by all pages)
        // ========================================================================

        /// <summary>
        /// Get notification details by ID (for offcanvas modal)
        /// </summary>
        Task<NotificationDetailDto?> GetNotificationByIdAsync(long notificationId, int userId);

        /// <summary>
        /// Mark notification as read
        /// </summary>
        Task<bool> MarkAsReadAsync(long notificationId, int userId);

        /// <summary>
        /// Mark notification as dismissed
        /// </summary>
        Task<bool> MarkAsDismissedAsync(long notificationId, int userId);

        /// <summary>
        /// Get unread notification count (for bell badge)
        /// </summary>
        Task<int> GetUnreadCountAsync(int userId);

        /// <summary>
        /// Get recent notifications for bell dropdown
        /// </summary>
        Task<List<InAppNotificationDto>> GetRecentNotificationsAsync(int userId, int count = 5);
    }
}
