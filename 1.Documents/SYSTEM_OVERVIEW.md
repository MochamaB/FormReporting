# KTDA ICT Reporting System - System Overview

## Executive Summary
The KTDA ICT Reporting System is a comprehensive ASP.NET Core MVC application designed to manage ICT infrastructure reporting across multiple KTDA facilities (factories, regional offices, subsidiaries). The system features **77 database tables** organized into **12 logical modules** with a simplified role-based access control (RBAC) system.

---

## System Architecture

```
KTDA ICT Reporting System
│
├── Core System (Cross-cutting)
│   ├── Multi-Tenancy
│   ├── Authentication & Authorization (RBAC)
│   └── Audit & Logging
│
├── Business Modules (Domain-specific)
│   ├── Organizational Management
│   ├── Forms & Submissions
│   ├── Reporting & Analytics
│   ├── Asset Management (Hardware & Software)
│   ├── Support & Ticketing
│   ├── Metrics & KPIs
│   ├── Financial Tracking
│   └── Media Management
│
└── System Configuration
    ├── Notifications
    ├── Templates
    └── Settings
```

---

## Module Breakdown

### **MODULE 1: Organizational Structure** 
**Namespace:** `FormReporting.Models.Organizational`  
**Tables:** 5 tables  
**Purpose:** Manage multi-tenant organizational hierarchy

| Table | Purpose | Key Entities |
|-------|---------|--------------|
| Regions | Geographic regions (for grouping factories) | RegionId, RegionCode, RegionName |
| Tenants | Unified tenant table (HeadOffice, Factory, Subsidiary) | TenantId, TenantType, TenantCode, RegionId |
| TenantGroups | Custom tenant groupings for flexible assignments | TenantGroupId, GroupName, GroupCode |
| TenantGroupMembers | Tenant membership in groups | GroupMemberId, TenantGroupId, TenantId |
| Departments | Organizational departments within tenants | DepartmentId, TenantId, DepartmentCode |

**Model Structure:**
- `Models/Entities/` - Database entities
- `Models/ViewModels/` - View display models
- `Models/DTOs/` - Data transfer objects (API)

---

### **MODULE 2: Identity & Access Management (RBAC)**
**Namespace:** `FormReporting.Models.Identity`
**Tables:** 11 tables
**Purpose:** User authentication, authorization, and permission management

| Table | Purpose | Key Entities |
|-------|---------|--------------|
| Roles | Job function roles with hierarchy | RoleId, RoleName, RoleCode, Level (1=HeadOffice, 2=Regional, 3=Factory) |
| Users | User accounts (ASP.NET Identity compatible) | UserId, UserName, Email, DepartmentId, PasswordHash |
| UserRoles | User-role assignments (many-to-many) | UserId, RoleId, AssignedBy |
| Modules | Application feature areas | ModuleId, ModuleName, ModuleCode, Icon, DisplayOrder |
| Permissions | Granular functional permissions | PermissionId, ModuleId, PermissionCode (e.g., 'Forms.Submit'), PermissionType |
| RolePermissions | Assign permissions to roles | RoleId, PermissionId, IsGranted (allow/deny) |
| MenuItems | Dynamic sidebar navigation (hierarchical) | MenuItemId, ParentMenuItemId, ModuleId, Route, Icon, Level |
| RoleMenuItems | Role-based menu visibility control | RoleId, MenuItemId, IsVisible |
| UserTenantAccess | Explicit tenant access exceptions | UserId, TenantId, GrantedBy, ExpiryDate, Reason |
| UserGroups | User groupings (teams, training cohorts, committees) | UserGroupId, GroupName, GroupCode, TenantId |
| UserGroupMembers | Group membership | UserId, UserGroupId, AddedBy |

**RBAC Design Philosophy:**

**Permissions (What you can DO):**
- Permissions are **functional capabilities**: `Templates.Design`, `Forms.Submit`, `Forms.Approve`, `Reports.Export`
- Assigned to **roles** via RolePermissions (not individual users)
- Permission types: View, Create, Edit, Delete, Approve, Export, Manage, Custom
- Checked via Claims-based authentication in controllers/services

**Tenant Access (Which data you can SEE):**
- Controlled by **Role.Level** automatic access:
  - **Level 1 (HeadOffice):** Access ALL 80 tenants automatically
  - **Level 2 (Regional):** Access all tenants in assigned region automatically
  - **Level 3 (Factory/Subsidiary):** Access ONLY own tenant automatically
