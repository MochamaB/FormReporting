using FormReporting.Models.Entities.Notifications;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text.Json;

namespace FormReporting.Services.Notifications.Providers
{
    /// <summary>
    /// Email delivery provider using SMTP
    /// Configuration is read from NotificationChannel.Configuration JSON
    /// </summary>
    public class EmailProvider : INotificationProvider
    {
        private readonly ILogger<EmailProvider> _logger;

        public string ChannelType => "Email";

        public EmailProvider(ILogger<EmailProvider> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Send email via SMTP (configuration from database)
        /// </summary>
        public async Task<bool> SendAsync(
            NotificationDelivery delivery,
            Notification notification,
            NotificationChannel channel)
        {
            try
            {
                // Parse SMTP configuration from channel
                var config = ParseSmtpConfiguration(channel.Configuration);

                if (config == null)
                {
                    _logger.LogError("Invalid SMTP configuration for channel {ChannelId}", channel.ChannelId);
                    delivery.Status = "Failed";
                    delivery.ErrorMessage = "Invalid SMTP configuration";
                    return false;
                }

                // Create email message
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(config.FromName, config.FromEmail));
                message.To.Add(new MailboxAddress(delivery.RecipientAddress, delivery.RecipientAddress));
                message.Subject = notification.Title;

                // Build HTML body
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = notification.Message
                };
                message.Body = bodyBuilder.ToMessageBody();

                // Send via SMTP
                using var smtpClient = new SmtpClient();

                // Connect to SMTP server
                await smtpClient.ConnectAsync(
                    config.SmtpHost,
                    config.SmtpPort,
                    config.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None
                );

                // Authenticate if credentials provided
                if (!string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
                {
                    await smtpClient.AuthenticateAsync(config.Username, config.Password);
                }

                // Send message
                var result = await smtpClient.SendAsync(message);

                await smtpClient.DisconnectAsync(true);

                // Update delivery status
                delivery.Status = "Sent";
                delivery.SentDate = DateTime.UtcNow;
                delivery.ExternalMessageId = result;
                delivery.ModifiedDate = DateTime.UtcNow;

                _logger.LogInformation(
                    "Email sent successfully to {Recipient} for notification {NotificationId}",
                    delivery.RecipientAddress,
                    notification.NotificationId
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send email to {Recipient} for notification {NotificationId}",
                    delivery.RecipientAddress,
                    notification.NotificationId
                );

                delivery.Status = "Failed";
                delivery.ErrorMessage = ex.Message;
                delivery.ModifiedDate = DateTime.UtcNow;

                return false;
            }
        }

        /// <summary>
        /// Validate SMTP configuration (for admin UI)
        /// </summary>
        public async Task<(bool isValid, List<string> errors)> ValidateConfigurationAsync(string configuration)
        {
            var errors = new List<string>();

            try
            {
                var config = JsonSerializer.Deserialize<SmtpConfiguration>(configuration);

                if (config == null)
                {
                    errors.Add("Invalid JSON configuration");
                    return (false, errors);
                }

                if (string.IsNullOrWhiteSpace(config.SmtpHost))
                    errors.Add("SMTP Host is required");

                if (config.SmtpPort <= 0 || config.SmtpPort > 65535)
                    errors.Add("SMTP Port must be between 1 and 65535");

                if (string.IsNullOrWhiteSpace(config.FromEmail))
                    errors.Add("From Email is required");

                if (string.IsNullOrWhiteSpace(config.FromName))
                    errors.Add("From Name is required");

                // Validate email format
                if (!string.IsNullOrWhiteSpace(config.FromEmail) && !IsValidEmail(config.FromEmail))
                    errors.Add("From Email is not a valid email address");

                return (errors.Count == 0, errors);
            }
            catch (JsonException ex)
            {
                errors.Add($"Invalid JSON: {ex.Message}");
                return (false, errors);
            }
        }

        /// <summary>
        /// Test SMTP connection (for admin UI)
        /// </summary>
        public async Task<(bool success, string message)> TestConnectionAsync(NotificationChannel channel)
        {
            try
            {
                var config = ParseSmtpConfiguration(channel.Configuration);

                if (config == null)
                {
                    return (false, "Invalid SMTP configuration");
                }

                using var smtpClient = new SmtpClient();

                // Try to connect
                await smtpClient.ConnectAsync(
                    config.SmtpHost,
                    config.SmtpPort,
                    config.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None
                );

                // Try to authenticate if credentials provided
                if (!string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
                {
                    await smtpClient.AuthenticateAsync(config.Username, config.Password);
                }

                await smtpClient.DisconnectAsync(true);

                return (true, $"Successfully connected to {config.SmtpHost}:{config.SmtpPort}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP connection test failed for channel {ChannelId}", channel.ChannelId);
                return (false, $"Connection failed: {ex.Message}");
            }
        }

        // ========================================================================
        // HELPER METHODS
        // ========================================================================

        /// <summary>
        /// Parse SMTP configuration from JSON
        /// </summary>
        private SmtpConfiguration? ParseSmtpConfiguration(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<SmtpConfiguration>(json);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse SMTP configuration: {Json}", json);
                return null;
            }
        }

        /// <summary>
        /// Basic email validation
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // ========================================================================
        // CONFIGURATION MODEL
        // ========================================================================

        /// <summary>
        /// SMTP configuration model (matches JSON in NotificationChannel.Configuration)
        /// </summary>
        private class SmtpConfiguration
        {
            public string SmtpHost { get; set; } = string.Empty;
            public int SmtpPort { get; set; } = 587;
            public bool UseSsl { get; set; } = true;
            public string FromEmail { get; set; } = string.Empty;
            public string FromName { get; set; } = string.Empty;
            public string? Username { get; set; }
            public string? Password { get; set; }
        }
    }
}
