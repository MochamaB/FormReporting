# Notification System - File Structure

## Overview

This document outlines all files that need to be created or modified to implement the MVP notification system. Files are organized by category and include their purpose and dependencies.

---

## File Structure Tree

```
FormReporting/
│
├── Models/
│   ├── Entities/
│   │   └── Notifications/
│   │       ├── Notification.cs                          [✅ EXISTS]
│   │       ├── NotificationTemplate.cs                  [✅ EXISTS]
│   │       ├── NotificationRecipient.cs                 [✅ EXISTS]
│   │       ├── NotificationDelivery.cs                  [✅ EXISTS]
│   │       ├── NotificationChannel.cs                   [✅ EXISTS]
│   │       ├── UserNotificationPreference.cs            [✅ EXISTS]
│   │       ├── AlertDefinition.cs                       [✅ EXISTS]
│   │       └── AlertHistory.cs                          [✅ EXISTS]
│   │
│   └── ViewModels/
│       └── Notifications/
│           └── NotificationDtos.cs                      [📝 CREATE]
│
├── Services/
│   └── Notifications/
│       ├── INotificationService.cs                      [📝 CREATE]
│       ├── NotificationService.cs                       [📝 CREATE]
│       ├── INotificationTemplateService.cs              [📝 CREATE]
│       ├── NotificationTemplateService.cs               [📝 CREATE]
│       ├── INotificationDeliveryService.cs              [📝 CREATE]
│       ├── NotificationDeliveryService.cs               [📝 CREATE]
│       ├── IAlertService.cs                             [📝 CREATE]
│       ├── AlertService.cs                              [📝 CREATE]
│       │
│       ├── Providers/
│       │   ├── INotificationProvider.cs                 [📝 CREATE]
│       │   ├── EmailProvider.cs                         [📝 CREATE]
│       │   └── InAppProvider.cs                         [📝 CREATE]
│       │
│       └── Jobs/
│           └── AlertMonitoringJob.cs                    [📝 CREATE]
│
├── Data/
│   ├── Configurations/
│   │   └── Notifications/
│   │       ├── NotificationConfiguration.cs             [📝 CREATE]
│   │       ├── NotificationTemplateConfiguration.cs     [📝 CREATE]
│   │       ├── NotificationRecipientConfiguration.cs    [📝 CREATE]
│   │       ├── NotificationDeliveryConfiguration.cs     [📝 CREATE]
│   │       ├── NotificationChannelConfiguration.cs      [📝 CREATE]
│   │       ├── AlertDefinitionConfiguration.cs          [📝 CREATE]
│   │       └── AlertHistoryConfiguration.cs             [📝 CREATE]
│   │
│   └── Seeders/
│       └── NotificationSeeder.cs                        [📝 CREATE]
│
├── Controllers/
│   ├── API/
│   │   └── NotificationApiController.cs                 [📝 CREATE]
│   │
│   └── Notifications/
│       └── NotificationController.cs                    [📝 CREATE]
│
├── Hubs/
│   └── NotificationHub.cs                               [📝 CREATE]
│
├── Views/
│   ├── Notifications/
│   │   ├── Index.cshtml                                 [📝 CREATE]
│   │   └── Details.cshtml                               [📝 CREATE]
│   │
│   └── Shared/
│       ├── Components/
│       │   └── NotificationBell/
│       │       ├── NotificationBellViewComponent.cs     [📝 CREATE]
│       │       └── Default.cshtml                       [📝 CREATE]
│       │
│       └── _Layout.cshtml                               [✏️ MODIFY]
│
├── wwwroot/
│   ├── js/
│   │   ├── notification-bell.js                         [📝 CREATE]
│   │   ├── notification-inbox.js                        [📝 CREATE]
│   │   └── signalr/                                     [✅ EXISTS - via CDN/NPM]
│   │
│   └── css/
│       └── notifications.css                            [📝 CREATE]
│
├── Migrations/
│   └── YYYYMMDDHHMMSS_AddNotificationTables.cs          [📝 CREATE via EF]
│
├── Program.cs                                           [✏️ MODIFY]
└── appsettings.json                                     [✏️ MODIFY]
```

