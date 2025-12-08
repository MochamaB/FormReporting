# KTDA ICT REPORTING SYSTEM - COMPREHENSIVE PROJECT DEEP DIVE

**Last Updated:** December 5, 2025
**Version:** 1.0
**Purpose:** Complete project reference for all future development sessions

---

## PROJECT OVERVIEW

**System Name:** KTDA ICT Reporting System
**Technology:** ASP.NET Core 9.0 MVC Web Application
**Database:** SQL Server with Entity Framework Core 9.0.10
**Authentication:** Cookie-based authentication (custom implementation without ASP.NET Identity scaffolding)
**Project Type:** Multi-tenant enterprise ICT management and reporting platform

---

## 1. SYSTEM ARCHITECTURE

### 1.1 Technology Stack

- **.NET 9.0** - Latest LTS framework
- **ASP.NET Core MVC** - Server-side rendering with Razor views
- **Entity Framework Core 9.0.10** - ORM with Code First approach
- **SQL Server** - Relational database with query splitting optimization
- **Authentication:** Cookie authentication with password hashing (PasswordHasher<User>)
- **Frontend:** Bootstrap 5, Remix Icons, Vanilla JavaScript (no jQuery), SortableJS for drag-drop
- **Architecture Pattern:** Layered architecture (Controllers → Services → Data/Repositories)

### 1.2 Project Structure

```
FormReporting/
├── Controllers/          # MVC controllers (UI) and API controllers (AJAX endpoints)
├── Services/            # Business logic layer (dependency injection)
├── Models/
│   ├── Entities/        # Database entity models (12 domains)
│   ├── ViewModels/      # DTOs and display models
│   └── Common/          # Base classes, interfaces, enums
├── Data/
│   ├── ApplicationDbContext.cs       # EF Core DbContext
│   ├── Configurations/               # Fluent API entity configurations
│   └── Seeders/                      # Database seed data
├── Views/
│   ├── Forms/           # Form builder and management views
│   ├── Shared/          # Reusable UI components
│   └── ...              # Other feature views
├── wwwroot/
│   ├── assets/js/       # JavaScript modules
│   └── css/             # Stylesheets
└── Extensions/          # Extension methods and helpers
```

---

## 2. DOMAIN MODELS & DATABASE SCHEMA

The system is organized into **12 major domains**, each with its own entities and relationships:

### 2.1 ORGANIZATIONAL STRUCTURE

**Purpose:** Multi-tenant organizational hierarchy

**Entities:**

1. **Region** - Geographic regions grouping factories
   - Fields: RegionId, RegionName, RegionCode, Description, IsActive
   - Table: `Regions`

2. **Tenant** - Unified tenant model (HeadOffice, Factory, Subsidiary, Branch, Warehouse)
   - Fields: TenantId, TenantType, TenantCode, TenantName, RegionId, Location, Latitude, Longitude
   - Relationships: Region (N:1), Departments (1:N), Users (1:N)
   - Table: `Tenants`

3. **Department** - Organizational units within tenants (hierarchical)
   - Fields: DepartmentId, TenantId, DepartmentName, DepartmentCode, ParentDepartmentId
   - Supports: Nested department hierarchy
   - Table: `Departments`

4. **TenantGroup** - Custom groupings of tenants
   - Purpose: Logical grouping for reports, assignments, projects
   - Table: `TenantGroups`

**Multi-Tenancy:** Every user belongs to a primary tenant, with optional extended access via UserTenantAccess

---

### 2.2 IDENTITY & ACCESS MANAGEMENT

**Purpose:** Hierarchical role-based access control with scope-based data isolation

**Entities:**

1. **User** - ASP.NET Identity-compatible user model
   - Path: `Models/Entities/Identity/User.cs`
   - Fields: UserId, UserName, Email, PasswordHash, SecurityStamp, FirstName, LastName, EmployeeNumber, TenantId, DepartmentId
   - Features: Lockout support, 2FA ready, phone confirmation, email confirmation
   - Table: `Users`

2. **Role** - Roles with scope-based access
   - Path: `Models/Entities/Identity/Role.cs`
   - Fields: RoleId, RoleName, RoleCode, ScopeLevelId
   - Relationships: ScopeLevel (defines data access breadth)
   - Table: `Roles`

3. **ScopeLevel** - Defines hierarchical access levels
   - Levels: **GLOBAL** (all data) → **REGIONAL** (region tenants) → **TENANT** (single tenant) → **DEPARTMENT** (dept only) → **INDIVIDUAL** (self only)
   - Table: `ScopeLevels`

4. **Permission** - Granular permissions
   - Types: View, Create, Edit, Delete, Approve, Export, Custom
   - Linked to modules and features
   - Table: `Permissions`

5. **UserRole** - User-role assignments (junction table)
   - Table: `UserRoles`

6. **RolePermission** - Role-permission mappings
   - Table: `RolePermissions`

7. **UserTenantAccess** - Explicit tenant access exceptions (grants cross-tenant access for audits, projects)
   - Table: `UserTenantAccesses`

8. **UserGroup** - User groupings (training groups, committees, project teams)
   - Table: `UserGroups`

**Authentication:** Custom cookie-based with 30-minute sliding expiration

**Authorization:** Scope-based filtering implemented in services (ScopeService determines accessible data)

---

### 2.3 FORMS & FORM BUILDER

**Purpose:** Visual drag-and-drop form template designer with dynamic submissions

**Core Entities:**

1. **FormTemplate** - Core form definition
   - Path: `Models/Entities/Forms/FormTemplate.cs`
   - Fields: TemplateId, TemplateName, TemplateCode, CategoryId, TemplateType (Daily/Weekly/Monthly), Version, PublishStatus (Draft/Published/Archived)
   - Workflow: Draft → Published → Archived
   - Features: Versioning, approval workflows, assignments
   - Table: `FormTemplates`

2. **FormTemplateSection** - Groups of fields (collapsible panels)
   - Path: `Models/Entities/Forms/FormTemplateSection.cs`
   - Fields: SectionId, TemplateId, SectionName, DisplayOrder, IsCollapsible, IconClass
   - Table: `FormTemplateSections`

3. **FormTemplateItem** - Individual form fields
   - Path: `Models/Entities/Forms/FormTemplateItem.cs`
   - Fields: ItemId, SectionId, ItemCode, ItemName, DataType (21 types), IsRequired, DisplayOrder
   - UI Fields: PlaceholderText, HelpText, PrefixText, SuffixText
   - Advanced: ConditionalLogic (JSON), LayoutType (Single/Matrix/Grid)
   - Table: `FormTemplateItems`

4. **FormItemOption** - Options for dropdowns/radio/checkbox fields
   - Path: `Models/Entities/Forms/FormItemOption.cs`
   - Fields: OptionId, ItemId, OptionLabel, OptionValue, DisplayOrder, IsDefault
   - Scoring: ScoreValue, ScoreWeight (for assessment forms)
   - Table: `FormItemOptions`

5. **FormItemConfiguration** - Key-value field configurations
   - Examples: minValue, maxValue, decimalPlaces, allowNegative, inputMask
   - Table: `FormItemConfigurations`

