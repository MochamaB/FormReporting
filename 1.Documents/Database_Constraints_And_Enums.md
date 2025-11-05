# KTDA Database - Constraints & Enums Reference

**Version:** 1.0 | **Date:** November 5, 2025

Complete reference of all CHECK constraints and enumerated values for UI development.

---

## SECTION 1: ORGANIZATIONAL STRUCTURE

### Tenants - TenantType
```sql
CHECK (TenantType IN ('HeadOffice', 'Factory', 'Subsidiary'))
```
**UI Options:** Head Office, Factory, Subsidiary

### TenantGroups - GroupType
```sql
CHECK (GroupType IN ('Region', 'Project', 'Performance', 'Custom'))
```
**UI Options:** Regional Group, Project Team, Performance Tier, Custom Group

---

## SECTION 2: IDENTITY & ACCESS

### Users - LoginProvider
```sql
CHECK (LoginProvider IS NULL OR LoginProvider IN ('Local', 'ActiveDirectory', 'Azure', 'Google', 'SAML'))
```
**UI Options:** Local Database, Active Directory, Azure AD, Google Workspace, SAML SSO

### UserGroups - GroupType
```sql
CHECK (GroupType IN ('Training', 'Project', 'Committee', 'Department', 'Custom'))
```
**UI Options:** Training Cohort, Project Team, Committee, Department, Custom Group

---

## SECTION 3: METRICS & KPI TRACKING

### MetricDefinitions - SourceType
```sql
CHECK (SourceType IN ('UserInput', 'SystemCalculated', 'ExternalSystem', 'ComplianceTracking', 'AutomatedCheck'))
```
**UI Options:** User Input, System Calculated, External System, Compliance Tracking, Automated Check

### MetricDefinitions - DataType
```sql
CHECK (DataType IN ('Integer', 'Decimal', 'Percentage', 'Boolean', 'Text', 'Duration', 'Date', 'DateTime'))
```
**UI Options:** Integer, Decimal, Percentage, Boolean, Text, Duration, Date, DateTime

### MetricDefinitions - Unit
```sql
CHECK (Unit IS NULL OR Unit IN ('Count', 'Percentage', 'Version', 'Status', 'Days', 'Hours', 'Minutes', 'Seconds', 'GB', 'MB', 'KB', 'TB', 'Bytes', 'None'))
```
**UI Options:** None, Count, %, Version, Status, Days, Hours, Minutes, Seconds, TB, GB, MB, KB, Bytes

### MetricDefinitions - AggregationType
```sql
CHECK (AggregationType IS NULL OR AggregationType IN ('SUM', 'AVG', 'MAX', 'MIN', 'LAST_VALUE', 'COUNT', 'NONE'))
```
**UI Options:** None, Sum, Average, Maximum, Minimum, Last Value, Count

### TenantMetrics - SourceType
```sql
CHECK (SourceType IS NULL OR SourceType IN ('UserInput', 'SystemCalculated', 'HangfireJob', 'ExternalAPI', 'Manual', 'Import'))
```

### MetricPopulationLog - Status
```sql
CHECK (Status IN ('Success', 'Failed', 'Skipped', 'Pending'))
```
**UI Badges:** Success (green), Failed (red), Skipped (gray), Pending (yellow)

---

## SECTION 4: FORM TEMPLATES & SUBMISSIONS

### FormTemplates - PublishStatus
```sql
CHECK (PublishStatus IN ('Draft', 'Published', 'Archived'))
```
**UI Badges:** Draft (gray), Published (green), Archived (orange)

### FormTemplates - FormMode
```sql
CHECK (FormMode IS NULL OR FormMode IN ('Create', 'Edit', 'View', 'Approve'))
```

### FormTemplateItems - ItemType
```sql
CHECK (ItemType IN ('Text', 'TextArea', 'Number', 'Decimal', 'Date', 'Time', 'DateTime', 
                    'Dropdown', 'Radio', 'Checkbox', 'MultiSelect', 'FileUpload', 'Image', 
                    'Signature', 'Rating', 'Slider', 'Section', 'Label', 'Divider'))
```
**Categories:**
- **Input:** Text, TextArea, Number, Decimal
- **Date/Time:** Date, Time, DateTime
- **Selection:** Dropdown, Radio, Checkbox, MultiSelect
- **Media:** FileUpload, Image, Signature
- **Rating:** Rating, Slider
- **Layout:** Section, Label, Divider

