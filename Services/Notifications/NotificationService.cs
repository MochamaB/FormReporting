using FormReporting.Data;
using FormReporting.Models.Entities.Notifications;
using FormReporting.Models.ViewModels.Notifications;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FormReporting.Services.Notifications
{
    /// <summary>
    /// Service for managing notifications
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationTemplateService _templateService;
        private readonly INotificationDeliveryService _deliveryService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            ApplicationDbContext context,
            INotificationTemplateService templateService,
            INotificationDeliveryService deliveryService,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _templateService = templateService;
            _deliveryService = deliveryService;
            _logger = logger;
        }

        // ========================================================================
        // CREATE (Used by Forms/Workflows/Assignments modules)
        // ========================================================================

        /// <summary>
        /// Create a new notification from a template
        /// </summary>
        public async Task<Notification> CreateNotificationAsync(CreateNotificationDto dto)
        {
            try
            {
                // 1. Get and validate template
                var template = await _templateService.GetTemplateByCodeAsync(dto.TemplateCode);
                if (template == null)
                {
                    throw new InvalidOperationException($"Template '{dto.TemplateCode}' not found");
                }

                // 2. Validate placeholders (log warnings but don't block)
                await _templateService.ValidatePlaceholdersAsync(dto.TemplateCode, dto.PlaceholderData);

                // 3. Render template with placeholders
                var (subject, body, pushMessage) = await _templateService.RenderTemplateAsync(
                    dto.TemplateCode,
                    dto.PlaceholderData
                );

                // 4. Create Notification record
                var notification = new Notification
                {
                    TemplateId = template.TemplateId,
                    Title = subject,
                    Message = body,
                    SourceEntityType = dto.SourceEntityType,
                    SourceEntityId = dto.SourceEntityId,
                    Priority = dto.CustomPriority ?? template.DefaultPriority,
                    ActionUrl = dto.ActionUrl,
                    ScheduledDate = dto.ScheduledDate,
                    CreatedDate = DateTime.UtcNow
                };

                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync(); // Save to get NotificationId

                // 5. Determine channels to use
                var channels = await GetChannelsForNotificationAsync(template, dto.CustomChannels);

                // 6. Create NotificationRecipient records (one per user)
                foreach (var userId in dto.RecipientUserIds)
                {
                    var recipient = new NotificationRecipient
                    {
                        NotificationId = notification.NotificationId,
                        UserId = userId,
                        IsRead = false,
                        IsDismissed = false,
                        IsActioned = false,
                        CreatedDate = DateTime.UtcNow
                    };

                    await _context.NotificationRecipients.AddAsync(recipient);
                }

                // 7. Create NotificationDelivery records (one per user per channel)
                foreach (var userId in dto.RecipientUserIds)
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null)
                    {
                        _logger.LogWarning("User {UserId} not found, skipping delivery", userId);
                        continue;
                    }

                    foreach (var channel in channels)
                    {
                        var delivery = new NotificationDelivery
                        {
                            NotificationId = notification.NotificationId,
                            RecipientUserId = userId,
                            ChannelId = channel.ChannelId,
                            Status = "Pending",
                            RecipientAddress = channel.ChannelType == "Email"
                                ? user.Email
                                : userId.ToString(),
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow
                        };

                        await _context.NotificationDeliveries.AddAsync(delivery);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Created notification {NotificationId} for {RecipientCount} recipients",
                    notification.NotificationId,
                    dto.RecipientUserIds.Count
                );

                // 8. Trigger delivery asynchronously (don't wait - fire and forget)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _deliveryService.SendNotificationAsync(notification.NotificationId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error delivering notification {NotificationId}", notification.NotificationId);
                    }
                });

                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification from template {TemplateCode}", dto.TemplateCode);
                throw;
            }
        }

        // ========================================================================
        // ALL NOTIFICATIONS PAGE (/Notifications/Index)
        // ========================================================================

        /// <summary>
        /// Get all notifications (Email + InApp) for DataTable view
        /// </summary>
        public async Task<PaginatedResult<NotificationListDto>> GetAllNotificationsAsync(
            int userId,
            string? searchTerm = null,
            string? categoryFilter = null,
            string? priorityFilter = null,
            string? statusFilter = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.NotificationRecipients
                .Where(nr => nr.UserId == userId && !nr.IsDismissed)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.Template)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.Deliveries)
                        .ThenInclude(d => d.Channel)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(nr =>
                    nr.Notification.Title.Contains(searchTerm) ||
                    nr.Notification.Message.Contains(searchTerm)
                );
            }

            if (!string.IsNullOrEmpty(categoryFilter))
            {
                query = query.Where(nr => nr.Notification.Template.Category == categoryFilter);
            }

            if (!string.IsNullOrEmpty(priorityFilter))
            {
                query = query.Where(nr => nr.Notification.Priority == priorityFilter);
            }

            if (statusFilter == "Unread")
            {
                query = query.Where(nr => !nr.IsRead);
            }
            else if (statusFilter == "Read")
            {
                query = query.Where(nr => nr.IsRead);
            }

            var totalCount = await query.CountAsync();

            var notifications = await query
                .OrderByDescending(nr => nr.Notification.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(nr => new NotificationListDto
                {
                    NotificationId = nr.NotificationId,
                    Title = nr.Notification.Title,
                    Message = StripHtml(nr.Notification.Message).Length > 100
                        ? StripHtml(nr.Notification.Message).Substring(0, 100) + "..."
                        : StripHtml(nr.Notification.Message),
                    Category = nr.Notification.Template.Category ?? "General",
                    Priority = nr.Notification.Priority,
                    ChannelType = nr.Notification.Deliveries
                        .OrderBy(d => d.ChannelId)
                        .Select(d => d.Channel.ChannelType)
                        .FirstOrDefault() ?? "InApp",
                    IsRead = nr.IsRead,
                    CreatedDate = nr.Notification.CreatedDate,
                    Status = nr.IsRead ? "Read" : "Unread",
                    ActionUrl = nr.Notification.ActionUrl
                })
                .ToListAsync();

            return new PaginatedResult<NotificationListDto>
            {
                Data = notifications,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Get stats for All Notifications page (4 cards)
        /// </summary>
        public async Task<NotificationStatsDto> GetNotificationStatsAsync(int userId)
        {
            var notifications = _context.NotificationRecipients
                .Where(nr => nr.UserId == userId && !nr.IsDismissed)
                .Include(nr => nr.Notification);

            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);

            return new NotificationStatsDto
            {
                TotalCount = await notifications.CountAsync(),
                UnreadCount = await notifications.Where(nr => !nr.IsRead).CountAsync(),
                TodayCount = await notifications
                    .Where(nr => nr.Notification.CreatedDate >= today)
                    .CountAsync(),
                ThisWeekCount = await notifications
                    .Where(nr => nr.Notification.CreatedDate >= weekStart)
                    .CountAsync()
            };
        }

        // ========================================================================
        // EMAIL NOTIFICATIONS PAGE (/Notifications/Email)
        // ========================================================================

        /// <summary>
        /// Get email notifications for mailbox view
        /// </summary>
        public async Task<PaginatedResult<EmailNotificationDto>> GetEmailNotificationsAsync(
            int userId,
            string filter = "all",
            string? recipientType = null,
            string? category = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.NotificationDeliveries
                .Where(nd => nd.RecipientUserId == userId)
                .Where(nd => nd.Channel.ChannelType == "Email")
                .Include(nd => nd.Notification)
                    .ThenInclude(n => n.Template)
                .Include(nd => nd.Notification)
                    .ThenInclude(n => n.Recipients.Where(r => r.UserId == userId))
                .AsQueryable();

            // Apply filters based on left sidebar
            query = filter.ToLower() switch
            {
                "sent" => query.Where(nd => nd.Status == "Sent" || nd.Status == "Delivered"),
                "pending" => query.Where(nd => nd.Status == "Pending"),
                "failed" => query.Where(nd => nd.Status == "Failed" || nd.Status == "Bounced"),
                _ => query
            };

            // Category filter
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(nd => nd.Notification.Template.Category == category);
            }

            var totalCount = await query.CountAsync();

            var emails = await query
                .OrderByDescending(nd => nd.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(nd => new EmailNotificationDto
                {
                    NotificationId = nd.NotificationId,
                    Subject = nd.Notification.Title,
                    From = "KTDA Form Reporting System",
                    PreviewText = StripHtml(nd.Notification.Message).Length > 100
                        ? StripHtml(nd.Notification.Message).Substring(0, 100) + "..."
                        : StripHtml(nd.Notification.Message),
                    DeliveryStatus = nd.Status,
                    SentDate = nd.SentDate ?? nd.CreatedDate,
                    IsRead = nd.Notification.Recipients.Any() && nd.Notification.Recipients.First().IsRead,
                    RecipientAddress = nd.RecipientAddress
                })
                .ToListAsync();

            return new PaginatedResult<EmailNotificationDto>
            {
                Data = emails,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Get stats for Email Notifications page (left sidebar)
        /// </summary>
        public async Task<EmailStatsDto> GetEmailStatsAsync(int userId)
        {
            var emails = _context.NotificationDeliveries
                .Where(nd => nd.RecipientUserId == userId)
                .Where(nd => nd.Channel.ChannelType == "Email");

            return new EmailStatsDto
            {
                AllEmailsCount = await emails.CountAsync(),
                SentCount = await emails
                    .Where(nd => nd.Status == "Sent" || nd.Status == "Delivered")
                    .CountAsync(),
                PendingCount = await emails
                    .Where(nd => nd.Status == "Pending")
                    .CountAsync(),
                FailedCount = await emails
                    .Where(nd => nd.Status == "Failed" || nd.Status == "Bounced")
                    .CountAsync()
            };
        }

        // ========================================================================
        // INAPP NOTIFICATIONS PAGE (/Notifications/InApp)
        // ========================================================================

        /// <summary>
        /// Get in-app notifications for mailbox view
        /// </summary>
        public async Task<PaginatedResult<InAppNotificationDto>> GetInAppNotificationsAsync(
            int userId,
            string filter = "all",
            string? priorityFilter = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.NotificationDeliveries
                .Where(nd => nd.RecipientUserId == userId)
                .Where(nd => nd.Channel.ChannelType == "InApp")
                .Include(nd => nd.Notification)
                    .ThenInclude(n => n.Template)
                .Include(nd => nd.Notification)
                    .ThenInclude(n => n.Recipients.Where(r => r.UserId == userId))
                .AsQueryable();

            // Apply filters
            if (filter.ToLower() == "unread")
            {
                query = query.Where(nd => nd.Notification.Recipients.Any() && !nd.Notification.Recipients.First().IsRead);
            }
            else if (filter.ToLower() == "read")
            {
                query = query.Where(nd => nd.Notification.Recipients.Any() && nd.Notification.Recipients.First().IsRead);
            }

            if (!string.IsNullOrEmpty(priorityFilter))
            {
                query = query.Where(nd => nd.Notification.Priority.ToLower() == priorityFilter.ToLower());
            }

            var totalCount = await query.CountAsync();

            var notifications = await query
                .OrderByDescending(nd => nd.Notification.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(nd => new InAppNotificationDto
                {
                    NotificationId = nd.NotificationId,
                    Title = nd.Notification.Title,
                    Message = nd.Notification.Template.PushTemplate ?? nd.Notification.Title,
                    Priority = nd.Notification.Priority,
                    IsRead = nd.Notification.Recipients.Any() && nd.Notification.Recipients.First().IsRead,
                    IsDismissed = nd.Notification.Recipients.Any() && nd.Notification.Recipients.First().IsDismissed,
                    CreatedDate = nd.Notification.CreatedDate,
                    IconClass = GetIconClass(nd.Notification.SourceEntityType),
                    ActionUrl = nd.Notification.ActionUrl
                })
                .ToListAsync();

            return new PaginatedResult<InAppNotificationDto>
            {
                Data = notifications,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Get stats for InApp Notifications page (left sidebar)
        /// </summary>
        public async Task<InAppStatsDto> GetInAppStatsAsync(int userId)
        {
            var notifications = _context.NotificationRecipients
                .Where(nr => nr.UserId == userId && !nr.IsDismissed)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.Deliveries.Where(d => d.Channel.ChannelType == "InApp"));

            return new InAppStatsDto
            {
                AllCount = await notifications.CountAsync(),
                UnreadCount = await notifications.Where(nr => !nr.IsRead).CountAsync(),
                ReadCount = await notifications.Where(nr => nr.IsRead).CountAsync(),
                HighPriorityCount = await notifications
                    .Where(nr => nr.Notification.Priority == "High" || nr.Notification.Priority == "Urgent")
                    .CountAsync()
            };
        }

        // ========================================================================
        // COMMON (Used by all pages)
        // ========================================================================

        /// <summary>
        /// Get notification details by ID (for offcanvas modal)
        /// </summary>
        public async Task<NotificationDetailDto?> GetNotificationByIdAsync(long notificationId, int userId)
        {
            var recipient = await _context.NotificationRecipients
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.Template)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.Deliveries)
                        .ThenInclude(d => d.Channel)
                .FirstOrDefaultAsync(nr =>
                    nr.NotificationId == notificationId &&
                    nr.UserId == userId
                );

            if (recipient == null)
            {
                return null;
            }

            var delivery = recipient.Notification.Deliveries
                .FirstOrDefault(d => d.RecipientUserId == userId);

            return new NotificationDetailDto
            {
                NotificationId = recipient.NotificationId,
                Title = recipient.Notification.Title,
                FullMessage = recipient.Notification.Message,
                Priority = recipient.Notification.Priority,
                Category = recipient.Notification.Template.Category ?? "General",
                ChannelType = delivery?.Channel.ChannelType ?? "InApp",
                IsRead = recipient.IsRead,
                IsDismissed = recipient.IsDismissed,
                CreatedDate = recipient.Notification.CreatedDate,
                ReadDate = recipient.ReadDate,
                ActionUrl = recipient.Notification.ActionUrl,
                SourceEntityType = recipient.Notification.SourceEntityType,
                SourceEntityId = recipient.Notification.SourceEntityId,
                DeliveryStatus = delivery?.Status,
                RecipientAddress = delivery?.RecipientAddress,
                SentDate = delivery?.SentDate,
                ErrorMessage = delivery?.ErrorMessage
            };
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        public async Task<bool> MarkAsReadAsync(long notificationId, int userId)
        {
            var recipient = await _context.NotificationRecipients
                .FirstOrDefaultAsync(nr =>
                    nr.NotificationId == notificationId &&
                    nr.UserId == userId
                );

            if (recipient == null)
            {
                return false;
            }

            if (!recipient.IsRead)
            {
                recipient.IsRead = true;
                recipient.ReadDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Marked notification {NotificationId} as read for user {UserId}",
                    notificationId,
                    userId
                );
            }

            return true;
        }

        /// <summary>
        /// Mark notification as dismissed
        /// </summary>
        public async Task<bool> MarkAsDismissedAsync(long notificationId, int userId)
        {
            var recipient = await _context.NotificationRecipients
                .FirstOrDefaultAsync(nr =>
                    nr.NotificationId == notificationId &&
                    nr.UserId == userId
                );

            if (recipient == null)
            {
                return false;
            }

            if (!recipient.IsDismissed)
            {
                recipient.IsDismissed = true;
                recipient.DismissedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Marked notification {NotificationId} as dismissed for user {UserId}",
                    notificationId,
                    userId
                );
            }

            return true;
        }

        /// <summary>
        /// Get unread notification count (for bell badge)
        /// </summary>
        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.NotificationRecipients
                .Where(nr => nr.UserId == userId && !nr.IsRead && !nr.IsDismissed)
                .CountAsync();
        }

        /// <summary>
        /// Get recent notifications for bell dropdown
        /// </summary>
        public async Task<List<InAppNotificationDto>> GetRecentNotificationsAsync(int userId, int count = 5)
        {
            return await _context.NotificationRecipients
                .Where(nr => nr.UserId == userId && !nr.IsDismissed)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.Template)
                .OrderByDescending(nr => nr.Notification.CreatedDate)
                .Take(count)
                .Select(nr => new InAppNotificationDto
                {
                    NotificationId = nr.NotificationId,
                    Title = nr.Notification.Title,
                    Message = nr.Notification.Template.PushTemplate ?? nr.Notification.Title,
                    Priority = nr.Notification.Priority,
                    IsRead = nr.IsRead,
                    IsDismissed = nr.IsDismissed,
                    CreatedDate = nr.Notification.CreatedDate,
                    IconClass = GetIconClass(nr.Notification.SourceEntityType),
                    ActionUrl = nr.Notification.ActionUrl
                })
                .ToListAsync();
        }

        // ========================================================================
        // HELPER METHODS
        // ========================================================================

        /// <summary>
        /// Get channels for notification based on template defaults or custom channels
        /// </summary>
        private async Task<List<NotificationChannel>> GetChannelsForNotificationAsync(
            NotificationTemplate template,
            List<string>? customChannels)
        {
            List<string> channelTypes;

            if (customChannels != null && customChannels.Any())
            {
                channelTypes = customChannels;
            }
            else if (!string.IsNullOrEmpty(template.DefaultChannels))
            {
                try
                {
                    channelTypes = JsonSerializer.Deserialize<List<string>>(template.DefaultChannels) ?? new List<string> { "InApp" };
                }
                catch (JsonException)
                {
                    _logger.LogWarning("Error parsing DefaultChannels for template {TemplateCode}, using InApp only", template.TemplateCode);
                    channelTypes = new List<string> { "InApp" };
                }
            }
            else
            {
                channelTypes = new List<string> { "InApp" };
            }

            return await _context.NotificationChannels
                .Where(c => c.IsEnabled && channelTypes.Contains(c.ChannelType))
                .ToListAsync();
        }

        /// <summary>
        /// Strip HTML tags from string
        /// </summary>
        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return string.Empty;
            }

            return Regex.Replace(html, "<.*?>", string.Empty);
        }

        /// <summary>
        /// Get icon class based on source entity type
        /// </summary>
        private string GetIconClass(string? sourceEntityType)
        {
            return sourceEntityType switch
            {
                "FormSubmission" => "ri-file-text-line",
                "WorkflowProgress" => "ri-task-line",
                "FormAssignment" => "ri-calendar-check-line",
                _ => "ri-notification-line"
            };
        }
    }
}