- **UserTenantAccess** table is for **exceptions only**:
  - Temporary access (audits, projects) with ExpiryDate
  - Cross-region assignments
  - Special access grants with Reason tracking

**Form Access (Which forms you can FILL):**
- Controlled by **FormTemplateAssignments** table (separate from permissions)
- 8 assignment types: All, TenantType, TenantGroup, SpecificTenant, Role, Department, UserGroup, SpecificUser
- User sees form if: (Has Forms.Submit permission) AND (Is assigned the template)

**Menu Visibility:**
- Controlled by **RoleMenuItems** table (role-based visibility)
- Menu query: User → UserRoles → RoleMenuItems → MenuItems
- Simple and efficient (no complex permission checks per menu item)

**Special Features:**
- ✅ ASP.NET Core Identity integration
- ✅ Three-tier role hierarchy (Level 1/2/3 with automatic tenant access)
- ✅ Role-based dynamic menu generation
- ✅ Claims-based authorization (roles + permissions in user claims)
- ✅ Simplified tenant access (automatic by level + explicit exceptions)
- ✅ No user-level permission overrides (role-based only for simplicity)
- ✅ Separation of concerns: Permissions (functional) vs Assignments (data access)

---

### **MODULE 3: Form Templates & Submissions**
**Namespace:** `FormReporting.Models.Forms`  
**Tables:** 13 tables  
**Purpose:** Dynamic form creation, submission, and workflow management

| Table | Purpose | Key Entities |
|-------|---------|--------------|
| FormCategories | Form categorization | CategoryId, CategoryName |
| FormTemplates | Reusable form definitions | TemplateId, TenantId |
| FormSections | Form section groupings | SectionId, TemplateId |
| FormFields | Individual form fields | FieldId, SectionId, FieldType |
| FormFieldOptions | Dropdown/checkbox options | OptionId, FieldId |
| FormSubmissions | Submitted form instances | SubmissionId, TemplateId |
| FormSubmissionValues | Field value storage | ValueId, SubmissionId, FieldId |
| FormSubmissionAttachments | File attachments | AttachmentId, SubmissionId |
| FormVersions | Template versioning | VersionId, TemplateId |
| FormApprovals | Approval workflow | ApprovalId, SubmissionId |
| FormSchedules | Recurring form assignments | ScheduleId, TemplateId |
| FormAssignments | User/tenant assignments | AssignmentId, TemplateId |
| FormValidationRules | Field validation logic | RuleId, FieldId |

**Key Features:**
- Dynamic form builder
- Multi-step forms with sections
- Approval workflow
- Recurring schedules (monthly, quarterly)
- File attachments

---

### **MODULE 4: Metrics & KPI Tracking**
**Namespace:** `FormReporting.Models.Metrics`  
**Tables:** 5 tables  
**Purpose:** Performance metrics and KPI monitoring

| Table | Purpose | Key Entities |
|-------|---------|--------------|
| MetricDefinitions | KPI/metric definitions | MetricId, MetricName, UnitOfMeasure |
| MetricTargets | Performance targets | TargetId, MetricId, TargetValue |
| MetricValues | Actual metric recordings | ValueId, MetricId, RecordedValue |
| TenantMetrics | Tenant-specific metrics | TenantId, MetricId |
| DashboardWidgets | Dashboard visualization configs | WidgetId, MetricId |

**Examples:**
- System uptime percentage
- Ticket resolution time
- Hardware utilization rates

---

### **MODULE 5: Software Management**
**Namespace:** `FormReporting.Models.Software`  
**Tables:** 5 tables  
**Purpose:** Software asset and license tracking

| Table | Purpose | Key Entities |
|-------|---------|--------------|
| SoftwareProducts | Software catalog | ProductId, ProductName, Vendor |
| SoftwareLicenses | License management | LicenseId, ProductId, LicenseKey |
| SoftwareInstallations | Installation tracking | InstallationId, ProductId, LocationId |
| SoftwareVersions | Version control | VersionId, ProductId |
| LicenseAllocations | License assignments | AllocationId, LicenseId, UserId |

---

### **MODULE 6: Hardware Inventory**
**Namespace:** `FormReporting.Models.Hardware`  
**Tables:** 6 tables  
**Purpose:** Physical IT asset tracking

| Table | Purpose | Key Entities |
|-------|---------|--------------|
| HardwareCategories | Device categorization | CategoryId, CategoryName |
| HardwareAssets | Physical assets | AssetId, AssetTag, SerialNumber |
| HardwareComponents | Asset components | ComponentId, AssetId |
| AssetAssignments | User/location assignments | AssignmentId, AssetId, UserId |
| AssetMaintenance | Maintenance records | MaintenanceId, AssetId |
| AssetDisposals | End-of-life tracking | DisposalId, AssetId |