6. **FormItemValidation** - Field validation rules
   - Types: Required, Email, Phone, URL, MinLength, MaxLength, Regex, Range, Custom
   - Table: `FormItemValidations`

7. **FormCategory** - Form categorization
   - Table: `FormCategories`

8. **FieldLibrary** - Reusable field definitions
   - Table: `FieldLibraries`

9. **FormItemOptionTemplate** - Pre-defined option sets
   - Path: `Models/Entities/Forms/FormItemOptionTemplate.cs`
   - Examples: Likert scales, Yes/No, Satisfaction scales
   - Fields: TemplateId, TemplateName, TemplateCode, Category, SubCategory, HasScoring
   - Table: `FormItemOptionTemplates`

10. **FormItemOptionTemplateItem** - Items within option templates
    - Table: `FormItemOptionTemplateItems`

**21 Supported Field Types:**
- **Input:** Text, TextArea, Number, Decimal
- **DateTime:** Date, Time, DateTime
- **Selection:** Dropdown, Radio, Checkbox, MultiSelect
- **Media:** FileUpload, Image, Signature
- **Rating:** Rating, Slider
- **Contact:** Email, Phone, URL
- **Specialized:** Currency, Percentage

**Form Submission Flow:**

1. **FormTemplateSubmission** - Form submission instances
   - Fields: SubmissionId, TemplateId, TenantId, ReportingYear, ReportingMonth, Status (Draft/Submitted/InApproval/Approved/Rejected)
   - Table: `FormTemplateSubmissions`

2. **FormTemplateResponse** - Individual field responses
   - Fields: ResponseId, SubmissionId, ItemId, ResponseValue (JSON)
   - Table: `FormTemplateResponses`

3. **WorkflowDefinition** + **WorkflowStep** - Approval workflows
   - Tables: `WorkflowDefinitions`, `WorkflowSteps`

4. **SubmissionWorkflowProgress** - Tracks approval progress
   - Table: `SubmissionWorkflowProgresses`

**Form Builder Features:**
- 3-step wizard (Setup → Build → Review/Publish)
- Drag-and-drop sections and fields
- Field properties panel with tabs (General, Configuration, Validation, Advanced, Options)
- Option templates (pre-defined option sets: Likert scales, Yes/No, Satisfaction scales)
- Field library integration
- Conditional logic (show/hide fields)
- Matrix layouts for rating grids
- Auto-save drafts
- Template versioning

---

### 2.4 METRICS & KPI TRACKING

**Purpose:** Track and measure ICT performance indicators

**Entities:**

1. **MetricDefinition** - KPI metric definitions
   - Path: `Models/Entities/Metrics/MetricDefinition.cs`
   - Fields: MetricId, MetricCode, MetricName, Category, SourceType (UserInput/SystemCalculated/ExternalSystem)
   - DataType: Integer, Decimal, Percentage, Currency, Duration, Count
   - KPI Thresholds: ThresholdGreen, ThresholdYellow, ThresholdRed
   - Table: `MetricDefinitions`

2. **TenantMetric** - Time-series metric values per tenant
   - Fields: TenantId, MetricId, MetricValue, ReportingPeriod
   - Table: `TenantMetrics`

3. **SystemMetricLog** - Automated system metrics
   - Table: `SystemMetricLogs`

4. **FormItemMetricMapping** - Maps form fields to metrics (post-submission population)
   - Table: `FormItemMetricMappings`

**Metric Sources:**
- User input (via forms)
- System calculated
- External system integration
- Compliance tracking
- Automated checks

---

### 2.5 SOFTWARE MANAGEMENT

**Purpose:** Track software products, versions, licenses, and installations

**Entities:**

1. **SoftwareProduct** - Software catalog
   - Path: `Models/Entities/Software/SoftwareProduct.cs`
   - Fields: ProductId, ProductCode, ProductName, Vendor, ProductCategory, LicenseModel, IsKTDAProduct
   - Table: `SoftwareProducts`

2. **SoftwareVersion** - Version registry
   - Fields: VersionId, ProductId, VersionNumber, ReleaseDate
   - Table: `SoftwareVersions`

3. **SoftwareLicense** - License management
   - Fields: LicenseId, ProductId, LicenseKey, LicenseType (Perpetual/Subscription/Trial), ExpiryDate
   - Status: Active, Expired, Expiring, Suspended
   - Table: `SoftwareLicenses`

4. **TenantSoftwareInstallation** - Per-tenant installations
   - Fields: InstallationId, TenantId, ProductId, VersionId, LicenseId, InstallationDate
   - Table: `TenantSoftwareInstallations`

5. **SoftwareInstallationHistory** - Installation audit trail
   - Table: `SoftwareInstallationHistories`

---

### 2.6 HARDWARE INVENTORY

**Purpose:** Track hardware assets across tenants

**Entities:**

1. **HardwareCategory** - Asset categories
   - Examples: Desktop, Laptop, Server, Network Equipment, Printer
   - Table: `HardwareCategories`

2. **HardwareItem** - Master hardware catalog
   - Path: `Models/Entities/Hardware/HardwareItem.cs`
   - Fields: HardwareItemId, CategoryId, ItemCode, ItemName, Manufacturer, Model, Specifications
   - Table: `HardwareItems`

3. **TenantHardware** - Tenant-specific hardware inventory
   - Fields: TenantHardwareId, TenantId, HardwareItemId, SerialNumber, AssetTag, PurchaseDate, WarrantyExpiryDate
   - Status: Available, Assigned, InUse, InStorage, UnderMaintenance, Damaged, Lost, Stolen, Disposed, Retired
   - Condition: Excellent, Good, Fair, Poor, Faulty
   - Table: `TenantHardwares`

4. **HardwareMaintenanceLog** - Maintenance history
   - Types: Preventive, Corrective, Emergency, Routine, Upgrade
   - Table: `HardwareMaintenanceLogs`

---

### 2.7 SUPPORT TICKETS

**Purpose:** Internal and external ticketing system integration

**Entities:**

1. **TicketCategory** - Ticket categorization
   - Table: `TicketCategories`

2. **Ticket** - Support tickets
   - Path: `Models/Entities/Tickets/Ticket.cs`
   - Fields: TicketId, TicketNumber, TenantId, Title, Description, Priority (Low/Medium/High/Critical), Status
   - People: ReportedBy, AssignedTo, EscalatedTo, ResolvedBy
   - SLA: SLADueDate, IsSLABreached
   - **External System Integration:**
     - IsExternal, ExternalSystem (Jira/ServiceNow/Zendesk), ExternalTicketId, ExternalTicketUrl
     - LastSyncDate, SyncStatus, SyncError
   - Asset Linkage: RelatedHardwareId, RelatedSoftwareId
   - Table: `Tickets`

3. **TicketComment** - Ticket conversation thread
   - Table: `TicketComments`

**Workflow:** Open → Assigned → InProgress → Resolved → Closed

---

### 2.8 FINANCIAL TRACKING

**Purpose:** ICT budget and expense management

**Entities:**

1. **BudgetCategory** - Budget line items
   - Examples: Hardware, Software, Licenses, Maintenance, Training, Infrastructure
   - Table: `BudgetCategories`

