using FormReporting.Data;
using FormReporting.Models.ViewModels.Notifications;
using FormReporting.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FormReporting.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;
        private readonly ApplicationDbContext _context;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger,
            ApplicationDbContext context)
        {
            _notificationService = notificationService;
            _logger = logger;
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // ========================================================================
        // PAGE ACTIONS
        // ========================================================================

        /// <summary>
        /// All Notifications page - DataTable view with stat cards
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search = null,
            string? status = null,
            string? category = null,
            string? priority = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var stats = await _notificationService.GetNotificationStatsAsync(userId);
                var notifications = await _notificationService.GetAllNotificationsAsync(
                    userId, search, category, priority, status);

                var viewModel = new AllNotificationsViewModel
                {
                    Stats = stats,
                    Notifications = notifications,
                    SearchTerm = search,
                    CategoryFilter = category,
                    PriorityFilter = priority,
                    StatusFilter = status
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading all notifications page");
                TempData["Error"] = "Failed to load notifications.";
                return View(new AllNotificationsViewModel());
            }
        }

        /// <summary>
        /// Email Notifications page - Mailbox view (sidebar + list + offcanvas)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Email()
        {
            try
            {
                var userId = GetCurrentUserId();
                var stats = await _notificationService.GetEmailStatsAsync(userId);
                var notifications = await _notificationService.GetEmailNotificationsAsync(userId);

                var viewModel = new EmailNotificationsViewModel
                {
                    Stats = stats,
                    Emails = notifications
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading email notifications page");
                TempData["Error"] = "Failed to load email notifications.";
                return View(new EmailNotificationsViewModel());
            }
        }

        /// <summary>
        /// InApp Notifications page - Mailbox view (sidebar + list + offcanvas)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> InApp()
        {
            try
            {
                var userId = GetCurrentUserId();
                var stats = await _notificationService.GetInAppStatsAsync(userId);
                var notifications = await _notificationService.GetInAppNotificationsAsync(userId);

                var viewModel = new InAppNotificationsViewModel
                {
                    Stats = stats,
                    Notifications = notifications
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading in-app notifications page");
                TempData["Error"] = "Failed to load in-app notifications.";
                return View(new InAppNotificationsViewModel());
            }
        }

        // ========================================================================
        // API ENDPOINTS (AJAX)
        // ========================================================================

        /// <summary>
        /// Get all notifications for DataTable (Index page)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? status = null,
            string? channel = null,
            string? priority = null,
            string? search = null,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation(
                    "GetAll called - UserId: {UserId}, Status: {Status}, Channel: {Channel}, Priority: {Priority}, Search: {Search}, Page: {Page}",
                    userId, status, channel, priority, search, page);

                var result = await _notificationService.GetAllNotificationsAsync(
                    userId, search, channel, priority, status, page, pageSize);

                _logger.LogInformation(
                    "GetAll result - Count: {Count}, TotalCount: {TotalCount}, Page: {Page}, TotalPages: {TotalPages}",
                    result.Data?.Count ?? 0, result.TotalCount, result.Page, result.TotalPages);

                return Json(new
                {
                    success = true,
                    data = result.Data,
                    pagination = new
                    {
                        page = result.Page,
                        pageSize = result.PageSize,
                        totalCount = result.TotalCount,
                        totalPages = result.TotalPages
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all notifications");
                return Json(new { success = false, error = "Failed to load notifications" });
            }
        }

        /// <summary>
        /// Get email notification list (Email page)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetEmailList(
            string filter = "all",
            string? recipientType = null,
            string? category = null,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.GetEmailNotificationsAsync(
                    userId, filter, recipientType, category, page, pageSize);

                return Json(new { data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email list");
                return Json(new { error = "Failed to load emails" });
            }
        }

        /// <summary>
        /// Get in-app notification list (InApp page)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetInAppList(
            string filter = "all",
            string? priorityFilter = null,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.GetInAppNotificationsAsync(
                    userId, filter, priorityFilter, page, pageSize);

                return Json(new { data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting in-app list");
                return Json(new { error = "Failed to load notifications" });
            }
        }

        /// <summary>
        /// Get notification details for offcanvas
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDetails(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var notification = await _notificationService.GetNotificationDetailsAsync(id, userId);

                if (notification == null)
                    return NotFound(new { error = "Notification not found" });

                return Json(new { success = true, data = notification });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification details for {NotificationId}", id);
                return Json(new { success = false, error = "Failed to load notification details" });
            }
        }

        /// <summary>
        /// Get stat card counts
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                var stats = await _notificationService.GetNotificationStatsAsync(userId);

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification stats");
                return Json(new { success = false, error = "Failed to load stats" });
            }
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _notificationService.MarkAsReadAsync(id, userId);

                if (success)
                    return Json(new { success = true, message = "Notification marked as read" });

                return Json(new { success = false, error = "Failed to mark as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
                return Json(new { success = false, error = "An error occurred" });
            }
        }

        /// <summary>
        /// Mark notification as unread
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAsUnread(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _notificationService.MarkAsUnreadAsync(id, userId);

                if (success)
                    return Json(new { success = true, message = "Notification marked as unread" });

                return Json(new { success = false, error = "Failed to mark as unread" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as unread", id);
                return Json(new { success = false, error = "An error occurred" });
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _notificationService.MarkAllAsReadAsync(userId);

                return Json(new {
                    success = true,
                    message = $"{count} notification{(count != 1 ? "s" : "")} marked as read",
                    count = count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return Json(new { success = false, error = "Failed to mark all as read" });
            }
        }

        /// <summary>
        /// Delete notification
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _notificationService.DeleteNotificationAsync(id, userId);

                if (success)
                    return Json(new { success = true, message = "Notification deleted" });

                return Json(new { success = false, error = "Failed to delete notification" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId}", id);
                return Json(new { success = false, error = "An error occurred" });
            }
        }

        /// <summary>
        /// Bulk mark notifications as read
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BulkMarkAsRead(List<long> ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                    return Json(new { success = false, error = "No notifications selected" });

                var userId = GetCurrentUserId();
                var successCount = 0;

                foreach (var id in ids)
                {
                    var success = await _notificationService.MarkAsReadAsync(id, userId);
                    if (success) successCount++;
                }

                return Json(new
                {
                    success = true,
                    message = $"{successCount} notification{(successCount != 1 ? "s" : "")} marked as read",
                    count = successCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk mark as read");
                return Json(new { success = false, error = "An error occurred" });
            }
        }

        /// <summary>
        /// Bulk delete notifications
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BulkDelete(List<long> ids)
        {
            try
            {
                if (ids == null || !ids.Any())
                    return Json(new { success = false, error = "No notifications selected" });

                var userId = GetCurrentUserId();
                var successCount = 0;

                foreach (var id in ids)
                {
                    var success = await _notificationService.DeleteNotificationAsync(id, userId);
                    if (success) successCount++;
                }

                return Json(new
                {
                    success = true,
                    message = $"{successCount} notification{(successCount != 1 ? "s" : "")} deleted",
                    count = successCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk delete");
                return Json(new { success = false, error = "An error occurred" });
            }
        }

        // ========================================================================
        // TEST DATA (Development Only)
        // ========================================================================

        /// <summary>
        /// Generate test notifications - Access at /Notifications/GenerateTestData
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GenerateTestData()
        {
            try
            {
                var userId = GetCurrentUserId();
                var createdCount = 0;

                // Test 1: Urgent form assignment (Email + InApp)
                try
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        TemplateCode = "ASSIGNMENT_CREATED",
                        SourceEntityType = "FormAssignment",
                        SourceEntityId = 1,
                        RecipientUserIds = new List<int> { userId },
                        PlaceholderData = new Dictionary<string, string>
                        {
                            { "RecipientName", "Test User" },
                            { "AssignmentTitle", "Monthly Performance Review" },
                            { "AssignmentType", "Individual" },
                            { "DueDate", DateTime.UtcNow.AddDays(7).ToString("MMM dd, yyyy") },
                            { "DepartmentName", "Finance Department" },
                            { "ActionUrl", "/Forms/Assignments/1" }
                        },
                        CustomPriority = "Urgent",
                        CustomChannels = new List<string> { "Email", "InApp" },
                        ActionUrl = "/Forms/Assignments/1"
                    });
                    createdCount++;
                }
                catch { }

                // Test 2: High priority form submission (Email + InApp)
                try
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        TemplateCode = "FORM_SUBMITTED",
                        SourceEntityType = "FormSubmission",
                        SourceEntityId = 1,
                        RecipientUserIds = new List<int> { userId },
                        PlaceholderData = new Dictionary<string, string>
                        {
                            { "RecipientName", "Test User" },
                            { "SubmitterName", "John Doe" },
                            { "FormName", "Leave Request Form" },
                            { "SubmittedDate", DateTime.UtcNow.AddHours(-2).ToString("MMM dd, yyyy h:mm tt") },
                            { "ActionUrl", "/Workflows/Review/1" }
                        },
                        CustomPriority = "High",
                        CustomChannels = new List<string> { "Email", "InApp" },
                        ActionUrl = "/Workflows/Review/1"
                    });
                    createdCount++;
                }
                catch { }

                // Test 3: Normal workflow step (InApp only)
                try
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        TemplateCode = "WORKFLOW_ASSIGNED",
                        SourceEntityType = "WorkflowProgress",
                        SourceEntityId = 2,
                        RecipientUserIds = new List<int> { userId },
                        PlaceholderData = new Dictionary<string, string>
                        {
                            { "RecipientName", "Test User" },
                            { "FormName", "Equipment Request" },
                            { "StepName", "Manager Approval" },
                            { "StepOrder", "2" },
                            { "SubmitterName", "Jane Smith" },
                            { "DueDate", DateTime.UtcNow.AddDays(3).ToString("MMM dd, yyyy") },
                            { "ActionUrl", "/Workflows/Review/2" }
                        },
                        CustomPriority = "Normal",
                        CustomChannels = new List<string> { "InApp" },
                        ActionUrl = "/Workflows/Review/2"
                    });
                    createdCount++;
                }
                catch { }

                // Test 4: Form approved (Email + InApp)
                try
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        TemplateCode = "FORM_APPROVED",
                        SourceEntityType = "WorkflowProgress",
                        SourceEntityId = 3,
                        RecipientUserIds = new List<int> { userId },
                        PlaceholderData = new Dictionary<string, string>
                        {
                            { "RecipientName", "Test User" },
                            { "FormName", "Travel Request" },
                            { "SubmittedDate", DateTime.UtcNow.AddDays(-5).ToString("MMM dd, yyyy") },
                            { "ApproverName", "Manager Smith" },
                            { "ActionUrl", "/Forms/Responses/1" }
                        },
                        CustomPriority = "Normal",
                        CustomChannels = new List<string> { "Email", "InApp" },
                        ActionUrl = "/Forms/Responses/1"
                    });
                    createdCount++;
                }
                catch { }

                // Test 5: Deadline reminder (High, Email + InApp)
                try
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        TemplateCode = "DEADLINE_REMINDER",
                        SourceEntityType = "FormAssignment",
                        SourceEntityId = 2,
                        RecipientUserIds = new List<int> { userId },
                        PlaceholderData = new Dictionary<string, string>
                        {
                            { "RecipientName", "Test User" },
                            { "AssignmentTitle", "Quarterly Report" },
                            { "DueDate", DateTime.UtcNow.AddHours(24).ToString("MMM dd, yyyy h:mm tt") },
                            { "ActionUrl", "/Forms/Assignments/2" }
                        },
                        CustomPriority = "High",
                        CustomChannels = new List<string> { "Email", "InApp" },
                        ActionUrl = "/Forms/Assignments/2"
                    });
                    createdCount++;
                }
                catch { }

                return Json(new
                {
                    success = true,
                    message = $"Created {createdCount} test notifications! Refresh your notifications page.",
                    count = createdCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating test data");
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// DEBUG: Direct database query to test data accessibility
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DebugData()
        {
            try
            {
                var userId = GetCurrentUserId();

                var recipients = await _context.NotificationRecipients
                    .Where(nr => nr.UserId == userId)
                    .Include(nr => nr.Notification)
                    .ToListAsync();

                return Json(new
                {
                    userId = userId,
                    recipientCount = recipients.Count,
                    recipients = recipients.Select(nr => new
                    {
                        recipientId = nr.RecipientId,
                        notificationId = nr.NotificationId,
                        userId = nr.UserId,
                        isRead = nr.IsRead,
                        isDismissed = nr.IsDismissed,
                        notification = new
                        {
                            title = nr.Notification?.Title ?? "NULL",
                            message = nr.Notification?.Message ?? "NULL",
                            priority = nr.Notification?.Priority ?? "NULL",
                            createdDate = nr.Notification?.CreatedDate
                        }
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