**Legend**:
- `[✅ EXISTS]` - File already exists
- `[📝 CREATE]` - New file to create
- `[✏️ MODIFY]` - Existing file to modify

---

## Detailed File Breakdown

### 1. Models & DTOs (3 files to create)

#### `Models/ViewModels/Notifications/NotificationDtos.cs`
**Purpose**: Data transfer objects for notification operations
**Contains**:
- `CreateNotificationDto` - For creating new notifications
- `NotificationListDto` - For inbox/list display
- `NotificationDetailDto` - For single notification view
- `UnreadCountDto` - For badge count
- `MarkAsReadDto` - For read status updates

**Dependencies**: None

**Estimated Lines**: ~150

---

### 2. Service Interfaces & Implementations (8 files to create)

#### `Services/Notifications/INotificationService.cs`
**Purpose**: Core notification service interface
**Methods**:
- `CreateNotificationAsync(CreateNotificationDto dto)`
- `GetUserNotificationsAsync(userId, unreadOnly, skip, take)`
- `GetNotificationByIdAsync(notificationId, userId)`
- `MarkAsReadAsync(notificationId, userId)`
- `MarkAsDismissedAsync(notificationId, userId)`
- `GetUnreadCountAsync(userId)`

**Dependencies**: NotificationDtos

**Estimated Lines**: ~50

---

#### `Services/Notifications/NotificationService.cs`
**Purpose**: Core notification service implementation
**Key Logic**:
- Create Notification record
- Resolve recipients (users, departments, roles)
- Create NotificationRecipient records
- Create NotificationDelivery records for each channel
- Replace template placeholders
- Trigger delivery service
- Scope filtering for queries

**Dependencies**:
- `INotificationTemplateService`
- `INotificationDeliveryService`
- `IScopeFilteringService`
- `ApplicationDbContext`
- `ILogger`

**Estimated Lines**: ~400

---

#### `Services/Notifications/INotificationTemplateService.cs`
**Purpose**: Template management interface
**Methods**:
- `GetTemplateByCodeAsync(templateCode)`
- `RenderTemplateAsync(templateCode, placeholderData)`
- `ValidatePlaceholdersAsync(templateCode, placeholderData)`

**Dependencies**: None

**Estimated Lines**: ~30

---

#### `Services/Notifications/NotificationTemplateService.cs`
**Purpose**: Template management implementation
**Key Logic**:
- Load templates from database
- Replace `{{placeholder}}` syntax with actual values
- Validate required placeholders present
- Cache frequently used templates

**Dependencies**:
- `ApplicationDbContext`
- `IMemoryCache`
- `ILogger`

**Estimated Lines**: ~150

---

#### `Services/Notifications/INotificationDeliveryService.cs`
**Purpose**: Multi-channel delivery interface
**Methods**:
- `SendNotificationAsync(notificationId)`
- `SendViaChannelAsync(deliveryId)`
- `RetryFailedDeliveriesAsync()`

**Dependencies**: None

**Estimated Lines**: ~30

---

#### `Services/Notifications/NotificationDeliveryService.cs`
**Purpose**: Multi-channel delivery orchestration
**Key Logic**:
- Get all NotificationDelivery records for notification
- Route to appropriate provider (Email, InApp, SMS)
- Update delivery status
- Handle retry logic
- Log delivery failures

**Dependencies**:
- `IEmailProvider`
- `IInAppProvider`
- `ApplicationDbContext`
- `ILogger`

**Estimated Lines**: ~250

---

#### `Services/Notifications/IAlertService.cs`
**Purpose**: Alert monitoring interface
**Methods**:
- `CheckDeadlineReminderAsync()`
- `CheckOverdueAlertsAsync()`
- `CheckPendingApprovalAlertsAsync()`
- `LogAlertTriggerAsync(alertCode, entityId)`
- `GetLastAlertAsync(alertCode, entityId)`

**Dependencies**: None

**Estimated Lines**: ~40

---

#### `Services/Notifications/AlertService.cs`
**Purpose**: Alert condition checking and triggering
**Key Logic**:
- Query entities matching alert conditions
- Check cooldown periods via AlertHistory
- Resolve alert recipients (including supervisors)
- Create notifications via NotificationService
- Log trigger in AlertHistory

