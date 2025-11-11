using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormReporting.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Section9_NotificationsAndAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationChannels",
                columns: table => new
                {
                    ChannelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ChannelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Configuration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Credentials = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxRetries = table.Column<int>(type: "int", nullable: false),
                    RetryDelayMinutes = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    DailySendLimit = table.Column<int>(type: "int", nullable: false),
                    DailySendCount = table.Column<int>(type: "int", nullable: false),
                    LastResetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationChannels", x => x.ChannelId);
                    table.CheckConstraint("CK_Channel_Priority", "[Priority] BETWEEN 1 AND 5");
                    table.CheckConstraint("CK_Channel_Type", "[ChannelType] IN ('Email', 'SMS', 'Push', 'InApp', 'Webhook')");
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubjectTemplate = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BodyTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SmsTemplate = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PushTemplate = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AvailablePlaceholders = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultPriority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DefaultChannels = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystemTemplate = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.TemplateId);
                    table.CheckConstraint("CK_Template_Priority", "[DefaultPriority] IN ('Low', 'Normal', 'High', 'Urgent')");
                    table.ForeignKey(
                        name: "FK_NotificationTemplates_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_NotificationTemplates_Users_ModifiedBy",
                        column: x => x.ModifiedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserNotificationPreferences",
                columns: table => new
                {
                    PreferenceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    QuietHoursStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    QuietHoursEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    MinimumPriority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationPreferences", x => x.PreferenceId);
                    table.CheckConstraint("CK_Preference_Frequency", "[Frequency] IN ('Immediate', 'Hourly', 'Daily', 'Weekly', 'Never')");
                    table.CheckConstraint("CK_Preference_Priority", "[MinimumPriority] IN ('Low', 'Normal', 'High', 'Urgent')");
                    table.ForeignKey(
                        name: "FK_UserNotificationPreferences_NotificationChannels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "NotificationChannels",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserNotificationPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlertDefinitions",
                columns: table => new
                {
                    AlertId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlertCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AlertName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AlertType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TriggerCondition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheckFrequencyMinutes = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    Recipients = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Channels = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CooldownMinutes = table.Column<int>(type: "int", nullable: false),
                    AutoResolveCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EscalationRules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastTriggeredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCheckDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TriggerCount = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertDefinitions", x => x.AlertId);
                    table.CheckConstraint("CK_Alert_Severity", "[Severity] IN ('Info', 'Warning', 'Error', 'Critical')");
                    table.ForeignKey(
                        name: "FK_AlertDefinitions_NotificationTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "NotificationTemplates",
                        principalColumn: "TemplateId");
                    table.ForeignKey(
                        name: "FK_AlertDefinitions_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_AlertDefinitions_Users_ModifiedBy",
                        column: x => x.ModifiedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SourceEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SourceEntityId = table.Column<long>(type: "bigint", nullable: true),
                    ActionUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ActionButtonText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateId = table.Column<int>(type: "int", nullable: true),
                    TriggeredBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.CheckConstraint("CK_Notification_Priority", "[Priority] IN ('Low', 'Normal', 'High', 'Urgent')");
                    table.ForeignKey(
                        name: "FK_Notifications_NotificationTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "NotificationTemplates",
                        principalColumn: "TemplateId");
                    table.ForeignKey(
                        name: "FK_Notifications_Users_TriggeredBy",
                        column: x => x.TriggeredBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "AlertHistory",
                columns: table => new
                {
                    HistoryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlertId = table.Column<int>(type: "int", nullable: false),
                    TriggeredDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TriggerDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NotificationId = table.Column<long>(type: "bigint", nullable: true),
                    AcknowledgedBy = table.Column<int>(type: "int", nullable: true),
                    AcknowledgedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgmentNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ResolvedBy = table.Column<int>(type: "int", nullable: true),
                    ResolvedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TimeToAcknowledgeMinutes = table.Column<int>(type: "int", nullable: true),
                    TimeToResolveMinutes = table.Column<int>(type: "int", nullable: true),
                    IsEscalated = table.Column<bool>(type: "bit", nullable: false),
                    EscalatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EscalationDetails = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertHistory", x => x.HistoryId);
                    table.CheckConstraint("CK_AlertHistory_Status", "[Status] IN ('Triggered', 'Acknowledged', 'Resolved', 'AutoResolved', 'Cancelled')");
                    table.ForeignKey(
                        name: "FK_AlertHistory_AlertDefinitions_AlertId",
                        column: x => x.AlertId,
                        principalTable: "AlertDefinitions",
                        principalColumn: "AlertId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlertHistory_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "NotificationId");
                    table.ForeignKey(
                        name: "FK_AlertHistory_Users_AcknowledgedBy",
                        column: x => x.AcknowledgedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_AlertHistory_Users_ResolvedBy",
                        column: x => x.ResolvedBy,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "NotificationDelivery",
                columns: table => new
                {
                    DeliveryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationId = table.Column<long>(type: "bigint", nullable: false),
                    RecipientUserId = table.Column<int>(type: "int", nullable: false),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecipientAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    NextRetryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalMessageId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProviderResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryCost = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationDelivery", x => x.DeliveryId);
                    table.CheckConstraint("CK_Delivery_Status", "[Status] IN ('Pending', 'Sent', 'Delivered', 'Failed', 'Bounced', 'Cancelled')");
                    table.ForeignKey(
                        name: "FK_NotificationDelivery_NotificationChannels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "NotificationChannels",
                        principalColumn: "ChannelId");
                    table.ForeignKey(
                        name: "FK_NotificationDelivery_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "NotificationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationDelivery_Users_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "NotificationRecipients",
                columns: table => new
                {
                    RecipientId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDismissed = table.Column<bool>(type: "bit", nullable: false),
                    DismissedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActioned = table.Column<bool>(type: "bit", nullable: false),
                    ActionedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationRecipients", x => x.RecipientId);
                    table.ForeignKey(
                        name: "FK_NotificationRecipients_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "NotificationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationRecipients_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertDefinitions_CreatedBy",
                table: "AlertDefinitions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AlertDefinitions_ModifiedBy",
                table: "AlertDefinitions",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AlertDefinitions_TemplateId",
                table: "AlertDefinitions",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Active",
                table: "AlertDefinitions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_LastCheck",
                table: "AlertDefinitions",
                column: "LastCheckDate",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Type",
                table: "AlertDefinitions",
                columns: new[] { "AlertType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "UQ_Alert_Code",
                table: "AlertDefinitions",
                column: "AlertCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlertHistory_AcknowledgedBy",
                table: "AlertHistory",
                column: "AcknowledgedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AlertHistory_Alert_Date",
                table: "AlertHistory",
                columns: new[] { "AlertId", "TriggeredDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AlertHistory_Date",
                table: "AlertHistory",
                column: "TriggeredDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AlertHistory_Escalated",
                table: "AlertHistory",
                column: "IsEscalated",
                filter: "[IsEscalated] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_AlertHistory_NotificationId",
                table: "AlertHistory",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertHistory_ResolvedBy",
                table: "AlertHistory",
                column: "ResolvedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AlertHistory_Status",
                table: "AlertHistory",
                columns: new[] { "Status", "TriggeredDate" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationChannels_Enabled",
                table: "NotificationChannels",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationChannels_Provider",
                table: "NotificationChannels",
                column: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationChannels_Type",
                table: "NotificationChannels",
                column: "ChannelType");

            migrationBuilder.CreateIndex(
                name: "IX_Delivery_Channel_Status",
                table: "NotificationDelivery",
                columns: new[] { "ChannelId", "Status", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Delivery_ExternalId",
                table: "NotificationDelivery",
                column: "ExternalMessageId",
                filter: "[ExternalMessageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Delivery_Notification_Channel",
                table: "NotificationDelivery",
                columns: new[] { "NotificationId", "ChannelId" });

            migrationBuilder.CreateIndex(
                name: "IX_Delivery_Retry",
                table: "NotificationDelivery",
                columns: new[] { "Status", "NextRetryDate" },
                filter: "[Status] = 'Pending' AND [NextRetryDate] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDelivery_RecipientUserId",
                table: "NotificationDelivery",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipients_User_Date",
                table: "NotificationRecipients",
                columns: new[] { "UserId", "CreatedDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Recipients_User_Unread",
                table: "NotificationRecipients",
                columns: new[] { "UserId", "IsRead", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "UQ_NotificationRecipient",
                table: "NotificationRecipients",
                columns: new[] { "NotificationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Expiry",
                table: "Notifications",
                column: "ExpiryDate",
                filter: "[ExpiryDate] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Priority_Date",
                table: "Notifications",
                columns: new[] { "Priority", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Scheduled",
                table: "Notifications",
                column: "ScheduledDate",
                filter: "[ScheduledDate] IS NOT NULL AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Source",
                table: "Notifications",
                columns: new[] { "SourceEntityType", "SourceEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TemplateId",
                table: "Notifications",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TriggeredBy",
                table: "Notifications",
                column: "TriggeredBy");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type_Date",
                table: "Notifications",
                columns: new[] { "NotificationType", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_CreatedBy",
                table: "NotificationTemplates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_ModifiedBy",
                table: "NotificationTemplates",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Active",
                table: "NotificationTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Category",
                table: "NotificationTemplates",
                columns: new[] { "Category", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "UQ_Template_Code",
                table: "NotificationTemplates",
                column: "TemplateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Preferences_User",
                table: "UserNotificationPreferences",
                columns: new[] { "UserId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreferences_ChannelId",
                table: "UserNotificationPreferences",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "UQ_UserPreference",
                table: "UserNotificationPreferences",
                columns: new[] { "UserId", "ChannelId", "NotificationType" },
                unique: true,
                filter: "[NotificationType] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertHistory");

            migrationBuilder.DropTable(
                name: "NotificationDelivery");

            migrationBuilder.DropTable(
                name: "NotificationRecipients");

            migrationBuilder.DropTable(
                name: "UserNotificationPreferences");

            migrationBuilder.DropTable(
                name: "AlertDefinitions");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "NotificationChannels");

            migrationBuilder.DropTable(
                name: "NotificationTemplates");
        }
    }
}