---

### **MODULE 7: Support Tickets**
**Namespace:** `FormReporting.Models.Support`  
**Tables:** 7 tables  
**Purpose:** IT support ticket management

| Table | Purpose | Key Entities |
|-------|---------|--------------|
| TicketCategories | Ticket classification | CategoryId, CategoryName |
| TicketPriorities | Priority levels | PriorityId, PriorityName |
| Tickets | Support requests | TicketId, Status, Priority |
| TicketComments | Ticket discussions | CommentId, TicketId |
| TicketAttachments | Supporting files | AttachmentId, TicketId |
| TicketAssignments | Agent assignments | AssignmentId, TicketId, UserId |
| TicketSLA | SLA tracking | SLAId, TicketId |

**Workflow:**
Open → Assigned → In Progress → Pending → Resolved → Closed

---

### **MODULE 8: Financial Tracking**
**Namespace:** `FormReporting.Models.Financial`  
**Tables:** 4 tables  
**Purpose:** IT budget and expenditure tracking

| Table | Purpose | Key Entities |
|-------|---------|--------------|
| Budgets | Annual/period budgets | BudgetId, TenantId, Amount |
| Expenditures | Actual spending | ExpenditureId, BudgetId, Amount |
| CostCenters | Cost allocation centers | CostCenterId, CenterCode |
| Vendors | Supplier management | VendorId, VendorName |

---

### **MODULE 9: Reporting & Analytics**
**Namespace:** `FormReporting.Models.Reporting`  
**Tables:** 5 tables  
**Purpose:** Report generation and business intelligence

| Table | Purpose | Key Entities |
|-------|---------|--------------|
| ReportDefinitions | Report templates | ReportId, ReportName |
| ReportSchedules | Automated report generation | ScheduleId, ReportId |
| ReportExecutions | Report generation history | ExecutionId, ReportId |
| ReportParameters | Dynamic report filters | ParameterId, ReportId |
| SavedReports | User-saved reports | SavedReportId, UserId |

---

### **MODULE 10: Notifications**
**Namespace:** `FormReporting.Models.Notifications`  
**Tables:** 5 tables  
**Purpose:** Unified notification system

| Table | Purpose | Key Entities |
|-------|---------|--------------|
| NotificationTemplates | Message templates | TemplateId, TemplateName |
| Notifications | Notification queue | NotificationId, RecipientId |
| NotificationRecipients | Delivery tracking | RecipientId, NotificationId |
| NotificationPreferences | User preferences | UserId, Channel, Enabled |
| NotificationChannels | Delivery methods | ChannelId, ChannelType |

**Channels:** Email, SMS, In-App, Web Push

---

### **MODULE 11: Media Management**
**Namespace:** `FormReporting.Models.Media`  
**Tables:** 3 tables  
**Purpose:** File and media storage

| Table | Purpose | Key Entities |
|-------|---------|--------------|
| MediaFiles | File metadata | FileId, FileName, ContentType |
| MediaFolders | Folder organization | FolderId, FolderName |
| MediaAccess | File access control | AccessId, FileId, UserId |

---

### **MODULE 12: Audit & Logging**
**Namespace:** `FormReporting.Models.Audit`  
**Tables:** 4 tables  
**Purpose:** System activity tracking and compliance

| Table | Purpose | Key Entities |
|-------|---------|--------------|
| AuditLogs | General activity logs | AuditId, Action, UserId |
| DataChangeLogs | Entity change tracking | ChangeId, TableName, OldValue, NewValue |
| LoginHistory | Authentication history | LoginId, UserId, IpAddress |
| PermissionAuditLog | Permission change tracking | AuditId, TargetUserId, Action |

---

## Project Structure