2. **TenantBudget** - Per-tenant budgets
   - Fields: BudgetId, TenantId, CategoryId, FiscalYear, Quarter, AllocatedAmount
   - Status: Draft, Submitted, Approved, Active, Closed
   - Table: `TenantBudgets`

3. **TenantExpense** - Expenditure tracking
   - Fields: ExpenseId, TenantId, CategoryId, Amount, ExpenseDate, Description, ApprovalStatus
   - Table: `TenantExpenses`

---

### 2.9 NOTIFICATIONS

**Purpose:** Unified multi-channel notification system

**Entities:**

1. **NotificationChannel** - Delivery channels
   - Types: Email, SMS, InApp, WebPush, Slack, Teams
   - Table: `NotificationChannels`

2. **Notification** - Central notification inbox
   - Path: `Models/Entities/Notifications/Notification.cs`
   - Fields: NotificationId, NotificationType, Title, Message, Priority, SourceEntityType, SourceEntityId, ActionUrl
   - Scheduling: ScheduledDate, ExpiryDate
   - Table: `Notifications`

3. **NotificationRecipient** - Notification recipients
   - Status: Pending, Sent, Delivered, Read, Failed
   - Table: `NotificationRecipients`

4. **NotificationDelivery** - Multi-channel delivery tracking
   - Table: `NotificationDeliveries`

5. **NotificationTemplate** - Reusable templates
   - Table: `NotificationTemplates`

6. **UserNotificationPreference** - User channel preferences
   - Table: `UserNotificationPreferences`

7. **AlertDefinition** - Automated alert rules
   - Table: `AlertDefinitions`

8. **AlertHistory** - Alert trigger history
   - Table: `AlertHistories`

---

### 2.10 REPORTING & ANALYTICS

**Purpose:** Custom report builder and dashboard analytics

**Entities:**

1. **TenantPerformanceSnapshot** - Pre-aggregated metrics
   - Table: `TenantPerformanceSnapshots`

2. **RegionalMonthlySnapshot** - Regional rollups
   - Table: `RegionalMonthlySnapshots`

3. **ReportDefinition** - Custom report definitions
   - Features: Dynamic fields, filters, groupings, sorting
   - Table: `ReportDefinitions`

4. **ReportField** - Report columns
   - Table: `ReportFields`

5. **ReportFilter** - Report filters
   - Table: `ReportFilters`

6. **ReportGrouping** - Grouping configuration
   - Table: `ReportGroupings`

7. **ReportSorting** - Sort order
   - Table: `ReportSortings`

8. **ReportSchedule** - Scheduled report execution
   - Table: `ReportSchedules`

9. **ReportCache** - Report result caching
   - Table: `ReportCaches`

10. **ReportAccessControl** - Report permissions
    - Table: `ReportAccessControls`

11. **ReportExecutionLog** - Report run history
    - Table: `ReportExecutionLogs`

**Export Formats:** PDF, Excel, CSV, HTML, JSON, XML

---

### 2.11 MEDIA MANAGEMENT

**Purpose:** Centralized file storage and polymorphic attachments

**Entities:**

1. **MediaFile** - Master file registry
   - Fields: FileId, FileName, FilePath, FileSize, MimeType, UploadedBy, UploadedDate
   - Table: `MediaFiles`

2. **EntityMediaFile** - Polymorphic file associations
   - Fields: EntityType (e.g., "Ticket", "FormSubmission"), EntityId, FileId
   - Table: `EntityMediaFiles`

3. **FileAccessLog** - File access audit trail
   - Table: `FileAccessLogs`

---

### 2.12 AUDIT & LOGGING

**Purpose:** Comprehensive audit trail and activity monitoring

**Entities:**

1. **AuditLog** - Data change audit trail
   - Actions: Create, Read, Update, Delete, Login, Logout, Approve, Reject, Export, Import
   - Stores: Entity type, entity ID, old values, new values, changed by, changed date
   - Table: `AuditLogs`

2. **UserActivityLog** - User activity tracking
   - Tracks: Page views, button clicks, form submissions, report generation
   - Table: `UserActivityLogs`

---

## 3. SERVICES LAYER (BUSINESS LOGIC)

### 3.1 Service Architecture

**Pattern:** Interface-based dependency injection

**Structure:**
```
Services/
├── Forms/
│   ├── IFormBuilderService + FormBuilderService
│   ├── IFormTemplateService + FormTemplateService
│   ├── IFormCategoryService + FormCategoryService
│   └── IFormItemOptionTemplateService + FormItemOptionTemplateService
├── Identity/
│   ├── IAuthenticationService + AuthenticationService
│   ├── IScopeService + ScopeService
│   ├── IUserService + UserService
│   └── IClaimsService + ClaimsService
├── Organizational/
│   ├── ITenantService + TenantService
│   └── IDepartmentService + DepartmentService
└── Metrics/
    ├── IMetricDefinitionService + MetricDefinitionService
    ├── IMetricMappingService + MetricMappingService
    └── IMetricPopulationService + MetricPopulationService
```

### 3.2 Key Services

**FormBuilderService**
- Path: `Services/Forms/FormBuilderService.cs`
- Responsibilities:
  - Load template for builder (with sections, fields, options, validations)
  - Get available field types palette (21 types)
  - Section CRUD operations (Add, Update, Delete, Duplicate, Reorder)
  - Field CRUD operations (Add, Update, Delete, Duplicate, Move, Reorder)
  - Option management (Add, Update, Delete, Reorder, SetDefault, ApplyTemplate)
  - Validation management (Add, Update, Delete, Reorder)
  - Field code generation
  - Template structure validation

**FormTemplateService**
- Path: `Services/Forms/FormTemplateService.cs`
- Responsibilities:
  - Load template for editing (includes all related data)
  - Create new versions from published templates
  - Analyze template progress (which step is complete)
  - Generate unique template codes
  - Validate template code format and uniqueness
  - Version management

**ScopeService**
- Path: `Services/Identity/ScopeService.cs`
- Responsibilities:
  - Get user's scope information
  - Determine accessible tenants based on scope hierarchy
  - Scope filtering logic: GLOBAL → REGIONAL → TENANT → DEPARTMENT → INDIVIDUAL

**UserService**
- Path: `Services/Identity/UserService.cs`
- Responsibilities:
  - Get accessible users based on current user's scope
  - Search users within scope
  - User access validation
  - Get users grouped by tenant (for bulk selection UIs)

**TenantService**
- Path: `Services/Organizational/TenantService.cs`
- Responsibilities:
  - Get accessible tenants based on current user's scope
  - Tenant access validation
  - Get tenants grouped by region

**AuthenticationService**
- Path: `Services/Identity/AuthenticationService.cs`
- Responsibilities:
  - User login (validate credentials, create claims)
  - User logout
  - Password validation
  - Security stamp management

---

## 4. CONTROLLERS & ROUTING

### 4.1 Controller Types

**UI Controllers** (return views)
- FormTemplatesController - `/Forms/FormTemplates`
- TenantsController - `/Organizational/Tenants`
- UsersController - `/Identity/Users`
- RolesController - `/Identity/Roles`
- DashboardController - `/Dashboard`

