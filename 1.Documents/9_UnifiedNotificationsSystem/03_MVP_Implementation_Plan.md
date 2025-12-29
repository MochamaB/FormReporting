# Notification System - MVP Implementation Plan

## Implementation Phases

---

## Phase 1: Foundation (Week 1)

### 1.1 Create DTOs and ViewModels

**File**: `Models/ViewModels/Notifications/NotificationDtos.cs`

**DTOs Needed**:
- `CreateNotificationDto` - For creating notifications
- `NotificationListDto` - For inbox display
- `NotificationDetailDto` - For single notification view
- `UnreadCountDto` - For badge count

**Estimated Time**: 4 hours

---

### 1.2 Create Service Interfaces

**Files**:
- `Services/Notifications/INotificationService.cs`
- `Services/Notifications/INotificationTemplateService.cs`

**Methods**:
```
INotificationService:
- CreateNotificationAsync(CreateNotificationDto)
- GetUserNotificationsAsync(userId, unreadOnly, skip, take)
- GetNotificationByIdAsync(notificationId, userId)
- MarkAsReadAsync(notificationId, userId)
- MarkAsDismissedAsync(notificationId, userId)
- GetUnreadCountAsync(userId)

INotificationTemplateService:
- GetTemplateByCodeAsync(templateCode)
- RenderTemplateAsync(templateCode, placeholderData)
```

**Estimated Time**: 2 hours

---

### 1.3 Implement Core Services

**File**: `Services/Notifications/NotificationService.cs`

**Key Logic**:
- Create Notification record
- Create NotificationRecipient records (one per user)
- Create NotificationDelivery records (Email + InApp)
- Replace placeholders in templates
- Trigger email sending
- Query notifications with scope filtering

**File**: `Services/Notifications/NotificationTemplateService.cs`

**Key Logic**:
- Load template from database
- Replace {{placeholders}} with actual values
- Return rendered subject and body

**Estimated Time**: 12 hours

---

### 1.4 Database Configuration

**Files**:
- `Data/Configurations/Notifications/NotificationConfiguration.cs`
- `Data/Configurations/Notifications/NotificationTemplateConfiguration.cs`
- `Data/Configurations/Notifications/AlertDefinitionConfiguration.cs`

**Configure**:
- Entity relationships
- Indexes on frequently queried fields
- String length constraints

**Estimated Time**: 3 hours

---

### 1.5 Seed Initial Data

**File**: `Data/Seeders/NotificationSeeder.cs`

**Seed**:
- 2 Notification Channels (Email, InApp)
- 9 Notification Templates (all from templates matrix)
- 3 Alert Definitions (Deadline Reminder, Overdue, Pending Approval)

**Estimated Time**: 4 hours

---

**Phase 1 Total**: 25 hours

---

## Phase 2: Email & InApp Delivery (Week 2)

### 2.1 Create Email Provider

**File**: `Services/Notifications/Providers/EmailProvider.cs`

**Dependencies**:
- System.Net.Mail or MailKit library
- SMTP configuration from appsettings.json

**Key Logic**:
- Load SMTP settings from NotificationChannel
- Compose email message (HTML body, subject, from, to)
- Send via SMTP
- Update NotificationDelivery status (Sent/Failed)
- Log errors

**Estimated Time**: 8 hours

---

### 2.2 Create InApp Provider

**File**: `Services/Notifications/Providers/InAppProvider.cs`

**Key Logic**:
- Mark NotificationDelivery as Delivered immediately
- InApp notifications stored in NotificationRecipient table
- Real-time push via SignalR (Phase 3)

**Estimated Time**: 2 hours

---

### 2.3 Create Delivery Service

**File**: `Services/Notifications/NotificationDeliveryService.cs`

**Interface**: `INotificationDeliveryService`

**Methods**:
- `SendNotificationAsync(notificationId)` - Sends to all recipients via all channels
- `SendViaChannelAsync(deliveryId)` - Send single delivery
- `GetProviderForChannel(channelType)` - Factory pattern for providers

