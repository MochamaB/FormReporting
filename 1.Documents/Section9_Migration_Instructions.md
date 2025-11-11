# Section 9: Notifications & Alerts - Migration Instructions

## ✅ Files Created

### Models (8 files)
- ✅ `Models/Entities/Notifications/NotificationChannel.cs`
- ✅ `Models/Entities/Notifications/Notification.cs`
- ✅ `Models/Entities/Notifications/NotificationRecipient.cs`
- ✅ `Models/Entities/Notifications/NotificationDelivery.cs`
- ✅ `Models/Entities/Notifications/NotificationTemplate.cs`
- ✅ `Models/Entities/Notifications/UserNotificationPreference.cs`
- ✅ `Models/Entities/Notifications/AlertDefinition.cs`
- ✅ `Models/Entities/Notifications/AlertHistory.cs`

### Configurations (8 files)
- ✅ `Data/Configurations/Notifications/NotificationChannelConfiguration.cs`
- ✅ `Data/Configurations/Notifications/NotificationConfiguration.cs`
- ✅ `Data/Configurations/Notifications/NotificationRecipientConfiguration.cs`
- ✅ `Data/Configurations/Notifications/NotificationDeliveryConfiguration.cs`
- ✅ `Data/Configurations/Notifications/NotificationTemplateConfiguration.cs`
- ✅ `Data/Configurations/Notifications/UserNotificationPreferenceConfiguration.cs`
- ✅ `Data/Configurations/Notifications/AlertDefinitionConfiguration.cs`
- ✅ `Data/Configurations/Notifications/AlertHistoryConfiguration.cs`

### Updated Files
- ✅ `Data/ApplicationDbContext.cs` - Added DbSets and configurations

---

## 📋 Migration Commands

### Step 1: Build the Project
First, ensure the project builds successfully:

```powershell
dotnet build
```

### Step 2: Create Migration
Open **Package Manager Console** in Visual Studio and run:

```powershell
Add-Migration Add_Section9_NotificationsAndAlerts
```

**Alternative using .NET CLI:**
```powershell
dotnet ef migrations add Add_Section9_NotificationsAndAlerts
```

### Step 3: Review Migration
- Navigate to `Data/Migrations` folder
- Open the newly created migration file
- Verify that all 8 tables are being created with correct columns and indexes

### Step 4: Apply Migration to Database
```powershell
Update-Database
```

**Alternative using .NET CLI:**
```powershell
dotnet ef database update
```

### Step 5: Verify Database
Run this SQL query to verify all tables were created:

```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN (
    'NotificationChannels',
    'Notifications',
    'NotificationRecipients',
    'NotificationDelivery',
    'NotificationTemplates',
    'UserNotificationPreferences',
    'AlertDefinitions',
    'AlertHistory'
)
ORDER BY TABLE_NAME;
```

---

## 🔄 Rollback Commands (If Needed)

### Remove Last Migration (before applying to database)
```powershell
Remove-Migration
```

### Revert Database to Previous Migration
```powershell
Update-Database <PreviousMigrationName>
```

---

## 📊 Database Tables Created

### 1. NotificationChannels
- Configures delivery channels (Email, SMS, Push, InApp)
- Stores provider credentials and settings
- Tracks daily send limits

### 2. Notifications
- Central notification inbox
- Supports multiple priorities and types
- Links to source entities (forms, tickets, alerts)

### 3. NotificationRecipients
- Tracks who receives each notification
- Read/dismissed/actioned status per user
- One-to-many relationship with Notifications

### 4. NotificationDelivery
- Multi-channel delivery tracking
- Retry logic with configurable attempts
- Provider response tracking

### 5. NotificationTemplates
- Reusable message templates
- Supports placeholders ({{UserName}}, {{FormName}})
- Separate templates for Email/SMS/Push

### 6. UserNotificationPreferences
- Per-user channel preferences
- Quiet hours configuration
- Delivery frequency settings (Immediate, Hourly, Daily, Weekly)

### 7. AlertDefinitions
- Automated alert rules
- JSON-based trigger conditions
- Cooldown periods to prevent spam
- Escalation rules

### 8. AlertHistory
- Alert trigger log
- Acknowledge/resolve workflow
- Time-to-acknowledge and time-to-resolve tracking
- Escalation tracking

---

## 🔗 Key Relationships

```
NotificationTemplate (1) ──→ (N) Notification
Notification (1) ──→ (N) NotificationRecipient
Notification (1) ──→ (N) NotificationDelivery
NotificationChannel (1) ──→ (N) NotificationDelivery
NotificationChannel (1) ──→ (N) UserNotificationPreference
User (1) ──→ (N) NotificationRecipient
User (1) ──→ (N) UserNotificationPreference
AlertDefinition (1) ──→ (N) AlertHistory
AlertDefinition (1) ──→ (1) NotificationTemplate
AlertHistory (1) ──→ (1) Notification
```

---

## ✨ Features Implemented

### Notification System
- ✅ Multi-channel delivery (Email, SMS, Push, InApp)
- ✅ Template-based messaging with placeholders
- ✅ User preferences per channel and notification type
- ✅ Read/unread tracking
- ✅ Priority levels (Low, Normal, High, Urgent)
- ✅ Scheduled notifications
- ✅ Expiry dates for time-sensitive notifications

### Alert System
- ✅ Automated alert rules with JSON conditions
- ✅ Configurable check frequency
- ✅ Severity levels (Info, Warning, Error, Critical)
- ✅ Cooldown periods
- ✅ Auto-resolve conditions
- ✅ Escalation rules
- ✅ Acknowledge/resolve workflow
- ✅ SLA tracking (time-to-acknowledge, time-to-resolve)

### Delivery Tracking
- ✅ Multi-channel delivery attempts
- ✅ Retry logic with configurable delays
- ✅ Provider response tracking
- ✅ Delivery cost tracking
- ✅ External message ID tracking

---

## 🎯 Next Steps

After successful migration:

1. **Seed Initial Data**
   - Create default notification channels (Email, SMS, InApp)
   - Create system notification templates
   - Set up default alert definitions

2. **Test Notification System**
   - Send test notifications
   - Verify delivery tracking
   - Test user preferences

3. **Implement Services**
   - NotificationService for sending notifications
   - AlertService for monitoring and triggering alerts
   - TemplateService for rendering templates with placeholders

4. **Create UI**
   - Notification inbox for users
   - Alert management dashboard
   - Template editor
   - User preference settings

---

## 📝 Notes

- All DateTime fields use UTC (`DateTime.UtcNow`)
- JSON fields store complex configurations and conditions
- Soft delete pattern used where appropriate
- Comprehensive indexing for performance
- Check constraints enforce data integrity
- Cascade delete configured for dependent records

---

## 🐛 Troubleshooting

### If migration fails:
1. Check for syntax errors in model classes
2. Verify all foreign key relationships
3. Ensure no circular dependencies
4. Check that all required packages are installed

### If database update fails:
1. Verify connection string in `appsettings.json`
2. Ensure SQL Server is running
3. Check database user permissions
4. Review migration SQL for conflicts

---

**Created:** November 11, 2025  
**Section:** 9 - Unified Notification System  
**Tables:** 8  
**Status:** Ready for Migration