**API Controllers** (return JSON)
- FormBuilderApiController - `/api/formbuilder`
- MetricMappingApiController - `/api/metricmapping`

### 4.2 FormTemplatesController

**Path:** `Controllers/Forms/FormTemplatesController.cs`
**Route Base:** `/Forms/FormTemplates`

**Key Actions:**

| Method | Route | Purpose |
|--------|-------|---------|
| GET | / | Template dashboard with stats, filters, pagination |
| GET | /Create | New template wizard (or resume draft) |
| GET | /Edit/{id} | Create new version from published template |
| GET | /FormBuilder/{id} | Step 2: Visual drag-drop form builder |
| GET | /ReviewPublish/{id} | Step 3: Validate and publish |
| POST | /PublishTemplate | Publish template |
| POST | /SaveDraft | AJAX: Save draft |
| GET | /ValidateStageCompletion | AJAX: Validate wizard progress |
| GET | /Preview/{id} | Template preview |
| POST | /Archive | Archive published template |
| GET | /MetricMapping/{id} | Map fields to metrics |
| GET | /CheckTemplateCode | AJAX: Validate template code uniqueness |
| GET | /GenerateTemplateCode | AJAX: Auto-generate unique code |
| GET | /SearchAssignableEntities | AJAX: Search users/roles/departments |
| GET | /GetUsersGroupedByTenant | AJAX: Get users grouped by tenant |

**Dashboard Statistics:**
- Total active templates
- Published templates (with 30-day growth trend)
- Submissions this month (with growth trend)
- Completion rate percentage

---

### 4.3 FormBuilderApiController

**Path:** `Controllers/API/FormBuilderApiController.cs`
**Route Base:** `/api/formbuilder`

**Section Operations:**

| Method | Route | Purpose |
|--------|-------|---------|
| GET | /{templateId} | Load complete template for builder |
| GET | /field-types | Get available field types palette |
| GET | /{templateId}/validate | Validate template structure |
| POST | /sections/add | Add new section |
| GET | /sections/{sectionId} | Get section details |
| PUT | /sections/{sectionId} | Update section properties |
| DELETE | /sections/{sectionId} | Delete section and fields |
| POST | /sections/{sectionId}/duplicate | Clone section |
| POST | /sections/reorder | Update display order after drag-drop |

**Field Operations:**

| Method | Route | Purpose |
|--------|-------|---------|
| POST | /fields/add | Add field to section |
| GET | /fields/{fieldId} | Get field details |
| PUT | /fields/{fieldId} | Update field properties |
| DELETE | /fields/{fieldId} | Delete field |
| PUT | /fields/{fieldId}/type | Update field type (inline edit) |
| POST | /fields/{fieldId}/move | Move field to different section |
| POST | /fields/reorder | Update field display order |

**Option Operations:**

| Method | Route | Purpose |
|--------|-------|---------|
| POST | /fields/{fieldId}/options | Add option to field |
| PUT | /fields/{fieldId}/options/{optionId} | Update option |
| DELETE | /fields/{fieldId}/options/{optionId} | Delete option |
| POST | /fields/{fieldId}/options/reorder | Reorder options |
| POST | /fields/{fieldId}/options/set-default/{optionId} | Set default option |
| POST | /fields/{fieldId}/apply-template/{templateId} | Apply option template |

---

## 5. VIEWS & FRONTEND ARCHITECTURE

### 5.1 View Structure

```
Views/
├── Shared/
│   ├── _Layout.cshtml               # Main layout
│   ├── _BuilderLayout.cshtml        # Form builder full-screen layout
│   ├── _AuthLayout.cshtml           # Login/register layout
│   ├── _Sidebar.cshtml              # Dynamic menu sidebar
│   ├── _Navbar.cshtml               # Top navigation bar
│   ├── _Breadcrumb.cshtml           # Breadcrumb navigation
│   ├── _Footer.cshtml               # Footer
│   ├── Components/
│   │   ├── StatisticCards/          # Stat card components
│   │   ├── DataTable/               # Reusable data table
│   │   ├── Form/                    # Form rendering engine
│   │   │   ├── _Form.cshtml
│   │   │   ├── _FormWizard.cshtml
│   │   │   ├── _FormSection.cshtml
│   │   │   ├── _FormField.cshtml
│   │   │   └── Fields/              # 21 field type partials
│   │   ├── Wizards/                 # Wizard components
│   │   │   ├── _HorizontalWizard.cshtml
│   │   │   └── _VerticalWizard.cshtml
│   │   ├── Tabs/                    # Tab components
│   │   │   ├── _HorizontalTabs.cshtml
│   │   │   └── _VerticalTabs.cshtml
│   │   ├── DetailCard/              # Detail display cards
│   │   │   ├── _DetailCard.cshtml
│   │   │   ├── _StandardDetailCard.cshtml
│   │   │   └── _ProfileDetailCard.cshtml
│   │   └── AssignmentManager/       # Bulk assignment UI
│   │       ├── _AssignmentManager.cshtml
│   │       ├── _AddAssignmentModal.cshtml
│   │       └── ModalContent/        # Modal content partials
├── Forms/
│   ├── FormTemplates/
│   │   ├── Index.cshtml             # Template dashboard
│   │   ├── Create.cshtml            # Template wizard
│   │   ├── FormBuilder.cshtml       # Visual form builder
│   │   ├── ReviewPublish.cshtml     # Review and publish step
│   │   ├── Preview.cshtml           # Template preview
│   │   └── Partials/
│   │       ├── _BasicInformation.cshtml
│   │       ├── _Settings.cshtml
│   │       └── FormBuilder/
│   │           ├── _ToolboxPanel.cshtml        # Left: Section toolbox
│   │           ├── _CanvasPanel.cshtml         # Center: Form canvas
│   │           ├── Properties/
│   │           │   ├── _PropertiesGeneral.cshtml
│   │           │   ├── _PropertiesConfiguration.cshtml
│   │           │   ├── _PropertiesValidation.cshtml
│   │           │   ├── _PropertiesAdvanced.cshtml
│   │           │   └── _OptionsTable.cshtml
│   │           └── Components/
│   │               ├── _BuilderSection.cshtml
│   │               ├── _BuilderFieldPreview.cshtml
│   │               ├── _FieldPalette.cshtml
│   │               ├── _FieldPaletteDropdown.cshtml
│   │               ├── _AddSectionModal.cshtml
│   │               ├── _AddFieldModal.cshtml
│   │               ├── _DeleteSectionModal.cshtml
│   │               ├── _DuplicateSectionModal.cshtml
│   │               ├── _DeleteOptionModal.cshtml
│   │               └── _ErrorModal.cshtml
│   └── FormCategories/
│       ├── Index.cshtml
│       ├── Create.cshtml
│       └── Edit.cshtml
├── Organizational/
│   ├── Tenants/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Partials/
│   └── Regions/
│       ├── Index.cshtml
│       ├── Create.cshtml
│       └── Edit.cshtml
├── Identity/
│   ├── Users/
│   │   └── Index.cshtml
│   └── Roles/
│       ├── Index.cshtml
│       ├── Create.cshtml
│       ├── Edit.cshtml
│       └── Partials/
│           ├── _BasicDetails.cshtml
│           ├── _AssignPermissions.cshtml
│           ├── _AssignUsers.cshtml
│           └── _ReviewAndConfirm.cshtml
├── Dashboard/
│   └── Index.cshtml
├── Home/
│   ├── Index.cshtml
│   └── Privacy.cshtml
├── Account/
│   ├── Login.cshtml
│   └── AccessDenied.cshtml
└── Password/
    ├── ChangePassword.cshtml
    ├── ForgotPassword.cshtml
    ├── ResetPassword.cshtml
    └── ResetPasswordConfirmation.cshtml
```

