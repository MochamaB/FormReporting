using FormReporting.Data;
using FormReporting.Models.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FormReporting.Services.Notifications
{
    /// <summary>
    /// Service for managing notification templates
    /// </summary>
    public class NotificationTemplateService : INotificationTemplateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<NotificationTemplateService> _logger;
        private const string CACHE_KEY_PREFIX = "NotificationTemplate_";
        private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(1);

        public NotificationTemplateService(
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger<NotificationTemplateService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Get template by template code (with caching)
        /// </summary>
        public async Task<NotificationTemplate?> GetTemplateByCodeAsync(string templateCode)
        {
            var cacheKey = $"{CACHE_KEY_PREFIX}{templateCode}";

            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out NotificationTemplate? cachedTemplate))
            {
                return cachedTemplate;
            }

            // Not in cache, get from database
            var template = await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.TemplateCode == templateCode && t.IsActive);

            if (template != null)
            {
                // Cache the template
                _cache.Set(cacheKey, template, CACHE_DURATION);
            }
            else
            {
                _logger.LogWarning("Template with code {TemplateCode} not found", templateCode);
            }

            return template;
        }

        /// <summary>
        /// Render template with placeholder data
        /// Returns (subject, body, pushMessage)
        /// </summary>
        public async Task<(string subject, string body, string pushMessage)> RenderTemplateAsync(
            string templateCode,
            Dictionary<string, string> placeholderData)
        {
            var template = await GetTemplateByCodeAsync(templateCode);

            if (template == null)
            {
                throw new InvalidOperationException($"Template '{templateCode}' not found");
            }

            // Replace placeholders in all template parts
            var subject = ReplacePlaceholders(template.SubjectTemplate, placeholderData);
            var body = ReplacePlaceholders(template.BodyTemplate, placeholderData);
            var pushMessage = ReplacePlaceholders(template.PushTemplate ?? template.SubjectTemplate, placeholderData);

            return (subject, body, pushMessage);
        }

        /// <summary>
        /// Replace placeholders in a string with actual values
        /// Placeholders use {{PlaceholderName}} syntax
        /// </summary>
        public string ReplacePlaceholders(string template, Dictionary<string, string> placeholderData)
        {
            if (string.IsNullOrEmpty(template))
            {
                return template;
            }

            // Replace all {{PlaceholderName}} with actual values
            var result = template;

            foreach (var placeholder in placeholderData)
            {
                var pattern = $"{{{{{placeholder.Key}}}}}"; // Matches {{PlaceholderName}}
                result = result.Replace(pattern, placeholder.Value ?? "[Not Available]");
            }

            // Find any remaining unreplaced placeholders and replace with default
            var unreplacedPattern = @"\{\{([^}]+)\}\}";
            var matches = Regex.Matches(result, unreplacedPattern);

            foreach (Match match in matches)
            {
                var placeholderName = match.Groups[1].Value;
                _logger.LogWarning(
                    "Placeholder {{{{PlaceholderName}}}} not found in data, replacing with default",
                    placeholderName
                );
                result = result.Replace(match.Value, "[Not Available]");
            }

            return result;
        }

        /// <summary>
        /// Validate that all required placeholders are present
        /// </summary>
        public async Task<bool> ValidatePlaceholdersAsync(
            string templateCode,
            Dictionary<string, string> placeholderData)
        {
            var template = await GetTemplateByCodeAsync(templateCode);

            if (template == null)
            {
                _logger.LogError("Template {TemplateCode} not found for validation", templateCode);
                return false;
            }

            // Parse available placeholders from JSON
            if (string.IsNullOrEmpty(template.AvailablePlaceholders))
            {
                return true; // No required placeholders
            }

            try
            {
                var requiredPlaceholders = JsonSerializer.Deserialize<string[]>(template.AvailablePlaceholders);

                if (requiredPlaceholders == null || requiredPlaceholders.Length == 0)
                {
                    return true;
                }

                // Check if all required placeholders are provided
                var missingPlaceholders = requiredPlaceholders
                    .Where(p => !placeholderData.ContainsKey(p))
                    .ToList();

                if (missingPlaceholders.Any())
                {
                    _logger.LogWarning(
                        "Missing placeholders for template {TemplateCode}: {MissingPlaceholders}",
                        templateCode,
                        string.Join(", ", missingPlaceholders)
                    );
                    return false;
                }

                return true;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing AvailablePlaceholders for template {TemplateCode}", templateCode);
                return false;
            }
        }
    }
}