### FormItemValidations - ValidationOperator
```sql
CHECK (ValidationOperator IN ('Equals', 'NotEquals', 'GreaterThan', 'LessThan', 
                              'GreaterThanOrEqual', 'LessThanOrEqual', 
                              'Contains', 'NotContains', 'StartsWith', 'EndsWith',
                              'Regex', 'IsEmail', 'IsUrl', 'IsPhoneNumber'))
```

### FormItemCalculations - CalculationType
```sql
CHECK (CalculationType IN ('Sum', 'Average', 'Min', 'Max', 'Count', 'Formula', 'Lookup'))
```

### FormTemplateSubmissions - SubmissionStatus
```sql
CHECK (SubmissionStatus IN ('Draft', 'Submitted', 'UnderReview', 'Approved', 'Rejected', 'Returned', 'Cancelled'))
```
**UI Badges:** Draft (gray), Submitted (blue), Under Review (yellow), Approved (green), Rejected (red), Returned (orange), Cancelled (black)

### SubmissionWorkflowProgress - Status
```sql
CHECK (Status IN ('Pending', 'Approved', 'Rejected', 'Skipped', 'Returned'))
```

### WorkflowDefinitions - WorkflowType
```sql
CHECK (WorkflowType IN ('Sequential', 'Parallel', 'Conditional', 'Hybrid'))
```
**UI Options:** Sequential (→), Parallel (⇉), Conditional (?), Hybrid (⊕)

### WorkflowSteps - StepType
```sql
CHECK (StepType IN ('Approval', 'Review', 'Notification', 'AutoAction', 'Condition'))
```

### FormItemMetricMappings - MappingType
```sql
CHECK (MappingType IN ('Direct', 'Calculated', 'Lookup', 'Aggregate'))
```

### FormTemplateAssignments - AssignmentType
```sql
CHECK (AssignmentType IN ('Tenant', 'TenantGroup', 'Region', 'Department', 'Role', 'User', 'UserGroup', 'All'))
```
**UI Options:** All Users, Specific Tenant, Tenant Group, Region, Department, Role, Individual User, User Group

---

## SECTION 5: SOFTWARE MANAGEMENT

### SoftwareVersions - SecurityLevel
```sql
CHECK (SecurityLevel IN ('Critical', 'Stable', 'Vulnerable', 'Unsupported'))
```
**UI Badges:** Stable (green), Vulnerable (yellow), Critical (red), Unsupported (gray)

### SoftwareLicenses - LicenseType
```sql
CHECK (LicenseType IN ('Perpetual', 'Subscription', 'Trial', 'Volume', 'Academic', 'OEM'))
```
**UI Options:** Perpetual, Subscription, Trial, Volume License, Academic, OEM

### TenantSoftwareInstallations - Status
```sql
CHECK (Status IN ('Active', 'Deprecated', 'NeedsUpgrade', 'EndOfLife', 'Uninstalled'))
```

### TenantSoftwareInstallations - InstallationType
```sql
CHECK (InstallationType IN ('Server', 'Workstation', 'Cloud', 'Virtual', 'Container') OR InstallationType IS NULL)
```

### SoftwareInstallationHistory - ChangeType
```sql
CHECK (ChangeType IN ('Install', 'Upgrade', 'Downgrade', 'Uninstall', 'Reinstall', 'Patch'))
```

---

## SECTION 6: HARDWARE INVENTORY

### TenantHardware - Status
```sql
CHECK (Status IS NULL OR Status IN ('Operational', 'Faulty', 'UnderRepair', 'Retired', 'InStorage', 'PendingDeployment', 'Disposed'))
```
**UI Badges:** Operational (green), Faulty (red), Under Repair (yellow), Retired (gray), In Storage (blue), Pending Deployment (orange), Disposed (black)

### HardwareMaintenanceLog - MaintenanceType
```sql
CHECK (MaintenanceType IS NULL OR MaintenanceType IN ('Preventive', 'Corrective', 'Upgrade', 'Emergency', 'Calibration', 'Inspection'))
```

---

## SECTION 7: SUPPORT TICKETS

### Tickets - TicketStatus
```sql
CHECK (TicketStatus IN ('Open', 'Assigned', 'InProgress', 'Resolved', 'Closed', 'Cancelled'))
```

### Tickets - Priority
```sql
CHECK (Priority IN ('Low', 'Normal', 'High', 'Urgent'))
```
**UI Badges:** Low (blue), Normal (green), High (orange), Urgent (red)

---

## SECTION 8: FINANCIAL TRACKING

### TenantExpenses - ExpenseType
```sql
CHECK (ExpenseType IN ('Purchase', 'Subscription', 'Maintenance', 'Service', 'Internal', 'Utility', 'Other'))
```

---

## SECTION 9: UNIFIED NOTIFICATION SYSTEM