### 5.2 JavaScript Architecture

**Form Builder Modules:**

1. **form-builder.js** - Main orchestrator
   - Path: `wwwroot/assets/js/pages/form-builder.js`
   - Responsibilities:
     - Initializes all components
     - Manages global state (templateId, selectedSectionId)
     - Validation and stage progression
     - Event coordination

2. **form-builder-dragdrop.js** - Drag & drop
   - Path: `wwwroot/assets/js/pages/form-builder-dragdrop.js`
   - Responsibilities:
     - SortableJS integration
     - Section reordering
     - Field drag from palette
     - Field reordering within sections
     - Cross-section field moves

3. **form-builder-fields.js** - Field operations
   - Path: `wwwroot/assets/js/pages/form-builder-fields.js`
   - Responsibilities:
     - Add/remove/edit fields
     - Field type changes
     - Show field editing modal
     - Field deletion with confirmation

4. **form-builder-properties.js** - Properties panel
   - Path: `wwwroot/assets/js/pages/form-builder-properties.js`
   - Responsibilities:
     - Dynamic tab switching (General/Configuration/Validation/Advanced/Options)
     - Field property updates
     - Real-time property saving

5. **form-builder-options.js** - Options management
   - Path: `wwwroot/assets/js/pages/form-builder-options.js`
   - Responsibilities:
     - Add/edit/delete options
     - Apply option templates
     - Reorder options
     - Set default options

**JavaScript Libraries:**
- **SortableJS** - Drag-and-drop functionality
- **Bootstrap 5** - UI components and modals
- **Remix Icons** - Icon library
- **Vanilla JavaScript** - No jQuery dependency

**CSS Structure:**
```
wwwroot/css/
├── form-builder/
│   └── form-builder.css         # Form builder specific styles
├── site.css                     # Global styles
└── [vendor CSS from CDN]
```

---

## 6. DATABASE CONFIGURATION

### 6.1 ApplicationDbContext

**Path:** `Data/ApplicationDbContext.cs`

**Features:**
- 89+ DbSet properties covering all 12 domains
- Fluent API configurations via IEntityTypeConfiguration pattern
- Automatic ModifiedDate updates on SaveChanges
- Query splitting optimization for multiple includes (prevents cartesian explosion)

**Configuration Organization:**
```
Data/Configurations/
├── Identity/          # User, Role, Permission, etc. (9 configurations)
├── Organizational/    # Tenant, Department, Region (5 configurations)
├── Forms/             # Form entities (17 configurations)
├── Metrics/           # Metric entities (3 configurations)
├── Software/          # Software management (5 configurations)
├── Hardware/          # Hardware inventory (4 configurations)
├── Tickets/           # Support tickets (3 configurations)
├── Financial/         # Budget and expenses (3 configurations)
├── Notifications/     # Notification system (8 configurations)
├── Reporting/         # Report definitions (10 configurations)
├── Media/             # File management (3 configurations)
└── Audit/             # Audit logging (2 configurations)
```

