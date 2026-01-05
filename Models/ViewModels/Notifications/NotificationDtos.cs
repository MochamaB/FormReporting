using System.ComponentModel.DataAnnotations;

namespace FormReporting.Models.ViewModels.Notifications
{
    // ============================================================================
    // CREATE DTOs (Used by Forms/Workflows/Assignments to create notifications)
    // ============================================================================

    /// <summary>
    /// DTO for creating a new notification from a template
    /// </summary>
    public class CreateNotificationDto
    {
        /// <summary>
        /// Template code (e.g., "FORM_SUBMITTED", "WORKFLOW_ASSIGNED")
        /// </summary>
        [Required]
        public string TemplateCode { get; set; } = string.Empty;

        /// <summary>
        /// Source entity type (e.g., "FormSubmission", "WorkflowProgress")
        /// </summary>
        public string? SourceEntityType { get; set; }

        /// <summary>
        /// Source entity ID
        /// </summary>
        public long? SourceEntityId { get; set; }

        /// <summary>
        /// List of user IDs to notify
        /// </summary>
        [Required]
        public List<int> RecipientUserIds { get; set; } = new();

        /// <summary>
        /// Placeholder data for template substitution
        /// Example: { "RecipientName": "John Doe", "FormName": "Leave Request" }
        /// </summary>
        [Required]
        public Dictionary<string, string> PlaceholderData { get; set; } = new();

        /// <summary>
        /// Optional: Override template's default priority
        /// </summary>
        public string? CustomPriority { get; set; }

        /// <summary>
        /// Optional: Override template's default channels
        /// </summary>
        public List<string>? CustomChannels { get; set; }

        /// <summary>
        /// Optional: Schedule notification for later delivery
        /// </summary>
        public DateTime? ScheduledDate { get; set; }

        /// <summary>
        /// Optional: Action URL for the notification
        /// </summary>
        public string? ActionUrl { get; set; }
    }

    // ============================================================================
    // ALL NOTIFICATIONS PAGE DTOs (DataTable view)
    // ============================================================================

    /// <summary>
    /// DTO for notification list in DataTable (All Notifications page)
    /// </summary>
    public class NotificationListDto
    {
        public long NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string ChannelType { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }

        // Badge colors for DataTable component
        public string PriorityColor => Priority switch
        {
            "Urgent" => "danger",
            "High" => "warning",
            "Normal" => "info",
            "Low" => "secondary",
            _ => "info"
        };

        public string ChannelColor => ChannelType switch
        {
            "Email" => "primary",
            "InApp" => "success",
            "SMS" => "warning",
            _ => "secondary"
        };

        public string StatusColor => IsRead ? "success" : "warning";

        public string IconClass { get; set; } = "ri-notification-line";
    }

    /// <summary>
    /// Stats for All Notifications page (4 cards)
    /// </summary>
    public class NotificationStatsDto
    {
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public int EmailCount { get; set; }
        public int InAppCount { get; set; }
        public int TodayCount { get; set; }
        public int ThisWeekCount { get; set; }
    }

    // ============================================================================
    // EMAIL NOTIFICATIONS PAGE DTOs (Mailbox view)
    // ============================================================================

    /// <summary>
    /// DTO for email notification list items
    /// </summary>
    public class EmailNotificationDto
    {
        public long NotificationId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string From { get; set; } = "KTDA Form Reporting System";
        public string PreviewText { get; set; } = string.Empty;
        public string DeliveryStatus { get; set; } = string.Empty;
        public DateTime SentDate { get; set; }
        public bool IsRead { get; set; }
        public string? RecipientAddress { get; set; }
    }

    /// <summary>
    /// Stats for Email Notifications page (left sidebar counts)
    /// </summary>
    public class EmailStatsDto
    {
        public int AllEmailsCount { get; set; }
        public int SentCount { get; set; }
        public int PendingCount { get; set; }
        public int FailedCount { get; set; }
    }

    // ============================================================================
    // INAPP NOTIFICATIONS PAGE DTOs (Mailbox view)
    // ============================================================================

    /// <summary>
    /// DTO for in-app notification list items
    /// </summary>
    public class InAppNotificationDto
    {
        public long NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool IsDismissed { get; set; }
        public DateTime CreatedDate { get; set; }
        public string IconClass { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
    }

    /// <summary>
    /// Stats for InApp Notifications page (left sidebar counts)
    /// </summary>
    public class InAppStatsDto
    {
        public int AllCount { get; set; }
        public int UnreadCount { get; set; }
        public int ReadCount { get; set; }
        public int HighPriorityCount { get; set; }
    }

    // ============================================================================
    // DETAIL DTO (Used by offcanvas modal on all pages)
    // ============================================================================

    /// <summary>
    /// DTO for notification detail view (offcanvas modal)
    /// </summary>
    public class NotificationDetailDto
    {
        public long NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string FullMessage { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ChannelType { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool IsDismissed { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ReadDate { get; set; }
        public string? ActionUrl { get; set; }
        public string? SourceEntityType { get; set; }
        public long? SourceEntityId { get; set; }

        // Email-specific fields
        public string? DeliveryStatus { get; set; }
        public string? RecipientAddress { get; set; }
        public DateTime? SentDate { get; set; }
        public string? ErrorMessage { get; set; }
    }

    // ============================================================================
    // COMMON DTOs
    // ============================================================================

    /// <summary>
    /// DTO for unread count (bell badge)
    /// </summary>
    public class UnreadCountDto
    {
        public int Count { get; set; }
    }

    /// <summary>
    /// Generic paginated result wrapper
    /// </summary>
    public class PaginatedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    // ============================================================================
    // VIEW MODELS (For MVC pages)
    // ============================================================================

    /// <summary>
    /// ViewModel for All Notifications page (/Notifications/Index)
    /// </summary>
    public class AllNotificationsViewModel
    {
        public NotificationStatsDto Stats { get; set; } = new();
        public PaginatedResult<NotificationListDto> Notifications { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? SearchFilter => SearchTerm; // Alias for view compatibility
        public string? CategoryFilter { get; set; }
        public string? PriorityFilter { get; set; }
        public string? StatusFilter { get; set; }
    }

    /// <summary>
    /// ViewModel for Email Notifications page (/Notifications/Email)
    /// </summary>
    public class EmailNotificationsViewModel
    {
        public EmailStatsDto Stats { get; set; } = new();
        public PaginatedResult<EmailNotificationDto> Emails { get; set; } = new();
        public string CurrentFilter { get; set; } = "all";
        public string? RecipientType { get; set; }
        public string? Category { get; set; }
    }

    /// <summary>
    /// ViewModel for InApp Notifications page (/Notifications/InApp)
    /// </summary>
    public class InAppNotificationsViewModel
    {
        public InAppStatsDto Stats { get; set; } = new();
        public PaginatedResult<InAppNotificationDto> Notifications { get; set; } = new();
        public string CurrentFilter { get; set; } = "all";
        public string? PriorityFilter { get; set; }
    }
}
