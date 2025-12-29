using FormReporting.Models.Entities.Notifications;

namespace FormReporting.Services.Notifications
{
    /// <summary>
    /// Service for managing notification templates
    /// </summary>
    public interface INotificationTemplateService
    {
        /// <summary>
        /// Get template by template code
        /// </summary>
        Task<NotificationTemplate?> GetTemplateByCodeAsync(string templateCode);

        /// <summary>
        /// Render template with placeholder data
        /// Returns (subject, body, pushMessage)
        /// </summary>
        Task<(string subject, string body, string pushMessage)> RenderTemplateAsync(
            string templateCode,
            Dictionary<string, string> placeholderData
        );

        /// <summary>
        /// Replace placeholders in a string with actual values
        /// </summary>
        string ReplacePlaceholders(string template, Dictionary<string, string> placeholderData);

        /// <summary>
        /// Validate that all required placeholders are present
        /// </summary>
        Task<bool> ValidatePlaceholdersAsync(string templateCode, Dictionary<string, string> placeholderData);
    }
}