**Dependencies**:
- `INotificationService`
- `IScopeFilteringService`
- `ApplicationDbContext`
- `ILogger`

**Estimated Lines**: ~350

---

### 3. Notification Providers (3 files to create)

#### `Services/Notifications/Providers/INotificationProvider.cs`
**Purpose**: Common interface for all delivery providers
**Methods**:
- `SendAsync(deliveryId)`
- `GetChannelType()`

**Dependencies**: None

**Estimated Lines**: ~20

---

#### `Services/Notifications/Providers/EmailProvider.cs`
**Purpose**: Email delivery via SMTP
**Key Logic**:
- Load SMTP settings from NotificationChannel
- Compose HTML email message
- Send via MailKit
- Update NotificationDelivery status
- Handle SMTP exceptions

**Dependencies**:
- `MailKit`
- `MimeKit`
- `ApplicationDbContext`
- `IConfiguration`
- `ILogger`

**Estimated Lines**: ~200

---

#### `Services/Notifications/Providers/InAppProvider.cs`
**Purpose**: InApp notification delivery
**Key Logic**:
- Mark NotificationDelivery as delivered immediately
- Trigger SignalR hub broadcast
- Update badge count

**Dependencies**:
- `IHubContext<NotificationHub>`
- `INotificationService`
- `ApplicationDbContext`
- `ILogger`

**Estimated Lines**: ~100

---

### 4. Background Jobs (1 file to create)

#### `Services/Notifications/Jobs/AlertMonitoringJob.cs`
**Purpose**: Hangfire recurring job for alert checks
**Key Logic**:
- Called by Hangfire scheduler
- Invokes AlertService methods
- Runs every 15-60 minutes (configurable)

**Dependencies**:
- `IAlertService`
- `ILogger`

**Estimated Lines**: ~80

---

### 5. Database Configurations (7 files to create)

#### `Data/Configurations/Notifications/NotificationConfiguration.cs`
**Purpose**: EF Core configuration for Notification entity
**Configures**:
- Table name, primary key
- Relationships (Template, Recipients, Deliveries)
- Indexes on SourceEntityType, SourceEntityId, CreatedDate
- String length constraints

**Dependencies**: Notification entity

**Estimated Lines**: ~60

---

#### `Data/Configurations/Notifications/NotificationTemplateConfiguration.cs`
**Purpose**: EF Core configuration for NotificationTemplate entity
**Configures**:
- Unique index on TemplateCode
- JSON column configurations
- String length constraints

**Dependencies**: NotificationTemplate entity

**Estimated Lines**: ~50

---

#### `Data/Configurations/Notifications/NotificationRecipientConfiguration.cs`
**Purpose**: EF Core configuration for NotificationRecipient entity
**Configures**:
- Composite index on (UserId, IsRead, CreatedDate)
- Relationships to Notification and User

**Dependencies**: NotificationRecipient entity

**Estimated Lines**: ~50

---

#### `Data/Configurations/Notifications/NotificationDeliveryConfiguration.cs`
**Purpose**: EF Core configuration for NotificationDelivery entity
**Configures**:
- Indexes on Status, SentDate, NextRetryDate
- Relationships to Notification, User, Channel

**Dependencies**: NotificationDelivery entity

**Estimated Lines**: ~60

---

#### `Data/Configurations/Notifications/NotificationChannelConfiguration.cs`
**Purpose**: EF Core configuration for NotificationChannel entity
**Configures**:
- Unique index on ChannelType
- JSON column for ProviderSettings

**Dependencies**: NotificationChannel entity

**Estimated Lines**: ~40

---

#### `Data/Configurations/Notifications/AlertDefinitionConfiguration.cs`
**Purpose**: EF Core configuration for AlertDefinition entity
**Configures**:
- Unique index on AlertCode
- JSON columns for TriggerCondition, Recipients, EscalationRules
- Relationships to Template, Creator

**Dependencies**: AlertDefinition entity

**Estimated Lines**: ~60

---