### 6.2 Connection String

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=FormReporting;..."
  }
}
```

**Query Splitting Configuration (Program.cs):**
```csharp
options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
);
```

### 6.3 Database Seeders

**Path:** `Data/Seeders/`

**Seed Order (must respect dependencies):**
1. ScopeLevelSeeder - Scope levels (GLOBAL, REGIONAL, etc.)
2. RoleSeeder - System roles
3. RegionSeeder - Geographic regions
4. TenantSeeder - Tenants (HeadOffice, Factories)
5. UserSeeder - System users
6. MenuSectionSeeder, ModuleSeeder, MenuItemSeeder - Menu system
7. FormItemOptionTemplateSeeder - Pre-defined option sets
8. MetricDefinitionSeeder - KPI metrics

**Note:** All seeders are currently commented out in Program.cs for manual execution

---

## 7. FORM BUILDER - DETAILED WORKFLOW

### 7.1 Three-Step Wizard

**Step 1: Template Setup**
- View: `Views/Forms/FormTemplates/Create.cshtml`
- Fields:
  - Template name (required)
  - Template code (auto-generated or manual)
  - Category (required, dropdown)
  - Template type (Daily/Weekly/Monthly/Quarterly/Annual)
  - Description (optional)
- Actions:
  - Save as draft
  - Continue to form builder
- **Validation:** Template name and category required

**Step 2: Form Builder** (Visual Drag-Drop Designer)
- View: `Views/Forms/FormTemplates/FormBuilder.cshtml`
- Layout: Full-screen 3-panel layout

**Left Panel (Toolbox):**
- Add Section button
- Section list (drag to reorder)
- Section actions: Edit, Duplicate, Delete

**Center Panel (Canvas):**
- Displays sections with collapsible panels
- Shows fields within sections
- Drag sections to reorder
- Drag fields from palette to sections
- Click section/field to select and show properties
- Field preview with type icon and properties summary
- Empty state prompts

**Right Panel (Properties):**
- **Section Properties:**
  - Section name, description
  - Icon class (Remix Icons)
  - Collapsible settings

- **Field Properties (5 Tabs):**
  1. **General Tab:**
     - Field label (ItemName)
     - Field type (dropdown, 21 types)
     - Required checkbox
     - Placeholder text
     - Help text
     - Default value

  2. **Configuration Tab:**
     - Number fields: Min value, max value, step, decimal places, allow negative
     - Text fields: Min length, max length, input mask, text transform, auto-trim
     - Date fields: Min date, max date, date format
     - File fields: Max file size, allowed file types
     - Rating fields: Max rating, rating style

  3. **Validation Tab:**
     - Add validation rules
     - Validation types: Required, Email, Phone, URL, Range, MinLength, MaxLength, Regex, Custom
     - Error messages
     - Validation order

  4. **Advanced Tab:**
     - Conditional logic (show/hide based on other fields)
     - Layout type (Single/Matrix/Grid/Inline)
     - Matrix group settings
     - Prefix/suffix text

  5. **Options Tab** (for Dropdown/Radio/Checkbox/MultiSelect only):
     - Option table with inline editing
     - Add option button
     - Delete option (minimum 2 options)
     - Reorder options (drag handles)
     - Set default option
     - Apply option template button
     - Option template picker modal

**Field Palette Dropdown:**
- Grouped by category:
  - Input (Text, TextArea, Number, Decimal)
  - DateTime (Date, Time, DateTime)
  - Selection (Dropdown, Radio, Checkbox, MultiSelect)
  - Media (FileUpload, Image, Signature)
  - Rating (Rating, Slider)
  - Contact (Email, Phone, URL)
  - Specialized (Currency, Percentage)
- Drag field type to section to add
- Field type icon and description

**Modals:**
- Add Section Modal - Create new section
- Add Field Modal - Quick add field
- Edit Section Modal - Edit section properties
- Delete Section Confirmation - Warn about field deletion
- Duplicate Section Modal - Clone section with fields
- Delete Option Confirmation - Enforce minimum options

**Validation:**
- At least one section required
- At least one field total required
- Warns if sections are empty (doesn't block)

**Step 3: Review & Publish**
- View: `Views/Forms/FormTemplates/ReviewPublish.cshtml`
- ViewModel: `ReviewPublishViewModel`

**Summary Section:**
- Template name, code, category
- Version number
- Created/modified dates

**Statistics Cards:**
- Total sections count
- Total fields count
- Field type breakdown (pie chart or list)

**Validation Checklist (6 Checks):**
1. ✓ Basic information complete (name, category)
2. ✓ Has at least one section (critical)
3. ✓ Has at least one field (critical)
4. ⚠ All sections have fields (warning)
5. ⚠ Description provided (warning)
6. ⚠ Required fields configured (warning)

**Section List:**
- Accordion view of all sections
- Shows field count per section
- Expand to see field list with types

**Actions:**
- Publish button (enabled only when critical validations pass)
- Back to Form Builder button
- Save as Draft button

**Post-Publish:**
- Template status changes to "Published"
- PublishedDate and PublishedBy recorded
- Template becomes read-only (cannot edit)
- Can create new version (Edit action creates Version 2)
- Can assign to users/roles/departments
- Can map fields to metrics

---

### 7.2 Option Templates Feature

**Purpose:** Provide pre-defined option sets for quick field configuration

**Examples:**
- Likert 5-Point Scale (Strongly Disagree → Strongly Agree)
- Satisfaction Scale (Very Dissatisfied → Very Satisfied)
- Frequency Scale (Never → Always)
- Yes/No/Not Applicable
- Rating 1-10
- Agreement Scale (Disagree → Agree)

**Usage:**
1. Add a selection field (Dropdown, Radio, Checkbox, MultiSelect)
2. Click "Options" tab in properties panel
3. Click "Apply Template" button
4. Select template from modal
5. Existing options are replaced with template options
6. Template usage count increments

**Template Structure:**
- Template metadata (name, code, category, description)
- Scoring support (optional score values and weights)
- Reusable across forms
- System templates vs. organization-specific templates

---

### 7.3 Form Rendering Engine

**Purpose:** Render published forms for end-user submission

**Components:**
- `Views/Shared/Components/Form/_Form.cshtml` - Main form renderer
- `Views/Shared/Components/Form/_FormSection.cshtml` - Section renderer
- `Views/Shared/Components/Form/_FormField.cshtml` - Field dispatcher
- `Views/Shared/Components/Form/Fields/` - 21 field type partials

**Render Modes:**
1. **SinglePage** - All sections visible with collapse/expand
2. **Wizard** - One section per step with navigation

**Features:**
- Progress bar (shows completion percentage)
- Auto-save drafts
- Validation (client-side HTML5 + server-side)
- Conditional logic (show/hide fields dynamically)
- Matrix layouts for rating grids
- File upload with preview
- Signature capture
- Date/time pickers

---

## 8. KEY TECHNICAL PATTERNS

### 8.1 Architectural Patterns

1. **Layered Architecture**
   - Presentation Layer (Controllers, Views)
   - Business Logic Layer (Services)
   - Data Access Layer (EF Core DbContext)
   - Clear separation of concerns

2. **Repository Pattern** - Implicit via EF Core DbContext
   - DbSet<T> acts as repository
   - Custom queries in services

3. **Service Layer Pattern** - Business logic encapsulation
   - Interface-based (IService + Service implementation)
   - Dependency injection via .NET DI container
   - Promotes testability

4. **DTO Pattern** - Data transfer objects
   - ViewModels for UI presentation
   - DTOs for API communication
   - Separation from domain entities

5. **Configuration Pattern** - EF Core Fluent API
   - IEntityTypeConfiguration<T>
   - Centralized entity configuration
   - Clean entity classes

### 8.2 Design Patterns

1. **Factory Pattern** - Field type creation
   - GetAvailableFieldTypes() creates field type definitions

2. **Builder Pattern** - Form configuration
   - FormConfig, FormSectionConfig, FormFieldConfig

3. **Template Method** - Form rendering
   - Base form renderer with field-specific implementations

4. **Strategy Pattern** - Scope-based filtering
   - Different filtering strategies based on scope level

5. **Observer Pattern** - Notification system
   - Event-driven notification delivery

### 8.3 Database Patterns

1. **Multi-tenancy** - TenantId filtering
   - IMultiTenant interface
   - Scope-based data isolation

2. **Audit Trails** - IAuditable interface
   - CreatedBy/Date, ModifiedBy/Date
   - Automatic timestamp updates

3. **Soft Delete** - ISoftDelete interface
   - IsDeleted flag instead of physical deletion
   - Preserves data for audit/recovery

4. **Normalization** - Proper table relationships
   - Separate tables for options, validations, configurations
   - Reduces data redundancy

5. **Polymorphic Associations** - EntityMediaFile
   - Generic file attachments to any entity
   - EntityType + EntityId pattern

### 8.4 Security Patterns

1. **Claims-Based Authorization**
   - User claims stored in authentication cookie
   - UserId, TenantId, RoleId, ScopeCode

2. **Scope-Based Access Control**
   - Hierarchical data filtering
   - Services enforce scope boundaries

3. **Password Security**
   - PasswordHasher<User> with PBKDF2
   - Security stamps for invalidation

4. **CSRF Protection**
   - Anti-forgery tokens on forms
   - ValidateAntiForgeryToken attribute

---

## 9. AUTHENTICATION & AUTHORIZATION

### 9.1 Authentication

**Implementation:** Custom cookie-based authentication (not using ASP.NET Identity scaffolding)

**Configuration (Program.cs):**
```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });
```

**Password Hashing:**
- Uses `IPasswordHasher<User>` from Microsoft.AspNetCore.Identity
- PBKDF2 algorithm with salt
- Compatible with ASP.NET Identity

**Login Process:**
1. User submits credentials
2. AuthenticationService validates username/password
3. Password verification via PasswordHasher
4. Security stamp check (invalidates on password change)
5. Claims principal created with user claims
6. Cookie issued via HttpContext.SignInAsync()

**Claims Stored:**
- ClaimTypes.NameIdentifier → UserId
- ClaimTypes.Name → UserName
- ClaimTypes.Email → Email
- "TenantId" → Primary tenant ID
- "RoleId" → User's role ID
- "ScopeCode" → Scope level code
- "FullName" → Display name

### 9.2 Scope-Based Authorization

**Scope Hierarchy:**
```
GLOBAL           → All tenants (system administrators)
  ↓
REGIONAL         → Tenants in region(s) (regional managers)
  ↓
TENANT           → Single tenant (tenant administrators)
  ↓