```
FormReporting/
│
├── Models/
│   ├── Entities/                    # Database entities (EF Core)
│   │   ├── Organizational/
│   │   ├── Identity/
│   │   ├── Forms/
│   │   ├── Metrics/
│   │   ├── Software/
│   │   ├── Hardware/
│   │   ├── Support/
│   │   ├── Financial/
│   │   ├── Reporting/
│   │   ├── Notifications/
│   │   ├── Media/
│   │   └── Audit/
│   │
│   ├── ViewModels/                  # UI display models
│   │   ├── Organizational/
│   │   ├── Identity/
│   │   ├── Forms/
│   │   └── ... (same structure)
│   │
│   ├── DTOs/                        # API data transfer objects
│   │   ├── Organizational/
│   │   ├── Identity/
│   │   ├── Forms/
│   │   └── ... (same structure)
│   │
│   └── Common/                      # Shared models
│       ├── BaseEntity.cs
│       ├── IAuditable.cs
│       ├── ISoftDelete.cs
│       └── PagedResult.cs
│
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/              # EF Core configurations
│   │   ├── Organizational/
│   │   ├── Identity/
│   │   └── ... (same structure)
│   │
│   └── Migrations/
│
├── Services/
│   ├── Interfaces/
│   └── Implementations/
│
├── Controllers/
├── Views/
└── wwwroot/
```

---

## Naming Conventions

### **1. Entity Models (Database Tables)**
- **Location:** `Models/Entities/{Module}/`
- **Naming:** Singular, PascalCase
- **Example:** `Tenant.cs`, `FormTemplate.cs`, `User.cs`

```csharp
namespace FormReporting.Models.Entities.Organizational
{
    public class Tenant : BaseEntity
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        // ... properties
    }
}
```

### **2. View Models (UI Display)**
- **Location:** `Models/ViewModels/{Module}/`
- **Naming:** `{Entity}ViewModel`
- **Example:** `TenantViewModel.cs`, `FormSubmissionViewModel.cs`

```csharp
namespace FormReporting.Models.ViewModels.Organizational
{
    public class TenantViewModel
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string DisplayName { get; set; }  // Computed/formatted
        public List<DepartmentViewModel> Departments { get; set; }
    }
}
```

### **3. DTOs (API Data Transfer)**
- **Location:** `Models/DTOs/{Module}/`
- **Naming:** `{Entity}Dto`, `Create{Entity}Dto`, `Update{Entity}Dto`
- **Example:** `TenantDto.cs`, `CreateTenantDto.cs`

```csharp
namespace FormReporting.Models.DTOs.Organizational
{
    public class CreateTenantDto
    {
        [Required]
        public string TenantName { get; set; }
        public string TenantCode { get; set; }
        // ... only required fields for creation
    }
}
```

---

## Technology Stack

| Layer | Technology |
|-------|------------|
| **Backend** | ASP.NET Core 8.0 MVC, C# 12 |
| **ORM** | Entity Framework Core 8.0 |
| **Database** | Microsoft SQL Server 2022 |
| **Authentication** | ASP.NET Core Identity |
| **Authorization** | Custom RBAC with Claims |
| **Frontend** | Razor Pages, Velzon Admin Template |
| **Styling** | Bootstrap 5, Custom KTDA Theme |
| **Caching** | IMemoryCache, Distributed Cache |
| **Logging** | Serilog |
| **Background Jobs** | Hangfire |
| **Real-time** | SignalR |

---

## Summary Statistics

| Metric | Count |
|--------|-------|
| **Total Tables** | 77 |
| **Modules** | 12 |
| **Entity Models** | 77 |
| **ViewModels** | ~77 |
| **DTOs** | ~150 (Create, Update, List) |
| **Controllers** | ~15-20 |
| **Views** | ~60-80 |

**Section Breakdown:**
- Section 1 (Organizational): 5 tables ✅ Implemented
- Section 2 (Identity & RBAC): 11 tables ⚠️ Schema needs update
- Section 3-12: 61 tables ⏳ Pending

---

## Next Steps

1. ✅ Create base classes and interfaces (`BaseEntity`, `IAuditable`)
2. ✅ Generate Entity Models by module
3. ✅ Create EF Core configurations
4. ✅ Generate ViewModels
5. ✅ Generate DTOs
6. ✅ Create ApplicationDbContext with DbSets
7. ✅ Run initial migration
8. ✅ Seed initial data

---

## Document Status

**Version:** 2.0
**Last Updated:** November 6, 2025
**Status:** ⚠️ Section 1 Implemented, Section 2 Requires Schema Updates

**Key Changes in v2.0:**
- ✅ Corrected Section 2 (Identity & RBAC) table count: 11 tables
- ✅ Added RBAC Design Philosophy section
- ✅ Clarified simplified tenant access model (Role.Level based)
- ✅ Changed to role-based menu visibility (RoleMenuItems)
- ✅ Removed UserPermissions table (role-based only)
- ✅ Updated total table count: 77 tables
- ✅ Added separation of concerns: Permissions (functional) vs Assignments (data access)