#### `Data/Configurations/Notifications/AlertHistoryConfiguration.cs`
**Purpose**: EF Core configuration for AlertHistory entity
**Configures**:
- Index on (AlertId, TriggeredDate)
- Relationships to Alert, Notification, Users

**Dependencies**: AlertHistory entity

**Estimated Lines**: ~50

---

### 6. Data Seeders (1 file to create)

#### `Data/Seeders/NotificationSeeder.cs`
**Purpose**: Seed initial notification data
**Seeds**:
- 2 NotificationChannels (Email, InApp)
- 9 NotificationTemplates (all from matrix)
- 3 AlertDefinitions (Deadline, Overdue, Pending Approval)

**Dependencies**:
- ApplicationDbContext
- JSON template content from documentation

**Estimated Lines**: ~600 (lots of template content)

---

### 7. API Controllers (1 file to create)

#### `Controllers/API/NotificationApiController.cs`
**Purpose**: REST API for notification operations
**Endpoints**:
- `GET /api/notifications` - Get user's notifications (paginated)
- `GET /api/notifications/{id}` - Get single notification
- `POST /api/notifications/{id}/read` - Mark as read
- `POST /api/notifications/{id}/dismiss` - Dismiss notification
- `GET /api/notifications/unread-count` - Get badge count

**Dependencies**:
- `INotificationService`
- `ILogger`

**Estimated Lines**: ~200

---

### 8. MVC Controllers (1 file to create)

#### `Controllers/Notifications/NotificationController.cs`
**Purpose**: MVC pages for notification inbox
**Actions**:
- `Index()` - Notification inbox page
- `Details(id)` - Single notification detail page

**Dependencies**:
- `INotificationService`
- `ILogger`

**Estimated Lines**: ~120

---

### 9. SignalR Hub (1 file to create)

#### `Hubs/NotificationHub.cs`
**Purpose**: Real-time notification broadcasting
**Methods**:
- `JoinUserGroup(userId)` - Subscribe to user's notifications
- `LeaveUserGroup(userId)` - Unsubscribe

**Client Events**:
- `ReceiveNotification` - New notification arrived
- `UpdateBadgeCount` - Unread count changed

**Dependencies**:
- `Microsoft.AspNetCore.SignalR`

**Estimated Lines**: ~60

---

### 10. View Components (2 files to create)

#### `Views/Shared/Components/NotificationBell/NotificationBellViewComponent.cs`
**Purpose**: Notification bell component logic
**Logic**:
- Get current user
- Get unread count
- Get latest 5 notifications
- Pass to view

**Dependencies**:
- `INotificationService`
- `IHttpContextAccessor`

**Estimated Lines**: ~50

---

#### `Views/Shared/Components/NotificationBell/Default.cshtml`
**Purpose**: Notification bell component markup
**Features**:
- Bell icon with badge
- Dropdown menu with recent notifications
- "View All" link
- Empty state message

**Dependencies**: NotificationListDto

**Estimated Lines**: ~80

---

### 11. Views (2 files to create)

#### `Views/Notifications/Index.cshtml`
**Purpose**: Notification inbox page
**Features**:
- Filter tabs (All, Unread)
- Notification list
- Mark as read buttons
- Pagination
- Empty state

**Dependencies**: NotificationListDto

**Estimated Lines**: ~150

---

#### `Views/Notifications/Details.cshtml`
**Purpose**: Single notification detail page
**Features**:
- Full notification content
- Action button
- Mark as read/dismiss
- Related entity link

**Dependencies**: NotificationDetailDto

**Estimated Lines**: ~100

---

### 12. JavaScript Files (2 files to create)

#### `wwwroot/js/notification-bell.js`
**Purpose**: Notification bell component behavior
**Features**:
- SignalR connection setup
- Real-time badge updates
- Dropdown interaction
- Toast notifications for High/Urgent priority
- Auto-refresh on new notification

**Dependencies**:
- SignalR client library
- jQuery (or vanilla JS)

**Estimated Lines**: ~250

---

#### `wwwroot/js/notification-inbox.js`
**Purpose**: Notification inbox page behavior
**Features**:
- Load notifications via API
- Filter by read/unread
- Mark as read (single/bulk)
- Pagination
- Auto-refresh via SignalR