DEPARTMENT       → Single department (department heads)
  ↓
TEAM             → Team members (team leaders)
  ↓
INDIVIDUAL       → Self only (regular users)
```

**Implementation:**

**ScopeService** determines accessible data:
- `GetUserScopeAsync()` - Retrieves user's scope from claims
- `GetAccessibleTenantIdsAsync()` - Returns list of accessible tenant IDs based on scope

**Services filter queries:**
```csharp
// Example from TenantService
var scope = await _scopeService.GetUserScopeAsync(currentUser);

switch (scope.ScopeCode)
{
    case "GLOBAL":
        // No filter - access all tenants
        break;
    case "REGIONAL":
        query = query.Where(t => t.RegionId == scope.RegionId);
        break;
    case "TENANT":
        query = query.Where(t => t.TenantId == scope.PrimaryTenantId);
        break;
    // ... other scopes
}
```

**No Controller-Level Authorization:**
- Authorization logic in services (not [Authorize] attributes)
- Allows fine-grained, scope-based filtering
- Services return only accessible data

---

## 10. COMMON INTERFACES & BASE CLASSES

### 10.1 Common Interfaces

**Path:** `Models/Common/`

1. **BaseEntity** - Base class for all entities
   - Properties: CreatedDate, ModifiedDate (both UTC)
   - Auto-updated: ModifiedDate set on SaveChanges

2. **IAuditable** - Audit trail interface
   - Properties: CreatedBy, ModifiedBy (UserId)
   - Use case: Track who created/modified records

3. **ISoftDelete** - Soft delete support
   - Properties: IsDeleted, DeletedDate, DeletedBy
   - Use case: Logical deletion without data loss

4. **IMultiTenant** - Multi-tenancy support
   - Properties: TenantId
   - Use case: Tenant data isolation

5. **IActivatable** - Enable/disable records
   - Properties: IsActive
   - Use case: Soft disable without deletion

**Usage Example:**
```csharp
public class Department : BaseEntity, IActivatable, IMultiTenant
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public int TenantId { get; set; }        // from IMultiTenant
    public bool IsActive { get; set; }       // from IActivatable
    // CreatedDate, ModifiedDate from BaseEntity
}
```

### 10.2 Common Enums

**Path:** `Models/Common/Enums.cs`

**Major Enum Categories:**

**Organizational:**
- TenantType: HeadOffice, Regional, Factory, Subsidiary, Warehouse, Branch
- TenantStatus: Active, Inactive, Suspended, Closed

**Forms:**
- FormFieldType: 21 field types (Text, Number, Date, Dropdown, etc.)
- ValidationRuleType: Required, MinLength, MaxLength, Email, Phone, URL, Regex, etc.
- SubmissionStatus: Draft, Submitted, UnderReview, Approved, Rejected, Revised, Cancelled
- RecurrenceFrequency: Daily, Weekly, BiWeekly, Monthly, Quarterly, SemiAnnually, Annually
- FormRenderMode: SinglePage, Wizard
- FieldLayoutType: Single, Matrix, Grid, Inline

**Tickets:**
- TicketStatus: Open, Assigned, InProgress, Pending, Resolved, Closed, Reopened, Cancelled
- TicketPriority: Low, Normal, High, Urgent, Critical
- TicketType: Incident, Request, Problem, Change, Question

**Assets:**
- AssetStatus: Available, Assigned, InUse, InStorage, UnderMaintenance, Damaged, Lost, Stolen, Disposed, Retired
- AssetCondition: Excellent, Good, Fair, Poor, Faulty
- MaintenanceType: Preventive, Corrective, Emergency, Routine, Upgrade

**Licenses:**
- LicenseType: Perpetual, Subscription, Trial, OpenSource, Freeware, Enterprise, Academic
- LicenseStatus: Active, Expired, Expiring, Suspended, Cancelled, Revoked

**Notifications:**
- NotificationChannel: Email, SMS, InApp, WebPush, Slack, Teams
- NotificationStatus: Pending, Sent, Delivered, Read, Failed, Cancelled
- NotificationPriority: Low, Normal, High, Urgent

**Permissions:**
- PermissionType: View, Create, Edit, Delete, Approve, Export, Custom
- RoleLevel: HeadOffice, Regional, Factory

**Reports:**
- ReportFormat: PDF, Excel, CSV, HTML, JSON, XML
- ReportExecutionStatus: Pending, Running, Completed, Failed, Cancelled

**Financial:**
- ExpenditureType: Hardware, Software, License, Maintenance, Training, Consulting, Infrastructure, Other
- BudgetPeriod: Monthly, Quarterly, SemiAnnual, Annual, Custom
- BudgetStatus: Draft, Submitted, Approved, Active, Closed, Cancelled

**Audit:**
- AuditAction: Create, Read, Update, Delete, Login, Logout, PermissionGranted, PermissionRevoked, Export, Import, Approve, Reject, Custom

**Metrics:**
- MetricDataType: Integer, Decimal, Percentage, Currency, Duration, Count
- MetricAggregation: Sum, Average, Min, Max, Count, Latest

---

## 11. INTEGRATION POINTS

### 11.1 External System Integration

**Ticketing Systems:**
- Supported: Jira, ServiceNow, Zendesk, Freshdesk, BMC, ManageEngine, Other
- Bi-directional sync capability
- Fields:
  - IsExternal (boolean flag)
  - ExternalSystem (system name)
  - ExternalTicketId (ID in external system)
  - ExternalTicketUrl (deep link to ticket)
  - LastSyncDate (last sync timestamp)
  - SyncStatus (Synced, Pending, Failed, NotApplicable)
  - SyncError (error message if sync failed)

**Future Integrations (Modeled but Not Implemented):**
- Active Directory/LDAP (user sync)
- Email services (SMTP for notifications)
- SMS gateways (for SMS notifications)
- Slack/Teams (for chat notifications)
- Reporting tools (Power BI, Tableau)
- Cloud storage (for file uploads)

### 11.2 API Capabilities

**Current APIs:**
- FormBuilderApiController - CRUD operations for form builder
- MetricMappingApiController - Metric field mapping

**Potential API Extensions:**
- REST API for form submissions
- REST API for reporting
- Webhook support for notifications
- OAuth2 authentication for external apps

---

## 12. DEPLOYMENT & CONFIGURATION

### 12.1 Configuration Files

**appsettings.json Structure:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=FormReporting;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 12.2 Program.cs Service Registration

**Path:** `Program.cs`

**Key Services Registered:**

**Infrastructure:**
- DbContext with query splitting
- Cookie authentication
- Password hasher
- Data protection

**Application Services:**
```csharp
// Forms services
builder.Services.AddScoped<IFormCategoryService, FormCategoryService>();
builder.Services.AddScoped<IFormTemplateService, FormTemplateService>();
builder.Services.AddScoped<IFormBuilderService, FormBuilderService>();
builder.Services.AddScoped<IFormItemOptionTemplateService, FormItemOptionTemplateService>();

// Metrics services
builder.Services.AddScoped<IMetricDefinitionService, MetricDefinitionService>();
builder.Services.AddScoped<IMetricMappingService, MetricMappingService>();
builder.Services.AddScoped<IMetricPopulationService, MetricPopulationService>();