**Key Logic**:
- Loop through NotificationDelivery records
- Call appropriate provider (Email or InApp)
- Update delivery status
- Handle exceptions (don't let one failure block others)

**Estimated Time**: 6 hours

---

### 2.4 Test End-to-End Email Delivery

**Manual Tests**:
1. Create test notification via service
2. Verify email sent to recipient
3. Check NotificationDelivery status updated
4. Verify placeholder replacement works
5. Test HTML rendering in email client

**Estimated Time**: 4 hours

---

**Phase 2 Total**: 20 hours

---

## Phase 3: Module Integration (Week 3)

### 3.1 Forms Module Integration

**Files to Modify**:
- `Services/Forms/FormSubmissionService.cs`

**Notifications to Add**:
- FORM_SUBMITTED (after CreateAsync)

**Estimated Time**: 4 hours

---

### 3.2 Workflows Module Integration

**Files to Modify**:
- `Services/Forms/WorkflowService.cs` (or WorkflowProgressService)

**Notifications to Add**:
- WORKFLOW_ASSIGNED (after step assignment)
- STEP_COMPLETED (after CompleteStepAsync)
- FORM_APPROVED (after final approval)
- FORM_REJECTED (after RejectStepAsync)

**Estimated Time**: 8 hours

---

### 3.3 Assignments Module Integration

**Files to Modify**:
- `Services/Forms/FormAssignmentService.cs`

**Notifications to Add**:
- ASSIGNMENT_CREATED (after CreateAsync)

**Helper Methods**:
- Resolve department users
- Handle multiple recipients

**Estimated Time**: 4 hours

---

### 3.4 Integration Testing

**Test Scenarios**:
1. Submit form → Verify assignee gets email
2. Approve workflow step → Verify submitter gets email
3. Reject form → Verify submitter gets email
4. Create assignment → Verify all users get emails
5. Test with department assignment

**Estimated Time**: 4 hours

---

**Phase 3 Total**: 20 hours

---

## Phase 4: User Interface (Week 4)

### 4.1 Create API Controller

**File**: `Controllers/API/NotificationApiController.cs`

**Endpoints**:
- `GET /api/notifications` - Get user's notifications (paginated)
- `GET /api/notifications/{id}` - Get single notification
- `POST /api/notifications/{id}/read` - Mark as read
- `POST /api/notifications/{id}/dismiss` - Dismiss notification
- `GET /api/notifications/unread-count` - Badge count

**Estimated Time**: 6 hours

---

### 4.2 Create Notification Bell Component

**Files**:
- `Views/Shared/Components/NotificationBell/NotificationBellViewComponent.cs`
- `Views/Shared/Components/NotificationBell/Default.cshtml`
- `wwwroot/js/notification-bell.js`

**Features**:
- Bell icon in header
- Unread count badge
- Dropdown with recent notifications (5 latest)
- "View All" link to inbox
- Mark as read on click

**Estimated Time**: 8 hours

---

### 4.3 Create Notification Inbox Page

**Files**:
- `Controllers/Notifications/NotificationController.cs`
- `Views/Notifications/Index.cshtml`
- `wwwroot/js/notification-inbox.js`

**Features**:
- List all notifications (paginated)
- Filter: All / Unread / Read
- Sort: Newest first
- Click to view details
- Bulk mark as read
- Visual indicators (read/unread, priority)

**Estimated Time**: 10 hours

---

### 4.4 Add SignalR Real-Time Updates

**Files**:
- `Hubs/NotificationHub.cs`
- Modify `wwwroot/js/notification-bell.js`

**Features**:
- Real-time badge count update
- Show toast notification for High/Urgent notifications
- Auto-refresh notification list when new arrives

**Estimated Time**: 6 hours

---

**Phase 4 Total**: 30 hours

---

## Phase 5: Alerts & Background Jobs (Week 5)

### 5.1 Create Alert Service

**Files**:
- `Services/Notifications/IAlertService.cs`
- `Services/Notifications/AlertService.cs`

**Methods**:
- `CheckAlertConditionAsync(alertId)` - Evaluate condition
- `TriggerAlertAsync(alertId, entityId)` - Create notification
- `LogAlertTriggerAsync(alertId, entityId)` - Record in AlertHistory

**Estimated Time**: 6 hours

---

### 5.2 Implement Alert Monitoring Job

**File**: `Services/Notifications/Jobs/AlertMonitoringJob.cs`

**Setup**:
- Use Hangfire or similar background job library
- Register recurring job in Program.cs

**Job Logic**:
- Get all active AlertDefinitions
- Loop through each alert
- Call AlertService.CheckAlertConditionAsync()
- Run every 15 minutes

**Estimated Time**: 6 hours

---

### 5.3 Configure Hangfire

**File**: `Program.cs`

**Setup**:
- Install Hangfire NuGet packages
- Configure Hangfire with SQL Server storage
- Register AlertMonitoringJob as recurring job
- Add Hangfire dashboard UI (optional)

**Estimated Time**: 4 hours

---

### 5.4 Implement Alert Conditions

**For Each Alert**:
- DEADLINE_REMINDER: Assignments due in 24 hours
- OVERDUE_ALERT: Assignments/submissions past due date
- PENDING_APPROVAL_ALERT: Workflow steps pending >48 hours

**Include**:
- Cooldown check via AlertHistory
- Supervisor resolution
- Multiple recipients

**Estimated Time**: 8 hours

---

### 5.5 Test Alerts End-to-End

**Test Scenarios**:
1. Create assignment due in 23 hours → Verify reminder sent
2. Create past-due assignment → Verify overdue alert sent
3. Create workflow step 48+ hours ago → Verify pending alert sent
4. Verify cooldown prevents duplicate alerts
5. Verify supervisors receive alerts

**Estimated Time**: 4 hours

---

**Phase 5 Total**: 28 hours

---

## Phase 6: Polish & Testing (Week 6)

### 6.1 Error Handling

- Wrap all notification creation in try-catch
- Log errors but don't block main operation
- Add user-friendly error messages
- Handle missing templates gracefully

**Estimated Time**: 4 hours

---

### 6.2 Logging

- Add comprehensive logging to all services
- Log notification creation, delivery attempts, failures
- Track performance metrics

**Estimated Time**: 3 hours

---

### 6.3 Scope Filtering

- Ensure notifications respect user's access scope
- Filter recipients by tenant/region/department
- Test with users in different scopes

**Estimated Time**: 4 hours

---

### 6.4 Performance Optimization

- Add database indexes
- Optimize notification queries
- Test with high volume (1000+ notifications)
- Ensure SignalR scales

**Estimated Time**: 4 hours

---

### 6.5 Documentation

- API documentation
- User guide (how to use notification inbox)
- Admin guide (managing templates/alerts)
- Code comments

**Estimated Time**: 4 hours

---

### 6.6 Comprehensive Testing

**Test Cases**:
- All 9 notification types
- All 3 alert types
- Multi-recipient notifications
- Email delivery failures
- Concurrent notifications
- Cross-browser testing (UI)
- Mobile responsive design

**Estimated Time**: 8 hours

---

**Phase 6 Total**: 27 hours

---

## Total Implementation Time

| Phase | Description | Hours |
|-------|-------------|-------|
| 1 | Foundation | 25 |
| 2 | Email & InApp Delivery | 20 |
| 3 | Module Integration | 20 |
| 4 | User Interface | 30 |
| 5 | Alerts & Background Jobs | 28 |
| 6 | Polish & Testing | 27 |
| **Total** | | **150 hours** |

**Estimated Duration**: 6 weeks (at ~25 hours/week)

---

## Dependencies & Prerequisites

### NuGet Packages Needed
- MailKit (or System.Net.Mail)
- Hangfire.Core
- Hangfire.SqlServer
- Microsoft.AspNetCore.SignalR

### Configuration Needed
- SMTP server credentials (Gmail, SendGrid, etc.)
- Hangfire dashboard authentication
- SignalR scaling strategy (if multiple servers)

### Database Changes
- All notification entities already exist in Models
- Need EF migration to create tables
- Seed data migration for templates/channels/alerts

---

## Out of MVP Scope (Future Enhancements)

These features are intentionally excluded from MVP to keep implementation focused:

❌ **SMS/Push Notifications** - Email and InApp only in MVP
❌ **User Preferences** - No per-user channel/frequency settings
❌ **Digest Notifications** - All notifications sent immediately
❌ **Retry Logic** - Failed emails not retried automatically
❌ **Template Editor UI** - Templates managed via database seeder only
❌ **Alert Acknowledge/Resolve** - Alerts fire but no workflow tracking
❌ **Analytics Dashboard** - No reporting on notification metrics
❌ **Notification Search** - Basic list only, no search/filter
❌ **Rich Notifications** - Plain text InApp, basic HTML email
❌ **Notification Groups** - No threading or conversation grouping

These can be added in future phases based on user feedback and requirements.

---

## Success Metrics

MVP is considered successful when:

1. ✅ All 9 notification types trigger correctly
2. ✅ Emails delivered successfully >95% of the time
3. ✅ InApp notifications appear in user's inbox
4. ✅ Unread badge count updates in real-time
5. ✅ All 3 alert types trigger on schedule
6. ✅ No performance degradation to existing features
7. ✅ Users can mark notifications as read
8. ✅ Clicking notification navigates to correct page
9. ✅ Scope filtering works correctly
10. ✅ Zero data leaks (users only see their notifications)

---

## Risk Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Email delivery failures | High | Implement logging, test with multiple providers |
| SignalR scaling issues | Medium | Start with single server, plan for Redis backplane |
| Performance with high volume | High | Add indexes, test with load, paginate queries |
| Template rendering errors | Medium | Validate placeholders, handle missing values |
| Alert spam | Medium | Implement cooldown, test threshold values |
| Integration breaks existing features | High | Comprehensive testing, feature flags |

---

## Deployment Plan

1. **Database Migration**: Run EF migration to create tables
2. **Seed Data**: Run seeder to populate templates/channels/alerts
3. **Configuration**: Add SMTP settings to appsettings.json
4. **Deploy Code**: Deploy services, controllers, views
5. **Start Hangfire**: Verify background job running
6. **Test Smoke Test**: Trigger each notification type manually
7. **Monitor**: Watch logs for errors, email delivery confirmations
8. **User Communication**: Notify users about new notification center

---

## Rollback Plan

If critical issues found post-deployment:

1. **Stop Hangfire**: Prevent new alerts from triggering
2. **Disable Integrations**: Comment out notification service calls in modules
3. **UI Rollback**: Hide notification bell component
4. **Database Rollback**: Optional - notifications don't affect core functionality
5. **Investigate**: Review logs, fix issues
6. **Redeploy**: After fixes validated

Notifications are designed to be non-blocking, so failures won't break Forms/Workflows/Assignments functionality.