**Dependencies**:
- SignalR client library
- jQuery (or vanilla JS)

**Estimated Lines**: ~300

---

### 13. CSS Files (1 file to create)

#### `wwwroot/css/notifications.css`
**Purpose**: Notification UI styling
**Styles**:
- Notification bell and badge
- Dropdown menu
- Notification list items (read/unread states)
- Priority indicators (colors)
- Toast notifications
- Inbox page layout

**Dependencies**: None (or Bootstrap/Tailwind)

**Estimated Lines**: ~200

---

### 14. Configuration Files (2 files to modify)

#### `Program.cs` [MODIFY]
**Changes Required**:
1. Register notification services in DI container
2. Configure Hangfire with SQL Server storage
3. Add Hangfire dashboard middleware
4. Register SignalR hub
5. Register recurring jobs for alerts
6. Call notification seeder on startup (development only)

**Example**:
```csharp
// Add services
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
builder.Services.AddScoped<INotificationDeliveryService, NotificationDeliveryService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IEmailProvider, EmailProvider>();
builder.Services.AddScoped<IInAppProvider, InAppProvider>();

// Add Hangfire
builder.Services.AddHangfire(config => config.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();

// Add SignalR
builder.Services.AddSignalR();

// After app.Build()
app.UseHangfireDashboard("/hangfire");
app.MapHub<NotificationHub>("/notificationHub");

// Register recurring jobs
RecurringJob.AddOrUpdate<IAlertService>("deadline-reminders", x => x.CheckDeadlineReminderAsync(), Cron.Hourly);
RecurringJob.AddOrUpdate<IAlertService>("overdue-alerts", x => x.CheckOverdueAlertsAsync(), Cron.Hourly);
RecurringJob.AddOrUpdate<IAlertService>("pending-approvals", x => x.CheckPendingApprovalAlertsAsync(), "0 */2 * * *");
```

**Estimated Changes**: ~50 lines

---

#### `appsettings.json` [MODIFY]
**Changes Required**:
Add SMTP configuration section