// Identity services
builder.Services.AddScoped<IScopeService, ScopeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IClaimsService, ClaimsService>();

// Organizational services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
```

**Middleware Pipeline:**
1. HTTPS redirection
2. Static files
3. Routing
4. Authentication
5. Authorization
6. MVC endpoints

---

## 13. DEVELOPMENT WORKFLOW

### 13.1 Adding a New Entity

1. **Create Entity Class** in `Models/Entities/[Domain]/`
2. **Create Configuration** in `Data/Configurations/[Domain]/`
3. **Add DbSet** to ApplicationDbContext
4. **Apply Configuration** in OnModelCreating
5. **Create Migration** and update database
6. **Create Service Interface and Implementation** in `Services/[Domain]/`
7. **Register Service** in Program.cs
8. **Create ViewModels** if needed
9. **Create Controller** if needed
10. **Create Views** if needed

### 13.2 Adding a New Form Field Type

1. **Add Enum Value** to FormFieldType in Enums.cs
2. **Add Field Type Definition** to FormBuilderService.GetAvailableFieldTypes()
3. **Create Field Partial View** in `Views/Shared/Components/Form/Fields/_[FieldType]Field.cshtml`
4. **Update Field Dispatcher** in `_FormField.cshtml` to include new type
5. **Add Configuration Options** in form-builder-properties.js if needed
6. **Update FormItemConfiguration** handling if special configs needed

### 13.3 Code Style & Conventions

**Naming Conventions:**
- Entities: PascalCase (e.g., FormTemplate)
- Tables: Pluralized PascalCase (e.g., FormTemplates)
- Foreign Keys: [EntityName]Id (e.g., TenantId)
- Junction Tables: [Entity1][Entity2] (e.g., UserRole)
- Services: I[Service]Service + [Service]Service (e.g., IFormBuilderService, FormBuilderService)
- ViewModels: [Purpose]ViewModel (e.g., FormBuilderViewModel)
- DTOs: [Purpose]Dto (e.g., CreateSectionDto)

**File Organization:**
- One entity per file
- Configuration classes separate from entities
- Services grouped by domain
- ViewModels grouped by feature

---

## 14. TROUBLESHOOTING GUIDE

### 14.1 Common Issues

**Issue: Cannot login**
- Check: User exists in database
- Check: Password is correctly hashed
- Check: SecurityStamp matches
- Check: User IsActive = true
- Check: Cookie authentication configured

**Issue: Cannot see data (empty lists)**
- Check: User's scope configuration
- Check: Tenant assignment
- Check: Role assignment
- Check: Data exists in database
- Check: Scope filtering in service

**Issue: Form builder not loading**
- Check: Template exists
- Check: JavaScript files loaded (F12 → Network tab)
- Check: API endpoints responding (F12 → Console)
- Check: Template status (Draft only)

**Issue: Cannot publish template**
- Check: Template has at least one section
- Check: Template has at least one field
- Check: All validations passed
- Check: PublishStatus = "Draft"

**Issue: Database migration fails**
- Check: Connection string correct
- Check: SQL Server accessible
- Check: Migration files exist
- Check: No pending migrations
- Run: `dotnet ef database update --verbose`

### 14.2 Debug Tips

**Enable Detailed Logging:**
```json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "Microsoft.EntityFrameworkCore": "Information"
  }
}
```

**View Generated SQL:**
- Set EF Core log level to Information
- Check console output for SQL queries
- Use SQL Server Profiler

**Browser DevTools:**
- F12 → Console: JavaScript errors
- F12 → Network: Failed API calls
- F12 → Application: Cookie/storage issues

---

## 15. PROJECT STATISTICS

**Project Metrics (as of Dec 2025):**

**Code Base:**
- Total Entities: 89+
- Total Controllers: 17
- Total Services: 15+
- Total Views: 100+
- Lines of Code: ~50,000+ (estimated)

**Form Builder:**
- Field Types: 21
- Field Properties: 15+
- Validation Types: 10+
- Option Templates: 10+ (can add more)

**Database:**
- Tables: 89+
- Domain Areas: 12
- Configuration Classes: 70+

**Feature Coverage:**
- Organizational Management: ✓
- Identity & Access Control: ✓
- Form Builder: ✓ (Complete)
- Form Submissions: ✓ (Modeled)
- Metrics & KPIs: ✓ (Modeled)
- Software Management: ✓ (Modeled)
- Hardware Inventory: ✓ (Modeled)
- Support Tickets: ✓ (Modeled)
- Financial Tracking: ✓ (Modeled)
- Notifications: ✓ (Modeled, UI pending)
- Reporting: ✓ (Modeled, UI pending)
- Media Management: ✓ (Modeled)
- Audit & Logging: ✓ (Modeled)

---

## 16. KEY FILES REFERENCE

**Essential Files to Understand:**

**Models:**
- `Models/Entities/Forms/FormTemplate.cs` - Core form entity
- `Models/Entities/Forms/FormTemplateItem.cs` - Form field entity
- `Models/Entities/Identity/User.cs` - User entity
- `Models/Entities/Identity/Role.cs` - Role entity
- `Models/Common/Enums.cs` - All system enums

**Services:**
- `Services/Forms/FormBuilderService.cs` - Form builder business logic
- `Services/Identity/ScopeService.cs` - Scope-based access control
- `Services/Identity/AuthenticationService.cs` - Login/logout

**Controllers:**
- `Controllers/Forms/FormTemplatesController.cs` - Form template management
- `Controllers/API/FormBuilderApiController.cs` - Form builder API

**Views:**
- `Views/Forms/FormTemplates/FormBuilder.cshtml` - Main form builder view
- `Views/Shared/Components/Form/_Form.cshtml` - Form rendering engine

**JavaScript:**
- `wwwroot/assets/js/pages/form-builder.js` - Main orchestrator
- `wwwroot/assets/js/pages/form-builder-dragdrop.js` - Drag & drop

**Configuration:**
- `Data/ApplicationDbContext.cs` - Database context
- `Program.cs` - Application startup and DI

---

## SUMMARY

The **KTDA ICT Reporting System** is a comprehensive enterprise platform with:

✓ **12 major functional domains** covering organizational structure, forms, metrics, software, hardware, tickets, financials, notifications, reporting, media, and audit
✓ **89+ database entities** with well-defined relationships and configurations
✓ **Sophisticated visual form builder** with drag-drop, 21 field types, validation, and conditional logic
✓ **Scope-based multi-tenancy** with 6-level hierarchical access control
✓ **Clean architecture** with service layer, DTOs, and separation of concerns
✓ **Modern frontend** using Bootstrap 5, Vanilla JavaScript, and SortableJS
✓ **Enterprise-grade features** including versioning, workflows, audit trails, and security

**The system is production-ready for the form builder module** and has comprehensive models for all other modules awaiting UI implementation.

---

**Document Version:** 1.0
**Last Updated:** December 5, 2025
**Maintained By:** Development Team
**Next Review:** As needed for new features

---

*This document should be referenced at the start of every new development session to ensure full context and understanding of the system architecture.*
