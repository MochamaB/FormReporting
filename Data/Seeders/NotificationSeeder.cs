using FormReporting.Models.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FormReporting.Data.Seeders
{
    /// <summary>
    /// Seeds notification system initial data
    /// Includes channels, templates, and alert definitions
    /// </summary>
    public static class NotificationSeeder
    {
        /// <summary>
        /// Seeds all notification-related data
        /// Must be called after Users are seeded (AlertDefinitions require CreatedBy)
        /// </summary>
        public static void SeedNotificationData(ApplicationDbContext context)
        {
            SeedNotificationChannels(context);
            SeedNotificationTemplates(context);
            SeedAlertDefinitions(context);
        }

        /// <summary>
        /// Seeds notification delivery channels (Email, InApp)
        /// </summary>
        private static void SeedNotificationChannels(ApplicationDbContext context)
        {
            // Check if channels already exist
            if (context.NotificationChannels.Any())
            {
                return; // Data already seeded
            }

            var channels = new List<NotificationChannel>
            {
                // ============================================================================
                // EMAIL CHANNEL
                // ============================================================================
                new NotificationChannel
                {
                    ChannelName = "Email",
                    ChannelType = "Email",
                    Description = "Email delivery via SMTP",
                    IsEnabled = true,
                    Configuration = JsonSerializer.Serialize(new
                    {
                        SmtpHost = "smtp.gmail.com",
                        SmtpPort = 587,
                        UseSsl = true,
                        FromEmail = "noreply@ktda.co.ke",
                        FromName = "KTDA Form Reporting System"
                    }),
                    Provider = "SMTP",
                    MaxRetries = 3,
                    RetryDelayMinutes = 5,
                    DailySendLimit = 10000,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================================
                // IN-APP CHANNEL
                // ============================================================================
                new NotificationChannel
                {
                    ChannelName = "InApp",
                    ChannelType = "InApp",
                    Description = "In-application notifications via database and SignalR",
                    IsEnabled = true,
                    Configuration = JsonSerializer.Serialize(new
                    {
                        UseSignalR = true,
                        HubPath = "/notificationHub",
                        StorageType = "Database"
                    }),
                    Provider = "SignalR",
                    MaxRetries = 1,
                    RetryDelayMinutes = 0,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.NotificationChannels.AddRange(channels);
            context.SaveChanges();
        }

        /// <summary>
        /// Seeds notification templates for all notification types
        /// </summary>
        private static void SeedNotificationTemplates(ApplicationDbContext context)
        {
            // Check if templates already exist
            if (context.NotificationTemplates.Any())
            {
                return; // Data already seeded
            }

            var templates = new List<NotificationTemplate>
            {
                // ============================================================================
                // FORMS MODULE NOTIFICATIONS
                // ============================================================================

                // 1. FORM_SUBMITTED
                new NotificationTemplate
                {
                    TemplateName = "Form Submitted",
                    TemplateCode = "FORM_SUBMITTED",
                    Category = "Forms",
                    Description = "Sent when a user submits a form",
                    SubjectTemplate = "New Form Submission: {{FormName}}",
                    BodyTemplate = @"
<p>Hello {{RecipientName}},</p>
<p>A new form has been submitted that requires your attention:</p>
<ul>
    <li><strong>Form:</strong> {{FormName}}</li>
    <li><strong>Submitted By:</strong> {{SubmitterName}}</li>
    <li><strong>Submitted Date:</strong> {{SubmittedDate}}</li>
</ul>
<p><a href='{{ActionUrl}}' style='display:inline-block;padding:10px 20px;background-color:#007bff;color:white;text-decoration:none;border-radius:4px;'>View Submission</a></p>
<p>Thank you,<br/>KTDA Form Reporting System</p>",
                    PushTemplate = "New form submission: {{FormName}} by {{SubmitterName}}",
                    DefaultPriority = "Normal",
                    DefaultChannels = JsonSerializer.Serialize(new[] { "Email", "InApp" }),
                    AvailablePlaceholders = JsonSerializer.Serialize(new[]
                    {
                        "RecipientName", "FormName", "FormCode", "SubmitterName",
                        "SubmittedDate", "ActionUrl"
                    }),
                    IsActive = true,
                    IsSystemTemplate = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================================
                // WORKFLOW MODULE NOTIFICATIONS
                // ============================================================================

                // 2. WORKFLOW_ASSIGNED
                new NotificationTemplate
                {
                    TemplateName = "Workflow Step Assigned",
                    TemplateCode = "WORKFLOW_ASSIGNED",
                    Category = "Workflows",
                    Description = "Sent when a workflow step is assigned to a user",
                    SubjectTemplate = "Action Required: {{StepName}} for {{FormName}}",
                    BodyTemplate = @"
<p>Hello {{RecipientName}},</p>
<p>A workflow step has been assigned to you:</p>
<ul>
    <li><strong>Form:</strong> {{FormName}}</li>
    <li><strong>Step:</strong> {{StepName}} ({{StepOrder}})</li>
    <li><strong>Submitted By:</strong> {{SubmitterName}}</li>
    <li><strong>Due Date:</strong> {{DueDate}}</li>
</ul>
<p><a href='{{ActionUrl}}' style='display:inline-block;padding:10px 20px;background-color:#28a745;color:white;text-decoration:none;border-radius:4px;'>Review Now</a></p>
<p>Thank you,<br/>KTDA Form Reporting System</p>",
                    PushTemplate = "{{StepName}} assigned to you for {{FormName}}. Due: {{DueDate}}",
                    DefaultPriority = "High",
                    DefaultChannels = JsonSerializer.Serialize(new[] { "Email", "InApp" }),
                    AvailablePlaceholders = JsonSerializer.Serialize(new[]
                    {
                        "RecipientName", "FormName", "StepName", "StepOrder",
                        "SubmitterName", "DueDate", "ActionUrl"
                    }),
                    IsActive = true,
                    IsSystemTemplate = true,
                    CreatedDate = DateTime.UtcNow
                },

                // 3. STEP_COMPLETED
                new NotificationTemplate
                {
                    TemplateName = "Workflow Step Completed",
                    TemplateCode = "STEP_COMPLETED",
                    Category = "Workflows",
                    Description = "Sent when a workflow step is completed",
                    SubjectTemplate = "{{StepName}} Completed for {{FormName}}",
                    BodyTemplate = @"
<p>Hello {{RecipientName}},</p>
<p>A workflow step for your submission has been completed:</p>
<ul>
    <li><strong>Form:</strong> {{FormName}}</li>
    <li><strong>Step:</strong> {{StepName}}</li>
    <li><strong>Completed By:</strong> {{ApproverName}}</li>
    <li><strong>Status:</strong> Approved</li>
</ul>
<p><a href='{{ActionUrl}}' style='display:inline-block;padding:10px 20px;background-color:#007bff;color:white;text-decoration:none;border-radius:4px;'>View Details</a></p>
<p>Thank you,<br/>KTDA Form Reporting System</p>",
                    PushTemplate = "{{StepName}} completed for your {{FormName}} submission",
                    DefaultPriority = "Normal",
                    DefaultChannels = JsonSerializer.Serialize(new[] { "Email", "InApp" }),
                    AvailablePlaceholders = JsonSerializer.Serialize(new[]
                    {
                        "RecipientName", "FormName", "StepName", "ApproverName", "ActionUrl"
                    }),
                    IsActive = true,
                    IsSystemTemplate = true,
                    CreatedDate = DateTime.UtcNow
                },

                // 4. FORM_APPROVED
                new NotificationTemplate
                {
                    TemplateName = "Form Approved",
                    TemplateCode = "FORM_APPROVED",
                    Category = "Workflows",
                    Description = "Sent when a form completes all workflow steps and is fully approved",
                    SubjectTemplate = "Form Approved: {{FormName}}",
                    BodyTemplate = @"
<p>Hello {{RecipientName}},</p>
<p>Good news! Your form submission has been fully approved:</p>
<ul>
    <li><strong>Form:</strong> {{FormName}}</li>
    <li><strong>Submitted Date:</strong> {{SubmittedDate}}</li>
    <li><strong>Final Approver:</strong> {{ApproverName}}</li>
</ul>
<p><a href='{{ActionUrl}}' style='display:inline-block;padding:10px 20px;background-color:#28a745;color:white;text-decoration:none;border-radius:4px;'>View Approved Form</a></p>
<p>Thank you,<br/>KTDA Form Reporting System</p>",
                    PushTemplate = "Your {{FormName}} submission has been approved!",
                    DefaultPriority = "Normal",
                    DefaultChannels = JsonSerializer.Serialize(new[] { "Email", "InApp" }),
                    AvailablePlaceholders = JsonSerializer.Serialize(new[]
                    {
                        "RecipientName", "FormName", "SubmittedDate", "ApproverName", "ActionUrl"
                    }),
                    IsActive = true,
                    IsSystemTemplate = true,
                    CreatedDate = DateTime.UtcNow
                },

                // 5. FORM_REJECTED
                new NotificationTemplate
                {
                    TemplateName = "Form Rejected",
                    TemplateCode = "FORM_REJECTED",
                    Category = "Workflows",
                    Description = "Sent when a workflow step is rejected",
                    SubjectTemplate = "Form Rejected: {{FormName}}",
                    BodyTemplate = @"
<p>Hello {{RecipientName}},</p>
<p>Your form submission has been rejected:</p>
<ul>
    <li><strong>Form:</strong> {{FormName}}</li>
    <li><strong>Rejected By:</strong> {{RejectorName}}</li>
    <li><strong>Reason:</strong> {{RejectionReason}}</li>
</ul>
<p>Please review the feedback and resubmit if necessary.</p>
<p><a href='{{ActionUrl}}' style='display:inline-block;padding:10px 20px;background-color:#dc3545;color:white;text-decoration:none;border-radius:4px;'>View Details</a></p>
<p>Thank you,<br/>KTDA Form Reporting System</p>",
                    PushTemplate = "Your {{FormName}} submission was rejected. Reason: {{RejectionReason}}",
                    DefaultPriority = "High",
                    DefaultChannels = JsonSerializer.Serialize(new[] { "Email", "InApp" }),
                    AvailablePlaceholders = JsonSerializer.Serialize(new[]
                    {
                        "RecipientName", "FormName", "RejectorName", "RejectionReason", "ActionUrl"
                    }),
                    IsActive = true,
                    IsSystemTemplate = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================================
                // ASSIGNMENTS MODULE NOTIFICATIONS
                // ============================================================================

                // 6. ASSIGNMENT_CREATED
                new NotificationTemplate
                {
                    TemplateName = "Assignment Created",
                    TemplateCode = "ASSIGNMENT_CREATED",
                    Category = "Assignments",
                    Description = "Sent when a new form assignment is created",
                    SubjectTemplate = "New Assignment: {{AssignmentTitle}}",
                    BodyTemplate = @"
<p>Hello {{RecipientName}},</p>
<p>A new form has been assigned to you:</p>
<ul>
    <li><strong>Form:</strong> {{AssignmentTitle}}</li>
    <li><strong>Assignment Type:</strong> {{AssignmentType}}</li>
    <li><strong>Due Date:</strong> {{DueDate}}</li>
    <li><strong>Department:</strong> {{DepartmentName}}</li>
</ul>
<p>Please complete this form before the due date.</p>
<p><a href='{{ActionUrl}}' style='display:inline-block;padding:10px 20px;background-color:#007bff;color:white;text-decoration:none;border-radius:4px;'>Start Form</a></p>
<p>Thank you,<br/>KTDA Form Reporting System</p>",
                    PushTemplate = "New assignment: {{AssignmentTitle}}. Due: {{DueDate}}",
                    DefaultPriority = "Normal",
                    DefaultChannels = JsonSerializer.Serialize(new[] { "Email", "InApp" }),
                    AvailablePlaceholders = JsonSerializer.Serialize(new[]
                    {
                        "RecipientName", "AssignmentTitle", "AssignmentType",
                        "DueDate", "DepartmentName", "ActionUrl"
                    }),
                    IsActive = true,
                    IsSystemTemplate = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================================
                // ALERT NOTIFICATIONS
                // ============================================================================

                // 7. DEADLINE_REMINDER
                new NotificationTemplate
                {
                    TemplateName = "Deadline Reminder",
                    TemplateCode = "DEADLINE_REMINDER",
                    Category = "Alerts",
                    Description = "Sent 24 hours before assignment deadline",
                    SubjectTemplate = "Reminder: {{AssignmentTitle}} Due Soon",
                    BodyTemplate = @"
<p>Hello {{RecipientName}},</p>
<p><strong>Reminder:</strong> You have an upcoming assignment deadline:</p>
<ul>
    <li><strong>Form:</strong> {{AssignmentTitle}}</li>
    <li><strong>Due Date:</strong> {{DueDate}}</li>
</ul>
<p>Please complete this assignment before the deadline to avoid it becoming overdue.</p>
<p><a href='{{ActionUrl}}' style='display:inline-block;padding:10px 20px;background-color:#ffc107;color:black;text-decoration:none;border-radius:4px;'>Complete Now</a></p>
<p>Thank you,<br/>KTDA Form Reporting System</p>",
                    PushTemplate = "Reminder: {{AssignmentTitle}} due on {{DueDate}}",
                    DefaultPriority = "High",
                    DefaultChannels = JsonSerializer.Serialize(new[] { "Email", "InApp" }),
                    AvailablePlaceholders = JsonSerializer.Serialize(new[]
                    {
                        "RecipientName", "AssignmentTitle", "DueDate", "ActionUrl"
                    }),
                    IsActive = true,
                    IsSystemTemplate = true,
                    CreatedDate = DateTime.UtcNow
                },

                // 8. OVERDUE_ALERT
                new NotificationTemplate
                {
                    TemplateName = "Overdue Alert",
                    TemplateCode = "OVERDUE_ALERT",
                    Category = "Alerts",
                    Description = "Sent when assignment is past due date",
                    SubjectTemplate = "OVERDUE: {{AssignmentTitle}}",
                    BodyTemplate = @"
<p>Hello {{RecipientName}},</p>
<p><strong>URGENT:</strong> The following assignment is now overdue:</p>
<ul>
    <li><strong>Form:</strong> {{AssignmentTitle}}</li>
    <li><strong>Due Date:</strong> {{DueDate}}</li>
    <li><strong>Days Overdue:</strong> {{DaysOverdue}}</li>
</ul>
<p>Please complete this assignment immediately. Your supervisor has been notified.</p>
<p><a href='{{ActionUrl}}' style='display:inline-block;padding:10px 20px;background-color:#dc3545;color:white;text-decoration:none;border-radius:4px;'>Complete Now</a></p>
<p>Thank you,<br/>KTDA Form Reporting System</p>",
                    PushTemplate = "OVERDUE: {{AssignmentTitle}} ({{DaysOverdue}})",
                    DefaultPriority = "Urgent",
                    DefaultChannels = JsonSerializer.Serialize(new[] { "Email", "InApp" }),
                    AvailablePlaceholders = JsonSerializer.Serialize(new[]
                    {
                        "RecipientName", "AssignmentTitle", "DueDate", "DaysOverdue",
                        "SupervisorName", "ActionUrl"
                    }),
                    IsActive = true,
                    IsSystemTemplate = true,
                    CreatedDate = DateTime.UtcNow
                },

                // 9. PENDING_APPROVAL_ALERT
                new NotificationTemplate
                {
                    TemplateName = "Pending Approval Alert",
                    TemplateCode = "PENDING_APPROVAL_ALERT",
                    Category = "Alerts",
                    Description = "Sent when workflow step is pending for >48 hours",
                    SubjectTemplate = "PENDING: {{StepName}} for {{FormName}}",
                    BodyTemplate = @"
<p>Hello {{RecipientName}},</p>
<p><strong>URGENT:</strong> The following workflow step has been pending for {{DaysOverdue}}:</p>
<ul>
    <li><strong>Form:</strong> {{FormName}}</li>
    <li><strong>Step:</strong> {{StepName}}</li>
    <li><strong>Submitted By:</strong> {{SubmitterName}}</li>
</ul>
<p>Please review and take action on this pending approval. Your supervisor has been notified.</p>
<p><a href='{{ActionUrl}}' style='display:inline-block;padding:10px 20px;background-color:#dc3545;color:white;text-decoration:none;border-radius:4px;'>Review Now</a></p>
<p>Thank you,<br/>KTDA Form Reporting System</p>",
                    PushTemplate = "PENDING: {{StepName}} for {{FormName}} ({{DaysOverdue}})",
                    DefaultPriority = "Urgent",
                    DefaultChannels = JsonSerializer.Serialize(new[] { "Email", "InApp" }),
                    AvailablePlaceholders = JsonSerializer.Serialize(new[]
                    {
                        "RecipientName", "FormName", "StepName", "SubmitterName",
                        "DaysOverdue", "SupervisorName", "ActionUrl"
                    }),
                    IsActive = true,
                    IsSystemTemplate = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.NotificationTemplates.AddRange(templates);
            context.SaveChanges();
        }

        /// <summary>
        /// Seeds alert definitions for automated monitoring
        /// Requires NotificationTemplates and Users to be seeded first
        /// </summary>
        private static void SeedAlertDefinitions(ApplicationDbContext context)
        {
            // Check if alert definitions already exist
            if (context.AlertDefinitions.Any())
            {
                return; // Data already seeded
            }

            // Get template IDs
            var deadlineTemplate = context.NotificationTemplates.First(t => t.TemplateCode == "DEADLINE_REMINDER");
            var overdueTemplate = context.NotificationTemplates.First(t => t.TemplateCode == "OVERDUE_ALERT");
            var pendingTemplate = context.NotificationTemplates.First(t => t.TemplateCode == "PENDING_APPROVAL_ALERT");

            // Get system admin user for CreatedBy (fallback to first user if system admin not found)
            var systemUser = context.Users.FirstOrDefault(u => u.UserName == "admin")
                ?? context.Users.First();

            var alerts = new List<AlertDefinition>
            {
                // ============================================================================
                // DEADLINE REMINDER ALERT
                // ============================================================================
                new AlertDefinition
                {
                    AlertCode = "DEADLINE_REMINDER",
                    AlertName = "Assignment Deadline Reminder",
                    Description = "Notifies users 24 hours before assignment deadline",
                    AlertType = "Scheduled",
                    TriggerCondition = JsonSerializer.Serialize(new
                    {
                        Entity = "FormAssignment",
                        Condition = "DueDate >= NOW() AND DueDate <= NOW() + INTERVAL 24 HOUR",
                        ExcludeSubmitted = true
                    }),
                    CheckFrequencyMinutes = 60,
                    Severity = "Warning",
                    TemplateId = deadlineTemplate.TemplateId,
                    Recipients = JsonSerializer.Serialize(new[]
                    {
                        new { Type = "Assignee" }
                    }),
                    Channels = JsonSerializer.Serialize(new[] { "Email", "InApp" }),
                    CooldownMinutes = 1440, // 24 hours - only alert once per day
                    IsActive = true,
                    CreatedBy = systemUser.UserId,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================================
                // OVERDUE ALERT
                // ============================================================================
                new AlertDefinition
                {
                    AlertCode = "OVERDUE_ALERT",
                    AlertName = "Overdue Assignment Alert",
                    Description = "Notifies users and supervisors when assignments are overdue",
                    AlertType = "Threshold",
                    TriggerCondition = JsonSerializer.Serialize(new
                    {
                        Entity = "FormAssignment",
                        Condition = "DueDate < NOW() AND SubmissionId IS NULL",
                        ExcludeCompleted = true
                    }),
                    CheckFrequencyMinutes = 60,
                    Severity = "Error",
                    TemplateId = overdueTemplate.TemplateId,
                    Recipients = JsonSerializer.Serialize(new[]
                    {
                        new { Type = "Assignee" },
                        new { Type = "Supervisor" }
                    }),
                    Channels = JsonSerializer.Serialize(new[] { "Email", "InApp" }),
                    CooldownMinutes = 1440, // 24 hours - alert daily until resolved
                    EscalationRules = JsonSerializer.Serialize(new
                    {
                        AfterDays = 3,
                        EscalateTo = new[]
                        {
                            new { Type = "Role", Code = "HOD" }
                        }
                    }),
                    IsActive = true,
                    CreatedBy = systemUser.UserId,
                    CreatedDate = DateTime.UtcNow
                },

                // ============================================================================
                // PENDING APPROVAL ALERT
                // ============================================================================
                new AlertDefinition
                {
                    AlertCode = "PENDING_APPROVAL_ALERT",
                    AlertName = "Pending Approval Escalation",
                    Description = "Escalates workflow steps pending for more than 48 hours",
                    AlertType = "Escalation",
                    TriggerCondition = JsonSerializer.Serialize(new
                    {
                        Entity = "WorkflowProgress",
                        Condition = "Status = 'Pending' AND CreatedDate < NOW() - INTERVAL 48 HOUR",
                        ExcludeCompleted = true
                    }),
                    CheckFrequencyMinutes = 120, // Check every 2 hours
                    Severity = "Critical",
                    TemplateId = pendingTemplate.TemplateId,
                    Recipients = JsonSerializer.Serialize(new[]
                    {
                        new { Type = "Assignee" },
                        new { Type = "Supervisor" }
                    }),
                    Channels = JsonSerializer.Serialize(new[] { "Email", "InApp" }),
                    CooldownMinutes = 1440, // 24 hours
                    EscalationRules = JsonSerializer.Serialize(new
                    {
                        AfterDays = 2,
                        EscalateTo = new[]
                        {
                            new { Type = "Role", Code = "FACTORY_MGR" }
                        }
                    }),
                    IsActive = true,
                    CreatedBy = systemUser.UserId,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.AlertDefinitions.AddRange(alerts);
            context.SaveChanges();
        }
    }
}