### NotificationChannels - ChannelType
```sql
CHECK (ChannelType IN ('Email', 'SMS', 'Push', 'InApp', 'Webhook'))
```

### Notifications - Priority
```sql
CHECK (Priority IN ('Critical', 'High', 'Normal', 'Low'))
```
**UI Badges:** Critical (red), High (orange), Normal (blue), Low (gray)

### NotificationDelivery - Status
```sql
CHECK (Status IN ('Pending', 'Sent', 'Delivered', 'Failed', 'Bounced', 'Spam', 'Unsubscribed', 'Cancelled'))
```

### UserNotificationPreferences - Frequency
```sql
CHECK (Frequency IN ('Immediate', 'Hourly', 'Daily', 'Weekly', 'Never'))
```

### UserNotificationPreferences - MinimumPriority
```sql
CHECK (MinimumPriority IN ('Low', 'Normal', 'High', 'Critical'))
```

### AlertDefinitions - Severity
```sql
CHECK (Severity IN ('Critical', 'High', 'Medium', 'Low'))
```

---

## SECTION 11: REPORTING & ANALYTICS

### ReportDefinitions - ReportType
```sql
CHECK (ReportType IN ('Tabular', 'Chart', 'Pivot', 'Dashboard', 'CrossTab'))
```

### ReportDefinitions - ChartType
```sql
CHECK (ChartType IS NULL OR ChartType IN ('Bar', 'Line', 'Pie', 'Area', 'Column', 'Scatter', 'Combo'))
```

### ReportFilters - Operator
```sql
CHECK (Operator IN ('Equals', 'NotEquals', 'GreaterThan', 'LessThan', 'Contains', 'Between', 'In'))
```

### ReportSchedules - ScheduleType
```sql
CHECK (ScheduleType IN ('Daily', 'Weekly', 'Monthly', 'Quarterly', 'Yearly', 'OnDemand'))
```

### ReportSchedules - OutputFormat
```sql
CHECK (OutputFormat IN ('PDF', 'Excel', 'CSV', 'JSON', 'HTML'))
```

### ReportSchedules - PageOrientation
```sql
CHECK (PageOrientation IN ('Portrait', 'Landscape'))
```

---

## SECTION 13: MEDIA MANAGEMENT

### MediaFiles - StorageProvider
```sql
CHECK (StorageProvider IN ('Local', 'Azure', 'AWS', 'GoogleCloud'))
```

### MediaFiles - VirusScanStatus
```sql
CHECK (VirusScanStatus IN ('Pending', 'Clean', 'Infected', 'Quarantined', 'Skipped'))
```
**UI Badges:** Clean (green), Infected (red), Quarantined (orange), Pending (yellow), Skipped (gray)

### EntityMediaFiles - EntityType
```sql
CHECK (EntityType IN ('Ticket', 'Submission', 'Expense', 'Hardware', 'User', 'Tenant'))
```

### FileAccessLog - AccessType
```sql
CHECK (AccessType IN ('View', 'Download', 'Upload', 'Delete', 'Share'))
```

---

## SECTION 14: AUDIT & LOGGING

### AuditLogs - ActionType
```sql
CHECK (ActionType IN ('INSERT', 'UPDATE', 'DELETE', 'SELECT'))
```

### UserActivityLog - ActivityType
```sql
CHECK (ActivityType IN ('Login', 'Logout', 'PageView', 'FormSubmit', 'FileUpload', 'FileDownload', 'ReportGenerate', 'Export', 'Import', 'PasswordChange'))
```

---

## APPLICATION ENUMS (C# Example)

```csharp
// Copy these enums to your application
public enum TenantType { HeadOffice, Factory, Subsidiary }
public enum FormItemType { Text, TextArea, Number, Decimal, Date, Time, DateTime, Dropdown, Radio, Checkbox, MultiSelect, FileUpload, Image, Signature, Rating, Slider, Section, Label, Divider }
public enum SubmissionStatus { Draft, Submitted, UnderReview, Approved, Rejected, Returned, Cancelled }
public enum WorkflowType { Sequential, Parallel, Conditional, Hybrid }
public enum Priority { Low, Normal, High, Critical }
public enum NotificationChannel { Email, SMS, Push, InApp, Webhook }
public enum HardwareStatus { Operational, Faulty, UnderRepair, Retired, InStorage, PendingDeployment, Disposed }
public enum SecurityLevel { Stable, Vulnerable, Critical, Unsupported }
public enum LicenseType { Perpetual, Subscription, Trial, Volume, Academic, OEM }
```

---

**End of Reference** | Total Constraints: 45+ | Total Enum Values: 200+
