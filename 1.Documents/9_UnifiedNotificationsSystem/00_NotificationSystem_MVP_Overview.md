# Notification System - MVP Overview

## Purpose
Unified notification system to keep users informed about Forms, Workflows, and Assignments activities through Email and InApp channels, with automated alerts for time-sensitive events.

## Core Components

### 1. Notification (Central Hub)
- Single notification message
- Links to source entity (FormSubmission, WorkflowProgress, Assignment)
- Supports multiple recipients
- Template-based message generation

### 2. NotificationRecipient (User Tracking)
- Tracks read/dismissed/actioned state per user
- Enables user-specific notification inbox
- Powers unread badge count

### 3. NotificationDelivery (Multi-Channel)
- Handles Email via SMTP
- Handles InApp via database/SignalR
- Tracks delivery status per channel

### 4. NotificationChannel (Delivery Config)
- Email channel (SMTP configuration)
- InApp channel (database + real-time)

### 5. NotificationTemplate (Message Patterns)
- Reusable templates with placeholders
- Separate formats for Email (HTML) and InApp (text)
- System templates cannot be deleted

### 6. AlertDefinition (Automated Monitoring)
- Rule-based triggers (e.g., overdue submissions)
- Scheduled checks (every 15/30/60 minutes)
- Auto-generates notifications when conditions met
- Cooldown to prevent spam

## How It Works

### Flow 1: Event-Triggered Notification
1. User submits form → FormSubmission created
2. FormSubmissionService calls NotificationService.CreateNotification()
3. System selects FORM_SUBMITTED template
4. Replaces placeholders with actual data
5. Creates Notification record
6. Determines recipients (reviewers based on workflow)
7. Creates NotificationRecipient for each user
8. Creates NotificationDelivery for Email + InApp channels
9. Email sent immediately via SMTP
10. InApp notification appears in user's inbox/bell

### Flow 2: Alert-Triggered Notification
1. AlertMonitoringJob runs every 30 minutes
2. Checks OVERDUE_SUBMISSIONS alert condition
3. Finds submissions with DueDate < Now AND Status != Completed
4. For each overdue submission:
   - Creates AlertHistory (Triggered status)
   - Generates Notification from OVERDUE_ALERT template
   - Recipients: Assignee + their Supervisor
   - Sends via Email + InApp
5. Updates alert LastTriggeredDate
6. Won't trigger again for 60 minutes (cooldown)

### Flow 3: User Views Notifications
1. User clicks notification bell icon
2. API call to GET /api/notifications/unread-count → Shows badge
3. User opens notification dropdown/inbox
4. API call to GET /api/notifications → Returns list
5. User clicks notification → Redirects to ActionUrl
6. API call to POST /api/notifications/{id}/read → Marks as read
7. Unread count decreases

## MVP Scope

### Included in MVP
✅ **Channels**: Email (SMTP) + InApp (database/SignalR)
✅ **Templates**: 8 core templates for Forms/Workflows/Assignments
✅ **Notifications**: Event-triggered from modules
✅ **Alerts**: 3 basic alerts (Overdue, Deadline Reminder, Pending Approvals)
✅ **UI**: Notification bell, dropdown list, inbox page, mark as read
✅ **Delivery**: Synchronous email send, InApp immediate
✅ **Tracking**: Read/dismissed/actioned states

### NOT in MVP (Future Phases)
❌ SMS/Push notifications
❌ User preferences (frequency, quiet hours)
❌ Digest notifications
❌ Retry logic for failed deliveries
❌ Background job queue (delivery is synchronous)
❌ Alert acknowledge/resolve workflow
❌ Template editor UI (templates seeded via migration)
❌ Analytics dashboard

## Technology Stack

- **Email Provider**: System.Net.Mail (SMTP) or MailKit
- **InApp Storage**: SQL Server (Notifications, NotificationRecipient tables)
- **Real-time Updates**: SignalR for live notification bell updates
- **Background Jobs**: Hangfire or similar for alert monitoring
- **Template Engine**: Simple string.Replace for placeholders

## Success Criteria

1. ✅ User receives email when workflow step assigned
2. ✅ User sees notification in bell dropdown
3. ✅ Unread count badge updates in real-time
4. ✅ Clicking notification navigates to relevant page
5. ✅ Overdue submissions trigger alert emails
6. ✅ All 8 notification templates work correctly
7. ✅ Notifications respect scope (users only see notifications for submissions they can access)

## Database Tables Used

1. **Notifications** - Core notification data
2. **NotificationRecipients** - User-specific tracking
3. **NotificationDelivery** - Delivery attempts per channel
4. **NotificationTemplates** - Message templates
5. **NotificationChannels** - Email and InApp config
6. **AlertDefinitions** - Automated monitoring rules
7. **AlertHistory** - Alert trigger log

## Implementation Timeline

- **Week 1**: Foundation (entities, services, basic CRUD)
- **Week 2**: Email delivery + InApp storage
- **Week 3**: Module integration (Forms, Workflows, Assignments)
- **Week 4**: UI (bell, inbox, mark as read) + Alerts
- **Week 5**: Testing, bug fixes, documentation

## Security Considerations

- Scope filtering: Users only see notifications for entities they can access
- Email credentials stored encrypted
- CSRF protection on all API endpoints
- XSS prevention in notification display
- Rate limiting to prevent notification spam