**Example**:
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "UseSsl": true,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@ktda.co.ke",
    "FromName": "KTDA Form Reporting"
  },
  "NotificationSettings": {
    "MaxRecipientsPerNotification": 100,
    "DefaultPageSize": 20,
    "EnableRealTimeUpdates": true
  }
}
```

**Estimated Changes**: ~15 lines

---

### 15. Database Migration (1 file to create via EF CLI)

#### `Migrations/YYYYMMDDHHMMSS_AddNotificationTables.cs`
**Purpose**: Create all notification tables
**Creates**:
- NotificationChannels
- NotificationTemplates
- Notifications
- NotificationRecipients
- NotificationDelivery
- UserNotificationPreferences
- AlertDefinitions
- AlertHistory

**Generated via**: `dotnet ef migrations add AddNotificationTables`

**Estimated Lines**: ~500 (auto-generated)

---

### 16. Module Integration Files (3 files to modify)

#### `Services/Forms/FormSubmissionService.cs` [MODIFY]
**Changes**:
- Inject `INotificationService`
- Add notification call after `CreateAsync` success
- Send FORM_SUBMITTED notification to assignee

**Estimated Changes**: ~20 lines

---

#### `Services/Forms/WorkflowService.cs` [MODIFY]
**Changes**:
- Inject `INotificationService`
- Add notifications for:
  - WORKFLOW_ASSIGNED (after step assignment)
  - STEP_COMPLETED (after CompleteStepAsync)
  - FORM_APPROVED (after final approval)
  - FORM_REJECTED (after RejectStepAsync)

**Estimated Changes**: ~80 lines

---

#### `Services/Forms/FormAssignmentService.cs` [MODIFY]
**Changes**:
- Inject `INotificationService`
- Add notification call after `CreateAsync` success
- Send ASSIGNMENT_CREATED to all assignees
- Handle department user resolution

**Estimated Changes**: ~30 lines

---

#### `Views/Shared/_Layout.cshtml` [MODIFY]
**Changes**:
- Add SignalR script reference
- Add notification bell ViewComponent to header
- Add current user ID hidden field for JavaScript
- Add notification CSS reference

**Estimated Changes**: ~10 lines

---

## Implementation Order

### Phase 1: Foundation (Week 1)
1. Create all DTOs (`NotificationDtos.cs`)
2. Create all service interfaces (4 files)
3. Create all EF configurations (7 files)
4. Create and run database migration
5. Create notification seeder
6. Run seeder to populate initial data

**Files**: 13 files

---

### Phase 2: Core Services (Week 2)
1. Implement `NotificationTemplateService.cs`
2. Implement provider interface and both providers (3 files)
3. Implement `NotificationDeliveryService.cs`
4. Implement `NotificationService.cs`
5. Register services in `Program.cs`
6. Test notification creation manually

**Files**: 6 files + 1 modification

---

### Phase 3: Module Integration (Week 3)
1. Modify `FormSubmissionService.cs`
2. Modify `WorkflowService.cs`
3. Modify `FormAssignmentService.cs`
4. Test each integration point

**Files**: 3 modifications

---

### Phase 4: User Interface (Week 4)
1. Create `NotificationApiController.cs`
2. Create `NotificationController.cs`
3. Create ViewComponent (2 files)
4. Create inbox views (2 files)
5. Create JavaScript files (2 files)
6. Create CSS file
7. Modify `_Layout.cshtml`

**Files**: 9 files + 1 modification

---

### Phase 5: Real-Time & Alerts (Week 5)
1. Create `NotificationHub.cs`
2. Configure SignalR in `Program.cs`
3. Update JavaScript for SignalR integration
4. Implement `AlertService.cs`
5. Create `AlertMonitoringJob.cs`
6. Configure Hangfire in `Program.cs`
7. Test real-time updates and alerts

**Files**: 3 files + 2 modifications

---

### Phase 6: Testing & Polish (Week 6)
1. End-to-end testing of all notification types
2. Performance testing with high volume
3. Error handling improvements
4. Logging enhancements
5. Documentation updates

**Files**: Refinements to existing files

---

## File Count Summary

| Category | New Files | Modified Files | Total |
|----------|-----------|----------------|-------|
| Models/DTOs | 1 | 0 | 1 |
| Services | 8 | 0 | 8 |
| Providers | 3 | 0 | 3 |
| Jobs | 1 | 0 | 1 |
| Configurations | 7 | 0 | 7 |
| Seeders | 1 | 0 | 1 |
| Controllers | 2 | 0 | 2 |
| Hubs | 1 | 0 | 1 |
| ViewComponents | 2 | 0 | 2 |
| Views | 2 | 1 | 3 |
| JavaScript | 2 | 0 | 2 |
| CSS | 1 | 0 | 1 |
| Configuration | 0 | 2 | 2 |
| Module Integration | 0 | 3 | 3 |
| **Total** | **31** | **6** | **37** |

Plus 1 EF migration (auto-generated)

---

## NuGet Packages Required

Add these packages to your project:

```xml
<PackageReference Include="MailKit" Version="4.3.0" />
<PackageReference Include="Hangfire.Core" Version="1.8.9" />
<PackageReference Include="Hangfire.SqlServer" Version="1.8.9" />
<PackageReference Include="Hangfire.AspNetCore" Version="1.8.9" />
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.0" />
```

SignalR client library (JavaScript):
```html
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@7.0.0/dist/browser/signalr.min.js"></script>
```

---

## Quick Start Commands

```bash
# Install NuGet packages
dotnet add package MailKit
dotnet add package Hangfire.Core
dotnet add package Hangfire.SqlServer
dotnet add package Hangfire.AspNetCore

# Create migration
dotnet ef migrations add AddNotificationTables

# Apply migration
dotnet ef database update

# Run seeder (in Program.cs during development)
# await NotificationSeeder.SeedAsync(context);
```

---

## Next Steps

1. Start with Phase 1 files (Foundation)
2. Follow implementation order sequentially
3. Test after each phase before proceeding
4. Refer to other documentation for detailed implementation logic
5. Use this document as a checklist to track progress

---

**Document Version**: 1.0
**Last Updated**: 2025-12-29
**Total Implementation**: 37 files (31 new + 6 modified)
